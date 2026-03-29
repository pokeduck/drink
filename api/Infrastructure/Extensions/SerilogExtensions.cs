using Microsoft.AspNetCore.Builder;
using Serilog;

namespace Drink.Infrastructure.Extensions;

public static class SerilogExtensions
{
  public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
  {
    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    return builder;
  }

  public static WebApplication UseSerilogLogging(this WebApplication app)
  {
    app.UseSerilogRequestLogging();
    return app;
  }
}
