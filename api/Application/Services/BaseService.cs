using System.Security.Claims;
using Drink.Application.Responses;
using Drink.Domain.Entities;
using Drink.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Drink.Application.Services;

public abstract class BaseService
{
    protected readonly IServiceProvider ServiceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    protected BaseService(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    }

    protected int CurrentUserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : 0;
        }
    }

    protected IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : BaseDataEntity
        => ServiceProvider.GetRequiredService<IGenericRepository<TEntity>>();

    protected static ApiResponse Success(object? data = null)
        => ApiResponse.Success(data);

    protected static ApiResponse<T> Success<T>(T? data = default)
        => ApiResponse<T>.Success(data);

    protected static ApiResponse Fail((int Code, string Error) errorCode, string message)
        => ApiResponse.Fail(errorCode, message);

    protected static ApiResponse<T> Fail<T>((int Code, string Error) errorCode, string message)
        => ApiResponse<T>.Fail(errorCode, message);
}
