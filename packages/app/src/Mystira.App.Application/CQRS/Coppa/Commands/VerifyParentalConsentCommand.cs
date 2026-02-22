using Mystira.Shared.CQRS;

namespace Mystira.App.Application.CQRS.Coppa.Commands;

/// <summary>
/// Command to verify parental consent using a verification token (COPPA compliance).
/// </summary>
public record VerifyParentalConsentCommand(
    string VerificationToken,
    string VerificationMethod
) : ICommand<ParentalConsentResult>;
