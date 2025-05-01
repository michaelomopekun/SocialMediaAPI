public interface IFollowService
{
    Task<FollowResponseDTO> GetFollowByFollowerAndFollowingIdAsync(string followerId, string followingId);
    
    Task<IEnumerable<FollowResponseDTO>> GetFollowersByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10);
    
    Task<IEnumerable<FollowResponseDTO>> GetFollowingByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10);
    
    Task<FollowResponseDTO> AddFollowAsync(FollowRequestDTO request, string followerUserId);
    
    Task<FollowResponseDTO> UpdateFollowAsync(string id, FollowResponseDTO request);
    
    Task<bool> UnFollowAsync(string id);
    
    Task<bool> FollowExistsAsync(string id);
    
    Task<bool> IsFollowingAsync(string followerId, string followingId);
    
    Task<int> GetFollowersCountAsync(string userId);
    
    Task<int> GetFollowingCountAsync(string userId);
    
    Task<bool> BlockUserAsync(string userId, string blockedUserId);
    
    Task<bool> UnblockUserAsync(string userId, string blockedUserId);
    
    Task<bool> IsBlockedAsync(string userId, string blockedUserId);
    
    Task<IEnumerable<FollowResponseDTO>> GetMutualFollowersAsync(string userId1, string userId2, int pageNumber = 1, int pageSize = 10);
    
    Task<IEnumerable<FollowResponseDTO>> GetBlockedUsersAsync(string userId, int pageNumber = 1, int pageSize = 10);
}