namespace Drink.Application.Responses;

public class FileUploadResponse
{
  public string FileName { get; set; } = null!;
  public string Url { get; set; } = null!;
  public string ContentType { get; set; } = null!;
  public long FileSize { get; set; }
}
