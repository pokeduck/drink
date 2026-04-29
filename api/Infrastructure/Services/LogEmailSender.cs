using Drink.Application.Interfaces;
using Drink.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Drink.Infrastructure.Services;

// TODO: replace with real mailer (SMTP / SendGrid / Resend) before production.
public class LogEmailSender : IEmailSender
{
  private readonly ILogger<LogEmailSender> _logger;

  public LogEmailSender(ILogger<LogEmailSender> logger)
  {
    _logger = logger;
  }

  public Task SendVerificationEmailAsync(string to, VerificationEmailType type, string token)
  {
    _logger.LogInformation("[MOCK EMAIL] to={To} type={Type} token={Token}", to, type, token);
    return Task.CompletedTask;
  }
}
