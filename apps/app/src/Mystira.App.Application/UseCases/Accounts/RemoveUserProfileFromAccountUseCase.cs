using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.Accounts;

/// <summary>
/// Use case for unlinking a user profile from an account
/// </summary>
public class RemoveUserProfileFromAccountUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveUserProfileFromAccountUseCase> _logger;

    public RemoveUserProfileFromAccountUseCase(
        IAccountRepository accountRepository,
        IUserProfileRepository userProfileRepository,
        IUnitOfWork unitOfWork,
        ILogger<RemoveUserProfileFromAccountUseCase> logger)
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
            throw new ValidationException("accountId", "accountId is required");
        }

        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ValidationException("profileId", "profileId is required");
        }

        var account = await _accountRepository.GetByIdAsync(accountId, ct);
        if (account == null)
        {
            throw new NotFoundException("Account", accountId);
        }

        var profile = await _userProfileRepository.GetByIdAsync(profileId, ct);
        if (profile == null)
        {
            throw new NotFoundException("UserProfile", profileId);
        }

        // Unlink profile from account
        if (profile.AccountId == accountId)
        {
            profile.AccountId = null;
            await _userProfileRepository.UpdateAsync(profile, ct);
        }

        // Remove profile ID from account's profile list
        if (account.UserProfileIds.Contains(profileId))
        {
            account.UserProfileIds.Remove(profileId);
            await _accountRepository.UpdateAsync(account, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Unlinked profile {ProfileId} from account {AccountId}", PiiMask.HashId(profileId), PiiMask.HashId(accountId));
        return account;
    }
}

