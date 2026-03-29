using Drink.Application.Constants;
using Drink.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Upload.API.Controllers;

[ApiController]
[Route("api/upload/[controller]")]
public abstract class BaseController : ControllerBase
{
  protected IActionResult ApiOk(object? data = null)
      => Ok(ApiResponse.Success(data));

  protected IActionResult ApiOk<T>(T? data = default)
      => Ok(ApiResponse<T>.Success(data));

  protected IActionResult ApiError((int Code, string Error) errorCode, string message, int statusCode = 400)
      => StatusCode(statusCode, ApiResponse.Fail(errorCode, message));
}
