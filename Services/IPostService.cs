using SocialMediaAPI.Models.DTOs;

public interface IPostService
{
    Task<IEnumerable<PostResponseDTO>> GetAllPostsAsync(int pageNumber = 1, int pageSize = 10);
    Task<PostResponseDTO?> GetPostByIdAsync(int id);
    Task<IEnumerable<PostResponseDTO>> GetPostsByUserIdAsync(string userId);
    Task<IEnumerable<PostResponseDTO>> GetFlowersPostsAsync(string userId);
    Task<PostResponseDTO> CreatePostAsync(CreatePostDTO createPostDTO, string userId);
    Task<PostResponseDTO?> UpdatePostAsync(int id, UpdatePostDTO updatePostDTO, string userId);
    Task<bool> DeletePostAsync(int id, string userId);
}