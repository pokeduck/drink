namespace Drink.Application.Settings;

public class UploadSettings
{
  public string StoragePath { get; set; } = "Uploads";
  public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB
  public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
  public string PublicBaseUrl { get; set; } = "http://localhost:5103/assets";
}
