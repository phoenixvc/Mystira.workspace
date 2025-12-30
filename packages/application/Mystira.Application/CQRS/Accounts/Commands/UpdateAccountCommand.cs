using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.Accounts.Commands;

public record UpdateAccountCommand(
    string AccountId,
    string? DisplayName,
    List<string>? UserProfileIds,
    SubscriptionDetails? Subscription,
    AccountSettings? Settings
) : ICommand<Account?>;
