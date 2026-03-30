namespace Drink.Application.Responses;

public class ApiResponse
{
  public object? Data { get; set; }
  public string? Message { get; set; }
  public int Code { get; set; }
  public string? Error { get; set; }
  public Dictionary<string, string[]>? Errors { get; set; }

  public static ApiResponse Success(object? data = null)
      => new() { Data = data, Code = 0 };

  public static ApiResponse Fail(int code, string error, string message)
      => new() { Code = code, Error = error, Message = message };

  public static ApiResponse Fail((int Code, string Error) errorCode, string message)
      => new() { Code = errorCode.Code, Error = errorCode.Error, Message = message };

  public static ApiResponse Fail((int Code, string Error) errorCode, string message, Dictionary<string, string[]> errors)
      => new() { Code = errorCode.Code, Error = errorCode.Error, Message = message, Errors = errors };

  public static ApiResponse ValidationFail(Dictionary<string, string[]> errors)
      => new()
      {
        Code = 40001,
        Error = "VALIDATION_ERROR",
        Message = "輸入驗證失敗",
        Errors = errors
      };
}

public class ApiResponse<T>
{
  public T? Data { get; set; }
  public string? Message { get; set; }
  public int Code { get; set; }
  public string? Error { get; set; }
  public Dictionary<string, string[]>? Errors { get; set; }

  public static ApiResponse<T> Success(T? data = default)
      => new() { Data = data, Code = 0 };

  public static ApiResponse<T> Fail(int code, string error, string message)
      => new() { Code = code, Error = error, Message = message };

  public static ApiResponse<T> Fail((int Code, string Error) errorCode, string message)
      => new() { Code = errorCode.Code, Error = errorCode.Error, Message = message };

  public static ApiResponse<T> Fail((int Code, string Error) errorCode, string message, Dictionary<string, string[]> errors)
      => new() { Code = errorCode.Code, Error = errorCode.Error, Message = message, Errors = errors };
}