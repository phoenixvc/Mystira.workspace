using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Contracts.App.Requests.Accounts;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.Accounts;

/// <summary>
/// Use case for updating account details
/// </summary>
public class UpdateAccountUseCase
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAccountUseCase> _logger;

    public UpdateAccountUseCase(
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateAccountUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Account> ExecuteAsync(string accountId, UpdateAccountRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ValidationException("accountId", "accountId is required");
        }

        if (request == null)
        {
            throw new ValidationException("request", "request is required");
        }

        var account = await _repository.GetByIdAsync(accountId, ct);
        if (account == null)
        {
            throw new NotFoundException("Account", accountId);
        }

        // Update properties if provided
        if (request.DisplayName != null)
        {
            account.DisplayName = request.DisplayName;
        }

        if (request.Settings != null)
        {
            // Map from Contracts AccountSettings to Domain AccountSettings
            // Only update properties that exist in the Contracts type
            account.Settings ??= new AccountSettings();
            account.Settings.PreferredLanguage = request.Settings.PreferredLanguage ?? account.Settings.PreferredLanguage;
            account.Settings.NotificationsEnabled = request.Settings.NotificationsEnabled ?? account.Settings.NotificationsEnabled;
            account.Settings.Theme = request.Settings.Theme ?? account.Settings.Theme;
        }

        account.LastLoginAt = DateTime.UtcNow;

        await _repository.UpdateAsync(account, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated account: {AccountId}", PiiMask.HashId(accountId));
        return account;
    }
}

