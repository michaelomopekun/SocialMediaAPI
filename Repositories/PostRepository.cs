
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

public class PostRepository : IPostRepository
{
    private readonly IMongoCollection<Post> _posts;
    private readonly IMongoCollection<ApplicationUser> _users;
    private readonly IMongoCollection<Follow> _follows;
    private readonly ILogger<PostRepository> _logger;
    private readonly IMemoryCache _cache;
    private const int CACHE_DURATION = 10;

    public PostRepository(ILogger<PostRepository> logger, IMemoryCache cache, IMongoCollection<Post> posts, IMongoCollection<ApplicationUser> users, IMongoCollection<Follow> follows)
    {
        _posts = posts;
        _logger = logger;
        _cache = cache;
        _users = users;
        _follows = follows;
    }

    public async Task<Post> CreatePostAsync(Post post)
    {
        try
        {
            await _posts.InsertOneAsync(post);

            _cache.Remove($"posts-page-1-10");

            return post;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePostAsync::Error creating post: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeletePostAsync(string Id)
    {
        try
        {
            var post = await _posts.Find(i => i.Id == Id).FirstOrDefaultAsync();
            if (post == null) return false;

            await _posts.DeleteOneAsync(p => p.Id == Id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeletePostAsync::Error deleting post: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<Post?>> GetAllPostsAsync(int pageNumber = 1, int pageSize = 10)
    {
        string cacheKey = $"posts-{pageNumber}-{pageSize}";

        if(!_cache.TryGetValue(cacheKey, out IEnumerable<Post>? posts))
        {
            try
            {
                posts = await _posts
                    .Find(_ => true)
                    .SortByDescending(c => c.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION));

                _cache.Set(cacheKey, posts, cacheOptions);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllPostsAsync::Error retrieving all posts: {Message}", ex.Message);
                throw;
            }
        }

        return posts!;
    }

    public async Task<IEnumerable<Post?>> GetFollowersPostsAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        var cacheKey = $"followers-posts-{userId}-{pageNumber}-{pageSize}";

        if (!_cache.TryGetValue(cacheKey, out IEnumerable<Post>? posts))
        {
            try
            {
                var followingIds = await _follows
                    .Find(f => f.FollowingUserId == userId)
                    .Project(f => f.FollowerUserId)
                    .ToListAsync();

                if (!followingIds.Any())
                {
                    return Enumerable.Empty<Post>();
                }

                posts = await _posts
                    .Find(p => followingIds.Contains(p.UserId))
                    .SortByDescending(c => c.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION))
                    .RegisterPostEvictionCallback((key, value, reason, state) =>
                    {
                        _logger.LogInformation("Cache entry {Key} was evicted due to {Reason}", key, reason);
                    });

                _cache.Set(cacheKey, posts, cacheOptions);

                return posts;
        
            }
            catch(Exception ex)
            {
                _logger.LogError("GetFollowersPostsAsync::Error getting Followers Post: {Message}", ex.Message);
                throw;
            }
        }

        return posts ?? Enumerable.Empty<Post>();
    }

    public async Task<Post?> GetPostByIdAsync(string id)
    {
        string cacheKey = $"post-{id}";

        if(!_cache.TryGetValue(cacheKey, out Post? post))
        {
            try
            {
                post = await _posts
                    .Find(p => p.Id == id)
                    .FirstOrDefaultAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION));

                if (post == null) return null!;

                _cache.Set(cacheKey, post, cacheOptions);
            }
            catch(Exception ex)
            {
                _logger.LogError("GetPostByIdAsync::Error getting post: {Message}", ex);
                throw;
            }

        }

        return post;
    }

    public async Task<IEnumerable<Post?>> GetPostsByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        string cacheKey = $"posts-user-{userId}-{pageNumber}-{pageSize}";

        if(!_cache.TryGetValue(cacheKey, out IEnumerable<Post>? posts))
        {
            try
            {
                posts = await _posts
                    .Find(p => p.UserId == userId)
                    .SortByDescending(c => c.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION));

                _cache.Set(cacheKey, posts, cacheOptions);
            }
            catch(Exception ex)
            {
                _logger.LogError("GetPostsByUserIdAsync::Error getting user's posts: {Message}", ex);
                throw;
            }
        }

        return posts!;
    }

    public async Task<IEnumerable<Post?>> GetPostsByUserNameAsync(string userName, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var user = await _users
                .Find(u => u.UserName == userName)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Enumerable.Empty<Post?>();
            }

            var posts = await _posts
                .Find(p => p.UserId == user.Id.ToString())
                .SortByDescending(c => c.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return posts;
        }
        catch(Exception ex)
        {
            _logger.LogError("GetPostsByUserIdAsync::Error getting user's posts: {Message}", ex);
            throw;
        }
    }

    public async Task<bool> PostExistsAsync(string id)
    {
        try
        {
            var exists = await _posts
                .CountDocumentsAsync(p => p.Id.ToString() == id);

            if (exists == 0)
            {
                _logger.LogWarning("PostExistsAsync::Post with ID {Id} does not exist", id);
                return false;
            }

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError("PostExistsAsync::Error validating check: {Message}", ex);
            throw;
        }
    }

    public async Task<Post> UpdatePostAsync(string Id, Post post)
    {
        try
        {
            var existingPost = await _posts.Find(i => i.Id == Id).FirstOrDefaultAsync();
            if (existingPost == null)
            {
                _logger.LogWarning("UpdatePostAsync::Post with ID {Id} does not exist", Id);
                return null!;
            }

            if (!string.IsNullOrWhiteSpace(post.Content))
                existingPost.Content = post.Content;

            if (!string.IsNullOrWhiteSpace(post.ImageUrl))
                existingPost.ImageUrl = post.ImageUrl;

            existingPost.UpdatedAt = DateTime.UtcNow;

            if (post.LikesCount != default)
                existingPost.LikesCount = post.LikesCount;

            if (post.CommentsCount != default)
                existingPost.CommentsCount = post.CommentsCount;

            if (post.SharesCount != default)
                existingPost.SharesCount = post.SharesCount;

            await _posts.UpdateOneAsync(
                Builders<Post>.Filter.Eq(p => p.Id, Id),
                Builders<Post>.Update
                    .Set(p => p.Content, existingPost.Content)
                    .Set(p => p.ImageUrl, existingPost.ImageUrl)
                    .Set(p => p.UpdatedAt, existingPost.UpdatedAt)
                    .Set(p => p.LikesCount, existingPost.LikesCount)
                    .Set(p => p.CommentsCount, existingPost.CommentsCount)
                    .Set(p => p.SharesCount, existingPost.SharesCount));

            _cache.Remove($"post-{Id}");
            _cache.Remove($"posts-page-1-10");

            return existingPost;
        }
        catch(Exception ex)
        {
            _logger.LogError("UpdatePostAsync::Error updating Post{Message}", ex);
            throw;
        }
    }

    public async Task<bool> incrementPostLikesCount(string postId, int count)
    {
        try
        {
            var post = await _posts.Find(postId).FirstOrDefaultAsync();
            if (post == null) return false;

            post.LikesCount = (byte)Math.Max(0, (post.LikesCount ?? 0) + count);

            await _posts.UpdateOneAsync(
                Builders<Post>.Filter.Eq(p => p.Id, post.Id),
                Builders<Post>.Update.Set(p => p.LikesCount, post.LikesCount));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("incrementPostLikesCount::Error incrementing likes count: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> incrementPostCommentsCount(string postId, int count)
    {
        try
        {
            var post = await _posts.Find(postId).FirstOrDefaultAsync();
            if (post == null) return false;

            post.CommentsCount = Math.Max(0, (post.CommentsCount ?? 0) + count);
            
            await _posts.UpdateOneAsync(
                Builders<Post>.Filter.Eq(p => p.Id, post.Id),
                Builders<Post>.Update.Set(p => p.CommentsCount, post.CommentsCount));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("incrementPostCommentsCount::Error incrementing comments count: {Message}", ex.Message);
            throw;
        }
    }
    public async Task<bool> incrementPostSharesCount(string postId, int count)
    {
        try
        {
            var post = await _posts.Find(postId).FirstOrDefaultAsync();
            if (post == null) return false;

            post.SharesCount = (byte)Math.Max(0, (post.SharesCount ?? 0) + count);
            await _posts.UpdateOneAsync(
                Builders<Post>.Filter.Eq(p => p.Id, post.Id),
                Builders<Post>.Update.Set(p => p.SharesCount, post.SharesCount));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("incrementPostSharesCount::Error incrementing shares count: {Message}", ex.Message);
            throw;
        }
    }

}