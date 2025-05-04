using AutoMapper;
using NanoidDotNet;
using SocialMediaAPI.Constants;
using SocialMediaAPI.Models.Domain;
using SocialMediaAPI.Models.DTOs;
using SocialMediaAPI.Repositories;

namespace SocialMediaAPI.Services;

public class LikeService : ILikeService
{
    private readonly ILikeRepository _likeRepository;
    private readonly IProfileRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<LikeService> _logger;
    private const string IdAlphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public LikeService(
        ILikeRepository likeRepository,
        IProfileRepository userRepository,
        ICacheService cacheService,
        IMapper mapper,
        ILogger<LikeService> logger)
    {
        _likeRepository = likeRepository;
        _userRepository = userRepository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LikeResponseDTO> ToggleLikeAsync(string userId, string postId, string? commentId = null, ReactionType reactionType = ReactionType.Like)
    {
        try
        {
            var like = new Like
            {
                Id = Nanoid.Generate(size: 12, alphabet: IdAlphabet),
                UserId = userId,
                PostId = postId,
                CommentId = commentId,
                Reaction = reactionType,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _likeRepository.CreateLikeAsync(like);
            if (result == null)
            {
                throw new Exception("Failed to toggle reaction");
            }

            var user = await _userRepository.GetProfileByIdAsync(userId);
            
            return new LikeResponseDTO
            {
                Id = result.Id,
                PostId = postId,
                CommentId = commentId,
                UserId = userId,
                UserName = user?.UserName ?? string.Empty,
                ReactionType = result.Reaction,
                CreatedAt = result.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling reaction for post {PostId}", postId);
            throw;
        }
    }

    public async Task<LikeStatusDTO> GetLikeStatusAsync(string postId, string userId, string? commentId = null)
    {
        try
        {
            var cacheKey = commentId == null 
                ? CacheKeys.PostReactionCounts(postId)
                : CacheKeys.CommentReactionCounts(commentId);

            var reactionCounts = await _likeRepository.GetReactionCountsAsync(postId, commentId);
            var userReaction = await _likeRepository.GetUserReactionAsync(userId, postId, commentId);
            var totalLikes = reactionCounts.Values.Sum();

            return new LikeStatusDTO
            {
                LikesCount = totalLikes,
                HasLiked = userReaction.HasValue,
                PostId = postId,
                CommentId = commentId,
                ReactionCounts = reactionCounts,
                UserReaction = userReaction
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reaction status for post {PostId}", postId);
            throw;
        }
    }

    private async Task InvalidateReactionCacheAsync(string postId, string? commentId, string userId)
    {
        var cacheKeys = new List<string>
        {
            CacheKeys.PostReactionCounts(postId),
            CacheKeys.UserReactionType(userId, postId)
        };

        if (commentId != null)
        {
            cacheKeys.Add(CacheKeys.CommentReactionCounts(commentId));
            cacheKeys.Add(CacheKeys.UserCommentReactionType(userId, commentId));
        }

        foreach (var key in cacheKeys)
        {
            await _cacheService.RemoveAsync(key);
        }
    }

}