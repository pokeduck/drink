using System.Text.Json;
using System.Text.Json.Serialization;
using Drink.Domain.Entities;
using Drink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Drink.Infrastructure.Extensions;

public static class EFExtension

{
  /// <summary>
  /// 註冊所有實體
  /// </summary>
  /// <param name="modelBuilder">modelBuilder</param>
  public static void RegisterAllEntities(this ModelBuilder modelBuilder)

  {
    var baseDataEntityType = typeof(BaseDataEntity);


    var types = baseDataEntityType.Assembly.GetExportedTypes()
      .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic && t != baseDataEntityType && baseDataEntityType.IsAssignableFrom(t));


    foreach (var type in types)

      modelBuilder.Entity(type);
  }

  /// <summary>
  /// 設定 Json 轉換
  /// </summary>
  /// <typeparam name="TProperty">實體屬性</typeparam>
  /// <param name="propertyBuilder">propertyBuilder</param>
  public static void SetJsonConversion<TProperty>(this PropertyBuilder<IEnumerable<TProperty>> propertyBuilder)

  {
    var jsonOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };


    propertyBuilder.HasConversion(
      x => JsonSerializer.Serialize(x, jsonOptions),
      x => JsonSerializer.Deserialize<IEnumerable<TProperty>>(x, jsonOptions)!,
      new ValueComparer<IEnumerable<TProperty>>(
        (c1, c2) => c1!.SequenceEqual(c2!),
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())),
        c => c.ToList())
    );
  }

  public static void SetCommaSeparatedEnumConversion<TEnum>(
    this PropertyBuilder<IEnumerable<TEnum>> propertyBuilder)
    where TEnum : struct, Enum

  {
    propertyBuilder.HasConversion(
      // Serialize: IEnumerable<Enum> → "1,2,3"
      v => string.Join(",", v.Select(e => Convert.ToInt32(e))),

      // Deserialize: "1,2,3" → IEnumerable<Enum>
      v => ParseCommaSeparatedEnum<TEnum>(v),

      // ValueComparer: for EF Core change tracking
      new ValueComparer<IEnumerable<TEnum>>(
        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
        c => c.ToList())
    );
  }

  private static IEnumerable<TEnum> ParseCommaSeparatedEnum<TEnum>(string? input)
    where TEnum : struct, Enum

  {
    if (string.IsNullOrWhiteSpace(input))

      return [];


    return input
      .Split(",", StringSplitOptions.RemoveEmptyEntries)
      .Select(s =>

      {
        if (int.TryParse(s, out var intValue))

        {
          var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), intValue);

          if (Enum.IsDefined(typeof(TEnum), enumValue))

            return enumValue;
        }


        return default!;
      })
      .Where(e => Enum.IsDefined(typeof(TEnum), e));
  }
}