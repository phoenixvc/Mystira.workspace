using Mystira.Shared.CQRS;

namespace Mystira.App.Application.CQRS.Coppa.Queries;

/// <summary>
/// Query to get the current consent status for a child profile.
/// </summary>
public record GetConsentStatusQuery(string ChildProfileId) : IQuery<ConsentStatusResult>;

public record ConsentStatusResult(
    string ConsentId,
    string ChildProfileId,
    string Status,
    bool IsActive,
    DateTime? ConsentedAt,
    string? VerificationMethod,
    string Message
);
