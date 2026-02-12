using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports;

namespace Mystira.App.Infrastructure.Azure.Services;

/// <summary>
/// Email service adapter using Azure Communication Services.
/// Implements IEmailService port from the Application layer.
/// </summary>
public class AzureEmailService : IEmailService
{
    private readonly EmailClient _emailClient;
    private readonly string _senderEmail;
    private readonly ILogger<AzureEmailService> _logger;

    public AzureEmailService(IConfiguration configuration, ILogger<AzureEmailService> logger)
    {
        _logger = logger;

        var connectionString = configuration["AzureCommunicationServices:ConnectionString"]
            ?? throw new InvalidOperationException("AzureCommunicationServices:ConnectionString is not configured");
        _senderEmail = configuration["AzureCommunicationServices:SenderEmail"]
            ?? throw new InvalidOperationException("AzureCommunicationServices:SenderEmail is not configured");

        _emailClient = new EmailClient(connectionString);
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogWarning("SendEmailAsync called with null or empty recipient address");
            return false;
        }

        try
        {
            _logger.LogInformation("Sending email to {Recipient}, subject: {Subject}", MaskEmail(to), subject);

            var emailMessage = new EmailMessage(
                senderAddress: _senderEmail,
                recipientAddress: to,
                content: new EmailContent(subject) { Html = htmlBody });

            var operation = await _emailClient.SendAsync(WaitUntil.Started, emailMessage, ct);

            _logger.LogInformation("Email send initiated, operation ID: {OperationId}", operation.Id);
            return true;
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure Communication Services email send failed: {ErrorCode} {Message}",
                ex.ErrorCode, ex.Message);
            return false;
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email to {Recipient}", MaskEmail(to));
            return false;
        }
    }

    private static string MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email))
            return "***@***";
        var atIndex = email.IndexOf('@');
        if (atIndex <= 1) return "***@***";
        return email[0] + "***" + email[(atIndex - 1)..];
    }
}
