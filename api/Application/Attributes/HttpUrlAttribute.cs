using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class HttpUrlAttribute : ValidationAttribute
{
  public HttpUrlAttribute() : base("請輸入有效的連結") { }

  public override bool IsValid(object? value)
  {
    if (value is null) return true;
    if (value is not string s) return false;
    if (string.IsNullOrWhiteSpace(s)) return true;
    if (!Uri.TryCreate(s, UriKind.Absolute, out var uri)) return false;
    return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
  }
}
