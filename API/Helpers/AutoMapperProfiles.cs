using API.DTOs;
using API.Entities;
using AutoMapper;
using System.Linq;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, UserDto>()
                .ForMember(dest => dest.PhotoUrl, 
                    option => option.MapFrom((src, dest) => src.Photos?.FirstOrDefault(p => p.IsMain)?.Url))
                .ForMember(dest => dest.Age,
                    option => option.MapFrom(src => src.GetAge()));

            CreateMap<Photo, PhotoDto>();
        }
    }
}
