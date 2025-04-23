using SocialMediaAPI.Models.DTOs;

public interface IPostService
{
    Task<IEnumerable<PostResponseDTO>> GetAllPostsAsync(int pageNumber = 1, int pageSize = 10);
    Task<PostResponseDTO?> GetPostByIdAsync(string id);
    Task<IEnumerable<PostResponseDTO>> GetPostsByUserIdAsync(string userId);
    Task<IEnumerable<PostResponseDTO>> GetFollowersPostsAsync(string userId);
    Task<PostResponseDTO> CreatePostAsync(CreatePostDTO createPostDTO, string userId);
    Task<PostResponseDTO?> UpdatePostAsync(string id, UpdatePostDTO updatePostDTO, string userId);
    Task<bool> DeletePostAsync(string id, string userId);
}