public interface IPostRepository
{
    Task<IEnumerable<Post>> GetAllPostsAsync(int pageNumber = 1, int pageSize = 10);
    Task<Post> GetPostByIdAsync(int id);
    Task <IEnumerable<Post>> GetPostsByUserIdAsync(string userId);
    Task<IEnumerable<Post>> GetPostsByUserNameAsync(string userName);
    Task <IEnumerable<Post>> GetFollowersPostsAsync(string userId);
    Task<Post> CreatePostAsync(Post post);
    Task<Post> UpdatePostAsync(int Id, Post post);
    Task<bool> DeletePostAsync(int Id); 
    Task<bool> PostExistsAsync(string id);

}