using SocialMediaAPI.Models.DTOs;

public interface IPostService
{
    Task<IEnumerable<PostResponseDTO>> GetAllPostsAsync(int pageNumber = 1, int pageSize = 10);
    Task<PostResponseDTO?> GetPostByIdAsync(string id);
    Task<IEnumerable<PostResponseDTO>> GetPostsByUserIdAsync(string userId);
    Task<IEnumerable<PostResponseDTO>> GetFeedAsync(string userId, int pageNumber = 1, int pageSize = 10);
    Task<PostResponseDTO> CreatePostAsync(CreatePostDTO createPostDTO, string userId);
    Task<PostResponseDTO?> UpdatePostAsync(string id, UpdatePostDTO updatePostDTO, string userId);
    Task<bool> DeletePostAsync(string id, string userId);
    Task<CommentResponseDTO> AddCommentToPostAsync(string postId, string userId, CreateCommentDTO createCommentDTO);
    Task<CommentResponseDTO?> UpdateCommentAsync(string commentId, string userId, UpdateCommentDTO updateCommentDTO);
    Task<bool> DeleteCommentAsync(string commentId, string userId);
    Task<IEnumerable<CommentResponseDTO>> GetPostCommentsAsync(string postId, int pageNumber = 1, int pageSize = 10);
    Task<CommentResponseDTO?> GetCommentByIdAsync(string commentId);
}