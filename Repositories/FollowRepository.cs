
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
        if(follow == null)
        {
            throw new ArgumentNullException(nameof(follow), "AddFollowAsync::Follow cannot be null");
        }

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
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(blockedUserId))
        {
            throw new ArgumentException("BlockUserAsync::User ID and Blocked User ID cannot be null or empty", nameof(userId));
        }

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
        
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("FollowExistsAsync::FollowID cannot be null or empty", nameof(id));
        }

        return await _context.Follows.AnyAsync(f => f.FollowerUserId == id || f.FollowingUserId == id);
    }

    public async Task<IEnumerable<Follow>> GetBlockedUsersAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("GetBlockedUsersAsync::User ID cannot be null or empty", nameof(userId));
        }

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

    public Task<Follow> GetFollowByFollowerAndFollowingIdAsync(string followerId, string followingId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Follow>> GetFollowersByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetFollowersCountAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Follow>> GetFollowingByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetFollowingCountAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Follow>> GetMutualFollowersAsync(string userId1, string userId2, int pageNumber = 1, int pageSize = 10)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsBlockedAsync(string userId, string blockedUserId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsFollowingAsync(string followerId, string followingId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UnblockUserAsync(string userId, string blockedUserId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UnFollowAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<Follow> UpdateFollowAsync(string id, Follow follow)
    {
        throw new NotImplementedException();
    }
}