namespace Mystira.Application.Ports;

/// <summary>
/// Port interface for sending emails. Decoupled from any specific provider
/// (Azure Communication Services, SendGrid, etc.).
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send an email to a single recipient.
    /// </summary>
    /// <param name="to">The recipient email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="htmlBody">The HTML body content of the email.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the email was sent successfully; otherwise, false.</returns>
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}
