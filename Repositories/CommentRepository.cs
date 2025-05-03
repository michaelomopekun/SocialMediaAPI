using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SocialMediaAPI.Data;

public class CommentRepository : ICommentRepository
{

    private readonly ApplicationDbContext _context;
    private readonly IPostRepository _postRepository;
    private readonly ILogger<CommentRepository> _logger;
    private readonly IMemoryCache _cache;
    private const int CACHE_DURATION = 30;

    public CommentRepository(ApplicationDbContext context, IPostRepository postRepository, ILogger<CommentRepository> logger, IMemoryCache cache)
    {
        _context = context;
        _postRepository = postRepository;
        _logger = logger;
        _cache = cache;
    }

    public async Task<IEnumerable<Comment>> GetAllCommentsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var cacheKey = $"comments_{pageNumber}_{pageSize}";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Comment> comments))
            {
                comments = await _context.Comments
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(CACHE_DURATION));

                _cache.Set(cacheKey, comments, cacheEntryOptions);
            }

            return comments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments from the database.");
            throw;
        }
    }

    public async Task<Comment> GetCommentByIdAsync(string id)
    {
        try
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (comment == null) return null;

            return comment;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comment with ID {Id} from the database.", id);
            throw;
        }
    }

    public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(string postId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var cacheKey = $"comments_post_{postId}_{pageNumber}_{pageSize}";
            if (!_cache.TryGetValue( cacheKey, out IEnumerable<Comment> comments))
            {
                comments = await _context.Comments
                    .Where(c => c.PostId == postId)
                    .AsNoTracking()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(CACHE_DURATION));

                _cache.Set(cacheKey, comments, cacheEntryOptions);
            }

            return comments;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for post with ID {PostId} from the database.", postId);
            throw;
        }
    }

    public async Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var cacheKey = $"comments_user_{userId}_{pageNumber}_{pageSize}";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Comment> comments))
            {
                comments = await _context.Comments
                    .Where(c => c.UserId == userId)
                    .AsNoTracking()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(CACHE_DURATION));

                _cache.Set(cacheKey, comments, cacheEntryOptions);
            }

            return comments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comments for user with ID {UserId} from the database.", userId);
            throw;
        }
    }


    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        try
        {

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return comment;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating comment in the database.");
            throw;
        }
    }

    public async Task<Comment> UpdateCommentAsync(string id, Comment comment)
    {
        try
        {
            var existingComment = await _context.Comments.FindAsync(id);
            if (existingComment == null) return null;

            _cache.Remove($"comments_post_{existingComment.PostId}");
            _cache.Remove($"comments_post_{comment.PostId}");

            existingComment.Content = comment.Content;
            existingComment.PostId = comment.PostId;
            existingComment.UserId = comment.UserId;

            await _context.SaveChangesAsync();
            return existingComment;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error updating comment with ID {Id} in the database.", id);
            throw;
        }
    }

    public async Task<bool> DeleteCommentAsync(string id)
    {
        try
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return false;

            _cache.Remove($"comments_post_{comment.PostId}_1_10");
            _cache.Remove($"comments_post_{comment.PostId}_2_10");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment with ID {Id} from the database.", id);
            throw;
        }
    }

    public async Task<bool> CommentExistsAsync(string id)
    {
        try
        {
            return await _context.Comments.AnyAsync(c => c.Id == id);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error checking if comment with ID {Id} exists in the database.", id);
            throw;
        }
    }

    public async Task<Comment?> AddCommentToPostAsync(string postId, Comment comment)
    {
        try
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return null;
            
            comment.Post = post;
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return await _context.Comments
                .Include(c => c.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == comment.Id);
        }
        catch(Exception ex)
        {
            _logger.LogError("AddCommentToPostAsync::Error adding comment to post: {Message}", ex);
            throw new Exception($"AddCommentToPostAsync::Error adding comment to post: {ex.Message}");
        }
    }
}