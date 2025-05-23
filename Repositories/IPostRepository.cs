public interface IPostRepository
{
    Task<IEnumerable<Post?>> GetAllPostsAsync(int pageNumber = 1, int pageSize = 10);
    Task<Post?> GetPostByIdAsync(string id);
    Task <IEnumerable<Post?>> GetPostsByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10);
    Task<IEnumerable<Post?>> GetPostsByUserNameAsync(string userName, int pageNumber = 1, int pageSize = 10);
    Task <IEnumerable<Post?>> GetFollowersPostsAsync(string userId, int pageNumber = 1, int pageSize = 10);
    Task<Post> CreatePostAsync(Post post);
    Task<Post> UpdatePostAsync(string Id, Post post);
    Task<bool> DeletePostAsync(string Id); 
    Task<bool> PostExistsAsync(string id);
    Task<bool> incrementPostSharesCount(string postId, int count);
    Task<bool> incrementPostLikesCount(string postId, int count);
    Task<bool> incrementPostCommentsCount(string postId, int count);
}