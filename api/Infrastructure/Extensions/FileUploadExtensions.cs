using Drink.Infrastructure.Services;
using Drink.Infrastructure.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Drink.Infrastructure.Extensions;

public static class FileUploadExtensions
{
  public static IServiceCollection AddFileUpload(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<UploadSettings>(configuration.GetSection("Upload"));
    services.AddScoped<IFileStorageService, FileStorageService>();
    return services;
  }

  /// <summary>
  /// Maps /assets/{**path} to the physical Uploads/ directory,
  /// hiding the real storage path from clients.
  /// </summary>
  public static WebApplication UseAssetFileServer(this WebApplication app, IConfiguration configuration)
  {
    var uploadSettings = configuration.GetSection("Upload").Get<UploadSettings>() ?? new UploadSettings();
    var storagePath = Path.GetFullPath(uploadSettings.StoragePath);
    Directory.CreateDirectory(storagePath);

    app.UseStaticFiles(new StaticFileOptions
    {
      FileProvider = new PhysicalFileProvider(storagePath),
      RequestPath = "/assets",
      ServeUnknownFileTypes = false,
      OnPrepareResponse = ctx =>
      {
        // Cache for 1 day
        ctx.Context.Response.Headers.CacheControl = "public, max-age=86400";
      }
    });

    return app;
  }
}
