using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ProfileController> _logger;
    private string? GetCurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    private bool IsUserAuthorized(string userId) => GetCurrentUserId() == userId;

    public ProfileController(IProfileService profileService, ICacheService cacheService, ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _cacheService = cacheService;
        _logger = logger;
    }


    [HttpPost("create")]
    [ProducesResponseType(typeof(ProfileResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDTO createProfileDTO)
    {
        try
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId)) return BadRequest(new { status = "Error", Message = "User ID is required." });

            if(!IsUserAuthorized(userId)) return Unauthorized(new { status = "Error", Message = "User ID does not match the authorized user." });

            var profileResponse = await _profileService.CreateProfileAsync(userId, createProfileDTO);

            return Ok(new { status = "Success", Message = "Profile created successfully.", Data = profileResponse });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateProfile::Error creating profile: {Message}", ex.Message);

            return StatusCode(500, new { status = "Error", Message = "An error occurred while creating the profile." });
        }
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(ProfileResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfile(string userId)
    {
        try
        {
            var profile = await _profileService.GetProfileByIdAsync(userId);

            if (profile == null) return NotFound(new { status = "Error", Message = "Profile not found." });

            return Ok(new { status = "Success", Data = profile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProfile::Error retrieving profile: {Message}", ex.Message);

            return StatusCode(500, new { status = "Error", Message = "An error occurred while retrieving the profile." });
        }
    }

    [HttpPut("{userId}")]
    [ProducesResponseType(typeof(ProfileResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProfile(string userId, [FromBody] UpdateProfileDTO updateProfileDTO)
    {
        try
        {
            var claimUserId = GetCurrentUserId();

            if (string.IsNullOrEmpty(claimUserId)) return BadRequest(new { status = "Error", Message = "User ID is required." });

            if(!IsUserAuthorized(claimUserId)) return Unauthorized(new { status = "Error", Message = "User ID does not match the authorized user." });

            var profileResponse = await _profileService.UpdateProfileAsync(userId, updateProfileDTO);

            if (profileResponse == null) return NotFound(new { status = "Error", Message = "Profile not found." });

            return Ok(new { status = "Success", Message = "Profile updated successfully.", Data = profileResponse });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateProfile::Error updating profile: {Message}", ex.Message);

            return StatusCode(500, new { status = "Error", Message = "An error occurred while updating the profile." });
        }
    }

    [HttpGet("exists/{userId}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProfileExists(string userId)
    {
        try
        {
            var exists = await _profileService.ProfileExistsAsync(userId);

            if (!exists) return NotFound(new { status = "False", Message = "Profile does not exist." });

            return Ok(new { status = "True", Data = exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProfileExists::Error checking profile existence: {Message}", ex.Message);

            return StatusCode(500, new { status = "Error", Message = "An error occurred while checking profile existence." });
        }
    }

    [HttpGet("username/{userName}")]
    [ProducesResponseType(typeof(ProfileResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfileByUserName(string userName)
    {
        try
        {
            var profile = await _profileService.GetProfileByUserNameAsync(userName);
            
            if (profile == null) return NotFound(new { status = "Error", Message = "Profile not found." });
            
            return Ok(new { status = "Success", Data = profile });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProfileByUserName::Error retrieving profile: {Message}", ex.Message);

            return StatusCode(500, new { status = "Error", Message = "An error occurred while retrieving the profile." });
        }
    }
}