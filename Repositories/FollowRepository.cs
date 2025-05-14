using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

public class FollowRepository : IFollowRepository
{
    private readonly IMongoCollection<Follow> _follows;
    private readonly IMongoCollection<ApplicationUser> _users;
    private readonly ILogger<FollowRepository> _logger;
    private readonly IMemoryCache _cache;
    private const int CACHE_DURATION = 10;

    public FollowRepository(ILogger<FollowRepository> logger, IMemoryCache cache, IMongoCollection<Follow> follows, IMongoCollection<ApplicationUser> users)
    {
        _logger = logger;
        _cache = cache;
        _follows = follows;
        _users = users;
    }


    public async Task<Follow> AddFollowAsync(Follow follow)
    {
        try
        {
            await _follows.InsertOneAsync(follow);

            _cache.Remove($"followers-{follow.FollowingUserId}-page-1-10");
            _cache.Remove($"following-{follow.FollowerUserId}-page-1-10");

            return follow;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "AddFollowAsync::Error adding follow for user {FollowerId} to {FollowingId}", follow.FollowerUserId, follow.FollowingUserId);
            throw;
        }
    }

    public async Task<bool> BlockUserAsync(string userId, string blockedUserId)
    {
        try
        {
            var blockedUser = await _follows.Find(f => f.FollowerUserId == blockedUserId && f.FollowingUserId == userId).FirstOrDefaultAsync();

            if (blockedUser == null) return false;
        
            blockedUser.IsBlocked = true;
            await _follows.ReplaceOneAsync(f => f.Id == blockedUser.Id, blockedUser);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BlockUserAsync::Error blocking user {BlockedUserId} by {UserId}", blockedUserId, userId);
            throw;
        }

    }

    public async Task<bool> FollowExistsAsync(string id)
    {
        var counts = await _follows.CountDocumentsAsync(f => f.FollowerUserId == id || f.FollowingUserId == id);

        return (int)counts > 0;
    }

    public async Task<IEnumerable<Follow>> GetBlockedUsersAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        string cacheKey = $"posts-{pageNumber}-{pageSize}";

        if(!_cache.TryGetValue(cacheKey, out IEnumerable<Follow>? blockedUsers))
        {
            try
            {
                blockedUsers = await _follows
                    .Find(f => f.FollowingUserId == userId && f.IsBlocked)
                    .Skip((pageNumber - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION));

                _cache.Set(cacheKey, blockedUsers, cacheOptions);

                return blockedUsers;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "GetBlockedUsersAsync::Error fetching Blocked users for user {UserId}",userId);
                throw;
            }
        }
        return blockedUsers ?? Enumerable.Empty<Follow>();
    }

    public async Task<Follow> GetFollowByFollowerAndFollowingIdAsync(string followerId, string followingId)
    {
        try
        {
            var follow = await _follows
                .Find(f => f.FollowerUserId == followerId &&
                      f.FollowingUserId == followingId).FirstOrDefaultAsync();
 
            return follow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting follow relationship between {FollowerId} and {FollowingId}", followerId, followingId);
            throw;
        }
    }

    public async Task<IEnumerable<Follow>> GetFollowersByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {

            return await _follows
                .Find(f => f.FollowingUserId == userId && !f.IsBlocked)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFollowersByUserIdAsync::Error fetching followers for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetFollowersCountAsync(string userId)
    {
        try
        {
            var counts = await _follows.CountDocumentsAsync(f => f.FollowingUserId == userId && !f.IsBlocked);
            return (int)counts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFollowersCountAsync::Error fetching followers count for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Follow>> GetFollowingByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            return await _follows
                .Find(f => f.FollowerUserId == userId && !f.IsBlocked)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFollowingByUserIdAsync::Error fetching following for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetFollowingCountAsync(string userId)
    {
        try
        {
            var count = await _follows.CountDocumentsAsync(f => f.FollowerUserId == userId && !f.IsBlocked);
            return (int)count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFollowingCountAsync::Error fetching following count for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Follow>> GetMutualFollowersAsync(string userId1, string userId2, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            return await _follows
                .Find(f => f.FollowerUserId == userId1 && f.FollowingUserId == userId2 && !f.IsBlocked)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMutualFollowersAsync::Error fetching mutual followers between user {UserId1} and {UserId2}", userId1, userId2);
            throw;
        }
    }

    public async Task<bool> IsBlockedAsync(string userId, string blockedUserId)
    {
        try
        {
            return await _follows
                .Find(f => f.FollowerUserId == userId && f.FollowingUserId == blockedUserId && f.IsBlocked)
                .AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IsBlockedAsync::Error checking if user {UserId} has blocked {BlockedUserId}", userId, blockedUserId);
            throw;
        }
    }

    public async Task<bool> IsFollowingAsync(string followerId, string followingId)
    {
        try
        {
            return await _follows
                .Find(f => f.FollowingUserId == followingId && f.FollowerUserId == followerId && !f.IsBlocked)
                .AnyAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "IsFollowingAsync::Error checking if user {FollowerId} is following {FollowingId}", followerId, followingId);
            throw;
        }
    }

    public async Task<bool> UnblockUserAsync(string userId, string blockedUserId)
    {
        try
        {
            var blockedUser = await _follows.Find(f => f.FollowerUserId == blockedUserId && f.FollowingUserId == userId).FirstOrDefaultAsync();
            if (blockedUser == null) return false;
        
            blockedUser.IsBlocked = false;
            await _follows.ReplaceOneAsync(f => f.Id == blockedUser.Id, blockedUser);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "UnblockUserAsync::Error unblocking user {BlockedUserId} by {UserId}", blockedUserId, userId);
            throw;
        }
    }

    public async Task<bool> UnFollowAsync(string id)
    {
        try
        {
            var follow = await _follows.Find(f => f.FollowingUserId == id).FirstOrDefaultAsync();
            if (follow == null) return false;

            await _follows.DeleteOneAsync(f => f.Id == follow.Id);

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "UnFollowAsync::Error unfollowing user with ID {Id}", id);
            throw;
        }
    }

    public async Task<Follow?> UpdateFollowAsync(string id, Follow follow)
    {
        try
        {
            var follows = await _follows.Find(f => f.Id == id).FirstOrDefaultAsync();
            if (follows == null) return null;

            follows.IsBlocked = follow.IsBlocked;
            follows.FollowerUserId = follow.FollowerUserId;
            follows.FollowingUserId = follow.FollowingUserId;
            follows.FollowedAt = follow.FollowedAt;

            await _follows.ReplaceOneAsync(f => f.Id == follows.Id, follows);

            return follows;

        }
        catch(Exception ex)
        {

            _logger.LogError(ex, "UpdateFollowAsync::Error updating follow with ID {Id}", id);
            throw;
        }
    }
}