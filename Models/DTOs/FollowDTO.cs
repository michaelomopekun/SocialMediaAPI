using System.ComponentModel.DataAnnotations;

public class FollowRequestDTO
{
    /// <summary>
    /// The ID of the user to be followed (target user).
    /// </summary>
    [Required(ErrorMessage = "ToFollowUserId is required.")]
    public string ToFollowUserId { get; set; } = string.Empty;
}

public class FollowResponseDTO
{
    /// <summary>
    /// The unique ID of the follow record.
    /// </summary>
    [Required]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user who is following.
    /// </summary>
    [Required]
    public string FollowerUserId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user being followed.
    /// </summary>
    [Required]
    public string FollowingUserId { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp of when the follow occurred.
    /// </summary>
    public DateTime FollowedAt { get; set; }

    /// <summary>
    /// Indicates whether the follow relationship is blocked.
    /// </summary>
    public bool IsBlocked { get; set; }
}
