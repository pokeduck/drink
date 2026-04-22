using Drink.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Drink.Infrastructure.Extensions;

public static class PaginationExtension
{
  public static PaginationList<T> ToPaginationList<T>(this IEnumerable<T> items, int page,
    int pageSize = 20) where T : class
  {
    var skip = (page - 1) * pageSize;

    return new PaginationList<T>
    {
      Page = page,
      PageSize = pageSize,
      Total = items.Count(),
      Items = items.Skip(skip).Take(pageSize).ToList()
    };
  }

  public static async Task<PaginationList<T>> ToPaginationList<T>(this IQueryable<T> items, int page,
    int pageSize = 20) where T : class
  {
    var skip = (page - 1) * pageSize;

    return new PaginationList<T>
    {
      Page = page,
      PageSize = pageSize,
      Total = await items.CountAsync(),
      Items = await items
        .AsSingleQuery()
        .Skip(skip)
        .Take(pageSize)
        .ToListAsync()
    };
  }
}
