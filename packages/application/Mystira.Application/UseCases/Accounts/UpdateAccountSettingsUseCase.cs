using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Accounts;

/// <summary>
/// Use case for updating account settings
/// </summary>
public class UpdateAccountSettingsUseCase
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateAccountSettingsUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateAccountSettingsUseCase"/> class.
    /// </summary>
    /// <param name="repository">The account repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="logger">The logger instance.</param>
    public UpdateAccountSettingsUseCase(
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateAccountSettingsUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Updates account settings.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="settings">The new settings.</param>
    /// <returns>The updated account.</returns>
    public async Task<Account> ExecuteAsync(string accountId, AccountSettings settings)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ArgumentException("Account ID cannot be null or empty", nameof(accountId));
        }

        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        var account = await _repository.GetByIdAsync(accountId);
        if (account == null)
        {
            throw new ArgumentException($"Account not found: {accountId}", nameof(accountId));
        }

        account.Settings = settings;
        await _repository.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated settings for account: {AccountId}", accountId);
        return account;
    }
}

