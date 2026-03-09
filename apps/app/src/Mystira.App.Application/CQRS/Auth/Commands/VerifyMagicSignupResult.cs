namespace Mystira.App.Application.CQRS.Auth.Commands;

public record VerifyMagicSignupResult(
    string Status,
    string Message,
    bool CanContinueWithEmail,
    bool CanContinueWithEntra
);
