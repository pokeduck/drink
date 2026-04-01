using System.Linq.Expressions;
using Drink.Application.Constants;
using Drink.Application.Mappings;
using Drink.Application.Requests.Admin;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin;
using Drink.Domain.Entities;
using Drink.Infrastructure.Extensions;

namespace Drink.Application.Services;

public class DrinkOptionService : BaseService
{
  public DrinkOptionService(IServiceProvider serviceProvider) : base(serviceProvider) { }

  // ==================== DrinkItem ====================

  public async Task<ApiResponse<PaginationExtension.PaginationList<DrinkItemListResponse>>> GetDrinkItemList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword)
  {
    return await GetPaginatedList<DrinkItem, DrinkItemListResponse>(
      page, pageSize, sortBy, sortOrder, keyword,
      kw => x => x.Name.Contains(kw),
      BuildDrinkItemOrder,
      DrinkOptionMapper.ToDrinkItemListResponse);
  }

  public async Task<ApiResponse<DrinkItemDetailResponse>> GetDrinkItemById(int id)
  {
    return await GetDetailById<DrinkItem, DrinkItemDetailResponse>(id, ErrorCodes.DrinkItemNotFound, "品名不存在",
      DrinkOptionMapper.ToDrinkItemDetailResponse);
  }

  public async Task<ApiResponse<DrinkItemDetailResponse>> CreateDrinkItem(CreateDrinkItemRequest request)
  {
    var repo = GetRepository<DrinkItem>();

    if (await repo.Any(x => x.Name == request.Name))
      return Fail<DrinkItemDetailResponse>(
        ErrorCodes.DrinkItemAlreadyExists, "品名已存在",
        new Dictionary<string, string[]> { ["name"] = ["品名已存在"] });

    var entity = new DrinkItem { Name = request.Name, Sort = request.Sort };
    await repo.Insert(entity);

    return Success((await repo.GetById(entity.Id))!.ToDrinkItemDetailResponse());
  }

  public async Task<ApiResponse<DrinkItemDetailResponse>> UpdateDrinkItem(int id, UpdateDrinkItemRequest request)
  {
    var repo = GetRepository<DrinkItem>();
    var entity = await repo.GetById(id, tracking: true);

    if (entity is null)
      return Fail<DrinkItemDetailResponse>(ErrorCodes.DrinkItemNotFound, "品名不存在");

    if (await repo.Any(x => x.Name == request.Name && x.Id != id))
      return Fail<DrinkItemDetailResponse>(
        ErrorCodes.DrinkItemAlreadyExists, "品名已存在",
        new Dictionary<string, string[]> { ["name"] = ["品名已存在"] });

    entity.Name = request.Name;
    entity.Sort = request.Sort;
    await repo.Update(entity);

    return Success((await repo.GetById(id))!.ToDrinkItemDetailResponse());
  }

  public async Task<ApiResponse> DeleteDrinkItem(int id)
  {
    // TODO: 店家菜單引用檢查（待 Shop spec 實作後加入）
    var repo = GetRepository<DrinkItem>();
    if (!await repo.Any(x => x.Id == id))
      return Fail(ErrorCodes.DrinkItemNotFound, "品名不存在");

    await repo.DeleteById(id);
    return Success();
  }

  public async Task<ApiResponse> BatchSortDrinkItems(BatchSortRequest request)
  {
    return await BatchSort<DrinkItem>(request, ErrorCodes.DrinkItemNotFound, "品名不存在");
  }

  public async Task<ApiResponse> BatchDeleteDrinkItems(BatchDeleteRequest request)
  {
    // TODO: 店家菜單引用檢查（待 Shop spec 實作後加入）
    return await BatchDelete<DrinkItem>(request, ErrorCodes.DrinkItemNotFound, "部分品名不存在");
  }

  // ==================== Sugar ====================

  public async Task<ApiResponse<PaginationExtension.PaginationList<SugarListResponse>>> GetSugarList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword)
  {
    return await GetPaginatedList<Sugar, SugarListResponse>(
      page, pageSize, sortBy, sortOrder, keyword,
      kw => x => x.Name.Contains(kw),
      BuildSugarOrder,
      DrinkOptionMapper.ToSugarListResponse);
  }

  public async Task<ApiResponse<SugarDetailResponse>> GetSugarById(int id)
  {
    return await GetDetailById<Sugar, SugarDetailResponse>(id, ErrorCodes.SugarNotFound, "甜度不存在",
      DrinkOptionMapper.ToSugarDetailResponse);
  }

  public async Task<ApiResponse<SugarDetailResponse>> CreateSugar(CreateSugarRequest request)
  {
    var repo = GetRepository<Sugar>();

    if (await repo.Any(x => x.Name == request.Name))
      return Fail<SugarDetailResponse>(
        ErrorCodes.SugarAlreadyExists, "甜度名稱已存���",
        new Dictionary<string, string[]> { ["name"] = ["甜度名稱已存在"] });

    var entity = new Sugar { Name = request.Name, DefaultPrice = request.DefaultPrice, Sort = request.Sort };
    await repo.Insert(entity);

    return Success((await repo.GetById(entity.Id))!.ToSugarDetailResponse());
  }

  public async Task<ApiResponse<SugarDetailResponse>> UpdateSugar(int id, UpdateSugarRequest request)
  {
    var repo = GetRepository<Sugar>();
    var entity = await repo.GetById(id, tracking: true);

    if (entity is null)
      return Fail<SugarDetailResponse>(ErrorCodes.SugarNotFound, "甜度不存在");

    if (await repo.Any(x => x.Name == request.Name && x.Id != id))
      return Fail<SugarDetailResponse>(
        ErrorCodes.SugarAlreadyExists, "甜度名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["甜度名稱已存在"] });

    entity.Name = request.Name;
    entity.DefaultPrice = request.DefaultPrice;
    entity.Sort = request.Sort;
    await repo.Update(entity);

    return Success((await repo.GetById(id))!.ToSugarDetailResponse());
  }

  public async Task<ApiResponse> DeleteSugar(int id)
  {
    // TODO: 店家菜單引用檢查
    var repo = GetRepository<Sugar>();
    if (!await repo.Any(x => x.Id == id))
      return Fail(ErrorCodes.SugarNotFound, "甜度不存在");

    await repo.DeleteById(id);
    return Success();
  }

  public async Task<ApiResponse> BatchSortSugars(BatchSortRequest request)
  {
    return await BatchSort<Sugar>(request, ErrorCodes.SugarNotFound, "甜度不存在");
  }

  public async Task<ApiResponse> BatchDeleteSugars(BatchDeleteRequest request)
  {
    // TODO: 店家菜單引用檢查
    return await BatchDelete<Sugar>(request, ErrorCodes.SugarNotFound, "部分甜度不存在");
  }

  // ==================== Ice ====================

  public async Task<ApiResponse<PaginationExtension.PaginationList<IceListResponse>>> GetIceList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword)
  {
    return await GetPaginatedList<Ice, IceListResponse>(
      page, pageSize, sortBy, sortOrder, keyword,
      kw => x => x.Name.Contains(kw),
      BuildIceOrder,
      DrinkOptionMapper.ToIceListResponse);
  }

  public async Task<ApiResponse<IceDetailResponse>> GetIceById(int id)
  {
    return await GetDetailById<Ice, IceDetailResponse>(id, ErrorCodes.IceNotFound, "冰塊不存在",
      DrinkOptionMapper.ToIceDetailResponse);
  }

  public async Task<ApiResponse<IceDetailResponse>> CreateIce(CreateIceRequest request)
  {
    var repo = GetRepository<Ice>();

    if (await repo.Any(x => x.Name == request.Name))
      return Fail<IceDetailResponse>(
        ErrorCodes.IceAlreadyExists, "冰塊名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["冰塊名稱已存在"] });

    var entity = new Ice { Name = request.Name, Sort = request.Sort };
    await repo.Insert(entity);

    return Success((await repo.GetById(entity.Id))!.ToIceDetailResponse());
  }

  public async Task<ApiResponse<IceDetailResponse>> UpdateIce(int id, UpdateIceRequest request)
  {
    var repo = GetRepository<Ice>();
    var entity = await repo.GetById(id, tracking: true);

    if (entity is null)
      return Fail<IceDetailResponse>(ErrorCodes.IceNotFound, "冰塊不存在");

    if (await repo.Any(x => x.Name == request.Name && x.Id != id))
      return Fail<IceDetailResponse>(
        ErrorCodes.IceAlreadyExists, "冰塊名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["冰塊名稱已存在"] });

    entity.Name = request.Name;
    entity.Sort = request.Sort;
    await repo.Update(entity);

    return Success((await repo.GetById(id))!.ToIceDetailResponse());
  }

  public async Task<ApiResponse> DeleteIce(int id)
  {
    // TODO: 店家菜單引用檢查
    var repo = GetRepository<Ice>();
    if (!await repo.Any(x => x.Id == id))
      return Fail(ErrorCodes.IceNotFound, "冰塊不存在");

    await repo.DeleteById(id);
    return Success();
  }

  public async Task<ApiResponse> BatchSortIces(BatchSortRequest request)
  {
    return await BatchSort<Ice>(request, ErrorCodes.IceNotFound, "冰塊不存在");
  }

  public async Task<ApiResponse> BatchDeleteIces(BatchDeleteRequest request)
  {
    // TODO: 店家菜單引用檢查
    return await BatchDelete<Ice>(request, ErrorCodes.IceNotFound, "部分冰塊不存在");
  }

  // ==================== Topping ====================

  public async Task<ApiResponse<PaginationExtension.PaginationList<ToppingListResponse>>> GetToppingList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword)
  {
    return await GetPaginatedList<Topping, ToppingListResponse>(
      page, pageSize, sortBy, sortOrder, keyword,
      kw => x => x.Name.Contains(kw),
      BuildToppingOrder,
      DrinkOptionMapper.ToToppingListResponse);
  }

  public async Task<ApiResponse<ToppingDetailResponse>> GetToppingById(int id)
  {
    return await GetDetailById<Topping, ToppingDetailResponse>(id, ErrorCodes.ToppingNotFound, "加料不存在",
      DrinkOptionMapper.ToToppingDetailResponse);
  }

  public async Task<ApiResponse<ToppingDetailResponse>> CreateTopping(CreateToppingRequest request)
  {
    var repo = GetRepository<Topping>();

    if (await repo.Any(x => x.Name == request.Name))
      return Fail<ToppingDetailResponse>(
        ErrorCodes.ToppingAlreadyExists, "加料名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["加料名稱已存在"] });

    var entity = new Topping { Name = request.Name, DefaultPrice = request.DefaultPrice, Sort = request.Sort };
    await repo.Insert(entity);

    return Success((await repo.GetById(entity.Id))!.ToToppingDetailResponse());
  }

  public async Task<ApiResponse<ToppingDetailResponse>> UpdateTopping(int id, UpdateToppingRequest request)
  {
    var repo = GetRepository<Topping>();
    var entity = await repo.GetById(id, tracking: true);

    if (entity is null)
      return Fail<ToppingDetailResponse>(ErrorCodes.ToppingNotFound, "加料不存在");

    if (await repo.Any(x => x.Name == request.Name && x.Id != id))
      return Fail<ToppingDetailResponse>(
        ErrorCodes.ToppingAlreadyExists, "加料名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["加料名稱已存在"] });

    entity.Name = request.Name;
    entity.DefaultPrice = request.DefaultPrice;
    entity.Sort = request.Sort;
    await repo.Update(entity);

    return Success((await repo.GetById(id))!.ToToppingDetailResponse());
  }

  public async Task<ApiResponse> DeleteTopping(int id)
  {
    // TODO: 店家菜單引用檢查
    var repo = GetRepository<Topping>();
    if (!await repo.Any(x => x.Id == id))
      return Fail(ErrorCodes.ToppingNotFound, "加料不存在");

    await repo.DeleteById(id);
    return Success();
  }

  public async Task<ApiResponse> BatchSortToppings(BatchSortRequest request)
  {
    return await BatchSort<Topping>(request, ErrorCodes.ToppingNotFound, "加料不存在");
  }

  public async Task<ApiResponse> BatchDeleteToppings(BatchDeleteRequest request)
  {
    // TODO: 店家菜單引用檢查
    return await BatchDelete<Topping>(request, ErrorCodes.ToppingNotFound, "部分加料不存在");
  }

  // ==================== Size ====================

  public async Task<ApiResponse<PaginationExtension.PaginationList<SizeListResponse>>> GetSizeList(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword)
  {
    return await GetPaginatedList<Size, SizeListResponse>(
      page, pageSize, sortBy, sortOrder, keyword,
      kw => x => x.Name.Contains(kw),
      BuildSizeOrder,
      DrinkOptionMapper.ToSizeListResponse);
  }

  public async Task<ApiResponse<SizeDetailResponse>> GetSizeById(int id)
  {
    return await GetDetailById<Size, SizeDetailResponse>(id, ErrorCodes.SizeNotFound, "容量不存在",
      DrinkOptionMapper.ToSizeDetailResponse);
  }

  public async Task<ApiResponse<SizeDetailResponse>> CreateSize(CreateSizeRequest request)
  {
    var repo = GetRepository<Size>();

    if (await repo.Any(x => x.Name == request.Name))
      return Fail<SizeDetailResponse>(
        ErrorCodes.SizeAlreadyExists, "容量名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["容量名稱已存在"] });

    var entity = new Size { Name = request.Name, Sort = request.Sort };
    await repo.Insert(entity);

    return Success((await repo.GetById(entity.Id))!.ToSizeDetailResponse());
  }

  public async Task<ApiResponse<SizeDetailResponse>> UpdateSize(int id, UpdateSizeRequest request)
  {
    var repo = GetRepository<Size>();
    var entity = await repo.GetById(id, tracking: true);

    if (entity is null)
      return Fail<SizeDetailResponse>(ErrorCodes.SizeNotFound, "容量不存在");

    if (await repo.Any(x => x.Name == request.Name && x.Id != id))
      return Fail<SizeDetailResponse>(
        ErrorCodes.SizeAlreadyExists, "容量名稱已存在",
        new Dictionary<string, string[]> { ["name"] = ["容量名稱已存在"] });

    entity.Name = request.Name;
    entity.Sort = request.Sort;
    await repo.Update(entity);

    return Success((await repo.GetById(id))!.ToSizeDetailResponse());
  }

  public async Task<ApiResponse> DeleteSize(int id)
  {
    // TODO: 店家菜單引用檢查
    var repo = GetRepository<Size>();
    if (!await repo.Any(x => x.Id == id))
      return Fail(ErrorCodes.SizeNotFound, "容量不存在");

    await repo.DeleteById(id);
    return Success();
  }

  public async Task<ApiResponse> BatchSortSizes(BatchSortRequest request)
  {
    return await BatchSort<Size>(request, ErrorCodes.SizeNotFound, "容量不存在");
  }

  public async Task<ApiResponse> BatchDeleteSizes(BatchDeleteRequest request)
  {
    // TODO: 店家菜單引用檢查
    return await BatchDelete<Size>(request, ErrorCodes.SizeNotFound, "部分容量不存在");
  }

  // ==================== Private Helpers ====================

  private async Task<ApiResponse<TResponse>> GetDetailById<TEntity, TResponse>(
    int id, (int Code, string Error) notFoundCode, string notFoundMessage,
    Func<TEntity, TResponse> mapper)
    where TEntity : BaseDataEntity
  {
    var repo = GetRepository<TEntity>();
    var entity = await repo.GetById(id);

    if (entity is null)
      return Fail<TResponse>(notFoundCode, notFoundMessage);

    return Success(mapper(entity));
  }

  private async Task<ApiResponse<PaginationExtension.PaginationList<TResponse>>> GetPaginatedList<TEntity, TResponse>(
    int page, int pageSize, string? sortBy, string? sortOrder, string? keyword,
    Func<string, Expression<Func<TEntity, bool>>> keywordFilterBuilder,
    Func<IQueryable<TEntity>, string?, string?, IQueryable<TEntity>> orderBuilder,
    Func<TEntity, TResponse> mapper)
    where TEntity : BaseDataEntity
    where TResponse : class
  {
    var repo = GetRepository<TEntity>();

    Expression<Func<TEntity, bool>>? predicate = null;
    if (!string.IsNullOrWhiteSpace(keyword))
      predicate = keywordFilterBuilder(keyword);

    var result = await repo.GetPaginationList(
      page, pageSize,
      predicate: predicate,
      order: q => orderBuilder(q, sortBy, sortOrder));

    var mapped = new PaginationExtension.PaginationList<TResponse>
    {
      Items = result.Items.Select(mapper).ToList(),
      Total = result.Total,
      Page = result.Page,
      PageSize = result.PageSize
    };

    return Success(mapped);
  }

  private async Task<ApiResponse> BatchSort<TEntity>(
    BatchSortRequest request, (int Code, string Error) notFoundCode, string notFoundMessage)
    where TEntity : BaseDataEntity
  {
    var repo = GetRepository<TEntity>();
    var ids = request.Items.Select(x => x.Id).ToList();

    var existingCount = await repo.Count(x => ids.Contains(x.Id));
    if (existingCount != ids.Count)
      return Fail(notFoundCode, notFoundMessage);

    var entities = await repo.GetList(predicate: x => ids.Contains(x.Id), tracking: true);
    var sortMap = request.Items.ToDictionary(x => x.Id, x => x.Sort);

    foreach (var entity in entities)
    {
      var sortProp = entity.GetType().GetProperty("Sort");
      sortProp?.SetValue(entity, sortMap[entity.Id]);
    }

    await repo.UpdateRange(entities);
    return Success();
  }

  private async Task<ApiResponse> BatchDelete<TEntity>(
    BatchDeleteRequest request, (int Code, string Error) notFoundCode, string notFoundMessage)
    where TEntity : BaseDataEntity
  {
    var repo = GetRepository<TEntity>();

    var existingCount = await repo.Count(x => request.Ids.Contains(x.Id));
    if (existingCount != request.Ids.Count)
      return Fail(notFoundCode, notFoundMessage);

    await repo.ExecuteDelete(x => request.Ids.Contains(x.Id));
    return Success();
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
