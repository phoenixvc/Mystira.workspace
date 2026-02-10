using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;
using System.Threading;

namespace Mystira.App.Application.UseCases.Accounts;

/// <summary>
/// Use case for linking a user profile to an account
/// </summary>
public class AddUserProfileToAccountUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddUserProfileToAccountUseCase> _logger;

    public AddUserProfileToAccountUseCase(
        IAccountRepository accountRepository,
        IUserProfileRepository userProfileRepository,
        IUnitOfWork unitOfWork,
        ILogger<AddUserProfileToAccountUseCase> logger)
    {
        _accountRepository = accountRepository;
        _userProfileRepository = userProfileRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Account> ExecuteAsync(string accountId, string profileId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ArgumentException("Account ID cannot be null or empty", nameof(accountId));
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Profile ID cannot be null or empty", nameof(profileId));
        }

        var account = await _accountRepository.GetByIdAsync(accountId, ct);
        if (account == null)
        {
            throw new ArgumentException($"Account not found: {accountId}", nameof(accountId));
        }

        var profile = await _userProfileRepository.GetByIdAsync(profileId, ct);
        if (profile == null)
        {
            throw new ArgumentException($"User profile not found: {profileId}", nameof(profileId));
        }

        // Check if profile is already linked
        if (profile.AccountId == accountId)
        {
            _logger.LogWarning("Profile {ProfileId} is already linked to account {AccountId}", PiiMask.HashId(profileId), PiiMask.HashId(accountId));
            return account;
        }

        // Link profile to account
        profile.AccountId = accountId;
        await _userProfileRepository.UpdateAsync(profile, ct);

        // Add profile ID to account's profile list if not already present
        if (!account.UserProfileIds.Contains(profileId))
        {
            account.UserProfileIds.Add(profileId);
            await _accountRepository.UpdateAsync(account, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Linked profile {ProfileId} to account {AccountId}", PiiMask.HashId(profileId), PiiMask.HashId(accountId));
        return account;
    }
}

