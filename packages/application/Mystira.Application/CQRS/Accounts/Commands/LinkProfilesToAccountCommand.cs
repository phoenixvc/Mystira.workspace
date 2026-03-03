namespace Mystira.Application.CQRS.Accounts.Commands;

/// <summary>
/// Command to link multiple user profiles to an account.
/// </summary>
/// <param name="AccountId">The unique identifier of the account.</param>
/// <param name="UserProfileIds">The list of user profile IDs to link to the account.</param>
public record LinkProfilesToAccountCommand(
    string AccountId,
    List<string> UserProfileIds
) : ICommand<bool>;
