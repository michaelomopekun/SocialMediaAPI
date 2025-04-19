
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
            return post;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreatePostAsync::Error creating post: {Message}", ex.Message);
            throw new Exception($"CreatePostAsync::Error creating post: {ex.Message}");
        }
    }

    public async Task<bool> DeletePostAsync(int Id)
    {
        try
        {
            var post = _context.Posts.Find(Id);
            if (post == null)
            {
                return false;
            }

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
        try
        {
            var posts = await _context.Posts
                .Include(u => u.User)
                .Include(l => l.Likes)
                .Include(c => c.Comments)
                .Include(s => s.Shares)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return posts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllPostsAsync::Error retrieving all posts: {Message}", ex.Message);
            throw new Exception($"GetAllPostsAsync::Error retrieving all posts: {ex.Message}");
        }
    }

    public async Task<IEnumerable<Post>> GetFollowersPostsAsync(string userId)
    {
        try
        {
            var followeingId = await _context.Follows
                .Where(f => f.FollowerUserId == userId)
                .Select(fi => fi.FollowingUserId)
                .ToListAsync();

            var posts = await _context.Posts
                .Include(u => u.User)
                .Include(l => l.Likes)
                .Include(c => c.Comments)
                .Include(s => s.Shares)
                .Where(u => followeingId.Contains(u.UserId))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return posts;
        }
        catch(Exception ex)
        {
            _logger.LogError("GetFollowersPostsAsync::Error getting Followers Post: {Message}", ex.Message);
            throw new Exception($"GetFollowersPostsAsync::Error getting Followers Post: {ex.Message}");
        }
    }

    public async Task<Post> GetPostByIdAsync(int id)
    {
        // string cacheKey = $"post-{id}";
        try
        {
            var post = await _context.Posts
                .Include(l => l.Likes)
                .Include(c => c.Comments)
                .Include(s => s.Shares)
                .Include(u => u.User)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            return post;
        }
        catch(Exception ex)
        {
            _logger.LogError("GetPostByIdAsync::Error getting post: {Message}", ex);
            throw new Exception($"GetPostByIdAsync::Error getting post: {ex.Message}");
        }
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

    public async Task<Post> UpdatePostAsync(int Id, Post post)
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

            return existingPost;
        }
        catch(Exception ex)
        {
            _logger.LogError("UpdatePostAsync::Error updating Post{Message}", ex);
            throw new Exception($"UpdatePostAsync::Error updating Post: {ex.Message}");
        }
    }
}