namespace Drink.Application.Responses.Admin.Order;

public class AdminOrderItemResponse
{
  public int Id { get; set; }
  public int UserId { get; set; }
  public string UserName { get; set; } = null!;
  public string RecipientName { get; set; } = null!;
  public string MenuItemName { get; set; } = null!;
  public string SizeName { get; set; } = null!;
  public string SugarName { get; set; } = null!;
  public string IceName { get; set; } = null!;
  public List<AdminOrderItemToppingResponse> Toppings { get; set; } = [];
  public decimal ItemPrice { get; set; }
  public decimal SugarPrice { get; set; }
  public decimal ToppingPrice { get; set; }
  public decimal TotalPrice { get; set; }
  public int Quantity { get; set; }
  public string? Note { get; set; }
  public DateTime CreatedAt { get; set; }
}
