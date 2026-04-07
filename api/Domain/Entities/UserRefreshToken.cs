using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class UserRefreshToken : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int UserId { get; set; }
  public User User { get; set; } = null!;

  public string Token { get; set; } = null!;

  public DateTime ExpiresAt { get; set; }
  public DateTime? RevokedAt { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }
}
