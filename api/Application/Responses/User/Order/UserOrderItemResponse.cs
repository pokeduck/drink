namespace Drink.Application.Responses.User.Order;

public class UserOrderItemResponse
{
  public int Id { get; set; }
  public string UserName { get; set; } = null!;
  public string RecipientName { get; set; } = null!;
  public string MenuItemName { get; set; } = null!;
  public string SizeName { get; set; } = null!;
  public string SugarName { get; set; } = null!;
  public string IceName { get; set; } = null!;
  public List<UserOrderItemToppingResponse> Toppings { get; set; } = [];
  public decimal ItemPrice { get; set; }
  public decimal SugarPrice { get; set; }
  public decimal ToppingPrice { get; set; }
  public decimal TotalPrice { get; set; }
  public int Quantity { get; set; }
  public string? Note { get; set; }
  public bool IsMine { get; set; }
  public DateTime CreatedAt { get; set; }
}
