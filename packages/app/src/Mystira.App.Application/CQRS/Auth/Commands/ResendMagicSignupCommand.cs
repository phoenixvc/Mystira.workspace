using Mystira.Shared.CQRS;

namespace Mystira.App.Application.CQRS.Auth.Commands;

public record ResendMagicSignupCommand(
    string Email,
    string VerificationBaseUrl
) : ICommand<MagicSignupResult>;
