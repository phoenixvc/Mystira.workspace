using Mystira.Shared.CQRS;

namespace Mystira.Core.CQRS.Auth.Commands;

public record ResendMagicSignupCommand(
    string Email,
    string VerificationBaseUrl
) : ICommand<MagicSignupResult>;
