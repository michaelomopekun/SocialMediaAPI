using System.ComponentModel.DataAnnotations;

public class CreateCommentDTO
{
    [Required]
    [MinLength(1)]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentDTO
{
    [Required]
    [MinLength(1)]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;
}

public class CommentResponseDTO
{
    [Required]
    public string Id { get; set; } = string.Empty;
    [Required]
    public string Content { get; set; } = string.Empty;
    [Required]
    public string PostId { get; set; } = string.Empty;
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required]
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}