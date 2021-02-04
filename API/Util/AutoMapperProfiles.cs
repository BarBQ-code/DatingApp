﻿using System.Linq;
using System.Text;
using API.DTOs;
using API.Entities;
using API.Exstensions;
using AutoMapper;

namespace API.Util
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl,
                    opt => 
                        opt.MapFrom(src => src.Photos.FirstOrDefault((x => x.IsMain)).Url))
                .ForMember(dest => dest.Age, opt => 
                    opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<Photo, PhotoDto>();
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<RegisterDto, AppUser>()
                .ForMember(dest => dest.Password,
                    opt => opt.MapFrom(src => Encoding.UTF8.GetBytes(src.Password)));
        }
    }
}