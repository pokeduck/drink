using AutoMapper;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;

namespace Drink.Application.Mappings;

public class AdminUserProfile : Profile
{
  public AdminUserProfile()
  {
    CreateMap<AdminUser, AdminUserListResponse>()
      .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));

    CreateMap<AdminUser, AdminUserDetailResponse>()
      .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
  }
}
