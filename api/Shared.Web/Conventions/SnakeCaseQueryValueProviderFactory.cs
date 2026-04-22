using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Drink.Shared.Web.Conventions;

/// <summary>
/// 將 snake_case query string 自動綁定到 camelCase / PascalCase 參數。
/// e.g. ?page_size=20 → 綁定到 pageSize 參數
/// </summary>
public class SnakeCaseQueryValueProviderFactory : IValueProviderFactory
{
  public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
  {
    context.ValueProviders.Add(new SnakeCaseQueryValueProvider(context.ActionContext.HttpContext.Request.Query));
    return Task.CompletedTask;
  }
}

internal class SnakeCaseQueryValueProvider : IValueProvider
{
  private readonly Dictionary<string, string?> _camelCaseMap;

  public SnakeCaseQueryValueProvider(IQueryCollection query)
  {
    _camelCaseMap = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    foreach (var kvp in query)
    {
      // 原始 key 保留（讓預設 provider 處理）
      // 額外加一筆 camelCase key 對應同一個 value
      var camelKey = SnakeToCamel(kvp.Key);
      if (camelKey != kvp.Key)
      {
        _camelCaseMap[camelKey] = kvp.Value;
      }
    }
  }

  public bool ContainsPrefix(string prefix)
  {
    return _camelCaseMap.ContainsKey(prefix);
  }

  public ValueProviderResult GetValue(string key)
  {
    if (_camelCaseMap.TryGetValue(key, out var value) && value is not null)
    {
      return new ValueProviderResult(value, CultureInfo.InvariantCulture);
    }
    return ValueProviderResult.None;
  }

  private static string SnakeToCamel(string snake)
  {
    var parts = snake.Split('_');
    if (parts.Length <= 1) return snake;

    return parts[0] + string.Concat(parts.Skip(1).Select(p =>
      p.Length > 0 ? char.ToUpperInvariant(p[0]) + p[1..] : p));
  }
}
