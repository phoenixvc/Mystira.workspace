namespace Mystira.App.Application.Ports;

/// <summary>
/// Port interface for sending emails. Decoupled from any specific provider
/// (Azure Communication Services, SendGrid, etc.)
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an email to a single recipient.
    /// </summary>
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}
