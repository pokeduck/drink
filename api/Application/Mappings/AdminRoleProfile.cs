using AutoMapper;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;

namespace Drink.Application.Mappings;

public class AdminRoleProfile : Profile
{
  public AdminRoleProfile()
  {
    CreateMap<AdminRole, AdminRoleListResponse>()
      .ForMember(dest => dest.StaffCount, opt => opt.MapFrom(src => src.Users.Count));

    CreateMap<AdminRole, AdminRoleDetailResponse>();
  }
}
