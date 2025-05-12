using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Follow
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("followedAt")]
    public DateTime FollowedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [BsonElement("blockedAt")]
    public DateTime? BlockedAt { get; set; }

    [BsonElement("unblockedAt")]
    public DateTime? UnblockedAt { get; set; }

    [BsonElement("unfollowedAt")]
    public DateTime? UnfollowedAt { get; set; }

    [BsonElement("followerUserId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FollowerUserId { get; set; } = string.Empty;

    [BsonElement("followingUserId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FollowingUserId { get; set; } = string.Empty;

    [BsonElement("isFollowing")]
    public bool IsFollowing { get; set; } = false;

    [BsonElement("isFollower")]
    public bool IsFollower { get; set; } = false;

    [BsonElement("isBlocked")]
    public bool IsBlocked { get; set; } = false;

}