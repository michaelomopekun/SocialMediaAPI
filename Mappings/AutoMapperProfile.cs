using AutoMapper;
using SocialMediaAPI.Models.Domain;
using SocialMediaAPI.Models.DTOs;

namespace SocialMediaAPI.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Post, PostResponseDTO>();
                // .ForMember(dest => dest.UserName, 
                //     opt => opt.MapFrom(src => src.User == null ? string.Empty : src.User.UserName));
            CreateMap<CreatePostDTO, Post>();
            CreateMap<UpdatePostDTO, Post>();
        }
    }
}