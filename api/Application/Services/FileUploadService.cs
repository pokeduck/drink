using System.Security.Cryptography;
using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Responses;
using Drink.Application.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SkiaSharp;

namespace Drink.Application.Services;

public class FileUploadService : BaseService
{
  private const int MaxDimension = 4000;
  private const int WebpQuality = 85;

  private readonly IFileStorageService _storageService;
  private readonly UploadSettings _uploadSettings;

  private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
  {
    ".jpg", ".jpeg", ".png", ".gif", ".webp"
  };

  public FileUploadService(
    ICurrentUserContext currentUser,
    IFileStorageService storageService,
    IOptions<UploadSettings> uploadSettings) : base(currentUser)
  {
    _storageService = storageService;
    _uploadSettings = uploadSettings.Value;
  }

  public async Task<ApiResponse<FileUploadResponse>> Upload(IFormFile file, CancellationToken ct = default)
  {
    if (file.Length == 0)
      return Fail<FileUploadResponse>(ErrorCodes.InvalidImage, "File is empty.");

    if (file.Length > _uploadSettings.MaxFileSizeBytes)
      return Fail<FileUploadResponse>(ErrorCodes.FileTooLarge, $"File size exceeds {_uploadSettings.MaxFileSizeBytes / 1024 / 1024}MB limit.");

    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (!AllowedExtensions.Contains(extension))
      return Fail<FileUploadResponse>(ErrorCodes.InvalidFileType, $"Extension '{extension}' is not allowed.");

    byte[] originalBytes;
    await using (var input = file.OpenReadStream())
    using (var ms = new MemoryStream())
    {
      await input.CopyToAsync(ms, ct);
      originalBytes = ms.ToArray();
    }

    var detectedFormat = DetectImageFormat(originalBytes);
    if (detectedFormat is null || !ExtensionMatchesFormat(extension, detectedFormat.Value))
      return Fail<FileUploadResponse>(ErrorCodes.InvalidImage, "File content does not match its declared format.");

    SKBitmap? bitmap;
    try
    {
      bitmap = DecodeImage(originalBytes, detectedFormat.Value);
    }
    catch
    {
      return Fail<FileUploadResponse>(ErrorCodes.InvalidImage, "Failed to decode image.");
    }

    if (bitmap is null)
      return Fail<FileUploadResponse>(ErrorCodes.InvalidImage, "Failed to decode image.");

    var (resized, ownsResized) = ResizeIfNeeded(bitmap);

    try
    {
      byte[] webpBytes;
      var width = resized.Width;
      var height = resized.Height;
      using (var image = SKImage.FromBitmap(resized))
      using (var data = image.Encode(SKEncodedImageFormat.Webp, WebpQuality))
      {
        if (data is null)
          return Fail<FileUploadResponse>(ErrorCodes.InvalidImage, "Failed to encode image.");
        webpBytes = data.ToArray();
      }

      var hash = ComputeSha256Hex(webpBytes);
      var prefix = hash[..6];
      var relativePath = $"images/{prefix}/{hash}.webp";

      await _storageService.WriteIfNotExistsAsync(relativePath, webpBytes, ct);

      return Success(new FileUploadResponse
      {
        Path = $"/assets/{relativePath}",
        Hash = hash,
        Size = webpBytes.LongLength,
        Width = width,
        Height = height,
        MimeType = "image/webp"
      });
    }
    catch
    {
      return Fail<FileUploadResponse>(ErrorCodes.InvalidImage, "Failed to process image.");
    }
    finally
    {
      if (ownsResized)
        resized.Dispose();
      bitmap.Dispose();
    }
  }

  private static SKBitmap? DecodeImage(byte[] bytes, ImageFormat format)
  {
    if (format == ImageFormat.Gif)
    {
      using var codec = SKCodec.Create(new MemoryStream(bytes, writable: false));
      if (codec is null)
        return null;

      var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
      var bitmap = new SKBitmap(info);
      var opts = new SKCodecOptions(0); // first frame
      var result = codec.GetPixels(info, bitmap.GetPixels(), opts);
      if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput)
      {
        bitmap.Dispose();
        return null;
      }
      return bitmap;
    }

    return SKBitmap.Decode(bytes);
  }

  private static (SKBitmap bitmap, bool ownsResized) ResizeIfNeeded(SKBitmap bitmap)
  {
    var maxSide = Math.Max(bitmap.Width, bitmap.Height);
    if (maxSide <= MaxDimension)
      return (bitmap, false);

    var scale = (double)MaxDimension / maxSide;
    var newWidth = (int)(bitmap.Width * scale);
    var newHeight = (int)(bitmap.Height * scale);

    var info = new SKImageInfo(newWidth, newHeight, bitmap.ColorType, bitmap.AlphaType);
    var resized = bitmap.Resize(info, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
    return resized is null ? (bitmap, false) : (resized, true);
  }

  private static string ComputeSha256Hex(byte[] bytes)
  {
    var hash = SHA256.HashData(bytes);
    return Convert.ToHexStringLower(hash);
  }

  private static ImageFormat? DetectImageFormat(byte[] bytes)
  {
    if (bytes.Length < 12) return null;

    // PNG: 89 50 4E 47 0D 0A 1A 0A
    if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47
        && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A)
      return ImageFormat.Png;

    // JPEG: FF D8 FF
    if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
      return ImageFormat.Jpeg;

    // GIF: 47 49 46 38 (37|39) 61
    if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38
        && (bytes[4] == 0x37 || bytes[4] == 0x39) && bytes[5] == 0x61)
      return ImageFormat.Gif;

    // WebP: 52 49 46 46 .. .. .. .. 57 45 42 50
    if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46
        && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
      return ImageFormat.Webp;

    return null;
  }

  private static bool ExtensionMatchesFormat(string extension, ImageFormat format)
  {
    return (extension, format) switch
    {
      (".jpg" or ".jpeg", ImageFormat.Jpeg) => true,
      (".png", ImageFormat.Png) => true,
      (".gif", ImageFormat.Gif) => true,
      (".webp", ImageFormat.Webp) => true,
      _ => false
    };
  }

  private enum ImageFormat
  {
    Jpeg,
    Png,
    Gif,
    Webp
  }
}
