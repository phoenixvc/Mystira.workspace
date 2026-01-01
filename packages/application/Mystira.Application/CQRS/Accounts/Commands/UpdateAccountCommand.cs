using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Accounts.Commands;

/// <summary>
/// Command to update an existing account's details.
/// </summary>
/// <param name="AccountId">The unique identifier of the account to update.</param>
/// <param name="DisplayName">The new display name for the account.</param>
/// <param name="UserProfileIds">The list of user profile IDs associated with the account.</param>
/// <param name="Subscription">The subscription details for the account.</param>
/// <param name="Settings">The account settings.</param>
public record UpdateAccountCommand(
    string AccountId,
    string? DisplayName,
    List<string>? UserProfileIds,
    SubscriptionDetails? Subscription,
    AccountSettings? Settings
) : ICommand<Account?>;
