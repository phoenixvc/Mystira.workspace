using Microsoft.Extensions.Logging;
using Mystira.App.Application.Helpers;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.Accounts.Commands;

/// <summary>
/// Wolverine handler for UpdateAccountCommand.
/// Uses static method convention for cleaner, more testable code.
/// </summary>
public static class UpdateAccountCommandHandler
{
    /// <summary>
    /// Handles the UpdateAccountCommand by updating an existing account in the repository.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<Account?> Handle(
        UpdateAccountCommand command,
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        var account = await repository.GetByIdAsync(command.AccountId, ct);
        if (account == null)
        {
            logger.LogWarning("Account not found: {AccountId}", LogAnonymizer.HashId(command.AccountId));
            return null;
        }

        if (!string.IsNullOrEmpty(command.DisplayName))
        {
            account.DisplayName = command.DisplayName;
        }

        if (command.UserProfileIds != null)
        {
            account.UserProfileIds = command.UserProfileIds;
        }

        if (command.Subscription != null)
        {
            account.Subscription = command.Subscription;
        }

        if (command.Settings != null)
        {
            account.Settings = command.Settings;
        }

        await repository.UpdateAsync(account, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Updated account {AccountId}", LogAnonymizer.HashId(account.Id));
        return account;
    }
}
