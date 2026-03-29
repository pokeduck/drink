using Drink.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Drink.Infrastructure.Services;

public interface IFileStorageService
{
  Task<FileStorageResult> SaveAsync(IFormFile file, string category);
  void Delete(string storedPath);
  string GetPhysicalPath(string storedPath);
}

public class FileStorageResult
{
  public string StoredFileName { get; set; } = null!;
  public string StoredPath { get; set; } = null!;
  public string OriginalFileName { get; set; } = null!;
  public string ContentType { get; set; } = null!;
  public long FileSize { get; set; }
}

public class FileStorageService : IFileStorageService
{
  private readonly UploadSettings _settings;

  public FileStorageService(IOptions<UploadSettings> settings)
  {
    _settings = settings.Value;
  }

  public async Task<FileStorageResult> SaveAsync(IFormFile file, string category)
  {
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

    if (!_settings.AllowedExtensions.Contains(extension))
      throw new InvalidOperationException($"File extension '{extension}' is not allowed.");

    if (file.Length > _settings.MaxFileSizeBytes)
      throw new InvalidOperationException($"File size exceeds the limit of {_settings.MaxFileSizeBytes / 1024 / 1024}MB.");

    // Generate unique stored filename: {guid}{extension}
    var storedFileName = $"{Guid.NewGuid():N}{extension}";
    var categoryDir = Path.Combine(_settings.StoragePath, category);
    Directory.CreateDirectory(categoryDir);

    var physicalPath = Path.Combine(categoryDir, storedFileName);
    await using var stream = new FileStream(physicalPath, FileMode.Create);
    await file.CopyToAsync(stream);

    return new FileStorageResult
    {
      StoredFileName = storedFileName,
      StoredPath = $"{category}/{storedFileName}",
      OriginalFileName = file.FileName,
      ContentType = file.ContentType,
      FileSize = file.Length
    };
  }

  public void Delete(string storedPath)
  {
    var physicalPath = GetPhysicalPath(storedPath);
    if (File.Exists(physicalPath))
      File.Delete(physicalPath);
  }

  public string GetPhysicalPath(string storedPath)
  {
    return Path.Combine(_settings.StoragePath, storedPath);
  }
}
