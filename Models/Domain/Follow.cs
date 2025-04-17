using SocialMediaAPI.Models.Domain.User;

public class Follow
{
    public int Id {get;set;}
    public DateTime FollowedAt {get;set;}
    public string FollowerUserId {get;set;}
    public string FollowingUserId {get;set;}
    public bool IsFollowing {get;set;}
    public bool IsFollower {get;set;}
    public bool IsBlocked {get;set;}
    public ApplicationUser? Follower {get;set;}
    public ApplicationUser? Following {get;set;}
}