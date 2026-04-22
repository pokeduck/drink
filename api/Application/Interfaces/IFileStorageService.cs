using Microsoft.AspNetCore.Http;

namespace Drink.Application.Interfaces;

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
