using Drink.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Drink.User.API.Controllers;

[ApiController]
[Route("api/user/[controller]")]
public abstract class BaseController : ControllerBase
{
  protected IActionResult ApiOk(object? data = null)
      => Ok(ApiResponse.Success(data));

  protected IActionResult ApiOk<T>(T? data = default)
      => Ok(ApiResponse<T>.Success(data));

  protected IActionResult ApiError((int Code, string Error) errorCode, string message, int httpStatus = 400)
      => StatusCode(httpStatus, ApiResponse.Fail(errorCode, message));

  protected IActionResult ApiValidationError(Dictionary<string, string[]> errors)
      => BadRequest(ApiResponse.ValidationFail(errors));
}