using System.ComponentModel.DataAnnotations;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class AdminUser : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  [StringLength(50)]
  public string Username { get; set; } = null!;

  public string PasswordHash { get; set; } = null!;

  public int RoleId { get; set; }
  public AdminRole Role { get; set; } = null!;

  public bool IsActive { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }

  public ICollection<AdminRefreshToken> RefreshTokens { get; set; } = [];
}
