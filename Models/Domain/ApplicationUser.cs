using Microsoft.AspNetCore.Identity;
namespace SocialMediaAPI.Models.Domain.User;


public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool ProfileCompleted { get; set; } = false;
    public bool ProfileIsDeleted { get; set; } = false;
    public ICollection<Follow>? Followers { get; set; } = new List<Follow>();
    public ICollection<Follow>? Following { get; set; } = new List<Follow>();
    public ICollection<Like>? Likes { get; set; } = new List<Like>();
    public ICollection<Share>? Shares { get; set; } = new List<Share>();
    public ICollection<Post>? Posts { get; set; } = new List<Post>();
    public ICollection<Comment>? Comments { get; set; } = new List<Comment>();
}