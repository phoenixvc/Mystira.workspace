namespace Mystira.Core.CQRS.Accounts.Commands;

public record DeleteAccountCommand(string AccountId) : ICommand<bool>;
