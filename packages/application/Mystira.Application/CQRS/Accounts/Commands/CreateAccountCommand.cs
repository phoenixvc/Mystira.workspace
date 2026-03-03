using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Accounts.Commands;

/// <summary>
/// Command to create a new account in the system.
/// </summary>
/// <param name="ExternalUserId">The external user ID from the authentication provider.</param>
/// <param name="Email">The email address for the account.</param>
/// <param name="DisplayName">The display name for the account.</param>
/// <param name="UserProfileIds">The list of user profile IDs to associate with the account.</param>
/// <param name="Subscription">The subscription details for the account.</param>
/// <param name="Settings">The account settings.</param>
public record CreateAccountCommand(
    string ExternalUserId,
    string Email,
    string? DisplayName,
    List<string>? UserProfileIds,
    SubscriptionDetails? Subscription,
    AccountSettings? Settings
) : ICommand<Account>;
