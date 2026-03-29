using System.Linq.Expressions;
using System.Security.Claims;
using Drink.Domain.Entities;
using Drink.Domain.Interfaces;
using Drink.Infrastructure.Data;
using Drink.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace Drink.Infrastructure.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity>
  where TEntity : BaseDataEntity
{
  private readonly DrinkDbContext _context;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly ILogger<GenericRepository<TEntity>> _logger;

  public GenericRepository(
    DrinkDbContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<GenericRepository<TEntity>> logger)
  {
    _context = context;
    _httpContextAccessor = httpContextAccessor;
    _logger = logger;
  }

  private int CurrentUserId
  {
    get
    {
      var claim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
      return int.TryParse(claim, out var id) ? id : 0;
    }
  }

  public DatabaseFacade Database => _context.Database;
  public DbSet<TEntity> DbSet => _context.Set<TEntity>();
  public IQueryable<TEntity> Query => ApplySoftDeleteFilter(_context.Set<TEntity>().AsNoTracking());

  public async Task<TEntity?> Get(
    Expression<Func<TEntity, bool>>? predicate = null,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? order = null,
    bool tracking = false)
  {
    return await BuildQuery(predicate, include, order, tracking).FirstOrDefaultAsync();
  }

  public async Task<TEntity?> GetById(
    int id,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
    bool tracking = false)
  {
    var query = BuildQuery(predicate: null, include, order: null, tracking);
    return await query.FirstOrDefaultAsync(x => x.Id == id);
  }

  public async Task<List<TEntity>> GetList(
    Expression<Func<TEntity, bool>>? predicate = null,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? order = null,
    bool tracking = false)
  {
    return await BuildQuery(predicate, include, order, tracking).ToListAsync();
  }

  public async Task<PaginationExtension.PaginationList<TEntity>> GetPaginationList(
    int page,
    int pageSize,
    Expression<Func<TEntity, bool>>? predicate = null,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? order = null,
    bool tracking = false)
  {
    return await BuildQuery(predicate, include, order, tracking).ToPaginationList(page, pageSize);
  }

  public async Task<int> Count(Expression<Func<TEntity, bool>>? predicate = null)
  {
    var query = ApplySoftDeleteFilter(_context.Set<TEntity>().AsNoTracking());

    if (predicate is not null)
      query = query.Where(predicate);

    return await query.CountAsync();
  }

  public async Task<bool> Any(Expression<Func<TEntity, bool>>? predicate = null)
  {
    var query = ApplySoftDeleteFilter(_context.Set<TEntity>().AsNoTracking());

    if (predicate is not null)
      return await query.AnyAsync(predicate);

    return await query.AnyAsync();
  }

  public async Task Insert(TEntity entity, bool saveImmediately = true)
  {
    var now = DateTime.UtcNow;
    var userId = CurrentUserId;

    if (entity is ICreateEntity createEntity)
    {
      if (createEntity.CreatedAt == default)
        createEntity.CreatedAt = now;
      createEntity.Creator = userId;
    }

    if (entity is IUpdateEntity updateEntity)
    {
      if (updateEntity.UpdatedAt == default)
        updateEntity.UpdatedAt = now;
      updateEntity.Updater = userId;
    }

    _context.Add(entity);

    if (saveImmediately)
      await _context.SaveChangesAsync();
  }

  public async Task InsertRange(IEnumerable<TEntity> entities)
  {
    foreach (var entity in entities)
      await Insert(entity, false);

    await _context.SaveChangesAsync();
  }

  public async Task Update(TEntity entity, bool saveImmediately = true)
  {
    if (entity is IUpdateEntity updateEntity)
    {
      updateEntity.UpdatedAt = DateTime.UtcNow;
      updateEntity.Updater = CurrentUserId;
    }

    _context.Update(entity);

    if (saveImmediately)
      await _context.SaveChangesAsync();
  }

  public async Task UpdateRange(IEnumerable<TEntity> entities)
  {
    var now = DateTime.UtcNow;
    var userId = CurrentUserId;

    foreach (var entity in entities)
    {
      if (entity is IUpdateEntity updateEntity)
      {
        updateEntity.UpdatedAt = now;
        updateEntity.Updater = userId;
      }
    }

    _context.UpdateRange(entities);
    await _context.SaveChangesAsync();
  }

  public async Task ExecuteUpdate(
    Expression<Func<TEntity, bool>> predicate,
    Action<UpdateSettersBuilder<TEntity>> setters)
  {
    await DbSet.Where(predicate).ExecuteUpdateAsync(setters);
  }

  public async Task ExecuteDelete(Expression<Func<TEntity, bool>> predicate)
  {
    await DbSet.Where(predicate).ExecuteDeleteAsync();
  }

  public async Task DeleteById(int id, bool saveImmediately = true)
  {
    var entity = await DbSet.FirstOrDefaultAsync(x => x.Id == id);
    if (entity == null) return;

    _context.Remove(entity);

    if (saveImmediately)
      await _context.SaveChangesAsync();
  }

  public async Task Delete(TEntity entity, bool saveImmediately = true)
  {
    _context.Remove(entity);

    if (saveImmediately)
      await _context.SaveChangesAsync();
  }

  public async Task DeleteRange(IEnumerable<TEntity> entities, bool saveImmediately = true)
  {
    DbSet.RemoveRange(entities);

    if (saveImmediately)
      await _context.SaveChangesAsync();
  }

  public async Task ExecuteInTransaction(Func<Task> action, string errorMessage)
  {
    await using var transaction = await Database.BeginTransactionAsync();

    try
    {
      await action();
      await transaction.CommitAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, errorMessage);
      await transaction.RollbackAsync();
      throw;
    }
  }

  private IQueryable<TEntity> BuildQuery(
    Expression<Func<TEntity, bool>>? predicate = null,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? order = null,
    bool tracking = false)
  {
    var query = tracking
      ? ApplySoftDeleteFilter(DbSet.AsQueryable())
      : Query;

    if (predicate is not null)
      query = query.Where(predicate);

    if (include is not null)
      query = include(query);

    if (order is not null)
      query = order(query);

    return query;
  }

  private static IQueryable<TEntity> ApplySoftDeleteFilter(IQueryable<TEntity> query)
  {
    if (typeof(ISoftDeleteEntity).IsAssignableFrom(typeof(TEntity)))
      query = query.Where(x => !((ISoftDeleteEntity)x).IsDeleted);

    return query;
  }
}
