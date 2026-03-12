using Mystira.Shared.CQRS;

namespace Mystira.Core.CQRS.Coppa.Commands;

/// <summary>
/// Command to revoke parental consent (triggers data deletion workflow).
/// </summary>
public record RevokeConsentCommand(
    string ChildProfileId,
    string ParentEmailHash
) : ICommand<ParentalConsentResult>;
