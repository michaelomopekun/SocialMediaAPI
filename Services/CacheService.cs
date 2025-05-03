using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

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
}
