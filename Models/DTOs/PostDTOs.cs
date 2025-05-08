using System.ComponentModel.DataAnnotations;

namespace SocialMediaAPI.Models.DTOs
{
    public class CreatePostDTO
    {
        [Required(ErrorMessage = "Content is required.")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Content must be between 1 and 500 characters.")]
        public string? Content { get; set; }

        [Url(ErrorMessage = "ImageUrl must be a valid URL.")]
        public string? ImageUrl { get; set; }
    }

    public class UpdatePostDTO
    {
        [StringLength(500, ErrorMessage = "Content must not exceed 500 characters.")]
        public string? Content { get; set; }

        [Url(ErrorMessage = "ImageUrl must be a valid URL.")]
        public string? ImageUrl { get; set; }
    }

    public class UpdatePostLikesDTO
    {
        [Required(ErrorMessage = "LikesCount is required.")]
        public int LikesCount { get; set; }
    }

    public class PostResponseDTO
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Url(ErrorMessage = "ImageUrl must be a valid URL.")]
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        public string? LikesCount { get; set; }
        public string? CommentsCount { get; set; }
    }
}
