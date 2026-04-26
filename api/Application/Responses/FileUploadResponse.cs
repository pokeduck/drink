namespace Drink.Application.Responses;

public class FileUploadResponse
{
  public string Path { get; set; } = null!;
  public string Hash { get; set; } = null!;
  public long Size { get; set; }
  public int Width { get; set; }
  public int Height { get; set; }
  public string MimeType { get; set; } = "image/webp";
}
