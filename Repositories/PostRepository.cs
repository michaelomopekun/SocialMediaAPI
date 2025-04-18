
using SocialMediaAPI.Data;

public class PostRepository : IPostRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PostRepository> _logger;

    public PostRepository(ApplicationDbContext context, ILogger<PostRepository> logger)
    {
        _context = context;
        _logger = logger;
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
            _logger.LogError(ex, "Error creating post: {Message}", ex.Message);
            throw new Exception("Error creating post", ex);
        }
    }

    public Task<bool> DeletePostAsync(string Id)
    {
        try
        {
            var post = _context.Posts.Find(Id);
            if (post == null)
            {
                return Task.FromResult(false);
            }

            _context.Posts.Remove(post);
            _context.SaveChanges();

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post: {Message}", ex.Message);
            throw new Exception("Error deleting post", ex);
        }
    }

    public async Task<IEnumerable<Post>> GetAllPostsAsync()
    {
        try
        {
            var posts = await _context.Posts
                .Include(u => u.User)
                .Include(l => l.Likes)
                .Include(c => c.Comments)
                .ToListAsync();
            return posts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all posts: {Message}", ex.Message);
            throw new Exception("Error retrieving all posts", ex);
        }
    }

    public Task<IEnumerable<Post>> GetFollowersPostsAsync(string userId)
    {

    }

    public Task GetPostByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task GetPostsByUserNameAsync(string userName)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PostExistsAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<Post> UpdatePostAsync(string Id, Post post)
    {
        throw new NotImplementedException();
    }
}