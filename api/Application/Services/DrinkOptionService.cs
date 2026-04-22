using System.Linq.Expressions;
using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Mappings;
using Drink.Application.Models;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;

namespace Drink.Application.Services;

public class DrinkOptionService : BaseService
{
  private readonly IGenericRepository<DrinkItem> _drinkItemRepo;
  private readonly IGenericRepository<Sugar> _sugarRepo;
  private readonly IGenericRepository<Ice> _iceRepo;
  private readonly IGenericRepository<Topping> _toppingRepo;
  private readonly IGenericRepository<Size> _sizeRepo;

  public DrinkOptionService(
    ICurrentUserContext currentUser,
    IGenericRepository<DrinkItem> drinkItemRepo,
    IGenericRepository<Sugar> sugarRepo,
    IGenericRepository<Ice> iceRepo,
    IGenericRepository<Topping> toppingRepo,
    IGenericRepository<Size> sizeRepo) : base(currentUser)
  {
    _drinkItemRepo = drinkItemRepo;
    _sugarRepo = sugarRepo;
    _iceRepo = iceRepo;
    _toppingRepo = toppingRepo;
    _sizeRepo = sizeRepo;
  }

  // ==================== DrinkItem ====================

  public async Task<ApiResponse<PaginationList<DrinkItemListResponse>>> GetDrinkItemList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword)
  {
    return await GetPaginatedList<DrinkItem, DrinkItemListResponse>(
      _drinkItemRepo,
      page, pageSize, sortBy, sortOrder, keyword,
      kw => x => x.Name.Contains(kw),
      BuildDrinkItemOrder,
      DrinkOptionMapper.ToDrinkItemListResponse);
  }

  public async Task<ApiResponse<DrinkItemDetailResponse>> GetDrinkItemById(int id)
  {
    return await GetDetailById<DrinkItem, DrinkItemDetailResponse>(
      _drinkItemRepo, id, ErrorCodes.DrinkItemNotFound, "品名不存在",
      DrinkOptionMapper.ToDrinkItemDetailResponse);
  }

  public async Task<ApiResponse<DrinkItemDetailResponse>> CreateDrinkItem(CreateDrinkItemRequest request)
  {
    if (await _drinkItemRepo.Any(x => x.Name == request.Name))
      return Fail<DrinkItemDetailResponse>(
        ErrorCodes.DrinkItemAlreadyExists, "品名已存在",
        new Dictionary<string, string[]> { ["name"] = ["品名已存在"] });

    var entity = new DrinkItem { Name = request.Name, Sort = request.Sort };
    await _drinkItemRepo.Insert(entity);

    return Success((await _drinkItemRepo.GetById(entity.Id))!.ToDrinkItemDetailResponse());
  }

  public async Task<ApiResponse<DrinkItemDetailResponse>> UpdateDrinkItem(int id, UpdateDrinkItemRequest request)
  {
    var entity = await _drinkItemRepo.GetById(id, tracking: true);

    if (entity is null)
      return Fail<DrinkItemDetailResponse>(ErrorCodes.DrinkItemNotFound, "品名不存在");

    if (await _drinkItemRepo.Any(x => x.Name == request.Name && x.Id != id))
      return Fail<DrinkItemDetailResponse>(
        ErrorCodes.DrinkItemAlreadyExists, "品名已存在",
        new Dictionary<string, string[]> { ["name"] = ["品名已存在"] });

    entity.Name = request.Name;
    entity.Sort = request.Sort;
    await _drinkItemRepo.Update(entity);

    return Success((await _drinkItemRepo.GetById(id))!.ToDrinkItemDetailResponse());
  }

  public async Task<ApiResponse> DeleteDrinkItem(int id)
  {
    // TODO: 店家菜單引用檢查（待 Shop spec 實作後加入）
    if (!await _drinkItemRepo.Any(x => x.Id == id))
      return Fail(ErrorCodes.DrinkItemNotFound, "品名不存在");

    await _drinkItemRepo.DeleteById(id);
    return Success();
  }

  public async Task<ApiResponse> BatchSortDrinkItems(BatchSortRequest request)
  {
    return await BatchSort(_drinkItemRepo, request, ErrorCodes.DrinkItemNotFound, "品名不存在");
  }

  public async Task<ApiResponse> BatchDeleteDrinkItems(BatchDeleteRequest request)
  {
    // TODO: 店家菜單引用檢查（待 Shop spec 實作後加入）
    return await BatchDelete(_drinkItemRepo, request, ErrorCodes.DrinkItemNotFound, "部分品名不存在");
  }

  // ==================== Sugar ====================

  public async Task<ApiResponse<PaginationList<SugarListResponse>>> GetSugarList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword)
  {
    return await GetPaginatedList<Sugar, SugarListResponse>(
      _sugarRepo,
      page, pageSize, sortBy, sortOrder, keyword,
      kw => x => x.Name.Contains(kw),
      BuildSugarOrder,
      DrinkOptionMapper.ToSugarListResponse);
  }

  public async Task<ApiResponse<SugarDetailResponse>> GetSugarById(int id)
  {
    return await GetDetailById<Sugar, SugarDetailResponse>(
      _sugarRepo, id, ErrorCodes.SugarNotFound, "甜度不存在",
      DrinkOptionMapper.ToSugarDetailResponse);
  }

  public async Task<ApiResponse<SugarDetailResponse>> CreateSugar(CreateSugarRequest request)
  {
    if (await _sugarRepo.Any(x => x.Name == request.Name))
      return Fail<SugarDetailResponse>(
        ErrorCodes.SugarAlreadyExists, "甜度名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["甜度名稱已存在"] });

    var entity = new Sugar { Name = request.Name, DefaultPrice = request.DefaultPrice, Sort = request.Sort };
    await _sugarRepo.Insert(entity);

    return Success((await _sugarRepo.GetById(entity.Id))!.ToSugarDetailResponse());
  }

  public async Task<ApiResponse<SugarDetailResponse>> UpdateSugar(int id, UpdateSugarRequest request)
  {
    var entity = await _sugarRepo.GetById(id, tracking: true);

    if (entity is null)
      return Fail<SugarDetailResponse>(ErrorCodes.SugarNotFound, "甜度不存在");

    if (await _sugarRepo.Any(x => x.Name == request.Name && x.Id != id))
      return Fail<SugarDetailResponse>(
        ErrorCodes.SugarAlreadyExists, "甜度名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["甜度名稱已存在"] });

    entity.Name = request.Name;
    entity.DefaultPrice = request.DefaultPrice;
    entity.Sort = request.Sort;
    await _sugarRepo.Update(entity);

    return Success((await _sugarRepo.GetById(id))!.ToSugarDetailResponse());
  }

  public async Task<ApiResponse> DeleteSugar(int id)
  {
    // TODO: 店家菜單引用檢查
    if (!await _sugarRepo.Any(x => x.Id == id))
      return Fail(ErrorCodes.SugarNotFound, "甜度不存在");

    await _sugarRepo.DeleteById(id);
    return Success();
  }

  public async Task<ApiResponse> BatchSortSugars(BatchSortRequest request)
  {
    return await BatchSort(_sugarRepo, request, ErrorCodes.SugarNotFound, "甜度不存在");
  }

  public async Task<ApiResponse> BatchDeleteSugars(BatchDeleteRequest request)
  {
    // TODO: 店家菜單引用檢查
    return await BatchDelete(_sugarRepo, request, ErrorCodes.SugarNotFound, "部分甜度不存在");
  }

  // ==================== Ice ====================

  public async Task<ApiResponse<PaginationList<IceListResponse>>> GetIceList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword)
  {
    return await GetPaginatedList<Ice, IceListResponse>(
      _iceRepo,
      page, pageSize, sortBy, sortOrder, keyword,
      kw => x => x.Name.Contains(kw),
      BuildIceOrder,
      DrinkOptionMapper.ToIceListResponse);
  }

  public async Task<ApiResponse<IceDetailResponse>> GetIceById(int id)
  {
    return await GetDetailById<Ice, IceDetailResponse>(
      _iceRepo, id, ErrorCodes.IceNotFound, "冰塊不存在",
      DrinkOptionMapper.ToIceDetailResponse);
  }

  public async Task<ApiResponse<IceDetailResponse>> CreateIce(CreateIceRequest request)
  {
    if (await _iceRepo.Any(x => x.Name == request.Name))
      return Fail<IceDetailResponse>(
        ErrorCodes.IceAlreadyExists, "冰塊名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["冰塊名稱已存在"] });

    var entity = new Ice { Name = request.Name, Sort = request.Sort };
    await _iceRepo.Insert(entity);

    return Success((await _iceRepo.GetById(entity.Id))!.ToIceDetailResponse());
  }

  public async Task<ApiResponse<IceDetailResponse>> UpdateIce(int id, UpdateIceRequest request)
  {
    var entity = await _iceRepo.GetById(id, tracking: true);

    if (entity is null)
      return Fail<IceDetailResponse>(ErrorCodes.IceNotFound, "冰塊不存在");

    if (await _iceRepo.Any(x => x.Name == request.Name && x.Id != id))
      return Fail<IceDetailResponse>(
        ErrorCodes.IceAlreadyExists, "冰塊名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["冰塊名稱已存在"] });

    entity.Name = request.Name;
    entity.Sort = request.Sort;
    await _iceRepo.Update(entity);

    return Success((await _iceRepo.GetById(id))!.ToIceDetailResponse());
  }

  public async Task<ApiResponse> DeleteIce(int id)
  {
    // TODO: 店家菜單引用檢查
    if (!await _iceRepo.Any(x => x.Id == id))
      return Fail(ErrorCodes.IceNotFound, "冰塊不存在");

    await _iceRepo.DeleteById(id);
    return Success();
  }

  public async Task<ApiResponse> BatchSortIces(BatchSortRequest request)
  {
    return await BatchSort(_iceRepo, request, ErrorCodes.IceNotFound, "冰塊不存在");
  }

  public async Task<ApiResponse> BatchDeleteIces(BatchDeleteRequest request)
  {
    // TODO: 店家菜單引用檢查
    return await BatchDelete(_iceRepo, request, ErrorCodes.IceNotFound, "部分冰塊不存在");
  }

  // ==================== Topping ====================

  public async Task<ApiResponse<PaginationList<ToppingListResponse>>> GetToppingList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword)
  {
    return await GetPaginatedList<Topping, ToppingListResponse>(
      _toppingRepo,
      page, pageSize, sortBy, sortOrder, keyword,
      kw => x => x.Name.Contains(kw),
      BuildToppingOrder,
      DrinkOptionMapper.ToToppingListResponse);
  }

  public async Task<ApiResponse<ToppingDetailResponse>> GetToppingById(int id)
  {
    return await GetDetailById<Topping, ToppingDetailResponse>(
      _toppingRepo, id, ErrorCodes.ToppingNotFound, "加料不存在",
      DrinkOptionMapper.ToToppingDetailResponse);
  }

  public async Task<ApiResponse<ToppingDetailResponse>> CreateTopping(CreateToppingRequest request)
  {
    if (await _toppingRepo.Any(x => x.Name == request.Name))
      return Fail<ToppingDetailResponse>(
        ErrorCodes.ToppingAlreadyExists, "加料名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["加料名稱已存在"] });

    var entity = new Topping { Name = request.Name, DefaultPrice = request.DefaultPrice, Sort = request.Sort };
    await _toppingRepo.Insert(entity);

    return Success((await _toppingRepo.GetById(entity.Id))!.ToToppingDetailResponse());
  }

  public async Task<ApiResponse<ToppingDetailResponse>> UpdateTopping(int id, UpdateToppingRequest request)
  {
    var entity = await _toppingRepo.GetById(id, tracking: true);

    if (entity is null)
      return Fail<ToppingDetailResponse>(ErrorCodes.ToppingNotFound, "加料不存在");

    if (await _toppingRepo.Any(x => x.Name == request.Name && x.Id != id))
      return Fail<ToppingDetailResponse>(
        ErrorCodes.ToppingAlreadyExists, "加料名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["加料名稱已存在"] });

    entity.Name = request.Name;
    entity.DefaultPrice = request.DefaultPrice;
    entity.Sort = request.Sort;
    await _toppingRepo.Update(entity);

    return Success((await _toppingRepo.GetById(id))!.ToToppingDetailResponse());
  }

  public async Task<ApiResponse> DeleteTopping(int id)
  {
    // TODO: 店家菜單引用檢查
    if (!await _toppingRepo.Any(x => x.Id == id))
      return Fail(ErrorCodes.ToppingNotFound, "加料不存在");

    await _toppingRepo.DeleteById(id);
    return Success();
  }

  public async Task<ApiResponse> BatchSortToppings(BatchSortRequest request)
  {
    return await BatchSort(_toppingRepo, request, ErrorCodes.ToppingNotFound, "加料不存在");
  }

  public async Task<ApiResponse> BatchDeleteToppings(BatchDeleteRequest request)
  {
    // TODO: 店家菜單引用檢查
    return await BatchDelete(_toppingRepo, request, ErrorCodes.ToppingNotFound, "部分加料不存在");
  }

  // ==================== Size ====================

  public async Task<ApiResponse<PaginationList<SizeListResponse>>> GetSizeList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword)
  {
    return await GetPaginatedList<Size, SizeListResponse>(
      _sizeRepo,
      page, pageSize, sortBy, sortOrder, keyword,
      kw => x => x.Name.Contains(kw),
      BuildSizeOrder,
      DrinkOptionMapper.ToSizeListResponse);
  }

  public async Task<ApiResponse<SizeDetailResponse>> GetSizeById(int id)
  {
    return await GetDetailById<Size, SizeDetailResponse>(
      _sizeRepo, id, ErrorCodes.SizeNotFound, "容量不存在",
      DrinkOptionMapper.ToSizeDetailResponse);
  }

  public async Task<ApiResponse<SizeDetailResponse>> CreateSize(CreateSizeRequest request)
  {
    if (await _sizeRepo.Any(x => x.Name == request.Name))
      return Fail<SizeDetailResponse>(
        ErrorCodes.SizeAlreadyExists, "容量名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["容量名稱已存在"] });

    var entity = new Size { Name = request.Name, Sort = request.Sort };
    await _sizeRepo.Insert(entity);

    return Success((await _sizeRepo.GetById(entity.Id))!.ToSizeDetailResponse());
  }

  public async Task<ApiResponse<SizeDetailResponse>> UpdateSize(int id, UpdateSizeRequest request)
  {
    var entity = await _sizeRepo.GetById(id, tracking: true);

    if (entity is null)
      return Fail<SizeDetailResponse>(ErrorCodes.SizeNotFound, "容量不存在");

    if (await _sizeRepo.Any(x => x.Name == request.Name && x.Id != id))
      return Fail<SizeDetailResponse>(
        ErrorCodes.SizeAlreadyExists, "容量名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["容量名稱已存在"] });

    entity.Name = request.Name;
    entity.Sort = request.Sort;
    await _sizeRepo.Update(entity);

    return Success((await _sizeRepo.GetById(id))!.ToSizeDetailResponse());
  }

  public async Task<ApiResponse> DeleteSize(int id)
  {
    // TODO: 店家菜單引用檢查
    if (!await _sizeRepo.Any(x => x.Id == id))
      return Fail(ErrorCodes.SizeNotFound, "容量不存在");

    await _sizeRepo.DeleteById(id);
    return Success();
  }

  public async Task<ApiResponse> BatchSortSizes(BatchSortRequest request)
  {
    return await BatchSort(_sizeRepo, request, ErrorCodes.SizeNotFound, "容量不存在");
  }

  public async Task<ApiResponse> BatchDeleteSizes(BatchDeleteRequest request)
  {
    // TODO: 店家菜單引用檢查
    return await BatchDelete(_sizeRepo, request, ErrorCodes.SizeNotFound, "部分容量不存在");
  }

  // ==================== Private Helpers ====================

  private static async Task<ApiResponse<TResponse>> GetDetailById<TEntity, TResponse>(
    IGenericRepository<TEntity> repo,
    int id, (int Code, string Error) notFoundCode, string notFoundMessage,
    Func<TEntity, TResponse> mapper)
    where TEntity : BaseDataEntity
  {
    var entity = await repo.GetById(id);

    if (entity is null)
      return ApiResponse<TResponse>.Fail(notFoundCode, notFoundMessage);

    return ApiResponse<TResponse>.Success(mapper(entity));
  }

  private static async Task<ApiResponse<PaginationList<TResponse>>> GetPaginatedList<TEntity, TResponse>(
    IGenericRepository<TEntity> repo,
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword,
    Func<string, Expression<Func<TEntity, bool>>> keywordFilterBuilder,
    Func<IQueryable<TEntity>, string?, string?, IQueryable<TEntity>> orderBuilder,
    Func<TEntity, TResponse> mapper)
    where TEntity : BaseDataEntity
    where TResponse : class
  {
    Expression<Func<TEntity, bool>>? predicate = null;
    if (!string.IsNullOrWhiteSpace(keyword))
      predicate = keywordFilterBuilder(keyword);

    var result = await repo.GetPaginationList(
      page, pageSize,
      predicate: predicate,
      order: q => orderBuilder(q, sortBy, sortOrder));

    var mapped = new PaginationList<TResponse>
    {
      Items = result.Items.Select(mapper).ToList(),
      Total = result.Total,
      Page = result.Page,
      PageSize = result.PageSize
    };

    return ApiResponse<PaginationList<TResponse>>.Success(mapped);
  }

  private static async Task<ApiResponse> BatchSort<TEntity>(
    IGenericRepository<TEntity> repo,
    BatchSortRequest request, (int Code, string Error) notFoundCode, string notFoundMessage)
    where TEntity : BaseDataEntity
  {
    var ids = request.Items.Select(x => x.Id).ToList();

    var existingCount = await repo.Count(x => ids.Contains(x.Id));
    if (existingCount != ids.Count)
      return ApiResponse.Fail(notFoundCode, notFoundMessage);

    var entities = await repo.GetList(predicate: x => ids.Contains(x.Id), tracking: true);
    var sortMap = request.Items.ToDictionary(x => x.Id, x => x.Sort);

    foreach (var entity in entities)
    {
      var sortProp = entity.GetType().GetProperty("Sort");
      sortProp?.SetValue(entity, sortMap[entity.Id]);
    }

    await repo.UpdateRange(entities);
    return ApiResponse.Success();
  }

  private static async Task<ApiResponse> BatchDelete<TEntity>(
    IGenericRepository<TEntity> repo,
    BatchDeleteRequest request, (int Code, string Error) notFoundCode, string notFoundMessage)
    where TEntity : BaseDataEntity
  {
    var existingCount = await repo.Count(x => request.Ids.Contains(x.Id));
    if (existingCount != request.Ids.Count)
      return ApiResponse.Fail(notFoundCode, notFoundMessage);

    await repo.ExecuteDelete(x => request.Ids.Contains(x.Id));
    return ApiResponse.Success();
  }

  // ==================== Order Builders ====================

  private static IQueryable<DrinkItem> BuildDrinkItemOrder(IQueryable<DrinkItem> query, string? sortBy, string? sortOrder)
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

  private static IQueryable<Sugar> BuildSugarOrder(IQueryable<Sugar> query, string? sortBy, string? sortOrder)
  {
    var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
    var ordered = sortBy?.ToLower() switch
    {
      "id" => isDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
      "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
      "default_price" => isDesc ? query.OrderByDescending(x => x.DefaultPrice) : query.OrderBy(x => x.DefaultPrice),
      "created_at" => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
      _ => isDesc ? query.OrderByDescending(x => x.Sort) : query.OrderBy(x => x.Sort)
    };
    return sortBy?.ToLower() is "sort" or null
      ? ordered.ThenBy(x => x.Id).ThenByDescending(x => x.CreatedAt)
      : ordered.ThenBy(x => x.Id);
  }

  private static IQueryable<Ice> BuildIceOrder(IQueryable<Ice> query, string? sortBy, string? sortOrder)
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

  private static IQueryable<Topping> BuildToppingOrder(IQueryable<Topping> query, string? sortBy, string? sortOrder)
  {
    var isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
    var ordered = sortBy?.ToLower() switch
    {
      "id" => isDesc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
      "name" => isDesc ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
      "default_price" => isDesc ? query.OrderByDescending(x => x.DefaultPrice) : query.OrderBy(x => x.DefaultPrice),
      "created_at" => isDesc ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
      _ => isDesc ? query.OrderByDescending(x => x.Sort) : query.OrderBy(x => x.Sort)
    };
    return sortBy?.ToLower() is "sort" or null
      ? ordered.ThenBy(x => x.Id).ThenByDescending(x => x.CreatedAt)
      : ordered.ThenBy(x => x.Id);
  }

  private static IQueryable<Size> BuildSizeOrder(IQueryable<Size> query, string? sortBy, string? sortOrder)
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
