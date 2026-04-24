using System.Linq.Expressions;
using Drink.Application.Models;
using Drink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;

namespace Drink.Application.Interfaces;

public interface IGenericRepository<TEntity>
  where TEntity : BaseDataEntity
{
  DatabaseFacade Database { get; }

  DbSet<TEntity> DbSet { get; }

  IQueryable<TEntity> Query { get; }

  Task<TEntity?> Get(
    Expression<Func<TEntity, bool>>? predicate = null,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? order = null,
    bool tracking = false);

  Task<TEntity?> GetById(
    int id,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
    bool tracking = false);

  Task<List<TEntity>> GetList(
    Expression<Func<TEntity, bool>>? predicate = null,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? order = null,
    bool tracking = false,
    bool splitQuery = false);

  Task<PaginationList<TEntity>> GetPaginationList(
    int page,
    int pageSize,
    Expression<Func<TEntity, bool>>? predicate = null,
    Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
    Func<IQueryable<TEntity>, IQueryable<TEntity>>? order = null,
    bool tracking = false);

  Task<int> Count(Expression<Func<TEntity, bool>>? predicate = null);

  Task<bool> Any(Expression<Func<TEntity, bool>>? predicate = null);

  Task Insert(TEntity entity, bool saveImmediately = true);

  Task InsertRange(IEnumerable<TEntity> entities);

  Task Update(TEntity entity, bool saveImmediately = true);

  Task UpdateRange(IEnumerable<TEntity> entities);

  Task ExecuteUpdate(
    Expression<Func<TEntity, bool>> predicate,
    Action<UpdateSettersBuilder<TEntity>> setters);

  Task ExecuteDelete(Expression<Func<TEntity, bool>> predicate);

  Task DeleteById(int id, bool saveImmediately = true);

  Task Delete(TEntity entity, bool saveImmediately = true);

  Task DeleteRange(IEnumerable<TEntity> entities, bool saveImmediately = true);

  Task ExecuteInTransaction(Func<Task> action, string errorMessage);
}
