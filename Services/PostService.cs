using System.Drawing;
using AutoMapper;
using NanoidDotNet;
using SocialMediaAPI.Models.DTOs;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PostService> _logger;
    private const string size = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    private const int idLength = 8;

    public PostService(IPostRepository postRepository, IMapper mapper, ILogger<PostService> logger, ICommentRepository commentRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _mapper = mapper;
        _logger = logger;
    }


    public async Task<PostResponseDTO> CreatePostAsync(CreatePostDTO createPostDTO, string userId)
    {
        var post = new Post
        {
            Id = Nanoid.Generate(size, idLength),
            Content = createPostDTO.Content,
            ImageUrl = createPostDTO.ImageUrl,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var createdPost = await _postRepository.CreatePostAsync(post);      

        return _mapper.Map<PostResponseDTO>(createdPost);
    }

    public async Task<bool> DeletePostAsync(string id, string userId)
    {

        var post = await _postRepository.GetPostByIdAsync(id);

        if (post?.UserId != userId)
        {
            throw new UnauthorizedAccessException("You cannot delete this post");
        }

        var isDeleted = await _postRepository.DeletePostAsync(id);
        if (!isDeleted) return false;

        return isDeleted;
    }

    public async Task<IEnumerable<PostResponseDTO>> GetAllPostsAsync(int pageNumber = 1, int pageSize = 10)
    {
        var posts = await _postRepository.GetAllPostsAsync(pageNumber, pageSize);
        return _mapper.Map<IEnumerable<PostResponseDTO>>(posts);
    }

    public async Task<IEnumerable<PostResponseDTO>> GetFollowersPostsAsync(string userId)
    {
        var posts = await _postRepository.GetFollowersPostsAsync(userId, 1, 10);       
        return _mapper.Map<IEnumerable<PostResponseDTO>>(posts);
    }

    public async Task<PostResponseDTO?> GetPostByIdAsync(string id)
    {
        var post = await _postRepository.GetPostByIdAsync(id);
        return _mapper.Map<PostResponseDTO>(post);        
    }

    public async Task<IEnumerable<PostResponseDTO>> GetPostsByUserIdAsync(string userId)
    {
        var posts = await _postRepository.GetPostsByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<PostResponseDTO>>(posts);
    }

    public async Task<PostResponseDTO?> UpdatePostAsync(string id, UpdatePostDTO updatePostDTO, string userId)
    {
        var existingPost = await _postRepository.GetPostByIdAsync(id);
        if (existingPost == null) return null;
   
        existingPost.Content = updatePostDTO.Content;
        existingPost.ImageUrl = updatePostDTO.ImageUrl;
        existingPost.UpdatedAt = DateTime.UtcNow;

        var updatedPost = await _postRepository.UpdatePostAsync(id, existingPost);
        if (updatedPost == null) return null;

        return _mapper.Map<PostResponseDTO>(updatedPost);
    }

    public async Task<CommentResponseDTO> AddCommentToPostAsync(string postId, string userId, CreateCommentDTO createCommentDTO)
    {
        try
        {
            var comment = new Comment
            {
                Id = Nanoid.Generate(size, 8),
                Content = createCommentDTO.Content,
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentRepository.AddCommentToPostAsync(postId, comment);
            if (createdComment == null) return null;

            return _mapper.Map<CommentResponseDTO>(createdComment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a comment to the post.");
            throw new Exception("Error adding comment to post", ex);
        }
    }

    public  async Task<CommentResponseDTO?> UpdateCommentAsync(string commentId, string userId, UpdateCommentDTO updateCommentDTO)
    {
        try
        {
            var existingComment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (existingComment == null) return null;

            existingComment.Content = updateCommentDTO.Content;
            existingComment.UpdatedAt = DateTime.UtcNow;

            var updatedComment = await _commentRepository.UpdateCommentAsync(commentId, existingComment);
            if (updatedComment == null) return null;

            return _mapper.Map<CommentResponseDTO>(updatedComment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the comment.");
            throw new Exception("Error updating comment", ex);
        }
    }

    public async Task<bool> DeleteCommentAsync(string commentId, string userId)
    {
        var isDeleted = await _commentRepository.DeleteCommentAsync(commentId);

        return isDeleted;
    }

    public async Task<IEnumerable<CommentResponseDTO>> GetPostCommentsAsync(string postId, int pageNumber = 1, int pageSize = 10)
    {
        var comments = await _commentRepository.GetCommentsByPostIdAsync(postId, pageNumber, pageSize);
        if(comments == null) return null;

        return _mapper.Map<IEnumerable<CommentResponseDTO>>(comments);
    }

    public async Task<CommentResponseDTO?> GetCommentByIdAsync(string commentId)
    {
        var comment = await _commentRepository.GetCommentByIdAsync(commentId);
        if (comment == null) return null;

        return _mapper.Map<CommentResponseDTO>(comment);
    }
}