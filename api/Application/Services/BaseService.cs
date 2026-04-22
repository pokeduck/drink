using Drink.Application.Interfaces;
using Drink.Application.Responses;

namespace Drink.Application.Services;

public abstract class BaseService
{
  protected readonly ICurrentUserContext CurrentUser;

  protected BaseService(ICurrentUserContext currentUser)
  {
    CurrentUser = currentUser;
  }

  protected int CurrentUserId => CurrentUser.UserId;

  protected static ApiResponse Success(object? data = null)
      => ApiResponse.Success(data);

  protected static ApiResponse<T> Success<T>(T? data = default)
      => ApiResponse<T>.Success(data);

  protected static ApiResponse Fail((int Code, string Error) errorCode, string message)
      => ApiResponse.Fail(errorCode, message);

  protected static ApiResponse Fail((int Code, string Error) errorCode, string message, Dictionary<string, string[]> errors)
      => ApiResponse.Fail(errorCode, message, errors);

  protected static ApiResponse<T> Fail<T>((int Code, string Error) errorCode, string message)
      => ApiResponse<T>.Fail(errorCode, message);

  protected static ApiResponse<T> Fail<T>((int Code, string Error) errorCode, string message, Dictionary<string, string[]> errors)
      => ApiResponse<T>.Fail(errorCode, message, errors);
}
