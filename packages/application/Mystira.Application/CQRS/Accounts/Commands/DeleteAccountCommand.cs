namespace Mystira.Application.CQRS.Accounts.Commands;

/// <summary>
/// Command to delete an account and all associated data.
/// </summary>
/// <param name="AccountId">The unique identifier of the account to delete.</param>
public record DeleteAccountCommand(string AccountId) : ICommand<bool>;
