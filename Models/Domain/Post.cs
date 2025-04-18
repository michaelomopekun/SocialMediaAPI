using SocialMediaAPI.Models.Domain.User;

public class Post
{
    public int Id {get;set;}
    public string? Content {get;set;}
    public string? ImageUrl {get;set;}
    public DateTime CreatedAt {get;set;}
    public DateTime UpdatedAt {get;set;}
    public string UserId {get;set;}
    public ApplicationUser? User {get;set;}
    public ICollection<Comment>? Comments {get;set;}
    public ICollection<Like>? Likes {get;set;}
    public ICollection<Share>? Shares {get;set;}
}


public class Comment
{
    public int Id {get;set;}
    public string? Content {get;set;}
    public DateTime CreatedAt {get;set;}
    public DateTime? UpdatedAt {get;set;}
    public int PostId {get;set;}
    public string UserId {get;set;}
    public string? RepliedUserId {get;set;}
    public Post? Post {get;set;}
    public ApplicationUser? User {get;set;}
    public ICollection<Like>? Likes {get;set;}
}


public class Like
{
    public int Id {get;set;}
    public DateTime CreatedAt {get;set;}
    public int PostId {get;set;}
    public string UserId {get;set;}
    public int CommentId {get;set;}
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
    Angry = 6
}


public class Share
{
    public int Id {get;set;}
    public DateTime CreatedAt {get;set;}
    public int PostId {get;set;}
    public string UserId {get;set;}
    public Post? Post {get;set;}
    public ApplicationUser? User {get;set;}
}