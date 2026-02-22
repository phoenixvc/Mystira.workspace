using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports;
using Mystira.App.Domain.Models;
using Mystira.Shared.Exceptions;

namespace Mystira.App.Application.CQRS.Coppa.Queries;

/// <summary>
/// Handles querying consent status for a child profile.
/// </summary>
public static class GetConsentStatusQueryHandler
{
    public static async Task<ConsentStatusResult> Handle(
        GetConsentStatusQuery query,
        ICoppaConsentRepository consentRepository,
        ILogger<GetConsentStatusQuery> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.ChildProfileId))
            throw new ValidationException("childProfileId", "Child profile ID is required");

        var consent = await consentRepository.GetByChildProfileIdAsync(query.ChildProfileId, ct);
        if (consent == null)
        {
            return new ConsentStatusResult(
                "",
                query.ChildProfileId,
                "None",
                false,
                null,
                null,
                "No consent record exists for this profile"
            );
        }

        var message = consent.Status switch
        {
            ConsentStatus.Verified => "Parental consent is active",
            ConsentStatus.Pending => "Consent requested, awaiting parent verification",
            ConsentStatus.EmailSent => "Verification email sent to parent",
            ConsentStatus.Expired => "Consent verification has expired",
            ConsentStatus.Revoked => "Consent has been revoked by parent",
            ConsentStatus.Denied => "Consent was denied",
            _ => consent.Status.ToString()
        };

        return new ConsentStatusResult(
            consent.Id,
            consent.ChildProfileId,
            consent.Status.ToString(),
            consent.IsActive,
            consent.ConsentedAt,
            consent.VerificationMethod.ToString(),
            message
        );
    }
}
