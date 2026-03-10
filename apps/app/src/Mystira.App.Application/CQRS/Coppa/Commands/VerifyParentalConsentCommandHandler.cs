using Microsoft.Extensions.Logging;
using Mystira.App.Application.Helpers;
using Mystira.App.Application.Ports;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;
using Mystira.Shared.Exceptions;

namespace Mystira.App.Application.CQRS.Coppa.Commands;

/// <summary>
/// Handles verifying parental consent using a verification token.
/// </summary>
public static class VerifyParentalConsentCommandHandler
{
    public static async Task<ParentalConsentResult> Handle(
        VerifyParentalConsentCommand command,
        ICoppaConsentRepository consentRepository,
        IUnitOfWork unitOfWork,
        ILogger<VerifyParentalConsentCommand> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.VerificationToken))
            throw new ValidationException("verificationToken", "Verification token is required");

        var consent = await consentRepository.GetByVerificationTokenAsync(command.VerificationToken, ct);
        if (consent == null)
        {
            return new ParentalConsentResult("", "NotFound", "Invalid or expired verification token");
        }

        if (consent.IsTokenExpired)
        {
            consent.Status = ConsentStatus.Expired;
            await consentRepository.UpdateAsync(consent, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return new ParentalConsentResult(consent.Id, "Expired", "Verification token has expired. Please request a new one.");
        }

        if (consent.IsActive)
        {
            return new ParentalConsentResult(consent.Id, "AlreadyVerified", "Consent already verified");
        }

        // Parse and validate the verification method
        if (!Enum.TryParse<ConsentVerificationMethod>(command.VerificationMethod, true, out var method)
            || method == ConsentVerificationMethod.None)
        {
            return new ParentalConsentResult(consent.Id, "InvalidMethod",
                $"Invalid verification method: {command.VerificationMethod}. Supported: Email, CreditCard, GovernmentId, VideoCall, SignedForm");
        }

        consent.Approve(method);
        await consentRepository.UpdateAsync(consent, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation(
            "Parental consent verified for child profile {ChildProfileIdHash}, method: {Method}",
            LogAnonymizer.HashId(consent.ChildProfileId), method);

        return new ParentalConsentResult(consent.Id, "Verified", "Parental consent has been verified");
    }
}
