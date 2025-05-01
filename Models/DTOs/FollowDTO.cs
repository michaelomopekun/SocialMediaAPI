public class FollowRequestDTO
{
    /// <summary>
    /// The ID of the user to be followed (target user)
    /// </summary>
    public string ToFollowUserId { get; set; } = string.Empty;
}

public class FollowResponseDTO
{
    public string Id { get; set; } = string.Empty;
    public string FollowerUserId { get; set; } = string.Empty;
    public string FollowingUserId { get; set; } = string.Empty;
    public DateTime FollowedAt { get; set; }
    public bool IsBlocked { get; set; }
}