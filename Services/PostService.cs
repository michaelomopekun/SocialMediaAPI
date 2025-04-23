using System.Drawing;
using AutoMapper;
using NanoidDotNet;
using SocialMediaAPI.Models.DTOs;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PostService> _logger;
    private const string size = "0123456789";

    public PostService(IPostRepository postRepository, IMapper mapper, ILogger<PostService> logger)
    {
        _postRepository = postRepository;
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
            Id = Nanoid.Generate(size, 8),
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
}