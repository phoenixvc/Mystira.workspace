using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mystira.Core.Ports;

namespace Mystira.App.Infrastructure.Azure.Services;

/// <summary>
/// No-op email service for development/testing when Azure Communication Services is not configured.
/// Logs email details instead of actually sending.
/// </summary>
public class NoOpEmailService : IEmailService
{
    private readonly ILogger<NoOpEmailService> _logger;

    public NoOpEmailService(ILogger<NoOpEmailService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "[NoOpEmailService] Email would be sent to {Recipient}, subject: {Subject} (email sending not configured)",
            to, subject);
        return Task.FromResult(true);
    }
}
