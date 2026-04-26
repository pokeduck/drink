using Drink.Application.Interfaces;
using Drink.Application.Settings;
using Drink.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;

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
  /// Adds CDN-friendly cache headers (immutable + ETag based on content hash filename).
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
        var headers = ctx.Context.Response.Headers;
        headers[HeaderNames.CacheControl] = "public, max-age=31536000, immutable";

        // ETag = file name without extension (which is the SHA-256 hash for our pipeline)
        var fileName = Path.GetFileNameWithoutExtension(ctx.File.Name);
        if (!string.IsNullOrEmpty(fileName))
          headers[HeaderNames.ETag] = $"\"{fileName}\"";
      }
    });

    return app;
  }
}
