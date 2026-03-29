using System.Text.Json;
using Drink.Application.Constants;
using Drink.Application.Responses;

namespace Drink.Upload.API.Middleware;

public class ApiKeyMiddleware
{
  private const string ApiKeyHeader = "X-Api-Key";
  private readonly RequestDelegate _next;
  private readonly string _apiKey;

  public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
  {
    _next = next;
    _apiKey = configuration["ApiKey"]
      ?? throw new InvalidOperationException("ApiKey is not configured.");
  }

  public async Task InvokeAsync(HttpContext context)
  {
    // Skip API key check for static files (/assets)
    if (context.Request.Path.StartsWithSegments("/assets"))
    {
      await _next(context);
      return;
    }

    if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var extractedKey) ||
        !string.Equals(extractedKey, _apiKey, StringComparison.Ordinal))
    {
      context.Response.ContentType = "application/json";
      context.Response.StatusCode = StatusCodes.Status401Unauthorized;

      var response = ApiResponse.Fail(ErrorCodes.Unauthorized, "Invalid API key.");
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
