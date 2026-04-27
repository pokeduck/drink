using System.Linq.Expressions;
using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Models;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Drink.Application.Services;

public class AdminShopService : BaseService
{
  private readonly IGenericRepository<Shop> _shopRepo;
  private readonly IGenericRepository<ShopCategory> _categoryRepo;
  private readonly IGenericRepository<ShopMenuItem> _menuItemRepo;
  private readonly IGenericRepository<ShopMenuItemSize> _menuItemSizeRepo;
  private readonly IGenericRepository<ShopMenuItemSugar> _menuItemSugarRepo;
  private readonly IGenericRepository<ShopMenuItemIce> _menuItemIceRepo;
  private readonly IGenericRepository<ShopMenuItemTopping> _menuItemToppingRepo;
  private readonly IGenericRepository<ShopSugarOverride> _sugarOverrideRepo;
  private readonly IGenericRepository<ShopToppingOverride> _toppingOverrideRepo;
  private readonly IGenericRepository<DrinkItem> _drinkItemRepo;
  private readonly IGenericRepository<Sugar> _sugarRepo;
  private readonly IGenericRepository<Topping> _toppingRepo;
  private readonly IGenericRepository<Ice> _iceRepo;
  private readonly IGenericRepository<Size> _sizeRepo;
  private readonly IGenericRepository<ShopEnabledSugar> _enabledSugarRepo;
  private readonly IGenericRepository<ShopEnabledIce> _enabledIceRepo;
  private readonly IGenericRepository<ShopEnabledTopping> _enabledToppingRepo;
  private readonly IGenericRepository<ShopEnabledSize> _enabledSizeRepo;
  private readonly AdminShopImageService _imageService;

  public AdminShopService(
    ICurrentUserContext currentUser,
    IGenericRepository<Shop> shopRepo,
    IGenericRepository<ShopCategory> categoryRepo,
    IGenericRepository<ShopMenuItem> menuItemRepo,
    IGenericRepository<ShopMenuItemSize> menuItemSizeRepo,
    IGenericRepository<ShopMenuItemSugar> menuItemSugarRepo,
    IGenericRepository<ShopMenuItemIce> menuItemIceRepo,
    IGenericRepository<ShopMenuItemTopping> menuItemToppingRepo,
    IGenericRepository<ShopSugarOverride> sugarOverrideRepo,
    IGenericRepository<ShopToppingOverride> toppingOverrideRepo,
    IGenericRepository<DrinkItem> drinkItemRepo,
    IGenericRepository<Sugar> sugarRepo,
    IGenericRepository<Topping> toppingRepo,
    IGenericRepository<Ice> iceRepo,
    IGenericRepository<Size> sizeRepo,
    IGenericRepository<ShopEnabledSugar> enabledSugarRepo,
    IGenericRepository<ShopEnabledIce> enabledIceRepo,
    IGenericRepository<ShopEnabledTopping> enabledToppingRepo,
    IGenericRepository<ShopEnabledSize> enabledSizeRepo,
    AdminShopImageService imageService) : base(currentUser)
  {
    _shopRepo = shopRepo;
    _categoryRepo = categoryRepo;
    _menuItemRepo = menuItemRepo;
    _menuItemSizeRepo = menuItemSizeRepo;
    _menuItemSugarRepo = menuItemSugarRepo;
    _menuItemIceRepo = menuItemIceRepo;
    _menuItemToppingRepo = menuItemToppingRepo;
    _sugarOverrideRepo = sugarOverrideRepo;
    _toppingOverrideRepo = toppingOverrideRepo;
    _drinkItemRepo = drinkItemRepo;
    _sugarRepo = sugarRepo;
    _toppingRepo = toppingRepo;
    _iceRepo = iceRepo;
    _sizeRepo = sizeRepo;
    _enabledSugarRepo = enabledSugarRepo;
    _enabledIceRepo = enabledIceRepo;
    _enabledToppingRepo = enabledToppingRepo;
    _enabledSizeRepo = enabledSizeRepo;
    _imageService = imageService;
  }

  // ==================== Shop CRUD ====================

  public async Task<ApiResponse<PaginationList<ShopListResponse>>> GetList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword, int? status)
  {
    Expression<Func<Shop, bool>> predicate = x => !x.IsDeleted;

    if (!string.IsNullOrWhiteSpace(keyword))
      predicate = x => !x.IsDeleted && x.Name.Contains(keyword);

    if (status.HasValue)
    {
      var statusEnum = (ShopStatus)status.Value;
      if (!string.IsNullOrWhiteSpace(keyword))
        predicate = x => !x.IsDeleted && x.Name.Contains(keyword) && x.Status == statusEnum;
      else
        predicate = x => !x.IsDeleted && x.Status == statusEnum;
    }

    var result = await _shopRepo.GetPaginationList(
      page, pageSize,
      predicate: predicate,
      include: q => q.Include(s => s.Categories).ThenInclude(c => c.MenuItems.Where(m => !m.IsDeleted)),
      order: q => BuildShopOrder(q, sortBy, sortOrder));

    var mapped = new PaginationList<ShopListResponse>
    {
      Items = result.Items.Select(s =>
      {
        var response = s.ToShopListResponse();
        response.CategoryCount = s.Categories.Count;
        response.MenuItemCount = s.Categories.Sum(c => c.MenuItems.Count);
        return response;
      }).ToList(),
      Total = result.Total,
      Page = result.Page,
      PageSize = result.PageSize
    };

    return Success(mapped);
  }

  public async Task<ApiResponse<ShopDetailResponse>> GetById(int id)
  {
    var entity = await _shopRepo.Get(predicate: x => x.Id == id && !x.IsDeleted);

    if (entity is null)
      return Fail<ShopDetailResponse>(ErrorCodes.ShopNotFound, "店家不存在");

    return Success(entity.ToShopDetailResponse());
  }

  public async Task<ApiResponse<ShopDetailResponse>> Create(CreateShopRequest request)
  {
    if (await _shopRepo.Any(x => x.Name == request.Name && !x.IsDeleted))
      return Fail<ShopDetailResponse>(
        ErrorCodes.ShopAlreadyExists, "店家名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["店家名稱已存在"] });

    var entity = new Shop
    {
      Name = request.Name,
      Phone = request.Phone,
      Address = request.Address,
      Note = request.Note,
      CoverImagePath = request.CoverImagePath,
      ExternalUrl = NormalizeExternalUrl(request.ExternalUrl),
      Status = (ShopStatus)request.Status,
      Sort = request.Sort,
      MaxToppingPerItem = request.MaxToppingPerItem
    };
    await _shopRepo.Insert(entity);

    return Success((await _shopRepo.GetById(entity.Id))!.ToShopDetailResponse());
  }

  public async Task<ApiResponse<ShopDetailResponse>> Update(int id, UpdateShopRequest request)
  {
    var entity = await _shopRepo.Get(predicate: x => x.Id == id && !x.IsDeleted, tracking: true);

    if (entity is null)
      return Fail<ShopDetailResponse>(ErrorCodes.ShopNotFound, "店家不存在");

    if (await _shopRepo.Any(x => x.Name == request.Name && !x.IsDeleted && x.Id != id))
      return Fail<ShopDetailResponse>(
        ErrorCodes.ShopAlreadyExists, "店家名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["店家名稱已存在"] });

    entity.Name = request.Name;
    entity.Phone = request.Phone;
    entity.Address = request.Address;
    entity.Note = request.Note;
    entity.CoverImagePath = request.CoverImagePath;
    entity.ExternalUrl = NormalizeExternalUrl(request.ExternalUrl);
    entity.Status = (ShopStatus)request.Status;
    entity.Sort = request.Sort;
    entity.MaxToppingPerItem = request.MaxToppingPerItem;
    await _shopRepo.Update(entity);

    return Success((await _shopRepo.GetById(id))!.ToShopDetailResponse());
  }

  public async Task<ApiResponse<ShopCoverImageUploadResponse>> UploadCoverImage(int shopId, IFormFile file, CancellationToken ct = default)
  {
    var entity = await _shopRepo.Get(predicate: x => x.Id == shopId && !x.IsDeleted, tracking: true);
    if (entity is null)
      return Fail<ShopCoverImageUploadResponse>(ErrorCodes.ShopNotFound, "店家不存在");

    var uploadResp = await _imageService.ForwardToImageUpload(file, ct);
    if (uploadResp.Code != 0 || uploadResp.Data == null)
    {
      return new ApiResponse<ShopCoverImageUploadResponse>
      {
        Data = null,
        Code = uploadResp.Code,
        Error = uploadResp.Error,
        Message = uploadResp.Message,
        Errors = uploadResp.Errors,
      };
    }

    entity.CoverImagePath = uploadResp.Data.Path;
    await _shopRepo.Update(entity);

    return Success(new ShopCoverImageUploadResponse { CoverImagePath = uploadResp.Data.Path });
  }

  private static string? NormalizeExternalUrl(string? value)
      => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

  public async Task<ApiResponse> Delete(int id)
  {
    var entity = await _shopRepo.Get(predicate: x => x.Id == id && !x.IsDeleted, tracking: true);

    if (entity is null)
      return Fail(ErrorCodes.ShopNotFound, "店家不存在");

    entity.IsDeleted = true;
    entity.DeletedAt = DateTime.UtcNow;
    await _shopRepo.Update(entity);

    // 底下品項一併 soft delete
    await _menuItemRepo.ExecuteUpdate(
      x => x.Category.ShopId == id && !x.IsDeleted,
      s => s.SetProperty(x => x.IsDeleted, true).SetProperty(x => x.DeletedAt, DateTime.UtcNow));

    return Success();
  }

  public async Task<ApiResponse> BatchSort(BatchSortRequest request)
  {
    var ids = request.Items.Select(x => x.Id).ToList();
    var entities = await _shopRepo.GetList(predicate: x => ids.Contains(x.Id) && !x.IsDeleted, tracking: true);

    if (entities.Count != ids.Count)
      return Fail(ErrorCodes.ShopNotFound, "部分店家不存在");

    var sortMap = request.Items.ToDictionary(x => x.Id, x => x.Sort);
    foreach (var entity in entities)
      entity.Sort = sortMap[entity.Id];

    await _shopRepo.UpdateRange(entities);
    return Success();
  }

  public async Task<ApiResponse> BatchDelete(BatchDeleteRequest request)
  {
    var entities = await _shopRepo.GetList(predicate: x => request.Ids.Contains(x.Id) && !x.IsDeleted, tracking: true);

    if (entities.Count != request.Ids.Count)
      return Fail(ErrorCodes.ShopNotFound, "部分店家不存在");

    foreach (var entity in entities)
    {
      entity.IsDeleted = true;
      entity.DeletedAt = DateTime.UtcNow;
    }
    await _shopRepo.UpdateRange(entities);

    // 底下品項一併 soft delete
    await _menuItemRepo.ExecuteUpdate(
      x => request.Ids.Contains(x.Category.ShopId) && !x.IsDeleted,
      s => s.SetProperty(x => x.IsDeleted, true).SetProperty(x => x.DeletedAt, DateTime.UtcNow));

    return Success();
  }

  // ==================== Menu Management ====================

  public async Task<ApiResponse<AdminShopMenuResponse>> GetMenu(int shopId)
  {
    if (!await _shopRepo.Any(x => x.Id == shopId && !x.IsDeleted))
      return Fail<AdminShopMenuResponse>(ErrorCodes.ShopNotFound, "店家不存在");

    var categories = await _categoryRepo.GetList(
      predicate: x => x.ShopId == shopId,
      include: q => q
        .Include(c => c.MenuItems.Where(m => !m.IsDeleted))
          .ThenInclude(m => m.DrinkItem)
        .Include(c => c.MenuItems.Where(m => !m.IsDeleted))
          .ThenInclude(m => m.Sizes).ThenInclude(s => s.Size)
        .Include(c => c.MenuItems.Where(m => !m.IsDeleted))
          .ThenInclude(m => m.Sugars)
        .Include(c => c.MenuItems.Where(m => !m.IsDeleted))
          .ThenInclude(m => m.Ices)
        .Include(c => c.MenuItems.Where(m => !m.IsDeleted))
          .ThenInclude(m => m.Toppings),
      order: q => q.OrderBy(c => c.Sort).ThenBy(c => c.Id),
      splitQuery: true);

    var sugarOverrides = await _sugarOverrideRepo.GetList(
      predicate: x => x.ShopId == shopId,
      include: q => q.Include(o => o.Sugar));

    var toppingOverrides = await _toppingOverrideRepo.GetList(
      predicate: x => x.ShopId == shopId,
      include: q => q.Include(o => o.Topping));

    var enabledSugars = await _enabledSugarRepo.GetList(
      predicate: x => x.ShopId == shopId,
      include: q => q.Include(e => e.Sugar),
      order: q => q.OrderBy(e => e.Sort).ThenBy(e => e.Id));
    var enabledIces = await _enabledIceRepo.GetList(
      predicate: x => x.ShopId == shopId,
      include: q => q.Include(e => e.Ice),
      order: q => q.OrderBy(e => e.Sort).ThenBy(e => e.Id));
    var enabledToppings = await _enabledToppingRepo.GetList(
      predicate: x => x.ShopId == shopId,
      include: q => q.Include(e => e.Topping),
      order: q => q.OrderBy(e => e.Sort).ThenBy(e => e.Id));
    var enabledSizes = await _enabledSizeRepo.GetList(
      predicate: x => x.ShopId == shopId,
      include: q => q.Include(e => e.Size),
      order: q => q.OrderBy(e => e.Sort).ThenBy(e => e.Id));

    var response = new AdminShopMenuResponse
    {
      Categories = categories.Select(c => new AdminShopMenuCategoryResponse
      {
        Id = c.Id,
        Name = c.Name,
        Sort = c.Sort,
        Items = c.MenuItems.OrderBy(m => m.Sort).ThenBy(m => m.Id).Select(m => new AdminShopMenuItemResponse
        {
          Id = m.Id,
          DrinkItemId = m.DrinkItemId,
          DrinkItemName = m.DrinkItem.Name,
          Description = m.Description,
          Sort = m.Sort,
          MaxToppingCount = m.MaxToppingCount,
          Sizes = m.Sizes.Select(s => new AdminShopMenuItemSizeResponse
          {
            SizeId = s.SizeId,
            SizeName = s.Size.Name,
            Price = s.Price
          }).ToList(),
          SugarIds = m.Sugars.Select(s => s.SugarId).ToList(),
          IceIds = m.Ices.Select(i => i.IceId).ToList(),
          ToppingIds = m.Toppings.Select(t => t.ToppingId).ToList()
        }).ToList()
      }).ToList(),
      SugarOverrides = sugarOverrides.Select(o => new AdminShopMenuSugarOverrideResponse
      {
        SugarId = o.SugarId,
        SugarName = o.Sugar.Name,
        Price = o.Price
      }).ToList(),
      ToppingOverrides = toppingOverrides.Select(o => new AdminShopMenuToppingOverrideResponse
      {
        ToppingId = o.ToppingId,
        ToppingName = o.Topping.Name,
        Price = o.Price
      }).ToList(),
      EnabledSugars = enabledSugars.Select(e => new AdminShopMenuOptionItem
      { Id = e.SugarId, Name = e.Sugar.Name, Sort = e.Sort }).ToList(),
      EnabledIces = enabledIces.Select(e => new AdminShopMenuOptionItem
      { Id = e.IceId, Name = e.Ice.Name, Sort = e.Sort }).ToList(),
      EnabledToppings = enabledToppings.Select(e => new AdminShopMenuOptionItem
      { Id = e.ToppingId, Name = e.Topping.Name, Sort = e.Sort }).ToList(),
      EnabledSizes = enabledSizes.Select(e => new AdminShopMenuOptionItem
      { Id = e.SizeId, Name = e.Size.Name, Sort = e.Sort }).ToList()
    };

    return Success(response);
  }

  // --- Category ---

  public async Task<ApiResponse<int>> CreateCategory(int shopId, CreateShopCategoryRequest request)
  {
    if (!await _shopRepo.Any(x => x.Id == shopId && !x.IsDeleted))
      return Fail<int>(ErrorCodes.ShopNotFound, "店家不存在");

    if (await _categoryRepo.Any(x => x.ShopId == shopId && x.Name == request.Name))
      return Fail<int>(
        ErrorCodes.CategoryAlreadyExists, "分類名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["分類名稱已存在"] });

    var entity = new ShopCategory
    {
      ShopId = shopId,
      Name = request.Name,
      Sort = request.Sort
    };
    await _categoryRepo.Insert(entity);

    return Success(entity.Id);
  }

  public async Task<ApiResponse> UpdateCategory(int shopId, int categoryId, UpdateShopCategoryRequest request)
  {
    var entity = await _categoryRepo.Get(
      predicate: x => x.Id == categoryId && x.ShopId == shopId, tracking: true);

    if (entity is null)
      return Fail(ErrorCodes.CategoryNotFound, "分類不存在");

    if (await _categoryRepo.Any(x => x.ShopId == shopId && x.Name == request.Name && x.Id != categoryId))
      return Fail(
        ErrorCodes.CategoryAlreadyExists, "分類名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["分類名稱已存在"] });

    entity.Name = request.Name;
    entity.Sort = request.Sort;
    await _categoryRepo.Update(entity);

    return Success();
  }

  public async Task<ApiResponse> DeleteCategory(int shopId, int categoryId)
  {
    var entity = await _categoryRepo.Get(
      predicate: x => x.Id == categoryId && x.ShopId == shopId);

    if (entity is null)
      return Fail(ErrorCodes.CategoryNotFound, "分類不存在");

    // 受影響的品項 (用於 cascade orphan ShopImage)
    var affectedDrinkItemIds = await _menuItemRepo.Query
      .Where(m => m.CategoryId == categoryId && !m.IsDeleted)
      .Select(m => m.DrinkItemId)
      .ToListAsync();

    await _categoryRepo.ExecuteInTransaction(async () =>
    {
      // 底下品項 soft delete
      await _menuItemRepo.ExecuteUpdate(
        x => x.CategoryId == categoryId && !x.IsDeleted,
        s => s.SetProperty(x => x.IsDeleted, true).SetProperty(x => x.DeletedAt, DateTime.UtcNow));

      // Cascade orphan ShopImage
      if (affectedDrinkItemIds.Count > 0)
        await _imageService.CascadeOrphan(shopId, affectedDrinkItemIds);

      await _categoryRepo.DeleteById(categoryId);
    }, "刪除分類失敗");
    return Success();
  }

  public async Task<ApiResponse> BatchSortCategories(int shopId, BatchSortRequest request)
  {
    var ids = request.Items.Select(x => x.Id).ToList();
    var entities = await _categoryRepo.GetList(
      predicate: x => x.ShopId == shopId && ids.Contains(x.Id), tracking: true);

    if (entities.Count != ids.Count)
      return Fail(ErrorCodes.CategoryNotFound, "部分分類不存在");

    var sortMap = request.Items.ToDictionary(x => x.Id, x => x.Sort);
    foreach (var entity in entities)
      entity.Sort = sortMap[entity.Id];

    await _categoryRepo.UpdateRange(entities);
    return Success();
  }

  // --- MenuItem ---

  public async Task<ApiResponse<int>> CreateMenuItem(int shopId, int categoryId, CreateShopMenuItemRequest request)
  {
    if (!await _categoryRepo.Any(x => x.Id == categoryId && x.ShopId == shopId))
      return Fail<int>(ErrorCodes.CategoryNotFound, "分類不存在");

    var enabledErrors = await ValidateMenuItemOptionsEnabled(
      shopId, request.SugarIds, request.IceIds, request.ToppingIds,
      request.Sizes.Select(s => s.SizeId).ToList());
    if (enabledErrors is not null)
      return Fail<int>(ErrorCodes.OptionNotEnabledForShop, "選項未在此店家啟用", enabledErrors);

    // Resolve DrinkItem
    int drinkItemId;
    if (request.DrinkItemId.HasValue)
    {
      if (!await _drinkItemRepo.Any(x => x.Id == request.DrinkItemId.Value))
        return Fail<int>(ErrorCodes.DrinkItemNotFound, "品名不存在");
      drinkItemId = request.DrinkItemId.Value;
    }
    else if (!string.IsNullOrWhiteSpace(request.DrinkItemName))
    {
      var existing = await _drinkItemRepo.Get(
        predicate: x => x.Name.ToLower() == request.DrinkItemName.ToLower());
      if (existing is not null)
      {
        drinkItemId = existing.Id;
      }
      else
      {
        var maxSort = await _drinkItemRepo.Any()
          ? (await _drinkItemRepo.GetList(order: q => q.OrderByDescending(x => x.Sort))).FirstOrDefault()?.Sort ?? 0
          : 0;
        var newDrinkItem = new DrinkItem { Name = request.DrinkItemName, Sort = maxSort + 1 };
        await _drinkItemRepo.Insert(newDrinkItem);
        drinkItemId = newDrinkItem.Id;
      }
    }
    else
    {
      return Fail<int>(ErrorCodes.DrinkItemNotFound, "必須提供品名 ID 或品名名稱");
    }

    var menuItem = new ShopMenuItem
    {
      CategoryId = categoryId,
      DrinkItemId = drinkItemId,
      Description = request.Description,
      Sort = request.Sort,
      MaxToppingCount = request.MaxToppingCount
    };
    await _menuItemRepo.Insert(menuItem);

    // Insert sub-relations
    await InsertMenuItemSubRelations(menuItem.Id, request.Sizes, request.SugarIds, request.IceIds, request.ToppingIds);

    return Success(menuItem.Id);
  }

  public async Task<ApiResponse> UpdateMenuItem(int shopId, int categoryId, int itemId, UpdateShopMenuItemRequest request)
  {
    var entity = await _menuItemRepo.Get(
      predicate: x => x.Id == itemId && x.CategoryId == categoryId && x.Category.ShopId == shopId && !x.IsDeleted,
      tracking: true);

    if (entity is null)
      return Fail(ErrorCodes.MenuItemNotFound, "品項不存在");

    var enabledErrors = await ValidateMenuItemOptionsEnabled(
      shopId, request.SugarIds, request.IceIds, request.ToppingIds,
      request.Sizes.Select(s => s.SizeId).ToList());
    if (enabledErrors is not null)
      return Fail(ErrorCodes.OptionNotEnabledForShop, "選項未在此店家啟用", enabledErrors);

    // Resolve DrinkItem
    if (request.DrinkItemId.HasValue)
    {
      if (!await _drinkItemRepo.Any(x => x.Id == request.DrinkItemId.Value))
        return Fail(ErrorCodes.DrinkItemNotFound, "品名不存在");
      entity.DrinkItemId = request.DrinkItemId.Value;
    }
    else if (!string.IsNullOrWhiteSpace(request.DrinkItemName))
    {
      var existing = await _drinkItemRepo.Get(
        predicate: x => x.Name.ToLower() == request.DrinkItemName.ToLower());
      if (existing is not null)
      {
        entity.DrinkItemId = existing.Id;
      }
      else
      {
        var maxSort = await _drinkItemRepo.Any()
          ? (await _drinkItemRepo.GetList(order: q => q.OrderByDescending(x => x.Sort))).FirstOrDefault()?.Sort ?? 0
          : 0;
        var newDrinkItem = new DrinkItem { Name = request.DrinkItemName, Sort = maxSort + 1 };
        await _drinkItemRepo.Insert(newDrinkItem);
        entity.DrinkItemId = newDrinkItem.Id;
      }
    }

    entity.Description = request.Description;
    entity.Sort = request.Sort;
    entity.MaxToppingCount = request.MaxToppingCount;
    await _menuItemRepo.Update(entity);

    // Delete-then-insert sub-relations
    await _menuItemSizeRepo.ExecuteDelete(x => x.MenuItemId == itemId);
    await _menuItemSugarRepo.ExecuteDelete(x => x.MenuItemId == itemId);
    await _menuItemIceRepo.ExecuteDelete(x => x.MenuItemId == itemId);
    await _menuItemToppingRepo.ExecuteDelete(x => x.MenuItemId == itemId);

    await InsertMenuItemSubRelations(itemId, request.Sizes, request.SugarIds, request.IceIds, request.ToppingIds);

    return Success();
  }

  public async Task<ApiResponse> DeleteMenuItem(int shopId, int categoryId, int itemId)
  {
    var entity = await _menuItemRepo.Get(
      predicate: x => x.Id == itemId && x.CategoryId == categoryId && x.Category.ShopId == shopId && !x.IsDeleted,
      tracking: true);

    if (entity is null)
      return Fail(ErrorCodes.MenuItemNotFound, "品項不存在");

    var drinkItemId = entity.DrinkItemId;

    await _menuItemRepo.ExecuteInTransaction(async () =>
    {
      entity.IsDeleted = true;
      entity.DeletedAt = DateTime.UtcNow;
      await _menuItemRepo.Update(entity);

      await _imageService.CascadeOrphan(shopId, new[] { drinkItemId });
    }, "刪除品項失敗");

    return Success();
  }

  public async Task<ApiResponse> BatchSortMenuItems(int shopId, int categoryId, BatchSortRequest request)
  {
    if (!await _categoryRepo.Any(x => x.Id == categoryId && x.ShopId == shopId))
      return Fail(ErrorCodes.CategoryNotFound, "分類不存在");

    var ids = request.Items.Select(x => x.Id).ToList();
    var entities = await _menuItemRepo.GetList(
      predicate: x => x.CategoryId == categoryId && ids.Contains(x.Id) && !x.IsDeleted,
      tracking: true);

    if (entities.Count != ids.Count)
      return Fail(ErrorCodes.MenuItemNotFound, "部分品項不存在");

    var sortMap = request.Items.ToDictionary(x => x.Id, x => x.Sort);
    foreach (var entity in entities)
      entity.Sort = sortMap[entity.Id];

    await _menuItemRepo.UpdateRange(entities);
    return Success();
  }

  // ==================== Override ====================

  public async Task<ApiResponse<ShopOverrideResponse>> GetOverrides(int shopId)
  {
    if (!await _shopRepo.Any(x => x.Id == shopId && !x.IsDeleted))
      return Fail<ShopOverrideResponse>(ErrorCodes.ShopNotFound, "店家不存在");

    var allSugars = await _sugarRepo.GetList(order: q => q.OrderBy(x => x.Sort).ThenBy(x => x.Id));
    var allToppings = await _toppingRepo.GetList(order: q => q.OrderBy(x => x.Sort).ThenBy(x => x.Id));

    var sugarOverrides = await _sugarOverrideRepo.GetList(predicate: x => x.ShopId == shopId);
    var toppingOverrides = await _toppingOverrideRepo.GetList(predicate: x => x.ShopId == shopId);

    var sugarOverrideMap = sugarOverrides.ToDictionary(x => x.SugarId);
    var toppingOverrideMap = toppingOverrides.ToDictionary(x => x.ToppingId);

    var response = new ShopOverrideResponse
    {
      SugarOverrides = allSugars.Select(s => new ShopSugarOverrideDetailResponse
      {
        SugarId = s.Id,
        SugarName = s.Name,
        DefaultPrice = s.DefaultPrice,
        OverridePrice = sugarOverrideMap.TryGetValue(s.Id, out var so) ? so.Price : null
      }).ToList(),
      ToppingOverrides = allToppings.Select(t => new ShopToppingOverrideDetailResponse
      {
        ToppingId = t.Id,
        ToppingName = t.Name,
        DefaultPrice = t.DefaultPrice,
        OverridePrice = toppingOverrideMap.TryGetValue(t.Id, out var to) ? to.Price : null
      }).ToList()
    };

    return Success(response);
  }

  public async Task<ApiResponse> UpdateOverrides(int shopId, UpdateShopOverrideRequest request)
  {
    if (!await _shopRepo.Any(x => x.Id == shopId && !x.IsDeleted))
      return Fail(ErrorCodes.ShopNotFound, "店家不存在");

    // Delete existing overrides
    await _sugarOverrideRepo.ExecuteDelete(x => x.ShopId == shopId);
    await _toppingOverrideRepo.ExecuteDelete(x => x.ShopId == shopId);

    // Insert new sugar overrides
    if (request.SugarOverrides.Count > 0)
    {
      var sugarEntities = request.SugarOverrides.Select(o => new ShopSugarOverride
      {
        ShopId = shopId,
        SugarId = o.SugarId,
        Price = o.Price
      });
      await _sugarOverrideRepo.InsertRange(sugarEntities);
    }

    // Insert new topping overrides
    if (request.ToppingOverrides.Count > 0)
    {
      var toppingEntities = request.ToppingOverrides.Select(o => new ShopToppingOverride
      {
        ShopId = shopId,
        ToppingId = o.ToppingId,
        Price = o.Price
      });
      await _toppingOverrideRepo.InsertRange(toppingEntities);
    }

    return Success();
  }

  // ==================== Private Helpers ====================

  private async Task InsertMenuItemSubRelations(
    int menuItemId, List<MenuItemSizeRequest> sizes, List<int> sugarIds, List<int> iceIds, List<int> toppingIds)
  {
    if (sizes.Count > 0)
    {
      await _menuItemSizeRepo.InsertRange(sizes.Select(s => new ShopMenuItemSize
      {
        MenuItemId = menuItemId,
        SizeId = s.SizeId,
        Price = s.Price
      }));
    }

    if (sugarIds.Count > 0)
    {
      await _menuItemSugarRepo.InsertRange(sugarIds.Select(id => new ShopMenuItemSugar
      {
        MenuItemId = menuItemId,
        SugarId = id
      }));
    }

    if (iceIds.Count > 0)
    {
      await _menuItemIceRepo.InsertRange(iceIds.Select(id => new ShopMenuItemIce
      {
        MenuItemId = menuItemId,
        IceId = id
      }));
    }

    if (toppingIds.Count > 0)
    {
      await _menuItemToppingRepo.InsertRange(toppingIds.Select(id => new ShopMenuItemTopping
      {
        MenuItemId = menuItemId,
        ToppingId = id
      }));
    }
  }

  private async Task<Dictionary<string, string[]>?> ValidateMenuItemOptionsEnabled(
    int shopId, List<int> sugarIds, List<int> iceIds, List<int> toppingIds, List<int> sizeIds)
  {
    var errors = new Dictionary<string, string[]>();

    if (sugarIds.Count > 0)
    {
      var enabled = await _enabledSugarRepo.Query
        .Where(x => x.ShopId == shopId && sugarIds.Contains(x.SugarId))
        .Select(x => x.SugarId).ToListAsync();
      var missing = sugarIds.Except(enabled).ToList();
      if (missing.Count > 0)
        errors["sugar_ids"] = [$"以下糖度未在此店家啟用：{string.Join(",", missing)}"];
    }
    if (iceIds.Count > 0)
    {
      var enabled = await _enabledIceRepo.Query
        .Where(x => x.ShopId == shopId && iceIds.Contains(x.IceId))
        .Select(x => x.IceId).ToListAsync();
      var missing = iceIds.Except(enabled).ToList();
      if (missing.Count > 0)
        errors["ice_ids"] = [$"以下冰塊未在此店家啟用：{string.Join(",", missing)}"];
    }
    if (toppingIds.Count > 0)
    {
      var enabled = await _enabledToppingRepo.Query
        .Where(x => x.ShopId == shopId && toppingIds.Contains(x.ToppingId))
        .Select(x => x.ToppingId).ToListAsync();
      var missing = toppingIds.Except(enabled).ToList();
      if (missing.Count > 0)
        errors["topping_ids"] = [$"以下加料未在此店家啟用：{string.Join(",", missing)}"];
    }
    if (sizeIds.Count > 0)
    {
      var enabled = await _enabledSizeRepo.Query
        .Where(x => x.ShopId == shopId && sizeIds.Contains(x.SizeId))
        .Select(x => x.SizeId).ToListAsync();
      var missing = sizeIds.Except(enabled).ToList();
      if (missing.Count > 0)
        errors["sizes"] = [$"以下尺寸未在此店家啟用：{string.Join(",", missing)}"];
    }

    return errors.Count > 0 ? errors : null;
  }

  private static IQueryable<Shop> BuildShopOrder(IQueryable<Shop> query, string? sortBy, string? sortOrder)
  {
    var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
    var ordered = sortBy?.ToLower() switch
    {
      "id" => isDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
      "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
      "created_at" => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
      _ => isDesc ? query.OrderByDescending(x => x.Sort) : query.OrderBy(x => x.Sort)
    };
    return sortBy?.ToLower() is "sort" or null
      ? ordered.ThenBy(x => x.Id).ThenByDescending(x => x.CreatedAt)
      : ordered.ThenBy(x => x.Id);
  }
}
