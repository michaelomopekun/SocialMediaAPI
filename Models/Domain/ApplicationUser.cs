using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class ApplicationUser : MongoIdentityUser
{
    [BsonElement("firstName")]
    public string? FirstName { get; set; }

    [BsonElement("lastName")]
    public string? LastName { get; set; }

    [BsonElement("profilePictureUrl")]
    public string? ProfilePictureUrl { get; set; }

    [BsonElement("dateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("profileCompleted")]
    public bool ProfileCompleted { get; set; } = false;

    [BsonElement("profileIsDeleted")]
    public bool ProfileIsDeleted { get; set; } = false;

}