using System.Text.Json;
using Drink.Application.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Drink.Shared.Web.Extensions;

public static class ApiBehaviorExtensions
{
  /// <summary>
  /// 把 ModelState 自動驗證失敗回的 ProblemDetails 改成 ApiResponse.ValidationFail，
  /// 並把 errors 字典 key 從 PascalCase 轉成 snake_case 以對齊前端表單欄位。
  /// </summary>
  public static IServiceCollection ConfigureApiValidationResponse(this IServiceCollection services)
  {
    services.Configure<ApiBehaviorOptions>(options =>
    {
      options.InvalidModelStateResponseFactory = context =>
      {
        var errors = context.ModelState
          .Where(e => e.Value is not null && e.Value.Errors.Count > 0)
          .ToDictionary(
            e => JsonNamingPolicy.SnakeCaseLower.ConvertName(e.Key),
            e => e.Value!.Errors.Select(err => err.ErrorMessage).ToArray());

        var response = ApiResponse.ValidationFail(errors);
        return new BadRequestObjectResult(response);
      };
    });

    return services;
  }
}
