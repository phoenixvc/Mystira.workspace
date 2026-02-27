using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;
using Mystira.Shared.Exceptions;

namespace Mystira.App.Application.CQRS.Auth.Commands;

public static class ResendMagicSignupCommandHandler
{
    private static readonly TimeSpan TokenTtl = TimeSpan.FromMinutes(30);

    public static async Task<MagicSignupResult> Handle(
        ResendMagicSignupCommand command,
        IPendingSignupRepository pendingSignupRepository,
        IEmailService emailService,
        MagicSignupEmailBuilder emailBuilder,
        IUnitOfWork unitOfWork,
        ILogger<ResendMagicSignupCommand> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("email", "Email is required");
        }

        if (string.IsNullOrWhiteSpace(command.VerificationBaseUrl))
        {
            throw new ValidationException("verificationBaseUrl", "Verification base URL is required");
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var pendingSignup = await pendingSignupRepository.GetByEmailAsync(normalizedEmail, ct);

        if (pendingSignup == null)
        {
            pendingSignup = new PendingSignup
            {
                Email = normalizedEmail,
                DisplayName = normalizedEmail.Split('@')[0],
                Status = PendingSignupStatus.Pending
            };
            await pendingSignupRepository.AddAsync(pendingSignup, ct);
        }

        var rawToken = MagicLinkTokenService.GenerateRawToken();
        var tokenHash = MagicLinkTokenService.HashToken(rawToken);
        pendingSignup.SetToken(tokenHash, DateTime.UtcNow.Add(TokenTtl));

        var verifyUrl = BuildVerifyUrl(command.VerificationBaseUrl, rawToken);
        var emailBody = emailBuilder.BuildVerificationEmail(
            pendingSignup.DisplayName ?? normalizedEmail,
            verifyUrl);

        var sent = await emailService.SendEmailAsync(
            normalizedEmail,
            emailBuilder.Subject,
            emailBody,
            ct);

        if (!sent)
        {
            pendingSignup.Status = PendingSignupStatus.Pending;
        }

        await pendingSignupRepository.UpdateAsync(pendingSignup, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Magic signup link resent for {EmailHash}. Email sent: {EmailSent}",
            EmailHasher.Hash(normalizedEmail),
            sent);

        return new MagicSignupResult(
            pendingSignup.Id,
            sent ? "EmailSent" : "Pending",
            sent
                ? "Magic link resent"
                : "Resend failed. Please retry.");
    }

    private static string BuildVerifyUrl(string baseUrl, string token)
    {
        var normalized = baseUrl.TrimEnd('/');
        var encodedToken = Uri.EscapeDataString(token);
        return $"{normalized}/authentication/magic-verify?token={encodedToken}";
    }
}
