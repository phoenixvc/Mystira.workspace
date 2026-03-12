using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.Accounts;

/// <summary>
/// Use case for updating account settings
/// </summary>
public class UpdateAccountSettingsUseCase
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAccountSettingsUseCase> _logger;

    public UpdateAccountSettingsUseCase(
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateAccountSettingsUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Account> ExecuteAsync(string accountId, AccountSettings settings, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ValidationException("accountId", "accountId is required");
        }

        if (settings == null)
        {
            throw new ValidationException("settings", "settings is required");
        }

        var account = await _repository.GetByIdAsync(accountId, ct);
        if (account == null)
        {
            throw new NotFoundException("Account", accountId);
        }

        account.Settings = settings;
        await _repository.UpdateAsync(account, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated settings for account: {AccountId}", PiiMask.HashId(accountId));
        return account;
    }
}

