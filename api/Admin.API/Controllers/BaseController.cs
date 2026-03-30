using Drink.Application.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Drink.Admin.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
public abstract class BaseController : ControllerBase
{
  protected IActionResult ApiOk(object? data = null)
      => Ok(ApiResponse.Success(data));

  protected IActionResult ApiOk<T>(T? data = default)
      => Ok(ApiResponse<T>.Success(data));

  protected IActionResult ApiError((int Code, string Error) errorCode, string message, int httpStatus = 400, Dictionary<string, string[]>? errors = null)
      => StatusCode(httpStatus, errors is null
          ? ApiResponse.Fail(errorCode, message)
          : new ApiResponse { Code = errorCode.Code, Error = errorCode.Error, Message = message, Errors = errors });

  protected IActionResult ApiValidationError(Dictionary<string, string[]> errors)
      => BadRequest(ApiResponse.ValidationFail(errors));
}