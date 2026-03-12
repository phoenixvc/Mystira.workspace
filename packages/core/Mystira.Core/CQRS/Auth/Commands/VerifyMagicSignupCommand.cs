using Mystira.Shared.CQRS;

namespace Mystira.Core.CQRS.Auth.Commands;

public record VerifyMagicSignupCommand(string Token) : ICommand<VerifyMagicSignupResult>;
