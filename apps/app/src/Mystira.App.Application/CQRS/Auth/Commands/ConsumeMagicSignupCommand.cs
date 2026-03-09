using Mystira.Shared.CQRS;

namespace Mystira.App.Application.CQRS.Auth.Commands;

public record ConsumeMagicSignupCommand(string Token) : ICommand<ConsumeMagicSignupResult>;
