using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Mystira.App.Application.Helpers;
using Mystira.App.Application.Ports;
using Mystira.App.Application.Services;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;
using Mystira.Shared.Exceptions;

namespace Mystira.App.Application.CQRS.Coppa.Commands;

/// <summary>
/// Handles requesting parental consent for a child profile.
/// Creates a consent record, generates a verification token, and sends the consent email.
/// </summary>
public static class RequestParentalConsentCommandHandler
{
    public static async Task<ParentalConsentResult> Handle(
        RequestParentalConsentCommand command,
        ICoppaConsentRepository consentRepository,
        IEmailService emailService,
        ConsentEmailBuilder emailBuilder,
        IUnitOfWork unitOfWork,
        ILogger<RequestParentalConsentCommand> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.ChildProfileId))
            throw new ValidationException("childProfileId", "Child profile ID is required");
        if (string.IsNullOrWhiteSpace(command.ParentEmail))
            throw new ValidationException("parentEmail", "Parent email is required");

        // Check for existing consent - update if expired/denied, skip if active
        var existing = await consentRepository.GetByChildProfileIdAsync(command.ChildProfileId, ct);
        if (existing != null && existing.IsActive)
        {
            return new ParentalConsentResult(existing.Id, "AlreadyVerified", "Consent already verified for this profile");
        }

        // Hash the parent email (never store plain text)
        var emailHash = EmailHasher.Hash(command.ParentEmail);

        // Generate a cryptographically secure verification token
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        ParentalConsent consent;
        if (existing != null)
        {
            // Re-use existing record (expired/denied/revoked) instead of creating duplicate
            consent = existing;
            consent.ParentEmailHash = emailHash;
            consent.ChildDisplayName = command.ChildDisplayName;
            consent.Status = ConsentStatus.Pending;
            consent.VerificationToken = token;
            consent.VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(48);
            consent.RevokedAt = null;
            consent.ConsentedAt = null;
            consent.VerifiedAt = null;
            consent.VerificationMethod = ConsentVerificationMethod.None;
            consent.UpdatedAt = DateTime.UtcNow;
            await consentRepository.UpdateAsync(consent, ct);
        }
        else
        {
            consent = new ParentalConsent
            {
                ParentEmailHash = emailHash,
                ChildProfileId = command.ChildProfileId,
                ChildDisplayName = command.ChildDisplayName,
                Status = ConsentStatus.Pending,
                VerificationToken = token,
                VerificationTokenExpiresAt = DateTime.UtcNow.AddHours(48)
            };
            await consentRepository.AddAsync(consent, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);

        // Send consent verification email
        var emailBody = emailBuilder.BuildVerificationEmail(command.ChildDisplayName, token);
        var emailSent = await emailService.SendEmailAsync(
            command.ParentEmail,
            emailBuilder.Subject,
            emailBody,
            ct);

        if (emailSent)
        {
            consent.Status = ConsentStatus.EmailSent;
            await consentRepository.UpdateAsync(consent, ct);
            await unitOfWork.SaveChangesAsync(ct);
        }
        else
        {
            logger.LogWarning(
                "Failed to send consent email for child profile {ChildProfileIdHash}, consent ID: {ConsentId}. Consent saved as Pending.",
                LogAnonymizer.HashId(command.ChildProfileId), consent.Id);
        }

        logger.LogInformation(
            "Parental consent requested for child profile {ChildProfileIdHash}, consent ID: {ConsentId}, email sent: {EmailSent}",
            LogAnonymizer.HashId(command.ChildProfileId), consent.Id, emailSent);

        var status = emailSent ? "EmailSent" : "Pending";
        var message = emailSent
            ? "Consent request email sent to parent"
            : "Consent request created but email delivery failed. Please try again.";

        return new ParentalConsentResult(consent.Id, status, message);
    }
}
