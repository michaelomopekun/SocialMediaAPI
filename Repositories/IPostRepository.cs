public interface IPostRepository
{
    Task<IEnumerable<Post>> GetAllPostsAsync();
    Task GetPostByIdAsync(int id);
    Task <IEnumerable<Post>> GetPostsByUserIdAsync(string userId);
    Task GetPostsByUserNameAsync(string userName);
    Task <IEnumerable<Post>> GetFollowersPostsAsync(string userId);
    Task<Post> CreatePostAsync(Post post);
    Task<Post> UpdatePostAsync(string Id, Post post);
    Task<bool> DeletePostAsync(string Id); 
    Task<bool> PostExistsAsync(string id);

}