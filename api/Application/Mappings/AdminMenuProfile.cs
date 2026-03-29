using AutoMapper;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;

namespace Drink.Application.Mappings;

public class AdminMenuProfile : Profile
{
  public AdminMenuProfile()
  {
    CreateMap<AdminMenu, MenuTreeResponse>()
      .ForMember(dest => dest.Children, opt => opt.Ignore());
  }
}
