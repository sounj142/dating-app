using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;
using System;
using System.Linq;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, UserDto>()
                .ForMember(dest => dest.PhotoUrl, 
                    option => option.MapFrom((src, dest) => src.Photos?.FirstOrDefault(p => p.IsMain && p.IsApproved)?.Url))
                .ForMember(dest => dest.Age,
                    option => option.MapFrom(src => src.GetAge()));

            CreateMap<Photo, PhotoDto>();
            CreateMap<UserUpdateDto, AppUser>();

            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest => dest.DateOfBirth,
                    option => option.MapFrom(src => 
                    new DateTime(src.DateOfBirth.Year, src.DateOfBirth.Month, src.DateOfBirth.Day))
                );

            CreateMap<AppUser, LikeDto>()
                .ForMember(dest => dest.PhotoUrl,
                    option => option.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain && p.IsApproved).Url))
                .ForMember(dest => dest.Age,
                    option => option.MapFrom(src => src.DateOfBirth.CalculateAge()))
                .ForMember(dest => dest.UserId,
                    option => option.MapFrom(src => src.Id));

            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.SenderPhotoUrl,
                    option => option.MapFrom(src => src.Sender.Photos.FirstOrDefault(p => p.IsMain && p.IsApproved).Url))
                .ForMember(dest => dest.RecipientPhotoUrl,
                    option => option.MapFrom(src => src.Recipient.Photos.FirstOrDefault(p => p.IsMain && p.IsApproved).Url));
        }
    }
}
