using System.Security.Claims;
using System.Text.Json;
using Drink.Application.Attributes;
using Drink.Application.Constants;
using Drink.Application.Responses;
using Drink.Domain.Entities;
using Drink.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Drink.Application.Middleware;

public class RoleMiddleware
{
  private readonly RequestDelegate _next;

  public RoleMiddleware(RequestDelegate next)
  {
    _next = next;
  }

  public async Task InvokeAsync(HttpContext context, DrinkDbContext db)
  {
    var endpoint = context.GetEndpoint();
    var attr = endpoint?.Metadata.GetMetadata<RequireRoleAttribute>();

    if (attr is null)
    {
      await _next(context);
      return;
    }

    // 取得 UserId（從 JWT ClaimTypes.NameIdentifier）
    var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
    {
      context.Response.StatusCode = StatusCodes.Status401Unauthorized;
      return;
    }

    // 從 DB 查詢使用者的 RoleId（即時，不快取）
    var user = await db.Set<AdminUser>()
      .AsNoTracking()
      .Where(u => u.Id == userId)
      .Select(u => new { u.RoleId })
      .FirstOrDefaultAsync();

    if (user is null)
    {
      context.Response.StatusCode = StatusCodes.Status401Unauthorized;
      return;
    }

    // 查詢該 Role 對應 Menu 的 CRUD 權限
    var menuRole = await db.Set<AdminMenuRole>()
      .AsNoTracking()
      .FirstOrDefaultAsync(mr => mr.RoleId == user.RoleId && mr.MenuId == attr.MenuId);

    var hasAccess = attr.Action switch
    {
      CrudAction.Read => menuRole?.CanRead ?? false,
      CrudAction.Create => menuRole?.CanCreate ?? false,
      CrudAction.Update => menuRole?.CanUpdate ?? false,
      CrudAction.Delete => menuRole?.CanDelete ?? false,
      _ => false
    };

    if (!hasAccess)
    {
      context.Response.StatusCode = StatusCodes.Status403Forbidden;
      context.Response.ContentType = "application/json";

      var response = ApiResponse.Fail(ErrorCodes.Forbidden, "無存取權限");
      var options = new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
      };
      await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
      return;
    }

    await _next(context);
  }
}
