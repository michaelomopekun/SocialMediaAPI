using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using SocialMediaAPI.Models.DTOs;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var cached = await _cache.GetStringAsync(key);
        return cached is null ? default : JsonSerializer.Deserialize<T>(cached);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
        });
    }

    public Task RemoveAsync(string key)
    {
         _cache.RemoveAsync(key);
        return Task.CompletedTask;
    }

    public async Task InvalidateUserFeedCache(string userId)
    {
        var cacheKey = $"feed:{userId}:page:1:size:10";
        await RemoveAsync(cacheKey);
    }

    public async Task SetFeedCacheAsync(string userId, IEnumerable<PostResponseDTO> posts, int pageNumber, int pageSize, TimeSpan? expiry = null)
    {
        var cacheKey = $"feed:{userId}:page:{pageNumber}:size:{pageSize}";
        await SetAsync(cacheKey, posts, expiry ?? TimeSpan.FromMinutes(30));
    }

    public async Task<IEnumerable<PostResponseDTO>?> GetFeedCacheAsync(string userId, int pageNumber, int pageSize)
    {
        var cacheKey = $"feed:{userId}:page:{pageNumber}:size:{pageSize}";
        return await GetAsync<IEnumerable<PostResponseDTO>>(cacheKey);
    }
}
