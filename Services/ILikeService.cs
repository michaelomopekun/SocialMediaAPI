using SocialMediaAPI.Models.Domain;
using SocialMediaAPI.Models.DTOs;

namespace SocialMediaAPI.Services;

public interface ILikeService
{
    Task<LikeResponseDTO> ToggleLikeAsync(string userId, string postId, string? commentId = null, ReactionType reactionType = ReactionType.Like);
    Task<bool> RemoveLikeAsync(string userId, string postId, string? commentId = null);
    Task<LikeStatusDTO> GetLikeStatusAsync(string postId, string userId, string? commentId = null);
}