using Microsoft.AspNetCore.Identity;


namespace SocialMediaAPI.Models.Domain.User;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // // Navigation properties
    // public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
    // public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
}