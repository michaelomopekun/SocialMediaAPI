
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SocialMediaAPI.Data;

public class FollowRepository : IFollowRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FollowRepository> _logger;
    private readonly IMemoryCache _cache;
    private const int CACHE_DURATION = 10;

    public FollowRepository(ApplicationDbContext context, ILogger<FollowRepository> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }


    public async Task<Follow> AddFollowAsync(Follow follow)
    {
        if(follow == null) throw new ArgumentNullException(nameof(follow), "AddFollowAsync::Follow cannot be null");

        try
        {
            await _context.Follows.AddAsync(follow);
            await _context.SaveChangesAsync();

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
            var blockedUser = _context.Follows.FirstOrDefault(f => f.FollowerUserId == blockedUserId && f.FollowingUserId == userId) ?? throw new ArgumentException("Blocked user not found", nameof(blockedUserId));
        
            blockedUser.IsBlocked = true;
            await _context.SaveChangesAsync();

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
        return await _context.Follows.AnyAsync(f => f.FollowerUserId == id || f.FollowingUserId == id);
    }

    public async Task<IEnumerable<Follow>> GetBlockedUsersAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        string cacheKey = $"posts-{pageNumber}-{pageSize}";

        if(!_cache.TryGetValue(cacheKey, out IEnumerable<Follow>? blockedUsers))
        {
            try
            {
                blockedUsers = await _context.Follows
                    .Where(f => f.FollowingUserId == userId && f.IsBlocked)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
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
        return Enumerable.Empty<Follow>();
    }

    public async Task<Follow> GetFollowByFollowerAndFollowingIdAsync(string followerId, string followingId)
    {
        try
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerUserId == followerId && 
                                        f.FollowingUserId == followingId) ?? throw new ArgumentException("Follow not found", nameof(followingId));
                
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

            return await _context.Follows
                .Where(f => f.FollowingUserId == userId && !f.IsBlocked)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFollowersByUserIdAsync::Error fetching followers for user {UserId}", userId);
            throw;
        }
    }

    public Task<int> GetFollowersCountAsync(string userId)
    {
        try
        {
            return _context.Follows.CountAsync(f => f.FollowingUserId == userId && !f.IsBlocked);
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
            return await _context.Follows
                .Where(f => f.FollowerUserId == userId && !f.IsBlocked)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFollowingByUserIdAsync::Error fetching following for user {UserId}", userId);
            throw;
        }
    }

    public Task<int> GetFollowingCountAsync(string userId)
    {
        try
        {
            return _context.Follows.CountAsync(f => f.FollowerUserId == userId && !f.IsBlocked);
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
            return await _context.Follows
                .Where(f => f.FollowerUserId == userId1 && f.FollowingUserId == userId2 && !f.IsBlocked)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
            return await _context.Follows
                .Where(f => f.FollowerUserId == userId && f.FollowingUserId == blockedUserId && f.IsBlocked)
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
            return await _context.Follows
                .Where(f => f.FollowingUserId == followingId && f.FollowerUserId == followerId && !f.IsBlocked)
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
            var blockedUser = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerUserId == blockedUserId && f.FollowingUserId == userId) ?? throw new ArgumentException("Blocked user not found", nameof(blockedUserId));
        
            blockedUser.IsBlocked = false;
            await _context.SaveChangesAsync();

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
            var follow = await _context.Follows.FirstAsync(f => f.Id.ToString() == id) ?? throw new ArgumentException("UnFollowAsync::Follow not found", nameof(id));
        
            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "UnFollowAsync::Error unfollowing user with ID {Id}", id);
            throw;
        }
    }

    public async Task<Follow> UpdateFollowAsync(string id, Follow follow)
    {
        try
        {
            var follows = await _context.Follows.FindAsync(id) ?? throw new ArgumentException("UpdateFollowAsync::Follow not found", nameof(id));

            follows.IsBlocked = follow.IsBlocked;
            follows.FollowerUserId = follow.FollowerUserId;
            follows.FollowingUserId = follow.FollowingUserId;
            follows.FollowedAt = follow.FollowedAt;

            _context.Follows.Update(follows);
            await _context.SaveChangesAsync();

            return follows;

        }
        catch(Exception ex)
        {

            _logger.LogError(ex, "UpdateFollowAsync::Error updating follow with ID {Id}", id);
            throw;
        }
    }
}