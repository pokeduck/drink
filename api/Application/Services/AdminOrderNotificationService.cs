using Drink.Application.Constants;
using Drink.Application.Interfaces;
using Drink.Application.Responses;
using Drink.Application.Responses.Admin.Order;
using Drink.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Drink.Application.Services;

public class AdminOrderNotificationService : BaseService
{
  private readonly AdminOrderService _orderService;
  private readonly IEmailSender _emailSender;
  private readonly ILogger<AdminOrderNotificationService> _logger;

  public AdminOrderNotificationService(
    ICurrentUserContext currentUser,
    AdminOrderService orderService,
    IEmailSender emailSender,
    ILogger<AdminOrderNotificationService> logger) : base(currentUser)
  {
    _orderService = orderService;
    _emailSender = emailSender;
    _logger = logger;
  }

  public async Task<ApiResponse<AdminOrderNotifyResponse>> NotifyAsync(int orderId)
  {
    var order = await _orderService.GetOrderWithItemsAsync(orderId);
    if (order is null)
      return Fail<AdminOrderNotifyResponse>(ErrorCodes.OrderNotFound, "揪團不存在");

    // deduplicate by UserId; each user receives one email regardless of how many drinks they ordered
    var recipients = order.OrderItems
      .GroupBy(i => i.UserId)
      .Select(g => g.First().User)
      .ToList();

    var result = new AdminOrderNotifyResponse
    {
      TotalRecipients = recipients.Count,
    };

    foreach (var user in recipients)
    {
      switch (user.NotificationType)
      {
        case NotificationType.None:
          result.NoneSkipped++;
          break;

        case NotificationType.WebPush:
          result.PushSkipped++;
          _logger.LogInformation(
            "[PUSH_NOT_IMPLEMENTED] userId={UserId} orderId={OrderId}", user.Id, orderId);
          break;

        case NotificationType.Email:
        case NotificationType.Both:
          try
          {
            await _emailSender.SendOrderNotificationAsync(
              user.Email, user.Name, order.Title, order.Shop.Name, order.Note);
            result.EmailSent++;

            if (user.NotificationType == NotificationType.Both)
            {
              result.PushSkipped++;
              _logger.LogInformation(
                "[PUSH_NOT_IMPLEMENTED] userId={UserId} orderId={OrderId}", user.Id, orderId);
            }
          }
          catch (Exception ex)
          {
            result.Failed++;
            _logger.LogError(ex,
              "Order notification email failed userId={UserId} orderId={OrderId}", user.Id, orderId);
          }
          break;
      }
    }

    return Success(result);
  }
}
