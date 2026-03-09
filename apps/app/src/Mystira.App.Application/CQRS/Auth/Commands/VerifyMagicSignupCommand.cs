using Mystira.Shared.CQRS;

namespace Mystira.App.Application.CQRS.Auth.Commands;

public record VerifyMagicSignupCommand(string Token) : ICommand<VerifyMagicSignupResult>;
