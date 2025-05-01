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
        if (string.IsNullOrEmpty(createPostDTO.Content))
        {
            throw new ArgumentException("Post content cannot be empty");
        }

        var post = new Post
        {
            Id = Nanoid.Generate(size, idLength),
            Content = createPostDTO.Content,
            ImageUrl = createPostDTO.ImageUrl,
            UserId = userId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        var createdPost = await _postRepository.CreatePostAsync(post);
        if (createdPost == null)
        {
            _logger.LogError("CreatePostAsync::Error creating post: {Message}", "Post creation failed");
            throw new Exception("CreatePostAsync::Error creating post: Post creation failed");
        }        

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
        if (!isDeleted)
        {
            _logger.LogError("DeletePostAsync::Error deleting post: {Message}", "Post deletion failed");
            throw new Exception("DeletePostAsync::Error deleting post: Post deletion failed");
        }

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
        if (existingPost == null)
        {
            _logger.LogError("UpdatePostAsync::Error updating post: {Message}", "Post not found");
            throw new Exception("UpdatePostAsync::Error updating post: Post not found");
        }

        if (existingPost.UserId != userId)
        {
            _logger.LogError("UpdatePostAsync::Error updating post: {Message}", "You cannot update this post");
            throw new UnauthorizedAccessException("You cannot update this post");
        }

        if (string.IsNullOrEmpty(updatePostDTO.Content))
        {
            throw new ArgumentException("Post content cannot be empty");
        }

   
        existingPost.Content = updatePostDTO.Content;
        existingPost.ImageUrl = updatePostDTO.ImageUrl;
        // existingPost.UserId = userId.ToString();
        existingPost.UpdatedAt = DateTime.UtcNow;

        var updatedPost = await _postRepository.UpdatePostAsync(id, existingPost);
        if (updatedPost == null)
        {
            _logger.LogError("UpdatePostAsync::Error updating post: {Message}", "Post update failed");
            throw new Exception("UpdatePostAsync::Error updating post: Post update failed");
        }

        return _mapper.Map<PostResponseDTO>(updatedPost);
    }

    public async Task<CommentResponseDTO> AddCommentToPostAsync(string postId, string userId, CreateCommentDTO createCommentDTO)
    {
        try
        {
            if (string.IsNullOrEmpty(postId))
            {
                throw new ArgumentException("Post ID cannot be empty");
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be empty");
            }
            
            if (string.IsNullOrEmpty(createCommentDTO.Content))
            {
                throw new ArgumentException("Comment content cannot be empty");
            }

            var comment = new Comment
            {
                Id = Nanoid.Generate(size, 8),
                Content = createCommentDTO.Content,
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _commentRepository.AddCommentToPostAsync(postId, comment);
            if (createdComment == null)
            {
                _logger.LogError("AddCommentToPostAsync::Error adding comment: {Message}", "Comment addition failed");
                throw new Exception("AddCommentToPostAsync::Error adding comment: Comment addition failed");
            }

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
        if (string.IsNullOrEmpty(commentId))
        {
            throw new ArgumentException("Comment ID cannot be empty");
        }

        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be empty");
        }

        if (string.IsNullOrEmpty(updateCommentDTO.Content))
        {
            throw new ArgumentException("Comment content cannot be empty");
        }

        try
        {
            var existingComment = await _commentRepository.GetCommentByIdAsync(commentId);
            if (existingComment == null)
            {
                _logger.LogError("UpdateCommentAsync::Error updating comment: {Message}", "Comment not found");
                throw new Exception("UpdateCommentAsync::Error updating comment: Comment not found");
            }

            if (existingComment.UserId != userId)
            {
                _logger.LogError("UpdateCommentAsync::Error updating comment: {Message}", "You cannot update this comment");
                throw new UnauthorizedAccessException("You cannot update this comment");
            }

            existingComment.Content = updateCommentDTO.Content;
            existingComment.UpdatedAt = DateTime.UtcNow;

            var updatedComment = await _commentRepository.UpdateCommentAsync(commentId, existingComment);
            if (updatedComment == null)
            {
                _logger.LogError("UpdateCommentAsync::Error updating comment: {Message}", "Comment update failed");
                throw new Exception("UpdateCommentAsync::Error updating comment: Comment update failed");
            }

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
        if (string.IsNullOrEmpty(commentId))
        {
            throw new ArgumentException("Comment ID cannot be empty");
        }

        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("User ID cannot be empty");
        }

        var isDeleted = await _commentRepository.DeleteCommentAsync(commentId);

        return isDeleted;
    }

    public async Task<IEnumerable<CommentResponseDTO>> GetPostCommentsAsync(string postId, int pageNumber = 1, int pageSize = 10)
    {
        if(string.IsNullOrEmpty(postId))
        {
            throw new ArgumentException("Post ID cannot be empty");
        }

        if(pageNumber < 1)
        {
            throw new ArgumentException("Page number must be greater than 0");
        }

        var comments = await _commentRepository.GetCommentsByPostIdAsync(postId, pageNumber, pageSize);
        if(comments == null)
        {
            _logger.LogError("GetPostCommentsAsync::Error getting comments: {Message}", "No comments found for this post");
            throw new Exception("GetPostCommentsAsync::Error getting comments: No comments found for this post");
        }

        return _mapper.Map<IEnumerable<CommentResponseDTO>>(comments);
    }

    public async Task<CommentResponseDTO?> GetCommentByIdAsync(string commentId)
    {
        if(string.IsNullOrEmpty(commentId))
        {
            throw new ArgumentException("Comment ID cannot be empty");
        }

        var comment = await _commentRepository.GetCommentByIdAsync(commentId);
        if (comment == null)
        {
            _logger.LogError("GetCommentByIdAsync::Error getting comment: {Message}", "Comment not found");
            throw new Exception("GetCommentByIdAsync::Error getting comment: Comment not found");
        }

        return _mapper.Map<CommentResponseDTO>(comment);
    }
}