using MongoDB.Driver;
using SocialMediaAPI.Constants;


namespace SocialMediaAPI.Repositories;

public class LikeRepository : ILikeRepository
{
    private readonly IMongoCollection<Like> _likes;
    private readonly ICacheService _cacheService;
    private readonly ILogger<LikeRepository> _logger;

    public LikeRepository(
        IMongoCollection<Like> likes,
        ICacheService cacheService,
        ILogger<LikeRepository> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
        _likes = likes;
    }

    public async Task<Like?> CreateLikeAsync(Like like)
    {
        try
        {
            var existingLike = await _likes
                .Find(l =>
                    l.UserId == like.UserId &&
                    l.PostId == like.PostId &&
                    (l.CommentId == like.CommentId || 
                    (l.CommentId == null && like.CommentId == null))).FirstOrDefaultAsync();

            if (existingLike != null)
            {
                if (existingLike.Reaction != like.Reaction)
                {
                    existingLike.Reaction = like.Reaction;

                    await _likes.UpdateOneAsync( l => l.Id == existingLike.Id, Builders<Like>.Update.Set(l => l.Reaction, like.Reaction));
                    await InvalidateCacheAsync(existingLike);
                }
                return existingLike;
            }

            like.Type = like.CommentId == null ? LikeType.Post : LikeType.Comment;
            
            await _likes.InsertOneAsync(like);

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
            var like = await _likes
                .Find(l => 
                    l.UserId == userId && 
                    l.PostId == postId &&
                    l.CommentId == commentId).FirstOrDefaultAsync();

            if (like == null)
            {
                return false;
            }

            await _likes.DeleteOneAsync(l => l.Id == like.Id);

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

        var exists = await _likes
            .CountDocumentsAsync(l => 
                l.UserId == userId && 
                l.PostId == postId && 
                l.CommentId == commentId) > 0;

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

        var count = await _likes
            .CountDocumentsAsync(l => 
                l.PostId == postId && 
                l.CommentId == commentId);

        await _cacheService.SetAsync(cacheKey, count, TimeSpan.FromMinutes(15));
        
        return (int)count;
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

        var filter = Builders<Like>.Filter.Eq(l => l.PostId, postId);
        if(commentId != null)
        {
            filter &= Builders<Like>.Filter.Eq(l => l.CommentId, commentId);
        }
        else
        {
            filter &= Builders<Like>.Filter.Eq(l => l.CommentId, null);
        }

        var reactions = await _likes.Aggregate()
            .Match(filter)
            .Group(l => l.Reaction, 
                   g => new { Reaction = g.Key, Count = g.Count() })
            .ToListAsync();

        var reactionCounts = reactions.ToDictionary(x => x.Reaction, y => y.Count);

        await _cacheService.SetAsync(cacheKey, reactionCounts, TimeSpan.FromMinutes(15));

        return reactionCounts;
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

        var reaction = await _likes
            .Find(l => 
                l.UserId == userId && 
                l.PostId == postId && 
                l.CommentId == commentId)
            .Project(l => l.Reaction)
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