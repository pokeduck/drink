using Drink.Domain.Enums;

namespace Drink.Application.Interfaces;

public interface IEmailSender
{
  Task SendVerificationEmailAsync(string to, VerificationEmailType type, string token);

  Task SendOrderNotificationAsync(string to, string recipientName, string groupTitle, string shopName, string? note);
}
