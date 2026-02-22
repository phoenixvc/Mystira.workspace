using Mystira.Shared.CQRS;

namespace Mystira.App.Application.CQRS.Coppa.Commands;

/// <summary>
/// Command to request parental consent for a child profile (COPPA compliance).
/// </summary>
public record RequestParentalConsentCommand(
    string ChildProfileId,
    string ParentEmail,
    string ChildDisplayName
) : ICommand<ParentalConsentResult>;

public record ParentalConsentResult(
    string ConsentId,
    string Status,
    string Message
);
