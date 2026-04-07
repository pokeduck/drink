using System.ComponentModel.DataAnnotations;
using Drink.Domain.Enums;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class VerificationEmail : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int UserId { get; set; }
  public User User { get; set; } = null!;

  public VerificationEmailType Type { get; set; }

  [StringLength(200)]
  public string Token { get; set; } = null!;

  public DateTime ExpiresAt { get; set; }

  public bool IsUsed { get; set; }
  public DateTime? UsedAt { get; set; }

  public bool IsSuccess { get; set; }

  [StringLength(500)]
  public string? ErrorMessage { get; set; }

  public DateTime SentAt { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }
}
