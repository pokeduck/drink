using Drink.Application.Interfaces;
using Drink.Application.Settings;
using Microsoft.Extensions.Options;

namespace Drink.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
  private readonly UploadSettings _settings;

  public FileStorageService(IOptions<UploadSettings> settings)
  {
    _settings = settings.Value;
  }

  public bool Exists(string relativePath)
  {
    return File.Exists(GetPhysicalPath(relativePath));
  }

  public async Task WriteIfNotExistsAsync(string relativePath, byte[] bytes, CancellationToken ct = default)
  {
    var physicalPath = GetPhysicalPath(relativePath);
    if (File.Exists(physicalPath))
      return;

    var dir = Path.GetDirectoryName(physicalPath);
    if (!string.IsNullOrEmpty(dir))
      Directory.CreateDirectory(dir);

    await File.WriteAllBytesAsync(physicalPath, bytes, ct);
  }

  public string GetPhysicalPath(string relativePath)
  {
    var safe = relativePath.Replace('\\', '/').TrimStart('/');
    return Path.Combine(_settings.StoragePath, safe);
  }
}
