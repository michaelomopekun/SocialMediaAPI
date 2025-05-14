namespace SocialMediaAPI.Repositories;

public interface ILikeRepository
{
    Task<Like?> CreateLikeAsync(Like like);
    Task<bool> DeleteLikeAsync(string userId, string postId, string? commentId = null);
    Task<bool> HasUserLikedAsync(string userId, string postId, string? commentId = null);
    Task<int> GetLikesCountAsync(string postId, string? commentId = null);
    Task<Dictionary<ReactionType, int>> GetReactionCountsAsync(string postId, string? commentId = null);
    Task<ReactionType?> GetUserReactionAsync(string userId, string postId, string? commentId = null);
}