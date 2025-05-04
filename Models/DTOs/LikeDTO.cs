namespace SocialMediaAPI.Models.DTOs;

public class ToggleLikeDTO
{
    public string PostId { get; set; } = string.Empty;
    public string? CommentId { get; set; }
    public ReactionType ReactionType { get; set; } = ReactionType.Like;
}

public class LikeStatusDTO
{
    public int LikesCount { get; set; }
    public bool HasLiked { get; set; }
    public string PostId { get; set; } = string.Empty;
    public string? CommentId { get; set; }
    public Dictionary<ReactionType, int> ReactionCounts { get; set; } = new();
    public ReactionType? UserReaction { get; set; }
}

public class LikeResponseDTO
{
    public string Id { get; set; } = string.Empty;
    public string PostId { get; set; } = string.Empty;
    public string? CommentId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ReactionType ReactionType { get; set; }
    public DateTime CreatedAt { get; set; }
}