using Acme.SaaS.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Acme.SaaS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending email to {To}: {Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string to, string name, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending welcome email to {Name} at {Email}", name, to);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string to, string resetLink, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending password reset to {Email}: {Link}", to, resetLink);
        return Task.CompletedTask;
    }
}
