
namespace SocialMediaAPI.Models.DTOs
{
    public class CreatePostDTO
    {
        public string? Content {get; set;}
        public string? ImageUrl {get; set;}
    }

    public class UpdatePostDTO
    {
        public string? Content {get; set;}
        public string? ImageUrl {get; set;}
    }
    public class PostResponseDTO
    {
        public int Id {get; set;}
        public string Content {get; set;} = string.Empty;
        public string? ImageUrl {get; set;}
        public DateTime CreatedAt {get; set;}
        public DateTime UpdatedAt {get; set;}
        public string UserId {get; set;} = string.Empty;
        public string UserName {get; set;} = string.Empty;
        public string ? LikesCount {get; set;}
        public string ? CommentsCount {get; set;}
    }
}