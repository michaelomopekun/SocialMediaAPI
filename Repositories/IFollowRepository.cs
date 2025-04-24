public interface IFollowRepository
{
    Task<Follow> GetFollowByFollowerAndFollowingIdAsync(string followerId, string followingId);

    Task<IEnumerable<Follow>> GetFollowersByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10);
    
    Task<IEnumerable<Follow>> GetFollowingByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10);
    
    Task<Follow> AddFollowAsync(Follow follow);
    
    Task<Follow> UpdateFollowAsync(string id, Follow follow);
    
    Task<bool> UnFollowAsync(string id);
    
    Task<bool> FollowExistsAsync(string id);
    
    Task<bool> IsFollowingAsync(string followerId, string followingId);
    
    Task<int> GetFollowersCountAsync(string userId);
    
    Task<int> GetFollowingCountAsync(string userId);
    
    Task<bool> BlockUserAsync(string userId, string blockedUserId);
    
    Task<bool> UnblockUserAsync(string userId, string blockedUserId);
    
    Task<bool> IsBlockedAsync(string userId, string blockedUserId);
    
    Task<IEnumerable<Follow>> GetMutualFollowersAsync(string userId1, string userId2, int pageNumber = 1, int pageSize = 10);
    
    Task<IEnumerable<Follow>> GetBlockedUsersAsync(string userId, int pageNumber = 1, int pageSize = 10);
}