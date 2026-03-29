using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class AdminMenuRole : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int RoleId { get; set; }
  public AdminRole Role { get; set; } = null!;

  public int MenuId { get; set; }
  public AdminMenu Menu { get; set; } = null!;

  public bool CanRead { get; set; }
  public bool CanCreate { get; set; }
  public bool CanUpdate { get; set; }
  public bool CanDelete { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }
}
