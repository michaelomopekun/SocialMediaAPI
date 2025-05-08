using System.ComponentModel.DataAnnotations;

public class CreateProfileDTO
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MinLength(5, ErrorMessage = "Address must be at least 5 characters")]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Url(ErrorMessage = "Invalid URL format")]
    [StringLength(500, ErrorMessage = "Profile picture URL must not exceed 500 characters")]
    public string ProfilePictureUrl { get; set; } = string.Empty;

    [Required]
    [StringLength(1000, ErrorMessage = "Bio must not exceed 1000 characters")]
    public string Bio { get; set; } = string.Empty;
}

public class UpdateProfileDTO
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string? Email { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
}

public class ProfileResponseDTO
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public DateTime DateOfBirth { get; set; }

    public string? PhoneNumber { get; set; } = string.Empty;

    public string? Address { get; set; } = string.Empty;

    public string? ProfilePictureUrl { get; set; } = string.Empty;

    public string? Bio { get; set; } = string.Empty;
    public bool ProfileCompleted { get; set; } = false;
    public bool ProfileIsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
