
using AutoMapper;
using NanoidDotNet;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;
    private readonly IProfileRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<FollowService> _logger;
    private const string size = "0123456789";
    private const int length = 8;

    public FollowService(IFollowRepository followRepository, IMapper mapper, ILogger<FollowService> logger, IProfileRepository userRepository)
    {
        _userRepository = userRepository;
        _followRepository = followRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FollowResponseDTO> AddFollowAsync(FollowRequestDTO request, string followerUserId)
    {
        if (request == null)  throw new ArgumentNullException(nameof(request), "Follow cannot be null");

        _logger.LogInformation("AddFollowAsync::Adding follow relationship: {FollowerId} -> {FollowingId}", followerUserId, request.ToFollowUserId);
        
        var userExistByFollowingId = await _userRepository.ProfileExistsAsync(request.ToFollowUserId);
        if (!userExistByFollowingId)
        {
            _logger.LogWarning("AddFollowAsync::User does not exist: {UserId}", request.ToFollowUserId);
            throw new UserNotFoundException(request.ToFollowUserId);
        }

        var existingFollow = await _followRepository.GetFollowByFollowerAndFollowingIdAsync(followerUserId, request.ToFollowUserId);
        if (existingFollow != null)
        {
            return _mapper.Map<FollowResponseDTO>(existingFollow);
        }

        var existingFollowReverse = await _followRepository.GetFollowByFollowerAndFollowingIdAsync(request.ToFollowUserId, followerUserId);
        if (existingFollowReverse != null)
        {
            return _mapper.Map<FollowResponseDTO>(existingFollowReverse);
        }


        try
        {
            var follow = new Follow
            {
                Id = Nanoid.Generate(size, length),
                FollowingUserId = request.ToFollowUserId,
                FollowerUserId = followerUserId,
                FollowedAt = DateTime.UtcNow,
                IsBlocked = false,
                IsFollowing = true
            };


            await _followRepository.AddFollowAsync(follow);
            return _mapper.Map<FollowResponseDTO>(follow);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "AddFollowAsync::Error adding follow: {Message}", ex.Message);
            throw new Exception($"AddFollowAsync::Error adding follow: {ex.Message}");
        }
    }

    public Task<bool> BlockUserAsync(string userId, string blockedUserId)
    {
        return _followRepository.BlockUserAsync(userId, blockedUserId);
    }

    public Task<bool> FollowExistsAsync(string id)
    {
        return _followRepository.FollowExistsAsync(id);
    }

    public async Task<IEnumerable<FollowResponseDTO>> GetBlockedUsersAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        var blockedUsers = await _followRepository.GetBlockedUsersAsync(userId, pageNumber, pageSize);
        return _mapper.Map<IEnumerable<FollowResponseDTO>>(blockedUsers);
    }

    public async Task<FollowResponseDTO> GetFollowByFollowerAndFollowingIdAsync(string followerId, string followingId)
    {
        var follow = await _followRepository.GetFollowByFollowerAndFollowingIdAsync(followerId, followingId);
        return _mapper.Map<FollowResponseDTO>(follow);
    }

    public async Task<IEnumerable<FollowResponseDTO>> GetFollowersByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        var followers = await _followRepository.GetFollowersByUserIdAsync(userId, pageNumber, pageSize);
        return _mapper.Map<IEnumerable<FollowResponseDTO>>(followers);
    }

    public Task<int> GetFollowersCountAsync(string userId)
    {
        return _followRepository.GetFollowersCountAsync(userId);
    }

    public async Task<IEnumerable<FollowResponseDTO>> GetFollowingByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 10)
    {
        var following = await _followRepository.GetFollowingByUserIdAsync(userId, pageNumber, pageSize);
        return _mapper.Map<IEnumerable<FollowResponseDTO>>(following);
    }

    public Task<int> GetFollowingCountAsync(string userId)
    {
        return _followRepository.GetFollowingCountAsync(userId);
    }

    public async Task<IEnumerable<FollowResponseDTO>> GetMutualFollowersAsync(string userId1, string userId2, int pageNumber = 1, int pageSize = 10)
    {
        var follow = await _followRepository.GetMutualFollowersAsync(userId1, userId2, pageNumber, pageSize);
        return _mapper.Map<IEnumerable<FollowResponseDTO>>(follow);
    }

    public Task<bool> IsBlockedAsync(string userId, string blockedUserId)
    {
        return _followRepository.IsBlockedAsync(userId, blockedUserId);
    }

    public Task<bool> IsFollowingAsync(string followerId, string followingId)
    {
        return _followRepository.IsFollowingAsync(followerId, followingId);
    }

    public Task<bool> UnblockUserAsync(string userId, string blockedUserId)
    {
        return _followRepository.UnblockUserAsync(userId, blockedUserId);
    }

    public Task<bool> UnFollowAsync(string id)
    {
        return _followRepository.UnFollowAsync(id);
    }

    public async Task<FollowResponseDTO> UpdateFollowAsync(string id, FollowResponseDTO request)
    {
        try
        {
            if (request == null) throw new ArgumentNullException(nameof(request), "Follow cannot be null");

            var follow = new Follow
            {
                Id = id,
                FollowerUserId = request.FollowerUserId,
                FollowingUserId = request.FollowingUserId,
                FollowedAt = DateTime.UtcNow,
                IsBlocked = request.IsBlocked,
                UpdatedAt = DateTime.UtcNow
            };

            if(follow.FollowerUserId == follow.FollowingUserId)
            {
                throw new ArgumentException("Users cannot follow themselves", nameof(follow.FollowerUserId));
            }

            _logger.LogInformation("Updating follow relationship: {Id} - Follower: {FollowerId}, Following: {FollowingId}",id, follow.FollowerUserId, follow.FollowingUserId);

            var updateFollow = await _followRepository.UpdateFollowAsync(id, follow);
            if (updateFollow == null)
            {
                throw new Exception($"UpdateFollowAsync::Error updating follow with id: {id}");
            }

            _logger.LogInformation("Successfully updated follow with id: {Id}", id);
            return _mapper.Map<FollowResponseDTO>(updateFollow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateFollowAsync::Error updating follow: {Message}", ex.Message);
            throw new Exception($"UpdateFollowAsync::Error updating follow: {ex.Message}");
        }


    }
}

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string userId)
        : base($"User with ID '{userId}' does not exist.")
    {
    }
}