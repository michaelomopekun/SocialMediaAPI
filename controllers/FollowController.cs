using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/follow")]
[Produces("application/json")]
[Consumes("application/json")]
public class FollowController : ControllerBase
{
    private readonly IFollowService _followService;
    private readonly ILogger<FollowController> _logger;

    public FollowController(IFollowService followService, ILogger<FollowController> logger)
    {
        _followService = followService;
        _logger = logger;
    }


    [HttpGet("followers/{userId}")]
    public async Task<IActionResult> GetFollowers(string userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var followers = await _followService.GetFollowersByUserIdAsync(userId, pageNumber, pageSize);
            return Ok(new
            {
                Message = "Followers retrieved successfully",
                Followers = followers,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFollowers::Error getting followers: {Message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpGet("following/{userId}")]
    public async Task<IActionResult> GetFollowing(string userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var following = await _followService.GetFollowingByUserIdAsync(userId, pageNumber, pageSize);
            return Ok(new
            {
                Message = "Following retrieved successfully",
                Following = following,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFollowing::Error getting following: {Message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpPost("follow")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Follow([FromBody] FollowRequestDTO request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("request data cannot be null or empty.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated.");
            }

            if(request.ToFollowUserId == currentUserId)
            {
                return BadRequest("Users cannot follow themselves.");
            }

            var follow = await _followService.AddFollowAsync(request, currentUserId);
            if (follow == null)
            {
                return BadRequest("Failed to follow user. Please try again.");
            }

            return CreatedAtAction(nameof(GetFollowers), new { userId = request.ToFollowUserId },
                new { Status = "Success", Message = "Successfully followed user", Data = follow });
        }
        catch (UserNotFoundException)
        {
            return NotFound("Cant follow a user that does not exist.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Follow::Error following user: {Message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("unfollow/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Unfollow(string id)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated.");
            }

            if(currentUserId == id)
            {
                return BadRequest("Users cannot unfollow or follow themselves.");
            }

            var unfollowed = await _followService.UnFollowAsync(id);
            if (!unfollowed)
            {
                return NotFound("Follow not found or already unfollowed.");
            }

            return Ok(new { Status = "Success", Message = "Successfully unfollowed user" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unfollow::Error unfollowing user: {Message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("isBlocked/{userId}/{blockedUserId}")]
    public async Task<IActionResult> IsBlocked(string userId, string blockedUserId)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated.");
            }

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(blockedUserId))
            {
                return BadRequest("User IDs cannot be null or empty.");
            }

            var isBlocked = await _followService.IsBlockedAsync(userId, blockedUserId);
            return Ok(new { IsBlocked = isBlocked });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IsBlocked::Error checking if user is blocked: {Message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("mutualFollowers/{userId1}/{userId2}")]
    public async Task<IActionResult> GetMutualFollowers(string userId1, string userId2, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrEmpty(userId1) || string.IsNullOrEmpty(userId2))
            {
                return BadRequest("User IDs cannot be null or empty.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated.");
            }

            var mutualFollowers = await _followService.GetMutualFollowersAsync(userId1, userId2, pageNumber, pageSize);
            return Ok(new
            {
                Message = "Mutual followers retrieved successfully",
                MutualFollowers = mutualFollowers,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetMutualFollowers::Error getting mutual followers: {Message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }

    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("blockedUsers/{userId}")]
    public async Task<IActionResult> GetBlockedUsers(string userId, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated.");
            }

            if(currentUserId != userId)
            {
                return Forbid("You are not authorized to view this user's blocked users.");
            }

            var blockedUsers = await _followService.GetBlockedUsersAsync(userId, pageNumber, pageSize);
            return Ok(new
            {
                Message = "Blocked users retrieved successfully",
                BlockedUsers = blockedUsers,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetBlockedUsers::Error getting blocked users: {Message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpGet("followersCount/{userId}")]
    public async Task<IActionResult> GetFollowersCount(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated.");
            }

            var count = await _followService.GetFollowersCountAsync(userId);
            return Ok(new { FollowersCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFollowersCount::Error getting followers count: {Message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpGet("followingCount/{userId}")]
    public async Task<IActionResult> GetFollowingCount(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID cannot be null or empty.");
            }
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated.");
            }

            var count = await _followService.GetFollowingCountAsync(userId);
            return Ok(new { FollowingCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetFollowingCount::Error getting following count: {Message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("isFollowing/{followingId}/{followerId}")]
    public async Task<IActionResult> IsFollowing(string followingId, string followerId)
    {
        try
        {
            if (string.IsNullOrEmpty(followingId) || string.IsNullOrEmpty(followerId))
            {
                return BadRequest("User IDs cannot be null or empty.");
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User not authenticated.");
            }

            var isFollowing = await _followService.IsFollowingAsync(followerId, followingId);
            return Ok(new { IsFollowing = isFollowing });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IsFollowing::Error checking if user is following: {Message}", ex.Message);
            return StatusCode(500, "Internal server error");
        }
    }


[HttpPost("block/{userToBlockId}")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> BlockUser(string userToBlockId)
{
    try
    {
        if (string.IsNullOrEmpty(userToBlockId))
        {
            return BadRequest("User ID to block cannot be null or empty.");
        }

        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized("User not authenticated.");
        }

        if (currentUserId == userToBlockId)
        {
            return BadRequest("Users cannot block themselves.");
        }

        var blocked = await _followService.BlockUserAsync(currentUserId, userToBlockId);
        if (!blocked)
        {
            return NotFound("User relationship not found or already blocked.");
        }

        _logger.LogInformation("User {CurrentUserId} blocked user {BlockedUserId}", 
            currentUserId, userToBlockId);

        return Ok(new 
        { 
            Status = "Success", 
            Message = "User blocked successfully" 
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "BlockUser::Error blocking user: {Message}", ex.Message);
        return StatusCode(500, "Internal server error");
    }
}

}