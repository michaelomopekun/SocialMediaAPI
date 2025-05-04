using System.ComponentModel.DataAnnotations.Schema;
using SocialMediaAPI.Models.Domain.User;

public class Post
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id {get;set;} = string.Empty;
    public string? Content {get;set;}
    public string? ImageUrl {get;set;}
    public DateTime CreatedAt {get;set;}
    public DateTime UpdatedAt {get;set;}
    public string UserId {get;set;} = string.Empty;
    public int? LikesCount {get;set;}
    public int? CommentsCount {get;set;}
    public int? SharesCount {get;set;}
    public ApplicationUser? User {get;set;}
    public ICollection<Comment>? Comments {get;set;}
    public ICollection<Like>? Likes {get;set;}
    public ICollection<Share>? Shares {get;set;}
}


public class Comment
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Id {get;set;} = string.Empty;
    public string? Content {get;set;}
    public DateTime CreatedAt {get;set;}
    public DateTime? UpdatedAt {get;set;}
    public string PostId {get;set;} = string.Empty;
    public string UserId {get;set;} = string.Empty;
    public string? RepliedUserId {get;set;}
    public Post? Post {get;set;}
    public ApplicationUser? User {get;set;}
    public ICollection<Like>? Likes {get;set;}
}


public class Like
{
    public string Id {get;set;} = string.Empty;
    public DateTime CreatedAt {get;set;}
    public string PostId {get;set;} = string.Empty;
    public string UserId {get;set;} = string.Empty;
    public string? CommentId {get;set;}
    public LikeType Type {get;set;}
    public ReactionType Reaction {get;set;}
    public Post? Post {get;set;}
    public Comment? Comment {get;set;}
    public ApplicationUser? User {get;set;}
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


public class Share
{
    public string Id {get;set;} = string.Empty;
    public DateTime CreatedAt {get;set;}
    public string PostId {get;set;} = string.Empty;
    public string UserId {get;set;} = string.Empty;
    public Post? Post {get;set;}
    public ApplicationUser? User {get;set;}
}