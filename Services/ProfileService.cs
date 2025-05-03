
using AutoMapper;
using SocialMediaAPI.Constants;
using SocialMediaAPI.Models.Domain.User;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _profileRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProfileService> _logger;
    private readonly ICacheService _cache;


    public ProfileService(IProfileRepository profileRepository, IMapper mapper, ILogger<ProfileService> logger, ICacheService cache)
    {
        _profileRepository = profileRepository;
        _mapper = mapper;
        _logger = logger;
        _cache = cache;
    }

    public async Task<ProfileResponseDTO> CreateProfileAsync(string userId, CreateProfileDTO createProfileDTO)
    {
        _logger.LogInformation("CreateProfileAsync::Creating profile for user: {UserId}", userId);
        try
        {
            if(createProfileDTO == null) throw new ArgumentNullException(nameof(createProfileDTO), "Profile cannot be null");

            var existingProfile = await _profileRepository.GetProfileByIdAsync(userId);
            if(existingProfile != null)
            {
                return _mapper.Map<ProfileResponseDTO>(existingProfile);
            }

            var profile = new ApplicationUser
            {
                Bio = createProfileDTO.Bio,
                ProfilePictureUrl = createProfileDTO.ProfilePictureUrl,
                UpdatedAt = DateTime.UtcNow,
                Location = createProfileDTO.Address,
                DateOfBirth = createProfileDTO.DateOfBirth,
                PhoneNumber = createProfileDTO.PhoneNumber
            };

            var createdProfile = await _profileRepository.CreateProfileAsync(profile) ?? throw new Exception("Error creating profile");

            var profileResponse = _mapper.Map<ProfileResponseDTO>(createdProfile);

            await _cache.SetAsync(CacheKeys.ProfileById(userId), profileResponse, TimeSpan.FromMinutes(30));
            await _cache.SetAsync(CacheKeys.ProfileByUserName(createdProfile.UserName), profileResponse, TimeSpan.FromMinutes(30));

            _logger.LogInformation("CreateProfileAsync::Profile created successfully: {UserName}", createdProfile.UserName);

            return profileResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateProfileAsync::Error creating profile: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<IEnumerable<ProfileResponseDTO>> GetAllProfilesAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            if (pageNumber < 1 || pageSize < 1) throw new ArgumentException("Page number and page size must be greater than 0", nameof(pageNumber));

            var profiles = await _profileRepository.GetAllProfilesAsync(pageNumber, pageSize);
            
            return _mapper.Map<IEnumerable<ProfileResponseDTO>>(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllProfilesAsync::Error getting all profiles: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<ProfileResponseDTO> GetProfileByIdAsync(string id)
    {
        try
        {
            var cacheKey = CacheKeys.ProfileById(id);

            var cached = await _cache.GetAsync<ProfileResponseDTO>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("GetProfileByIdAsync::Profile retrieved from cache: {Id}", id);
                return cached;
            }

            var profile = await _profileRepository.GetProfileByIdAsync(id) ?? throw new Exception($"Profile with id {id} not found");

            var profileResponse = _mapper.Map<ProfileResponseDTO>(profile);

            await _cache.SetAsync(cacheKey, profileResponse, TimeSpan.FromMinutes(30));
            
            _logger.LogInformation("GetProfileByIdAsync::Profile cached successfully: {Id}", id);

            _logger.LogInformation("GetProfileByIdAsync::Profile retrieved successfully: {Id}", id);

            return profileResponse;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProfileByIdAsync::Error getting profile by id: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<ProfileResponseDTO> GetProfileByUserNameAsync(string userName)
    {
        try
        {
            userName = userName.Trim().ToLowerInvariant();

            var cacheKey = CacheKeys.ProfileByUserName(userName);

            var cached = await _cache.GetAsync<ProfileResponseDTO>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("GetProfileByUserNameAsync::Profile retrieved from cache: {UserName}", userName);
                return cached;
            }

            var profile = await _profileRepository.GetProfileByUserNameAsync(userName) ?? throw new Exception($"Profile with UserName {userName} not found");;

            var profileResponse = _mapper.Map<ProfileResponseDTO>(profile);

            await _cache.SetAsync(cacheKey, profileResponse, TimeSpan.FromMinutes(30));

            _logger.LogInformation("GetProfileByUserNameAsync::Profile cached successfully: {UserName}", userName);

            _logger.LogInformation("GetProfileByUserNameAsync::Profile retrieved successfully: {UserName}", userName);

            return profileResponse;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProfileByUserNameAsync::Error getting profile by userName: {Message}", ex.Message);
            throw;
        }
    }

    public Task<bool> ProfileExistsAsync(string userId)
    {
        return _profileRepository.ProfileExistsAsync(userId);
    }

    public async Task<ProfileResponseDTO?> UpdateProfileAsync(string userId, UpdateProfileDTO updateProfileDTO)
    {
        if (updateProfileDTO == null) throw new ArgumentNullException(nameof(updateProfileDTO), "Profile cannot be null");

        var existingProfile = await _profileRepository.GetProfileByIdAsync(userId);
        if (existingProfile == null)
        {
            throw new Exception($"Profile with id {userId} not found");
        }

        existingProfile.Bio = updateProfileDTO.Bio;
        existingProfile.ProfilePictureUrl = updateProfileDTO.ProfilePictureUrl;
        existingProfile.UpdatedAt = DateTime.UtcNow;
        existingProfile.Location = updateProfileDTO.Address;
        existingProfile.DateOfBirth = updateProfileDTO.DateOfBirth ?? DateTime.UtcNow;
        existingProfile.PhoneNumber = updateProfileDTO.PhoneNumber;
        existingProfile.FirstName = updateProfileDTO.FirstName;
        existingProfile.LastName = updateProfileDTO.LastName;
        existingProfile.UserName = updateProfileDTO.UserName;
        existingProfile.Email = updateProfileDTO.Email;


        await _profileRepository.UpdateProfileAsync(userId, existingProfile);
        
        var response = _mapper.Map<ProfileResponseDTO>(existingProfile);

        await _cache.RemoveAsync(CacheKeys.ProfileById(userId));

        if (!string.IsNullOrEmpty(existingProfile.UserName))
        {
            await _cache.RemoveAsync(CacheKeys.ProfileByUserName(existingProfile.UserName));
            _logger.LogInformation("UpdateProfileAsync::Cache removed for UserId: {UserId} and UserName: {UserName}", userId, existingProfile.UserName);

        }

        _logger.LogInformation("UpdateProfileAsync::Profile updated successfully: {UserName}", existingProfile.UserName);

        return response;
    }

}