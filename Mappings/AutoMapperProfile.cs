using AutoMapper;
using SocialMediaAPI.Models.DTOs;

namespace SocialMediaAPI.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Post, PostResponseDTO>()
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.LikesCount, opt => opt.MapFrom(src => src.LikesCount ?? 0))
                .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.CommentsCount ?? 0));

            CreateMap<CreatePostDTO, Post>()
                .ForMember(dest => dest.CreatedAt, 
                    opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdatePostDTO, Post>()
                .ForMember(dest => dest.UpdatedAt, 
                    opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Comment, CommentResponseDTO>()
                .ForMember(dest => dest.UserName, opt => opt.Ignore());;

            CreateMap<CreateCommentDTO, Comment>();
            CreateMap<UpdateCommentDTO, Comment>();
            CreateMap<Follow, FollowResponseDTO>();
            CreateMap<Follow, FollowRequestDTO>();
            CreateMap<ApplicationUser, ProfileResponseDTO>();
            CreateMap<ApplicationUser, CreateProfileDTO>();
            CreateMap<ApplicationUser, UpdateProfileDTO>();
            CreateMap<ApplicationUser, ProfileResponseDTO>();
        }
    }
}