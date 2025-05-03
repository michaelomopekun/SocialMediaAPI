
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SocialMediaAPI.Data;

public class PostRepository : IPostRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PostRepository> _logger;
    private readonly IMemoryCache _cache;
    private const int CACHE_DURATION = 10;

    public PostRepository(ApplicationDbContext context, ILogger<PostRepository> logger, IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Post> CreatePostAsync(Post post)
    {
        try
        {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();

            _cache.Remove($"posts-page-1-10");

            return post;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePostAsync::Error creating post: {Message}", ex.Message);
            throw new Exception($"CreatePostAsync::Error creating post: {ex.Message}");
        }
    }

    public async Task<bool> DeletePostAsync(string Id)
    {
        try
        {
            var post = await _context.Posts.FindAsync(Id);
            if (post == null) return false;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeletePostAsync::Error deleting post: {Message}", ex.Message);
            throw new Exception($"DeletePostAsync::Error deleting post: {ex.Message}");
        }
    }

    public async Task<IEnumerable<Post>> GetAllPostsAsync(int pageNumber = 1, int pageSize = 10)
    {
        string cacheKey = $"posts-{pageNumber}-{pageSize}";

        if(!_cache.TryGetValue(cacheKey, out IEnumerable<Post>? posts))
        {
            try
            {
                posts = await _context.Posts
                    .Include(u => u.User)
                    .Include(l => l.Likes)
                    .Include(c => c.Comments)
                    .Include(s => s.Shares)
                    .AsNoTracking()
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new Post
                    {
                        Id = p.Id,
                        Content = p.Content,
                        ImageUrl = p.ImageUrl,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        UserId = p.UserId,
                        User = p.User,
                        LikesCount = p.Likes != null ? p.Likes.Count() : 0,
                        CommentsCount = p.Comments != null ? p.Comments.Count() : 0,
                        SharesCount = p.Shares != null ? p.Shares.Count() : 0
                    })
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION));

                _cache.Set(cacheKey, posts, cacheOptions);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAllPostsAsync::Error retrieving all posts: {Message}", ex.Message);
                throw new Exception($"GetAllPostsAsync::Error retrieving all posts: {ex.Message}");
            }
        }

        return posts!;
    }

    public async Task<IEnumerable<Post>> GetFollowersPostsAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var posts = await _context.Posts
            .Include(u => u.User)
            .Include(l => l.Likes)
            .Include(c => c.Comments)
            .Include(s => s.Shares)
            .Where(p => _context.Follows
                .Where(f => f.FollowerUserId == userId)
                .Select(f => f.FollowingUserId)
                .Contains(p.UserId))
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

            return posts;
        }
        catch(Exception ex)
        {
            _logger.LogError("GetFollowersPostsAsync::Error getting Followers Post: {Message}", ex.Message);
            throw new Exception($"GetFollowersPostsAsync::Error getting Followers Post: {ex.Message}");
        }
    }

    public async Task<Post> GetPostByIdAsync(string id)
    {
        string cacheKey = $"post-{id}";

        if(!_cache.TryGetValue(cacheKey, out Post? post))
        {
            try
            {
                post = await _context.Posts
                    .Include(l => l.Likes)
                    .Include(c => c.Comments)
                    .Include(s => s.Shares)
                    .Include(u => u.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(CACHE_DURATION));

                if (post == null) return null!;

                _cache.Set(cacheKey, post, cacheOptions);
            }
            catch(Exception ex)
            {
                _logger.LogError("GetPostByIdAsync::Error getting post: {Message}", ex);
                throw new Exception($"GetPostByIdAsync::Error getting post: {ex.Message}");
            }

        }

        return post!;
    }

    public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId)
    {
        try
        {
            var posts = await _context.Posts
                .Include(l => l.Likes)
                .Include(c => c.Comments)
                .Include(s => s.Shares)
                .Include(u => u.User)
                .Where(ui => ui.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return posts;
        }
        catch(Exception ex)
        {
            _logger.LogError("GetPostsByUserIdAsync::Error getting user's posts: {Message}", ex);
            throw new Exception($"GetPostsByUserIdAsync::Error getting user's posts: {ex.Message}");
        }
    }

    public async Task<IEnumerable<Post>> GetPostsByUserNameAsync(string userName)
    {
        try
        {
            var posts = await _context.Posts
                .Include(l => l.Likes)
                .Include(c => c.Comments)
                .Include(s => s.Shares)
                .Include(u => u.User)
                .Where(p => p.User.UserName == userName)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return posts;
        }
        catch(Exception ex)
        {
            _logger.LogError("GetPostsByUserIdAsync::Error getting user's posts: {Message}", ex);
            throw new Exception($"GetPostsByUserIdAsync::Error getting user's posts: {ex.Message}");
        }
    }

    public async Task<bool> PostExistsAsync(string id)
    {
        try
        {
            var exists = await _context.Posts
                .AnyAsync(p => p.Id.ToString() == id);
            
            if(!exists)
            {
                return false;
            }

            return exists;
        }
        catch(Exception ex)
        {
            _logger.LogError("PostExistsAsync::Error validating check: {Message}", ex);
            throw new Exception($"PostExistsAsync::Error validating check: {ex.Message}");
        }
    }

    public async Task<Post> UpdatePostAsync(string Id, Post post)
    {
        try
        {
            var existingPost = await _context.Posts
                .FindAsync(Id);
            
            if(existingPost == null)
            {
                return null;
            }

            existingPost.Content = post.Content;
            existingPost.ImageUrl = post.ImageUrl;
            existingPost.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _cache.Remove($"post-{Id}");
            _cache.Remove($"posts-page-1-10");

            return existingPost;
        }
        catch(Exception ex)
        {
            _logger.LogError("UpdatePostAsync::Error updating Post{Message}", ex);
            throw new Exception($"UpdatePostAsync::Error updating Post: {ex.Message}");
        }
    }

}