using MongoDB.Driver;


public class ProfileRepository : IProfileRepository
{
    private readonly IMongoCollection<ApplicationUser> _users;
    private readonly ILogger<ProfileRepository> _logger;


    public ProfileRepository(ILogger<ProfileRepository> logger, IMongoCollection<ApplicationUser> users)
    {
        _logger = logger;
        _users = users;
    }

    public async Task<ApplicationUser> CreateProfileAsync(string userId, ApplicationUser profile)
    {
        try
        {
            var userProfile = await _users.Find(i => i.Id.ToString() == userId).FirstOrDefaultAsync();
            if(userProfile == null) return null!;

            userProfile.Bio = profile.Bio;
            userProfile.DateOfBirth = profile.DateOfBirth;
            userProfile.PhoneNumber = profile.PhoneNumber;
            userProfile.ProfilePictureUrl = profile.ProfilePictureUrl;
            userProfile.UpdatedAt = profile.UpdatedAt;
            userProfile.Location = profile.Location;

            if(profile.Bio != null && profile.ProfilePictureUrl != null && profile.DateOfBirth != null && profile.Location != null && profile.PhoneNumber != null)
            {
                userProfile.ProfileCompleted = true;
            }

            await _users.UpdateOneAsync(u => u.Id.ToString() == userId, Builders<ApplicationUser>.Update
                .Set(u => u.Bio, userProfile.Bio)
                .Set(u => u.DateOfBirth, userProfile.DateOfBirth)
                .Set(u => u.PhoneNumber, userProfile.PhoneNumber)
                .Set(u => u.ProfilePictureUrl, userProfile.ProfilePictureUrl)
                .Set(u => u.UpdatedAt, userProfile.UpdatedAt)
                .Set(u => u.Location, userProfile.Location)
                .Set(u => u.ProfileCompleted, userProfile.ProfileCompleted));

            _logger.LogInformation("CreateProfileAsync::Profile created successfully: {UserName}", profile.UserName);

            return userProfile!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateProfileAsync::Error creating profile: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteProfileAsync(string id)
    {
        try
        {
            var userProfile = await _users.Find(u => u.Id.ToString() == id).FirstOrDefaultAsync();
            if (userProfile == null) return false;

            userProfile.ProfileIsDeleted = true;
            userProfile.UpdatedAt = DateTime.UtcNow;

            await _users.UpdateOneAsync(u => u.Id.ToString() == id, Builders<ApplicationUser>.Update
                .Set(u => u.UpdatedAt, userProfile.UpdatedAt)
                .Set(u => u.ProfileIsDeleted, userProfile.ProfileIsDeleted));

            _logger.LogInformation("DeleteProfileAsync::Profile deleted successfully: {Id}", id);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteProfileAsync::Error deleting profile: {Message}", ex.Message);
            throw;
        }   
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllProfilesAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var profiles = await _users
                .Find(_ => true && _.ProfileIsDeleted == false)
                .SortBy(u => u.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            _logger.LogInformation("GetAllProfilesAsync::Retrieved {Count} profiles", profiles.Count);

            return profiles!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllProfilesAsync::Error retrieving profiles: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<ApplicationUser?> GetProfileByIdAsync(string id)
    {
        try
        {
            var profile = await _users
                .Find(u => u.Id.ToString() == id && u.ProfileIsDeleted == false)
                .FirstOrDefaultAsync();

            if (profile == null || profile.Bio == null || profile.ProfilePictureUrl == null || profile.DateOfBirth == null || profile.Location == null || profile.PhoneNumber == null)
            {
                _logger.LogWarning("GetProfileByIdAsync::Profile not found or incomplete: {Id}", id);
            }

            _logger.LogInformation("GetProfileByIdAsync::Profile retrieved successfully: {Id}", id);

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProfileByIdAsync::Error retrieving profile by ID: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<ApplicationUser?> GetProfileByUserNameAsync(string userName)
    {
        try
        {
            userName = userName.Trim().ToLowerInvariant();

            var profile = await _users
                .Find(u => u.UserName == userName && u.ProfileIsDeleted == false)
                .FirstOrDefaultAsync();

            if (profile == null || profile.Bio == null || profile.ProfilePictureUrl == null || profile.DateOfBirth == null || profile.Location == null || profile.PhoneNumber == null)
            {
                _logger.LogWarning("GetProfileByIdAsync::Profile not found or incomplete: {UserName}", userName);
                return null!;
            }

            _logger.LogInformation("GetProfileByUserNameAsync::Profile retrieved successfully: {UserName}", userName);

            return profile!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetProfileByUserNameAsync::Error retrieving profile by UserName: {Message}", ex.Message);
            throw;
        }
    }

    public async Task<bool> ProfileExistsAsync(string userId)
    {
        try
        {
            var profileExists = await _users
                                    .Find(u => u.Id.ToString() == userId && u.ProfileIsDeleted == false && (u.ProfileCompleted == true || u.ProfileCompleted == false))
                                    .Project(u => new ApplicationUser
                                    {
                                        Id = u.Id,
                                        UserName = u.UserName,
                                        PhoneNumber = u.PhoneNumber,
                                        ProfilePictureUrl = u.ProfilePictureUrl,
                                        Bio = u.Bio,
                                        DateOfBirth = u.DateOfBirth,
                                        Location = u.Location
                                    })
                                    .AnyAsync();

            if (!profileExists)
            {
                _logger.LogWarning("ProfileExistsAsync::Profile not found or incomplete: {UserId}", userId);
                return false;
            }

            _logger.LogInformation("ProfileExistsAsync::Profile exists: {UserId}", userId);

            return profileExists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ProfileExistsAsync::Error checking if profile exists: {Message}", ex.Message);
            throw;
        }
    }


    public async Task<ApplicationUser> UpdateProfileAsync(string id, ApplicationUser profile)
    {
        try
        {
            var existingProfile = await _users.Find(i => i.Id.ToString() == id).FirstOrDefaultAsync();
            if (existingProfile == null) return null!;

            existingProfile.Bio = profile.Bio;
            existingProfile.DateOfBirth = profile.DateOfBirth;
            existingProfile.Location = profile.Location;
            existingProfile.PhoneNumber = profile.PhoneNumber;
            existingProfile.ProfilePictureUrl = profile.ProfilePictureUrl;
            existingProfile.UpdatedAt = DateTime.UtcNow;
            existingProfile.ProfileCompleted = false;

            if(profile.Bio != null && profile.ProfilePictureUrl != null && profile.DateOfBirth != null && profile.Location != null && profile.PhoneNumber != null)
            {
                existingProfile.ProfileCompleted = true;
            }

            await _users.UpdateOneAsync(u => u.Id.ToString() == id, Builders<ApplicationUser>.Update
                .Set(u => u.Bio, existingProfile.Bio)
                .Set(u => u.DateOfBirth, existingProfile.DateOfBirth)
                .Set(u => u.PhoneNumber, existingProfile.PhoneNumber)
                .Set(u => u.ProfilePictureUrl, existingProfile.ProfilePictureUrl)
                .Set(u => u.UpdatedAt, existingProfile.UpdatedAt)
                .Set(u => u.Location, existingProfile.Location)
                .Set(u => u.ProfileCompleted, existingProfile.ProfileCompleted));


            _logger.LogInformation("UpdateProfileAsync::Profile updated successfully: {Id}", id);

            return existingProfile!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateProfileAsync::Error updating profile: {Message}", ex.Message);
            throw;
        }
    }
}