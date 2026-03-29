namespace Drink.Application.Helpers;

public static class SortByValidator
{
  public static string Validate(string? sortBy, HashSet<string> allowedFields, string defaultField = "created_at")
  {
    if (string.IsNullOrWhiteSpace(sortBy))
      return defaultField;

    return allowedFields.Contains(sortBy) ? sortBy : defaultField;
  }
}