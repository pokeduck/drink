using System.ComponentModel.DataAnnotations;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class AdminRole : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  [StringLength(50)]
  public string Name { get; set; } = null!;

  public bool IsSystem { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }

  public ICollection<AdminUser> Users { get; set; } = [];
  public ICollection<AdminMenuRole> MenuRoles { get; set; } = [];
}
