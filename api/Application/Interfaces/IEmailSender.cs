using Drink.Domain.Enums;

namespace Drink.Application.Interfaces;

public interface IEmailSender
{
  Task SendVerificationEmailAsync(string to, VerificationEmailType type, string token);
}
