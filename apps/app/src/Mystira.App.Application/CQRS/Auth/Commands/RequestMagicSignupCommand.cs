using Mystira.Shared.CQRS;

namespace Mystira.App.Application.CQRS.Auth.Commands;

public record RequestMagicSignupCommand(
    string Email,
    string? DisplayName,
    string VerificationBaseUrl
) : ICommand<MagicSignupResult>;
