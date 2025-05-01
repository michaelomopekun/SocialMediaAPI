using SocialMediaAPI.Models.Domain.User;

public class Follow
{
    public int Id {get;set;}
    public DateTime FollowedAt {get;set;}
    public DateTime? UpdatedAt {get;set;}
    public DateTime? BlockedAt {get;set;}
    public DateTime? UnblockedAt {get;set;}
    public DateTime? UnfollowedAt {get;set;}
    public string FollowerUserId {get;set;} = string.Empty;
    public string FollowingUserId {get;set;} = string.Empty;
    public bool IsFollowing {get;set;} = false;
    public bool IsFollower {get;set;} = false;
    public bool IsBlocked {get;set;} = false;
    public ApplicationUser? Follower {get;set;}
    public ApplicationUser? Following {get;set;}
}