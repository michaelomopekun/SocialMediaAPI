using SocialMediaAPI.Models.DTOs;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task InvalidateUserFeedCache(string userId);
    Task SetFeedCacheAsync(string userId, IEnumerable<PostResponseDTO> posts, int pageNumber, int pageSize, TimeSpan? expiry = null);
    Task<IEnumerable<PostResponseDTO>?> GetFeedCacheAsync(string userId, int pageNumber, int pageSize);

}
