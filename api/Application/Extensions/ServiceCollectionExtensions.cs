using Drink.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Drink.Application.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddApplicationServices(this IServiceCollection services)
  {
    services.Scan(scan => scan
        .FromAssemblyOf<BaseService>()
        .AddClasses(classes => classes.AssignableTo<BaseService>())
        .AsSelf()
        .WithScopedLifetime());

    return services;
  }
}