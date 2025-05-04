using Microsoft.EntityFrameworkCore;
using SocialMediaAPI.Constants;
using SocialMediaAPI.Data;
using SocialMediaAPI.Models.Domain;

namespace SocialMediaAPI.Repositories;

public class LikeRepository : ILikeRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<LikeRepository> _logger;

    public LikeRepository(
        ApplicationDbContext context,
        ICacheService cacheService,
        ILogger<LikeRepository> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Like?> CreateLikeAsync(Like like)
    {
        try
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => 
                    l.UserId == like.UserId && 
                    l.PostId == like.PostId &&
                    (l.CommentId == like.CommentId || 
                    (l.CommentId == null && like.CommentId == null)));

            if (existingLike != null)
            {
                if (existingLike.Reaction != like.Reaction)
                {
                    existingLike.Reaction = like.Reaction;
                    await _context.SaveChangesAsync();
                    await InvalidateCacheAsync(existingLike);
                }
                return existingLike;
            }

            like.Type = like.CommentId == null ? LikeType.Post : LikeType.Comment;
            
            await _context.Likes.AddAsync(like);
            await _context.SaveChangesAsync();

            await InvalidateCacheAsync(like);

            return like;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating like for post {PostId}", like.PostId);
            throw;
        }
    }

    public async Task<bool> DeleteLikeAsync(string userId, string postId, string? commentId = null)
    {
        try
        {
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => 
                    l.UserId == userId && 
                    l.PostId == postId &&
                    l.CommentId == commentId);

            if (like == null)
            {
                return false;
            }

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            await InvalidateCacheAsync(like);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting like for post {PostId}", postId);
            throw;
        }
    }

    public async Task<bool> HasUserLikedAsync(string userId, string postId, string? commentId = null)
    {
        var cacheKey = commentId == null 
            ? CacheKeys.UserLikeStatus(userId, postId)
            : CacheKeys.UserCommentLikeStatus(userId, commentId);

        var cached = await _cacheService.GetAsync<bool?>(cacheKey);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var exists = await _context.Likes
            .AnyAsync(l => 
                l.UserId == userId && 
                l.PostId == postId && 
                l.CommentId == commentId);

        await _cacheService.SetAsync(cacheKey, exists, TimeSpan.FromMinutes(30));
        
        return exists;
    }

    public async Task<int> GetLikesCountAsync(string postId, string? commentId = null)
    {
        var cacheKey = commentId == null 
            ? CacheKeys.PostLikesCount(postId)
            : CacheKeys.CommentLikesCount(commentId);

        var cached = await _cacheService.GetAsync<int?>(cacheKey);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var count = await _context.Likes
            .CountAsync(l => 
                l.PostId == postId && 
                l.CommentId == commentId);

        await _cacheService.SetAsync(cacheKey, count, TimeSpan.FromMinutes(15));
        
        return count;
    }

    public async Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(string postId, string? commentId = null)
    {
        var cacheKey = commentId == null 
            ? CacheKeys.PostReactionCounts(postId)
            : CacheKeys.CommentReactionCounts(commentId);

        var cached = await _cacheService.GetAsync<Dictionary<ReactionType, int>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var reactions = await _context.Likes
            .Where(l => l.PostId == postId && l.CommentId == commentId)
            .GroupBy(l => l.Reaction)
            .Select(g => new { Reaction = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Reaction, x => x.Count);

        await _cacheService.SetAsync(cacheKey, reactions, TimeSpan.FromMinutes(15));
        
        return reactions;
    }

    public async Task<ReactionType?> GetUserReactionAsync(string userId, string postId, string? commentId = null)
    {
        var cacheKey = commentId == null 
            ? CacheKeys.UserReactionType(userId, postId)
            : CacheKeys.UserCommentReactionType(userId, commentId);

        var cached = await _cacheService.GetAsync<ReactionType?>(cacheKey);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        var reaction = await _context.Likes
            .Where(l => 
                l.UserId == userId && 
                l.PostId == postId && 
                l.CommentId == commentId)
            .Select(l => l.Reaction)
            .FirstOrDefaultAsync();

        await _cacheService.SetAsync(cacheKey, reaction, TimeSpan.FromMinutes(30));
        
        return reaction;
    }

    private async Task InvalidateCacheAsync(Like like)
    {
        var cacheKeys = new List<string>
        {
            CacheKeys.LikesByPost(like.PostId),
            CacheKeys.UserLikeStatus(like.UserId, like.PostId),
            CacheKeys.PostLikesCount(like.PostId)
        };

        if (like.CommentId != null)
        {
            cacheKeys.Add(CacheKeys.LikesByComment(like.CommentId));
            cacheKeys.Add(CacheKeys.UserCommentLikeStatus(like.UserId, like.CommentId));
            cacheKeys.Add(CacheKeys.CommentLikesCount(like.CommentId));
        }

        foreach (var key in cacheKeys)
        {
            await _cacheService.RemoveAsync(key);
        }
    }
}