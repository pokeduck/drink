using Drink.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Drink.Application.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services, params Type[] excludeTypes)
  {
    services.Scan(scan => scan
        .FromAssemblyOf<BaseService>()
        .AddClasses(classes => classes
            .AssignableTo<BaseService>()
            .Where(t => !excludeTypes.Contains(t)))
        .AsSelf()
        .WithScopedLifetime());

    return services;
  }
}