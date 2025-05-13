public interface IProfileRepository
{
    Task<ApplicationUser?> GetProfileByIdAsync(string id);
    Task<ApplicationUser?> GetProfileByUserNameAsync(string userName);
    Task<IEnumerable<ApplicationUser>> GetAllProfilesAsync(int pageNumber = 1, int pageSize = 10);
    Task<ApplicationUser> CreateProfileAsync(string userId, ApplicationUser profile);
    Task<ApplicationUser> UpdateProfileAsync(string id, ApplicationUser profile);
    Task<bool> DeleteProfileAsync(string id);
    Task<bool> ProfileExistsAsync(string id);
}