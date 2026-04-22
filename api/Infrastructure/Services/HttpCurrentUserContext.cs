using System.Security.Claims;
using Drink.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Drink.Infrastructure.Services;

public class HttpCurrentUserContext : ICurrentUserContext
{
  private readonly IHttpContextAccessor _accessor;

  public HttpCurrentUserContext(IHttpContextAccessor accessor)
  {
    _accessor = accessor;
  }

  public int UserId
  {
    get
    {
      var claim = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
      return int.TryParse(claim, out var id) ? id : 0;
    }
  }
}
