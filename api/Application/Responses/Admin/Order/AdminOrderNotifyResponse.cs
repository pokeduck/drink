namespace Drink.Application.Responses.Admin.Order;

public class AdminOrderNotifyResponse
{
  public int TotalRecipients { get; set; }
  public int EmailSent { get; set; }
  public int PushSkipped { get; set; }
  public int NoneSkipped { get; set; }
  public int Failed { get; set; }
}
