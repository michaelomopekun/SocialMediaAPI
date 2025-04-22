using AutoMapper;
using NanoidDotNet;
using SocialMediaAPI.Models.DTOs;

public class PostService : IPostService
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PostService> _logger;

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
            Id = int.Parse(Nanoid.Generate(size: 7)),
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

    public async Task<bool> DeletePostAsync(int id, string userId)
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

    public async Task<IEnumerable<PostResponseDTO>> GetFlowersPostsAsync(string userId)
    {
        var posts = await _postRepository.GetFollowersPostsAsync(userId, 1, 10);       
        return _mapper.Map<IEnumerable<PostResponseDTO>>(posts);
    }

    public async Task<PostResponseDTO?> GetPostByIdAsync(int id)
    {
        var post = await _postRepository.GetPostByIdAsync(id);
        return _mapper.Map<PostResponseDTO>(post);        
    }

    public async Task<IEnumerable<PostResponseDTO>> GetPostsByUserIdAsync(string userId)
    {
        var posts = await _postRepository.GetPostsByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<PostResponseDTO>>(posts);
    }

    public async Task<PostResponseDTO?> UpdatePostAsync(int id, UpdatePostDTO updatePostDTO, string userId)
    {
        var post = new Post
        {
            Id = id,
            Content = updatePostDTO.Content,
            ImageUrl = updatePostDTO.ImageUrl,
            UserId = userId.ToString(),
            UpdatedAt = DateTime.UtcNow
        };

        var updatedPost = await _postRepository.UpdatePostAsync(id, post);
        if (updatedPost == null)
        {
            _logger.LogError("UpdatePostAsync::Error updating post: {Message}", "Post update failed");
            throw new Exception("UpdatePostAsync::Error updating post: Post update failed");
        }

        return _mapper.Map<PostResponseDTO>(updatedPost);
    }
}