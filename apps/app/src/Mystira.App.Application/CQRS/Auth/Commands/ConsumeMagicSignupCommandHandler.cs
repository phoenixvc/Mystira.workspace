using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;
using Mystira.Shared.Exceptions;

namespace Mystira.App.Application.CQRS.Auth.Commands;

public static class ConsumeMagicSignupCommandHandler
{
    public static async Task<ConsumeMagicSignupResult> Handle(
        ConsumeMagicSignupCommand command,
        IPendingSignupRepository pendingSignupRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<ConsumeMagicSignupCommand> logger,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
        {
            throw new ValidationException("token", "Token is required");
        }

        var tokenHash = MagicLinkTokenService.HashToken(command.Token);
        var pendingSignup = await pendingSignupRepository.GetByVerificationTokenHashAsync(tokenHash, ct);

        if (pendingSignup == null)
        {
            return new ConsumeMagicSignupResult("Invalid", "Invalid magic link", null);
        }

        if (pendingSignup.IsVerificationTokenExpired)
        {
            pendingSignup.MarkExpired();
            await pendingSignupRepository.UpdateAsync(pendingSignup, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return new ConsumeMagicSignupResult("Expired", "Magic link expired", null);
        }

        if (pendingSignup.Status == PendingSignupStatus.Consumed)
        {
            return new ConsumeMagicSignupResult("Consumed", "Magic link already used", null);
        }

        if (pendingSignup.Status == PendingSignupStatus.Verified && pendingSignup.IsVerifiedWindowExpired)
        {
            pendingSignup.MarkExpired();
            await pendingSignupRepository.UpdateAsync(pendingSignup, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return new ConsumeMagicSignupResult("Expired", "Verification window expired", null);
        }

        if (pendingSignup.Status != PendingSignupStatus.Verified)
        {
            pendingSignup.MarkVerified(DateTime.UtcNow.AddDays(7));
        }

        var account = await accountRepository.GetByEmailAsync(pendingSignup.Email, ct);
        if (account == null)
        {
            account = new Account
            {
                Id = Guid.NewGuid().ToString(),
                ExternalUserId = string.Empty,
                Email = pendingSignup.Email,
                DisplayName = pendingSignup.DisplayName ?? pendingSignup.Email.Split('@')[0],
                UserProfileIds = new List<string>(),
                CompletedScenarioIds = new List<string>(),
                Subscription = new SubscriptionDetails(),
                Settings = new AccountSettings(),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await accountRepository.AddAsync(account, ct);
        }
        else
        {
            account.LastLoginAt = DateTime.UtcNow;
            await accountRepository.UpdateAsync(account, ct);
        }

        pendingSignup.MarkConsumed();
        await pendingSignupRepository.UpdateAsync(pendingSignup, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Magic signup consumed for {EmailHash} and account {AccountIdHash}",
            EmailHasher.Hash(account.Email),
            EmailHasher.Hash(account.Id));

        return new ConsumeMagicSignupResult("Authenticated", "Authentication successful", account);
    }
}
