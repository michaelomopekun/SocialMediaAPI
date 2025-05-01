using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SocialMediaAPI.Data;

public class CommentRepository : ICommentRepository
{

    private readonly ApplicationDbContext _context;
    private readonly IPostRepository _postRepository;
    private readonly ILogger<CommentRepository> _logger;
    private readonly IMemoryCache _cache;
    private const int CACHE_DURATION = 10;

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
        if(string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Comment ID cannot be null or empty.", nameof(id));
        }

        try
        {
            return await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(i => i.Id == id) ?? throw new KeyNotFoundException($"Comment with ID {id} not found.");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error retrieving comment with ID {Id} from the database.", id);
            throw;
        }
    }

    public async Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(string postId, int pageNumber = 1, int pageSize = 10)
    {
        if(string.IsNullOrEmpty(postId))
        {
            throw new ArgumentException("Post ID cannot be null or empty.", nameof(postId));
        }

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
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
        }

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
        if(comment == null)
        {
            throw new ArgumentNullException(nameof(comment), "Comment cannot be null.");
        }
        if(string.IsNullOrEmpty(comment.PostId))
        {
            throw new ArgumentException("Post ID cannot be null or empty.", nameof(comment.PostId));
        }
        if(string.IsNullOrEmpty(comment.UserId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(comment.UserId));
        }

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
        if(string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Comment ID cannot be null or empty.", nameof(id));
        }
        if(comment == null)
        {
            throw new ArgumentNullException(nameof(comment), "Comment cannot be null.");
        }
        if(string.IsNullOrEmpty(comment.Content))
        {
            throw new ArgumentException("Comment content cannot be null or empty.", nameof(comment.Content));
        }
        if(string.IsNullOrEmpty(comment.PostId))
        {
            throw new ArgumentException("Post ID cannot be null or empty.", nameof(comment.PostId));
        }
        if(string.IsNullOrEmpty(comment.UserId))
        {
            throw new ArgumentException("User ID cannot be null or empty.", nameof(comment.UserId));
        }

        try
        {
            var existingComment = await _context.Comments.FindAsync(id) ?? throw new KeyNotFoundException($"Comment with ID {id} not found.");

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
        if(string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Comment ID cannot be null or empty.", nameof(id));
        }

        try
        {
            var comment = _context.Comments.Find(id) ?? throw new KeyNotFoundException($"Comment with ID {id} not found.");

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
        if(string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("Comment ID cannot be null or empty.", nameof(id));
        }

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
            var post = await _context.Posts.FindAsync(postId) ?? throw new KeyNotFoundException($"Post with ID {postId} not found.");
            
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