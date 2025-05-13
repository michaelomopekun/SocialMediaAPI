using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

public class CommentRepository : ICommentRepository
{

    private readonly IMongoCollection<Comment> _comments;
    private readonly IMongoCollection<Post> _posts;
    private readonly ILogger<CommentRepository> _logger;
    private readonly IMemoryCache _cache;
    private const int CACHE_DURATION = 30;

    public CommentRepository(ILogger<CommentRepository> logger, IMemoryCache cache, IMongoCollection<Comment> comments, IMongoCollection<Post> posts)
    {
        _logger = logger;
        _comments = comments;
        _posts = posts;
        _cache = cache;
    }

    public async Task<IEnumerable<Comment>> GetAllCommentsAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var cacheKey = $"comments_{pageNumber}_{pageSize}";
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Comment>? comments))
            {
                comments = await _comments.Find(_ => true)
                    .SortByDescending(c => c.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(CACHE_DURATION));

                _cache.Set(cacheKey, comments, cacheEntryOptions);
            }

            return comments!;
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
            var comment = await _comments
                .Find(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (comment == null) return null!;

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
            if (!_cache.TryGetValue( cacheKey, out IEnumerable<Comment>? comments))
            {
                comments = await _comments
                    .Find(c => c.PostId == postId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(CACHE_DURATION));

                _cache.Set(cacheKey, comments, cacheEntryOptions);
            }

            return comments!;
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
            if (!_cache.TryGetValue(cacheKey, out IEnumerable<Comment>? comments))
            {
                comments = await _comments
                    .Find(c => c.UserId == userId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Limit(pageSize)
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(CACHE_DURATION));

                _cache.Set(cacheKey, comments, cacheEntryOptions);
            }

            return comments!;
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
            await _comments.InsertOneAsync(comment);

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
            var existingComment = await _comments.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (existingComment == null) return null!;

            _cache.Remove($"comments_post_{existingComment.PostId}");
            _cache.Remove($"comments_post_{comment.PostId}");
            
            existingComment.Content = comment.Content;
            existingComment.PostId = comment.PostId;
            existingComment.UserId = comment.UserId;

            await _comments.ReplaceOneAsync(c => c.Id == id, existingComment);

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
            var comment = await _comments.Find(i => i.Id == id).FirstOrDefaultAsync();
            if (comment == null) return false;

            _cache.Remove($"comments_post_{comment.PostId}_1_10");
            _cache.Remove($"comments_post_{comment.PostId}_2_10");

            await _comments.DeleteOneAsync(c => c.Id == id);

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
            var count = await _comments.CountDocumentsAsync(c => c.Id == id);
            return count > 0;
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
            var post = await _posts.Find(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null) return null;

            comment.PostId = post.Id;

            await _comments.InsertOneAsync(comment);

            return await _comments.Find(i => i.Id == comment.Id).FirstOrDefaultAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError("AddCommentToPostAsync::Error adding comment to post: {Message}", ex);
            throw new Exception($"AddCommentToPostAsync::Error adding comment to post: {ex.Message}");
        }
    }
}