using AutoMapper;
using MiniNetwork.Domain.Entities;
using MiniNetwork.Application.Users.DTOs;

namespace MiniNetwork.Application.Common.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User -> UserProfileDto
        CreateMap<User, UserProfileDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

        // User -> UserSummaryDto
        CreateMap<User, UserSummaryDto>();
    }
}
