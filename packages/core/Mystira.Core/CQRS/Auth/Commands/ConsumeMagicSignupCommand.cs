using Mystira.Shared.CQRS;

namespace Mystira.Core.CQRS.Auth.Commands;

public record ConsumeMagicSignupCommand(string Token) : ICommand<ConsumeMagicSignupResult>;
