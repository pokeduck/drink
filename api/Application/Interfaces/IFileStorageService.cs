namespace Drink.Application.Interfaces;

public interface IFileStorageService
{
  bool Exists(string relativePath);
  Task WriteIfNotExistsAsync(string relativePath, byte[] bytes, CancellationToken ct = default);
  string GetPhysicalPath(string relativePath);
}
