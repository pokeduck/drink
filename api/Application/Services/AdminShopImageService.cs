using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text.Json;
using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Application.Settings;
using Drink.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Drink.Application.Services;

public class AdminShopImageService : BaseService
{
  private const int MaxImagesPerItem = 10;

  private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

  private readonly IGenericRepository<ShopImage> _imageRepo;
  private readonly IGenericRepository<Shop> _shopRepo;
  private readonly IGenericRepository<ShopMenuItem> _menuItemRepo;
  private readonly IHttpClientFactory _httpClientFactory;
  private readonly UploadApiSettings _uploadApiSettings;

  public AdminShopImageService(
    ICurrentUserContext currentUser,
    IGenericRepository<ShopImage> imageRepo,
    IGenericRepository<Shop> shopRepo,
    IGenericRepository<ShopMenuItem> menuItemRepo,
    IHttpClientFactory httpClientFactory,
    IOptions<UploadApiSettings> uploadApiSettings) : base(currentUser)
  {
    _imageRepo = imageRepo;
    _shopRepo = shopRepo;
    _menuItemRepo = menuItemRepo;
    _httpClientFactory = httpClientFactory;
    _uploadApiSettings = uploadApiSettings.Value;
  }

  // ===== List =====

  public async Task<ApiResponse<ShopImageListResponse>> GetList(int shopId, ShopImageListQuery query)
  {
    if (!await _shopRepo.Any(s => s.Id == shopId && !s.IsDeleted))
      return Fail<ShopImageListResponse>(ErrorCodes.ShopNotFound, "店家不存在");

    Expression<Func<ShopImage, bool>> predicate = i => i.ShopId == shopId;

    var filter = query.Filter?.ToLowerInvariant();
    if (filter == "orphan")
      predicate = i => i.ShopId == shopId && i.DrinkItemId == null;
    else if (filter == "assigned")
      predicate = i => i.ShopId == shopId && i.DrinkItemId != null;

    if (query.DrinkItemId.HasValue)
    {
      var did = query.DrinkItemId.Value;
      predicate = i => i.ShopId == shopId && i.DrinkItemId == did;
    }

    if (!string.IsNullOrWhiteSpace(query.Keyword))
    {
      var kw = query.Keyword.Trim();
      var didFilter = query.DrinkItemId;
      predicate = i => i.ShopId == shopId
        && (didFilter == null || i.DrinkItemId == didFilter)
        && i.OriginalFileName != null
        && i.OriginalFileName.Contains(kw);
    }

    var page = query.Page < 1 ? 1 : query.Page;
    var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

    var pagination = await _imageRepo.GetPaginationList(
      page, pageSize,
      predicate: predicate,
      include: q => q.Include(i => i.DrinkItem!),
      order: q => q
        .OrderBy(i => i.DrinkItemId == null)  // false (assigned) 先；true (orphan) 後
        .ThenBy(i => i.DrinkItemId)
        .ThenBy(i => i.Sort)
        .ThenByDescending(i => i.CreatedAt)
        .ThenBy(i => i.Id));

    return Success(new ShopImageListResponse
    {
      Items = pagination.Items.ToShopImageResponseList(),
      Total = pagination.Total,
      Page = pagination.Page,
      PageSize = pagination.PageSize,
    });
  }

  public async Task<ApiResponse<List<ShopImageResponse>>> GetByDrinkItem(int shopId, int drinkItemId)
  {
    if (!await _shopRepo.Any(s => s.Id == shopId && !s.IsDeleted))
      return Fail<List<ShopImageResponse>>(ErrorCodes.ShopNotFound, "店家不存在");

    var images = await _imageRepo.GetList(
      i => i.ShopId == shopId && i.DrinkItemId == drinkItemId,
      include: q => q.Include(i => i.DrinkItem!),
      order: q => q.OrderBy(i => i.Sort).ThenBy(i => i.Id));

    return Success(images.ToShopImageResponseList());
  }

  // ===== Upload =====

  public async Task<ApiResponse<ShopImageResponse>> Upload(int shopId, IFormFile file, int? drinkItemId, CancellationToken ct = default)
  {
    if (!await _shopRepo.Any(s => s.Id == shopId && !s.IsDeleted))
      return Fail<ShopImageResponse>(ErrorCodes.ShopNotFound, "店家不存在");

    if (drinkItemId.HasValue)
    {
      var did = drinkItemId.Value;
      var inMenu = await _menuItemRepo.Any(m =>
        !m.IsDeleted && m.DrinkItemId == did && m.Category.ShopId == shopId);
      if (!inMenu)
        return Fail<ShopImageResponse>(ErrorCodes.ImageInvalidDrinkItem, "品項不存在或不屬於此店",
          new Dictionary<string, string[]> { ["drink_item_id"] = ["品項不存在或不屬於此店"] });

      var count = await _imageRepo.Count(i => i.ShopId == shopId && i.DrinkItemId == did);
      if (count >= MaxImagesPerItem)
        return Fail<ShopImageResponse>(ErrorCodes.ImageLimitReached, "此品項圖片已達 10 張上限",
          new Dictionary<string, string[]> { ["drink_item_id"] = ["此品項圖片已達 10 張上限"] });
    }

    // 1. forward 至 image-upload pipeline
    var uploadResp = await ForwardToImageUpload(file, ct);
    if (uploadResp.Code != 0 || uploadResp.Data == null)
    {
      return new ApiResponse<ShopImageResponse>
      {
        Data = null,
        Code = uploadResp.Code,
        Error = uploadResp.Error,
        Message = uploadResp.Message,
        Errors = uploadResp.Errors,
      };
    }
    var uploaded = uploadResp.Data;

    // 2. 計算 Sort / IsCover
    int sort = 0;
    bool isCover = false;
    if (drinkItemId.HasValue)
    {
      var did = drinkItemId.Value;
      var maxSort = await _imageRepo.Query
        .Where(i => i.ShopId == shopId && i.DrinkItemId == did)
        .Select(i => (int?)i.Sort)
        .MaxAsync(ct) ?? -1;
      sort = maxSort + 1;
      var hasCover = await _imageRepo.Any(i => i.ShopId == shopId && i.DrinkItemId == did && i.IsCover);
      isCover = !hasCover;
    }

    // 3. 建立 ShopImage row
    var now = DateTime.UtcNow;
    var entity = new ShopImage
    {
      ShopId = shopId,
      DrinkItemId = drinkItemId,
      Path = uploaded.Path,
      Hash = uploaded.Hash,
      Width = uploaded.Width,
      Height = uploaded.Height,
      FileSize = uploaded.Size,
      OriginalFileName = file.FileName,
      Sort = sort,
      IsCover = isCover,
      CreatedAt = now,
      Creator = CurrentUserId,
      UpdatedAt = now,
      Updater = CurrentUserId,
    };
    await _imageRepo.Insert(entity);

    var saved = await _imageRepo.GetById(entity.Id, q => q.Include(i => i.DrinkItem!));
    return Success(saved!.ToShopImageResponse());
  }

  // ===== Update =====

  public async Task<ApiResponse<ShopImageResponse>> Update(int shopId, int imageId, ShopImageUpdateRequest request)
  {
    var image = await _imageRepo.Get(
      i => i.Id == imageId && i.ShopId == shopId,
      tracking: true);
    if (image == null)
      return Fail<ShopImageResponse>(ErrorCodes.ShopImageNotFound, "圖片不存在");

    // 設封面對孤兒無效（且未一併改綁）
    if (request.IsCover == true && image.DrinkItemId == null
        && (!request.ChangeDrinkItem || request.DrinkItemId == null))
    {
      return Fail<ShopImageResponse>(ErrorCodes.ValidationError, "驗證錯誤",
        new Dictionary<string, string[]> { ["is_cover"] = ["孤兒圖不可設為封面"] });
    }

    // 預先驗證改綁參數
    if (request.ChangeDrinkItem && request.DrinkItemId.HasValue)
    {
      var newDid = request.DrinkItemId.Value;
      var inMenu = await _menuItemRepo.Any(m =>
        !m.IsDeleted && m.DrinkItemId == newDid && m.Category.ShopId == shopId);
      if (!inMenu)
        return Fail<ShopImageResponse>(ErrorCodes.ImageInvalidDrinkItem, "品項不存在或不屬於此店",
          new Dictionary<string, string[]> { ["drink_item_id"] = ["品項不存在或不屬於此店"] });

      var count = await _imageRepo.Count(i => i.ShopId == shopId && i.DrinkItemId == newDid && i.Id != imageId);
      if (count >= MaxImagesPerItem)
        return Fail<ShopImageResponse>(ErrorCodes.ImageLimitReached, "目標品項圖片已達 10 張上限",
          new Dictionary<string, string[]> { ["drink_item_id"] = ["目標品項圖片已達 10 張上限"] });
    }

    var oldDrinkItemId = image.DrinkItemId;

    await _imageRepo.ExecuteInTransaction(async () =>
    {
      // 1. 改綁
      if (request.ChangeDrinkItem)
      {
        if (request.DrinkItemId.HasValue)
        {
          var newDid = request.DrinkItemId.Value;
          var maxSort = await _imageRepo.Query
            .Where(i => i.ShopId == shopId && i.DrinkItemId == newDid)
            .Select(i => (int?)i.Sort)
            .MaxAsync() ?? -1;
          image.DrinkItemId = newDid;
          image.Sort = maxSort + 1;
          var hasCover = await _imageRepo.Any(i => i.ShopId == shopId && i.DrinkItemId == newDid && i.IsCover && i.Id != imageId);
          image.IsCover = !hasCover;
        }
        else
        {
          image.DrinkItemId = null;
          image.Sort = 0;
          image.IsCover = false;
        }
      }

      // 2. 設封面（已綁品項時才有效；改綁分支可能已處理）
      if (request.IsCover.HasValue && image.DrinkItemId.HasValue)
      {
        if (request.IsCover.Value && !image.IsCover)
        {
          await _imageRepo.ExecuteUpdate(
            i => i.ShopId == shopId && i.DrinkItemId == image.DrinkItemId && i.IsCover && i.Id != imageId,
            s => s.SetProperty(i => i.IsCover, false));
          image.IsCover = true;
        }
        else if (!request.IsCover.Value && image.IsCover)
        {
          image.IsCover = false;
        }
      }

      // 3. 改 Sort（已綁品項時才有效）
      if (request.Sort.HasValue && image.DrinkItemId.HasValue)
      {
        image.Sort = request.Sort.Value;
      }

      image.UpdatedAt = DateTime.UtcNow;
      image.Updater = CurrentUserId;
      await _imageRepo.Update(image);

      // 4. 補封面：原品項若失去封面，挑下一張
      if (oldDrinkItemId.HasValue && oldDrinkItemId != image.DrinkItemId)
      {
        await EnsureCoverForGroup(shopId, oldDrinkItemId.Value);
      }
    }, "圖片更新失敗");

    var refreshed = await _imageRepo.GetById(imageId, q => q.Include(i => i.DrinkItem!));
    return Success(refreshed!.ToShopImageResponse());
  }

  private async Task EnsureCoverForGroup(int shopId, int drinkItemId)
  {
    var hasCover = await _imageRepo.Any(i => i.ShopId == shopId && i.DrinkItemId == drinkItemId && i.IsCover);
    if (hasCover) return;
    var next = await _imageRepo.Query
      .Where(i => i.ShopId == shopId && i.DrinkItemId == drinkItemId)
      .OrderBy(i => i.Sort).ThenBy(i => i.Id)
      .FirstOrDefaultAsync();
    if (next != null)
    {
      next.IsCover = true;
      next.UpdatedAt = DateTime.UtcNow;
      next.Updater = CurrentUserId;
      await _imageRepo.Update(next);
    }
  }

  // ===== Sort batch =====

  public async Task<ApiResponse> UpdateSort(int shopId, int drinkItemId, ShopImageSortRequest request)
  {
    var existing = await _imageRepo.GetList(
      i => i.ShopId == shopId && i.DrinkItemId == drinkItemId,
      tracking: true);
    var existingIds = existing.Select(e => e.Id).ToHashSet();
    var inputIds = request.Items.Select(x => x.Id).ToHashSet();
    if (existingIds.Count != inputIds.Count || !existingIds.SetEquals(inputIds))
    {
      return Fail(ErrorCodes.ValidationError, "排序清單與該品項目前圖片不一致");
    }

    var sortMap = request.Items.ToDictionary(x => x.Id, x => x.Sort);
    var now = DateTime.UtcNow;
    foreach (var img in existing)
    {
      img.Sort = sortMap[img.Id];
      img.UpdatedAt = now;
      img.Updater = CurrentUserId;
    }
    await _imageRepo.UpdateRange(existing);
    return Success();
  }

  // ===== Delete =====

  public async Task<ApiResponse<ShopImageBatchDeleteResponse>> BatchDelete(int shopId, ShopImageBatchDeleteRequest request)
  {
    var images = await _imageRepo.GetList(
      i => i.ShopId == shopId && request.Ids.Contains(i.Id),
      tracking: true);
    if (images.Count != request.Ids.Count)
    {
      return Fail<ShopImageBatchDeleteResponse>(ErrorCodes.ValidationError, "包含不存在或不屬於此店的圖片 id");
    }

    var affectedDrinkItems = images
      .Where(i => i.DrinkItemId.HasValue && i.IsCover)
      .Select(i => i.DrinkItemId!.Value)
      .Distinct()
      .ToList();

    await _imageRepo.ExecuteInTransaction(async () =>
    {
      await _imageRepo.DeleteRange(images);
      foreach (var did in affectedDrinkItems)
      {
        await EnsureCoverForGroup(shopId, did);
      }
    }, "批量刪除失敗");

    return Success(new ShopImageBatchDeleteResponse { Deleted = images.Count });
  }

  public async Task<ApiResponse<ShopImageBatchDeleteResponse>> DeleteOrphans(int shopId)
  {
    var orphans = await _imageRepo.GetList(
      i => i.ShopId == shopId && i.DrinkItemId == null,
      tracking: true);
    if (orphans.Count == 0)
      return Success(new ShopImageBatchDeleteResponse { Deleted = 0 });

    await _imageRepo.DeleteRange(orphans);
    return Success(new ShopImageBatchDeleteResponse { Deleted = orphans.Count });
  }

  // ===== Cascade orphan (called by AdminShopMenuService) =====

  /// <summary>
  /// 將指定 (ShopId, DrinkItemId) 的所有 ShopImage 孤兒化。
  /// 由 ShopMenuItem 軟刪流程於同一 transaction 中呼叫。
  /// </summary>
  public async Task CascadeOrphan(int shopId, IEnumerable<int> drinkItemIds)
  {
    var ids = drinkItemIds.Distinct().ToList();
    if (ids.Count == 0) return;
    var now = DateTime.UtcNow;
    var updater = CurrentUserId;
    await _imageRepo.ExecuteUpdate(
      i => i.ShopId == shopId && i.DrinkItemId.HasValue && ids.Contains(i.DrinkItemId.Value),
      s => s.SetProperty(i => i.DrinkItemId, (int?)null)
            .SetProperty(i => i.Sort, 0)
            .SetProperty(i => i.IsCover, false)
            .SetProperty(i => i.UpdatedAt, now)
            .SetProperty(i => i.Updater, updater));
  }

  // ===== HTTP forward to Upload.API =====

  /// <summary>
  /// 將 multipart 上傳轉發給 Upload.API 並原樣回傳結果（含成功與錯誤 ApiResponse）。
  /// </summary>
  private async Task<ApiResponse<FileUploadResponse>> ForwardToImageUpload(IFormFile file, CancellationToken ct)
  {
    using var client = _httpClientFactory.CreateClient();
    client.DefaultRequestHeaders.Add("X-Api-Key", _uploadApiSettings.ApiKey);

    using var content = new MultipartFormDataContent();
    await using var stream = file.OpenReadStream();
    var fileContent = new StreamContent(stream);
    if (!string.IsNullOrEmpty(file.ContentType))
      fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
    content.Add(fileContent, "file", file.FileName);

    var response = await client.PostAsync($"{_uploadApiSettings.BaseUrl}/api/upload/files", content, ct);
    var json = await response.Content.ReadAsStringAsync(ct);

    var result = JsonSerializer.Deserialize<ApiResponse<FileUploadResponse>>(json, JsonOpts);
    return result ?? ApiResponse<FileUploadResponse>.Fail(ErrorCodes.FileUploadFailed, "上傳失敗");
  }
}
