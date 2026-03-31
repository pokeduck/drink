using System.ComponentModel.DataAnnotations;

namespace Drink.Application.Requests.Admin;

// --- DrinkItem ---

public class CreateDrinkItemRequest
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  [Required]
  public int Sort { get; set; }
}

public class UpdateDrinkItemRequest
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  [Required]
  public int Sort { get; set; }
}

// --- Sugar ---

public class CreateSugarRequest
{
  [Required]
  [StringLength(50)]
  public string Name { get; set; } = null!;

  [Required]
  [Range(0, double.MaxValue)]
  public decimal DefaultPrice { get; set; }

  [Required]
  public int Sort { get; set; }
}

public class UpdateSugarRequest
{
  [Required]
  [StringLength(50)]
  public string Name { get; set; } = null!;

  [Required]
  [Range(0, double.MaxValue)]
  public decimal DefaultPrice { get; set; }

  [Required]
  public int Sort { get; set; }
}

// --- Ice ---

public class CreateIceRequest
{
  [Required]
  [StringLength(50)]
  public string Name { get; set; } = null!;

  [Required]
  public int Sort { get; set; }
}

public class UpdateIceRequest
{
  [Required]
  [StringLength(50)]
  public string Name { get; set; } = null!;

  [Required]
  public int Sort { get; set; }
}

// --- Topping ---

public class CreateToppingRequest
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  [Required]
  [Range(0, double.MaxValue)]
  public decimal DefaultPrice { get; set; }

  [Required]
  public int Sort { get; set; }
}

public class UpdateToppingRequest
{
  [Required]
  [StringLength(100)]
  public string Name { get; set; } = null!;

  [Required]
  [Range(0, double.MaxValue)]
  public decimal DefaultPrice { get; set; }

  [Required]
  public int Sort { get; set; }
}

// --- Size ---

public class CreateSizeRequest
{
  [Required]
  [StringLength(50)]
  public string Name { get; set; } = null!;

  [Required]
  public int Sort { get; set; }
}

public class UpdateSizeRequest
{
  [Required]
  [StringLength(50)]
  public string Name { get; set; } = null!;

  [Required]
  public int Sort { get; set; }
}

// --- Shared ---

public class BatchSortRequest
{
  [Required]
  public List<SortItem> Items { get; set; } = [];
}

public class SortItem
{
  [Required]
  public int Id { get; set; }

  [Required]
  public int Sort { get; set; }
}

public class BatchDeleteRequest
{
  [Required]
  public List<int> Ids { get; set; } = [];
}
