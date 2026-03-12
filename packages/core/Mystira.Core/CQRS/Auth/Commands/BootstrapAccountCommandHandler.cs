using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

using Mystira.Shared.Exceptions;

namespace Mystira.Core.CQRS.Auth.Commands;

public static class BootstrapAccountCommandHandler
{
    public static async Task<Account?> Handle(
        BootstrapAccountCommand command,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<BootstrapAccountCommand> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.ExternalUserId))
        {
            throw new ValidationException("externalUserId", "External user ID is required");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            throw new ValidationException("email", "Email is required");
        }

        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var account = await accountRepository.GetByExternalUserIdAsync(command.ExternalUserId, ct);

        if (account != null)
        {
            account.LastLoginAt = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(account.Email))
            {
                account.Email = normalizedEmail;
            }

            if (string.IsNullOrWhiteSpace(account.DisplayName) && !string.IsNullOrWhiteSpace(command.DisplayName))
            {
                account.DisplayName = command.DisplayName.Trim();
            }

            await accountRepository.UpdateAsync(account, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return account;
        }

        account = await accountRepository.GetByEmailAsync(normalizedEmail, ct);
        if (account != null)
        {
            if (string.IsNullOrWhiteSpace(account.ExternalUserId))
            {
                account.ExternalUserId = command.ExternalUserId;
            }

            account.LastLoginAt = DateTime.UtcNow;
            await accountRepository.UpdateAsync(account, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return account;
        }

        var newAccount = new Account
        {
            Id = Guid.NewGuid().ToString(),
            ExternalUserId = command.ExternalUserId,
            Email = normalizedEmail,
            DisplayName = string.IsNullOrWhiteSpace(command.DisplayName)
                ? normalizedEmail.Split('@')[0]
                : command.DisplayName.Trim(),
            UserProfileIds = new List<string>(),
            CompletedScenarioIds = new List<string>(),
            Subscription = new SubscriptionDetails(),
            Settings = new AccountSettings(),
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        await accountRepository.AddAsync(newAccount, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Bootstrapped account {AccountId} for external user {ExternalUserId}",
            newAccount.Id,
            command.ExternalUserId);

        return newAccount;
    }
}
