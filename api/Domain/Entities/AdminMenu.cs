using System.ComponentModel.DataAnnotations;
using Drink.Domain.Interfaces;

namespace Drink.Domain.Entities;

public class AdminMenu : BaseDataEntity, ICreateEntity, IUpdateEntity
{
  public int? ParentId { get; set; }
  public AdminMenu? Parent { get; set; }

  [StringLength(50)]
  public string Name { get; set; } = null!;

  [StringLength(50)]
  public string? Icon { get; set; }

  [StringLength(200)]
  public string? Endpoint { get; set; }

  public int Sort { get; set; }

  public bool IsPermissionOnly { get; set; }

  public DateTime CreatedAt { get; set; }
  public int Creator { get; set; }
  public DateTime UpdatedAt { get; set; }
  public int Updater { get; set; }

  public ICollection<AdminMenu> Children { get; set; } = [];
  public ICollection<AdminMenuRole> MenuRoles { get; set; } = [];
}
