using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Drink.Application.Services;

public class AdminShopOptionService : BaseService
{
  private readonly IGenericRepository<Shop> _shopRepo;
  private readonly IGenericRepository<Sugar> _sugarRepo;
  private readonly IGenericRepository<Ice> _iceRepo;
  private readonly IGenericRepository<Topping> _toppingRepo;
  private readonly IGenericRepository<Size> _sizeRepo;
  private readonly IGenericRepository<ShopEnabledSugar> _enabledSugarRepo;
  private readonly IGenericRepository<ShopEnabledIce> _enabledIceRepo;
  private readonly IGenericRepository<ShopEnabledTopping> _enabledToppingRepo;
  private readonly IGenericRepository<ShopEnabledSize> _enabledSizeRepo;
  private readonly IGenericRepository<ShopMenuItem> _menuItemRepo;
  private readonly IGenericRepository<ShopMenuItemSugar> _menuItemSugarRepo;
  private readonly IGenericRepository<ShopMenuItemIce> _menuItemIceRepo;
  private readonly IGenericRepository<ShopMenuItemTopping> _menuItemToppingRepo;
  private readonly IGenericRepository<ShopMenuItemSize> _menuItemSizeRepo;

  public AdminShopOptionService(
    ICurrentUserContext currentUser,
    IGenericRepository<Shop> shopRepo,
    IGenericRepository<Sugar> sugarRepo,
    IGenericRepository<Ice> iceRepo,
    IGenericRepository<Topping> toppingRepo,
    IGenericRepository<Size> sizeRepo,
    IGenericRepository<ShopEnabledSugar> enabledSugarRepo,
    IGenericRepository<ShopEnabledIce> enabledIceRepo,
    IGenericRepository<ShopEnabledTopping> enabledToppingRepo,
    IGenericRepository<ShopEnabledSize> enabledSizeRepo,
    IGenericRepository<ShopMenuItem> menuItemRepo,
    IGenericRepository<ShopMenuItemSugar> menuItemSugarRepo,
    IGenericRepository<ShopMenuItemIce> menuItemIceRepo,
    IGenericRepository<ShopMenuItemTopping> menuItemToppingRepo,
    IGenericRepository<ShopMenuItemSize> menuItemSizeRepo) : base(currentUser)
  {
    _shopRepo = shopRepo;
    _sugarRepo = sugarRepo;
    _iceRepo = iceRepo;
    _toppingRepo = toppingRepo;
    _sizeRepo = sizeRepo;
    _enabledSugarRepo = enabledSugarRepo;
    _enabledIceRepo = enabledIceRepo;
    _enabledToppingRepo = enabledToppingRepo;
    _enabledSizeRepo = enabledSizeRepo;
    _menuItemRepo = menuItemRepo;
    _menuItemSugarRepo = menuItemSugarRepo;
    _menuItemIceRepo = menuItemIceRepo;
    _menuItemToppingRepo = menuItemToppingRepo;
    _menuItemSizeRepo = menuItemSizeRepo;
  }

  public async Task<ApiResponse<ShopOptionsResponse>> GetOptions(int shopId)
  {
    if (!await _shopRepo.Any(x => x.Id == shopId && !x.IsDeleted))
      return Fail<ShopOptionsResponse>(ErrorCodes.ShopNotFound, "店家不存在");

    var allSugars = await _sugarRepo.GetList(order: q => q.OrderBy(x => x.Sort).ThenBy(x => x.Id));
    var allIces = await _iceRepo.GetList(order: q => q.OrderBy(x => x.Sort).ThenBy(x => x.Id));
    var allToppings = await _toppingRepo.GetList(order: q => q.OrderBy(x => x.Sort).ThenBy(x => x.Id));
    var allSizes = await _sizeRepo.GetList(order: q => q.OrderBy(x => x.Sort).ThenBy(x => x.Id));

    var enabledSugars = (await _enabledSugarRepo.GetList(predicate: x => x.ShopId == shopId))
      .ToDictionary(x => x.SugarId);
    var enabledIces = (await _enabledIceRepo.GetList(predicate: x => x.ShopId == shopId))
      .ToDictionary(x => x.IceId);
    var enabledToppings = (await _enabledToppingRepo.GetList(predicate: x => x.ShopId == shopId))
      .ToDictionary(x => x.ToppingId);
    var enabledSizes = (await _enabledSizeRepo.GetList(predicate: x => x.ShopId == shopId))
      .ToDictionary(x => x.SizeId);

    var response = new ShopOptionsResponse
    {
      Sugars = allSugars.Select(s => new ShopSugarOptionItem
      {
        SugarId = s.Id,
        SugarName = s.Name,
        DefaultPrice = s.DefaultPrice,
        IsEnabled = enabledSugars.ContainsKey(s.Id),
        Sort = enabledSugars.TryGetValue(s.Id, out var es) ? es.Sort : 0
      }).ToList(),
      Ices = allIces.Select(i => new ShopIceOptionItem
      {
        IceId = i.Id,
        IceName = i.Name,
        IsEnabled = enabledIces.ContainsKey(i.Id),
        Sort = enabledIces.TryGetValue(i.Id, out var ei) ? ei.Sort : 0
      }).ToList(),
      Toppings = allToppings.Select(t => new ShopToppingOptionItem
      {
        ToppingId = t.Id,
        ToppingName = t.Name,
        DefaultPrice = t.DefaultPrice,
        IsEnabled = enabledToppings.ContainsKey(t.Id),
        Sort = enabledToppings.TryGetValue(t.Id, out var et) ? et.Sort : 0
      }).ToList(),
      Sizes = allSizes.Select(s => new ShopSizeOptionItem
      {
        SizeId = s.Id,
        SizeName = s.Name,
        IsEnabled = enabledSizes.ContainsKey(s.Id),
        Sort = enabledSizes.TryGetValue(s.Id, out var ez) ? ez.Sort : 0
      }).ToList()
    };

    return Success(response);
  }

  public async Task<ApiResponse<ShopOptionsPreviewResponse>> Preview(int shopId, UpdateShopOptionsRequest request)
  {
    if (!await _shopRepo.Any(x => x.Id == shopId && !x.IsDeleted))
      return Fail<ShopOptionsPreviewResponse>(ErrorCodes.ShopNotFound, "店家不存在");

    var validationErrors = await ValidateRequestIds(request);
    if (validationErrors is not null)
      return Fail<ShopOptionsPreviewResponse>(ErrorCodes.ValidationError, "選項 ID 無效", validationErrors);

    var newlyDisabled = await ComputeNewlyDisabled(shopId, request);
    var affected = await ComputeAffectedMenuItems(shopId, newlyDisabled);

    return Success(new ShopOptionsPreviewResponse
    {
      NewlyDisabled = newlyDisabled,
      AffectedMenuItems = affected,
      AffectedMenuItemsCount = affected.Count
    });
  }

  public async Task<ApiResponse<UpdateShopOptionsResponse>> Update(int shopId, UpdateShopOptionsRequest request)
  {
    if (!await _shopRepo.Any(x => x.Id == shopId && !x.IsDeleted))
      return Fail<UpdateShopOptionsResponse>(ErrorCodes.ShopNotFound, "店家不存在");

    var validationErrors = await ValidateRequestIds(request);
    if (validationErrors is not null)
      return Fail<UpdateShopOptionsResponse>(ErrorCodes.ValidationError, "選項 ID 無效", validationErrors);

    var affectedCount = 0;

    await _shopRepo.ExecuteInTransaction(async () =>
    {
      var newlyDisabled = await ComputeNewlyDisabled(shopId, request);
      var affected = await ComputeAffectedMenuItems(shopId, newlyDisabled);
      affectedCount = affected.Count;

      // delete-then-insert 啟用清單
      await _enabledSugarRepo.ExecuteDelete(x => x.ShopId == shopId);
      await _enabledIceRepo.ExecuteDelete(x => x.ShopId == shopId);
      await _enabledToppingRepo.ExecuteDelete(x => x.ShopId == shopId);
      await _enabledSizeRepo.ExecuteDelete(x => x.ShopId == shopId);

      if (request.Sugars.Count > 0)
        await _enabledSugarRepo.InsertRange(request.Sugars.Select(s => new ShopEnabledSugar
        {
          ShopId = shopId,
          SugarId = s.SugarId,
          Sort = s.Sort
        }));

      if (request.Ices.Count > 0)
        await _enabledIceRepo.InsertRange(request.Ices.Select(i => new ShopEnabledIce
        {
          ShopId = shopId,
          IceId = i.IceId,
          Sort = i.Sort
        }));

      if (request.Toppings.Count > 0)
        await _enabledToppingRepo.InsertRange(request.Toppings.Select(t => new ShopEnabledTopping
        {
          ShopId = shopId,
          ToppingId = t.ToppingId,
          Sort = t.Sort
        }));

      if (request.Sizes.Count > 0)
        await _enabledSizeRepo.InsertRange(request.Sizes.Select(s => new ShopEnabledSize
        {
          ShopId = shopId,
          SizeId = s.SizeId,
          Sort = s.Sort
        }));

      // cascade 刪除新停用選項對應的 ShopMenuItem* row（限該店家底下）
      var menuItemIds = await _menuItemRepo.Query
        .Where(m => m.Category.ShopId == shopId && !m.IsDeleted)
        .Select(m => m.Id)
        .ToListAsync();

      if (menuItemIds.Count > 0)
      {
        if (newlyDisabled.SugarIds.Count > 0)
          await _menuItemSugarRepo.ExecuteDelete(x =>
            menuItemIds.Contains(x.MenuItemId) && newlyDisabled.SugarIds.Contains(x.SugarId));
        if (newlyDisabled.IceIds.Count > 0)
          await _menuItemIceRepo.ExecuteDelete(x =>
            menuItemIds.Contains(x.MenuItemId) && newlyDisabled.IceIds.Contains(x.IceId));
        if (newlyDisabled.ToppingIds.Count > 0)
          await _menuItemToppingRepo.ExecuteDelete(x =>
            menuItemIds.Contains(x.MenuItemId) && newlyDisabled.ToppingIds.Contains(x.ToppingId));
        if (newlyDisabled.SizeIds.Count > 0)
          await _menuItemSizeRepo.ExecuteDelete(x =>
            menuItemIds.Contains(x.MenuItemId) && newlyDisabled.SizeIds.Contains(x.SizeId));
      }
    }, "更新店家選項啟用清單失敗");

    return Success(new UpdateShopOptionsResponse { AffectedMenuItemsCount = affectedCount });
  }

  // ==================== Private Helpers ====================

  private async Task<Dictionary<string, string[]>?> ValidateRequestIds(UpdateShopOptionsRequest request)
  {
    var errors = new Dictionary<string, string[]>();

    var sugarIds = request.Sugars.Select(s => s.SugarId).ToList();
    var iceIds = request.Ices.Select(i => i.IceId).ToList();
    var toppingIds = request.Toppings.Select(t => t.ToppingId).ToList();
    var sizeIds = request.Sizes.Select(s => s.SizeId).ToList();

    if (sugarIds.Count != sugarIds.Distinct().Count())
      errors["sugars"] = ["sugars 內含重複 ID"];
    if (iceIds.Count != iceIds.Distinct().Count())
      errors["ices"] = ["ices 內含重複 ID"];
    if (toppingIds.Count != toppingIds.Distinct().Count())
      errors["toppings"] = ["toppings 內含重複 ID"];
    if (sizeIds.Count != sizeIds.Distinct().Count())
      errors["sizes"] = ["sizes 內含重複 ID"];

    if (sugarIds.Count > 0)
    {
      var existing = await _sugarRepo.Query.Where(x => sugarIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
      var missing = sugarIds.Except(existing).ToList();
      if (missing.Count > 0)
        errors["sugars"] = [$"以下 sugar_id 不存在：{string.Join(",", missing)}"];
    }
    if (iceIds.Count > 0)
    {
      var existing = await _iceRepo.Query.Where(x => iceIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
      var missing = iceIds.Except(existing).ToList();
      if (missing.Count > 0)
        errors["ices"] = [$"以下 ice_id 不存在：{string.Join(",", missing)}"];
    }
    if (toppingIds.Count > 0)
    {
      var existing = await _toppingRepo.Query.Where(x => toppingIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
      var missing = toppingIds.Except(existing).ToList();
      if (missing.Count > 0)
        errors["toppings"] = [$"以下 topping_id 不存在：{string.Join(",", missing)}"];
    }
    if (sizeIds.Count > 0)
    {
      var existing = await _sizeRepo.Query.Where(x => sizeIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
      var missing = sizeIds.Except(existing).ToList();
      if (missing.Count > 0)
        errors["sizes"] = [$"以下 size_id 不存在：{string.Join(",", missing)}"];
    }

    return errors.Count > 0 ? errors : null;
  }

  private async Task<ShopOptionsNewlyDisabledIds> ComputeNewlyDisabled(int shopId, UpdateShopOptionsRequest request)
  {
    var currentSugarIds = await _enabledSugarRepo.Query
      .Where(x => x.ShopId == shopId).Select(x => x.SugarId).ToListAsync();
    var currentIceIds = await _enabledIceRepo.Query
      .Where(x => x.ShopId == shopId).Select(x => x.IceId).ToListAsync();
    var currentToppingIds = await _enabledToppingRepo.Query
      .Where(x => x.ShopId == shopId).Select(x => x.ToppingId).ToListAsync();
    var currentSizeIds = await _enabledSizeRepo.Query
      .Where(x => x.ShopId == shopId).Select(x => x.SizeId).ToListAsync();

    var requestSugarIds = request.Sugars.Select(s => s.SugarId).ToHashSet();
    var requestIceIds = request.Ices.Select(i => i.IceId).ToHashSet();
    var requestToppingIds = request.Toppings.Select(t => t.ToppingId).ToHashSet();
    var requestSizeIds = request.Sizes.Select(s => s.SizeId).ToHashSet();

    return new ShopOptionsNewlyDisabledIds
    {
      SugarIds = currentSugarIds.Where(id => !requestSugarIds.Contains(id)).ToList(),
      IceIds = currentIceIds.Where(id => !requestIceIds.Contains(id)).ToList(),
      ToppingIds = currentToppingIds.Where(id => !requestToppingIds.Contains(id)).ToList(),
      SizeIds = currentSizeIds.Where(id => !requestSizeIds.Contains(id)).ToList()
    };
  }

  private async Task<List<ShopOptionsAffectedMenuItem>> ComputeAffectedMenuItems(int shopId, ShopOptionsNewlyDisabledIds newlyDisabled)
  {
    var hasAny = newlyDisabled.SugarIds.Count + newlyDisabled.IceIds.Count
                 + newlyDisabled.ToppingIds.Count + newlyDisabled.SizeIds.Count > 0;
    if (!hasAny) return [];

    var menuItems = await _menuItemRepo.Query
      .Where(m => m.Category.ShopId == shopId && !m.IsDeleted)
      .Include(m => m.DrinkItem)
      .Include(m => m.Sugars)
      .Include(m => m.Ices)
      .Include(m => m.Toppings)
      .Include(m => m.Sizes)
      .ToListAsync();

    var affected = new List<ShopOptionsAffectedMenuItem>();
    foreach (var item in menuItems)
    {
      var removedSugars = item.Sugars.Where(x => newlyDisabled.SugarIds.Contains(x.SugarId)).Select(x => x.SugarId).ToList();
      var removedIces = item.Ices.Where(x => newlyDisabled.IceIds.Contains(x.IceId)).Select(x => x.IceId).ToList();
      var removedToppings = item.Toppings.Where(x => newlyDisabled.ToppingIds.Contains(x.ToppingId)).Select(x => x.ToppingId).ToList();
      var removedSizes = item.Sizes.Where(x => newlyDisabled.SizeIds.Contains(x.SizeId)).Select(x => x.SizeId).ToList();

      if (removedSugars.Count + removedIces.Count + removedToppings.Count + removedSizes.Count == 0)
        continue;

      affected.Add(new ShopOptionsAffectedMenuItem
      {
        Id = item.Id,
        Name = item.DrinkItem.Name,
        RemovedOptions = new ShopOptionsRemovedOptions
        {
          Sugars = removedSugars,
          Ices = removedIces,
          Toppings = removedToppings,
          Sizes = removedSizes
        }
      });
    }
    return affected;
  }
}
