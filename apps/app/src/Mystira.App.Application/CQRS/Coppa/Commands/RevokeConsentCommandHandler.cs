using Microsoft.Extensions.Logging;
using Mystira.App.Application.Helpers;
using Mystira.App.Application.Ports;
using Mystira.App.Domain.Models;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.CQRS.Coppa.Commands;

/// <summary>
/// Handles revoking parental consent. Creates a data deletion request.
/// Uses IUnitOfWork to ensure atomicity between consent revocation and deletion request creation.
/// </summary>
public static class RevokeConsentCommandHandler
{
    public static async Task<ParentalConsentResult> Handle(
        RevokeConsentCommand command,
        ICoppaConsentRepository consentRepository,
        IDataDeletionRepository deletionRepository,
        IUnitOfWork unitOfWork,
        ILogger<RevokeConsentCommand> logger,
        CancellationToken ct)
    {
        var consent = await consentRepository.GetByChildProfileIdAsync(command.ChildProfileId, ct);
        if (consent == null)
        {
            return new ParentalConsentResult("", "NotFound", "No consent record found for this profile");
        }

        if (consent.ParentEmailHash != command.ParentEmailHash)
        {
            return new ParentalConsentResult("", "Unauthorized", "Parent email does not match consent record");
        }

        consent.Revoke();
        await consentRepository.UpdateAsync(consent, ct);

        // Create a data deletion request (7-day soft delete per COPPA)
        var deletionRequest = new DataDeletionRequest
        {
            ChildProfileId = command.ChildProfileId,
            RequestedBy = DeletionRequestSource.Parent,
            Status = DeletionStatus.Pending,
            ScheduledDeletionAt = DateTime.UtcNow.AddDays(7)
        };
        deletionRequest.AddAuditEntry("ConsentRevoked", command.ParentEmailHash);

        await deletionRepository.AddAsync(deletionRequest, ct);

        // Save both operations atomically
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation(
            "Consent revoked for child profile {ChildProfileIdHash}. Deletion scheduled for {DeletionDate}",
            LogAnonymizer.HashId(command.ChildProfileId), deletionRequest.ScheduledDeletionAt);

        return new ParentalConsentResult(consent.Id, "Revoked", "Consent revoked. Data deletion scheduled.");
    }
}
