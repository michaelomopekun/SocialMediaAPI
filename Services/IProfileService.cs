public interface IProfileService
{
    Task<ProfileResponseDTO> GetProfileByIdAsync(string userId);
    Task<ProfileResponseDTO> GetProfileByUserNameAsync(string userName);
    Task<IEnumerable<ProfileResponseDTO>> GetAllProfilesAsync(int pageNumber = 1, int pageSize = 10);
    Task<ProfileResponseDTO> CreateProfileAsync(string userId, CreateProfileDTO createProfileDTO);
    Task<ProfileResponseDTO?> UpdateProfileAsync(string userId, UpdateProfileDTO updateProfileDTO);
    Task<bool> ProfileExistsAsync(string userId);
}