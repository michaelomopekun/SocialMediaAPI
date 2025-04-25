using AutoMapper;
using SocialMediaAPI.Models.Domain;
using SocialMediaAPI.Models.DTOs;

namespace SocialMediaAPI.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Post, PostResponseDTO>()
                .ForMember(dest => dest.UserName, 
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.LikesCount,
                    opt => opt.MapFrom(src => src.Likes != null ? src.Likes.Count : 0))
                .ForMember(dest => dest.CommentsCount,
                    opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Count : 0));

            CreateMap<CreatePostDTO, Post>()
                .ForMember(dest => dest.CreatedAt, 
                    opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdatePostDTO, Post>()
                .ForMember(dest => dest.UpdatedAt, 
                    opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}