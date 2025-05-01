public interface ICommentRepository
{
    Task<IEnumerable<Comment>> GetAllCommentsAsync(int pageNumber = 1, int pageSize = 10);
    Task<Comment> GetCommentByIdAsync(string id);
    Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(string postId, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Comment>> GetCommentsByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10);
    // Task<IEnumerable<Comment>> GetCommentsByUserNameAsync(string userName, int pageNumber = 1, int pageSize = 10);
    Task<Comment> CreateCommentAsync(Comment comment);
    Task<Comment> UpdateCommentAsync(string id, Comment comment);
    Task<Comment?> AddCommentToPostAsync(string postId, Comment comment);

    Task<bool> DeleteCommentAsync(string id);
    Task<bool> CommentExistsAsync(string id);
}