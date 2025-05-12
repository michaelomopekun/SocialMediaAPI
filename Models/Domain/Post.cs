using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Post
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id {get;set;} = string.Empty;
    
    [BsonElement("content")]
    public string? Content {get;set;}
    
    [BsonElement("imageUrl")]
    public string? ImageUrl {get;set;}
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt {get;set;}
    
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt {get;set;}
    
    [BsonElement("userId")]
    public string UserId {get;set;} = string.Empty;
    
    [BsonElement("likesCount")]
    public int? LikesCount {get;set;}
    
    [BsonElement("commentsCount")]
    public int? CommentsCount {get;set;}
    
    [BsonElement("sharesCount")]
    public int? SharesCount {get;set;}
    
}


public class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id {get;set;} = string.Empty;
    
    [BsonElement("content")]
    public string? Content {get;set;}
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt {get;set;}
    
    [BsonElement("updatedAt")]
    public DateTime? UpdatedAt {get;set;}
    
    [BsonElement("postId")]
    public string PostId {get;set;} = string.Empty;
    
    [BsonElement("userId")]
    public string UserId {get;set;} = string.Empty;

    [BsonElement("repliedUserId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? RepliedUserId {get;set;}
}


public class Like
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id {get;set;} = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt {get;set;}

    [BsonElement("postId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PostId {get;set;} = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId {get;set;} = string.Empty;

    [BsonElement("commentId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? CommentId {get;set;}

    [BsonElement("type")]
    public LikeType Type { get; set; }

    [BsonElement("reaction")]
    public ReactionType Reaction { get; set; }

}

public class Share
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id {get;set;} = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt {get;set;}

    [BsonElement("postId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PostId {get;set;} = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId {get;set;} = string.Empty;
}


public enum LikeType
{
    Post = 1,
    Comment = 2
}


public enum ReactionType
{
    Like = 1,
    Love = 2,
    Haha = 3,
    Wow = 4,
    Sad = 5,
    Angry = 6,
    DisLike = 7,
    Care = 8
}

