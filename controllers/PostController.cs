using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAPI.Models.DTOs;
using SocialMediaAPI.Services;

[Authorize]
[ApiController]
[Route("api/posts")]
[Produces("application/json")]
[Consumes("application/json")]
public class PostController : ControllerBase
{

    private readonly IPostService _postService;
    private readonly ILogger<PostController> _logger;
    private readonly ILikeService _likeService;

    public PostController(IPostService postService, ILogger<PostController> logger, ILikeService likeService)
    {
        _postService = postService;
        _logger = logger;
        _likeService = likeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PostResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<PostResponseDTO>>> GetAllPosts([FromQuery] int pageNumber=1, [FromQuery] int pageSize=10)
    {
        try
        {
            var posts = await _postService.GetAllPostsAsync(pageNumber, pageSize);
            if (posts == null || !posts.Any())
            {
                return NotFound(new { Status = "Error", Message = "No posts found" });
            }

            return Ok(new { Status = "Success", Data = posts });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving posts.");
            return StatusCode(500, new { Status = "Error", Message = "Error retrieving posts" });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PostResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PostResponseDTO>> GetPost(string id)
    {
        try
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound(new { Status = "Error", Message = "Post not found" });
            }

            return Ok(new { Status = "Success", Data = post });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the post.");
            return StatusCode(500, new { Status = "Error", Message = "Error retrieving post" });
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(PostResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PostResponseDTO>> CreatePost([FromBody] CreatePostDTO createPostDTO)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Status = "Error", Message = "User not authorized" });
            }

            var post = await _postService.CreatePostAsync(createPostDTO, userId);
            if (post == null)
            {
                return BadRequest(new { Status = "Error", Message = "Error creating post" });
            }

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, new { Status = "Success", Data = post });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the post.");
            return StatusCode(500, new { Status = "Error", Message = "Error creating post" });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PostResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PostResponseDTO>> UpdatePost(string id, [FromBody] UpdatePostDTO updatePostDTO)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Status = "Error", Message = "User not authorized" });
            }

            var updatePost = await _postService.UpdatePostAsync(id, updatePostDTO, userId);
            if (updatePost == null)
            {
                return NotFound(new { Status = "Error", Message = "Post not found" });
            }

            return Ok(new { Status = "Success", Data = updatePost });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden(new { Status = "Error", Message = "You are not authorized to update this post" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the post.");
            return StatusCode(500, new { Status = "Error", Message = "Error updating post" });
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeletePost(string id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Status = "Error", Message = "User not authorized" });
            }

            var result = await _postService.DeletePostAsync(id, userId);
            if (!result)
            {
                return NotFound(new { Status = "Error", Message = "Post not found" });
            }

            return Ok( new { Status = "Success", Message = "Post deleted successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden(new { Status = "Error", Message = "You are not authorized to delete this post" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the post.");
            return StatusCode(500, new { Status = "Error", Message = "Error deleting post" });
        }
    }

    [HttpGet("user/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<PostResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<PostResponseDTO>>> GetPostsByUser(string userId
    // , [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10
    )
    {
        try
        {
            var posts = await _postService.GetPostsByUserIdAsync(userId);
            if (posts == null || !posts.Any())
            {
                return NotFound(new { Status = "Error", Message = "No posts found for this user" });
            }

            return Ok(new { Status = "Success", Data = posts });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving posts by user.");
            return StatusCode(500, new { Status = "Error", Message = "Error retrieving posts by user" });
        }
    }

    [HttpGet("feed")]
    [ProducesResponseType(typeof(IEnumerable<PostResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<PostResponseDTO>>> GetFeed([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Status = "Error", Message = "User not authorized" });
            }

            var posts = await _postService.GetFeedAsync(userId, pageNumber, pageSize);
            if (posts == null || !posts.Any())
            {
                return NotFound(new { Status = "Error", Message = "No posts found in feed" });
            }

            return Ok(new { 
                Status = "Success", 
                Data = posts,
                Pagination = new {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    HasMore = posts.Count() >= pageSize
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user's feed.");
            return StatusCode(500, new { Status = "Error", Message = "Error retrieving feed" });
        }
    }


    [HttpPost("{postId}/comments")]
    [ProducesResponseType(typeof(CreateCommentDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CreateCommentDTO>> AddCommentToPost(string postId, [FromBody] CreateCommentDTO createCommentDTO)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Status = "Error", Message = "User not authorized" });
            }

            var comment = await _postService.AddCommentToPostAsync(postId, userId, createCommentDTO);
            if (comment == null)
            {
                return NotFound(new { Status = "Error", Message = "Post not found" });
            }

            return CreatedAtAction(nameof(GetPost), new { id = postId }, new { Status = "Success", Data = comment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a comment to the post.");
            return StatusCode(500, new { Status = "Error", Message = "Error adding comment" });
        }
    }


    [HttpGet("{postId}/comments")]
    [ProducesResponseType(typeof(IEnumerable<CommentResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<CommentResponseDTO>>> GetPostComments(string postId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if(string.IsNullOrEmpty(postId))
            {
                return BadRequest(new { Status = "Error", Message = "Post ID is required" });
            }

            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest(new { Status = "Error", Message = "Page number and size must be greater than zero" });
            }

            var comments = await _postService.GetPostCommentsAsync(postId, pageNumber, pageSize);
            if (comments == null || !comments.Any())
            {
                return NotFound(new { Status = "Error", Message = "No comments found for this post" });
            }

            return Ok(new { Status = "Success", Data = comments });
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving comments for the post.");
            return StatusCode(500, new { Status = "Error", Message = "Error retrieving comments" });
        }
    }


    [HttpGet("{postId}/comments/{commentId}")]
    [ProducesResponseType(typeof(CommentResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CommentResponseDTO>> GetComment(string postId, string commentId)
    {
        try
        {
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(commentId))
            {
                return BadRequest(new { Status = "Error", Message = "Post ID and Comment ID are required" });
            }

            var comment = await _postService.GetCommentByIdAsync(commentId);
            if (comment == null)
            {
                return NotFound(new { Status = "Error", Message = "Comment not found" });
            }
            return Ok(new { Status = "Success", Data = comment });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the comment.");
            return StatusCode(500, new { Status = "Error", Message = "Error retrieving comment" });
        }
    }


    [HttpDelete("{postId}/comments/{commentId}")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteComment(string postId, string commentId)
    {
        try
        {
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(commentId))
            {
                return BadRequest(new { Status = "Error", Message = "Post ID and Comment ID are required" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Status = "Error", Message = "User not authorized" });
            }

            var result = await _postService.DeleteCommentAsync(commentId, userId);
            if (!result)
            {
                return NotFound(new { Status = "Error", Message = "Comment not found" });
            }

            return Ok(new { Status = "Success", Message = "Comment deleted successfully" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden(new { Status = "Error", Message = "You are not authorized to delete this comment" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the comment.");
            return StatusCode(500, new { Status = "Error", Message = "Error deleting comment" });
        }
    }

    [HttpPut("{postId}/comments/{commentId}")]
    [ProducesResponseType(typeof(CommentResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CommentResponseDTO>> UpdateComment(string postId, string commentId, [FromBody] UpdateCommentDTO updateCommentDTO)
    {
        try
        {
            if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(commentId)) return BadRequest(new { Status = "Error", Message = "Post ID and Comment ID are required" });
            
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { Status = "Error", Message = "User not authorized" });

            var updatedComment = await _postService.UpdateCommentAsync(commentId, userId, updateCommentDTO);
            if (updatedComment == null) return NotFound(new { Status = "Error", Message = "Comment not found" });

            return Ok(new { Status = "Success", Data = updatedComment });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbidden(new { Status = "Error", Message = "You are not authorized to update this comment" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the comment.");
            return StatusCode(500, new { Status = "Error", Message = "Error updating comment" });
        }
    }
    

    [HttpPost("{postId}/like")]
    [ProducesResponseType(typeof(LikeResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LikeResponseDTO>> ToggleLike(string postId, [FromBody] ToggleLikeDTO request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Status = "Error", Message = "User not authorized" });
            }

            request.PostId = postId;
            
            var response = await _likeService.ToggleLikeAsync(
                userId, 
                request.PostId, 
                request.CommentId, 
                request.ReactionType);

            return Ok(new { Status = "Success", Data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling like for post {PostId}", postId);
            return StatusCode(500, new { Status = "Error", Message = "Error toggling like" });
        }
    }

    [HttpGet("{postId}/likes")]
    [ProducesResponseType(typeof(LikeStatusDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LikeStatusDTO>> GetLikeStatus(string postId, [FromQuery] string? commentId = null)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Status = "Error", Message = "User not authorized" });
            }

            var status = await _likeService.GetLikeStatusAsync(postId, userId, commentId);
            return Ok(new { Status = "Success", Data = status });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting like status for post {PostId}", postId);
            return StatusCode(500, new { Status = "Error", Message = "Error getting like status" });
        }
    }


    private ObjectResult Forbidden(object value)
    {
        return StatusCode(StatusCodes.Status403Forbidden, value);
    }
}