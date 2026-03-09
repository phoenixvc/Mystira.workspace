namespace Mystira.App.Application.CQRS.Auth.Commands;

public record MagicSignupResult(
    string PendingSignupId,
    string Status,
    string Message
);
