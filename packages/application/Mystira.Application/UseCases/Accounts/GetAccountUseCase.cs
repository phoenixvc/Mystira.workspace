using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Accounts;

/// <summary>
/// Use case for retrieving an account by ID
/// </summary>
public class GetAccountUseCase
{
    private readonly IAccountRepository _repository;
    private readonly ILogger<GetAccountUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAccountUseCase"/> class.
    /// </summary>
    /// <param name="repository">The account repository.</param>
    /// <param name="logger">The logger instance.</param>
    public GetAccountUseCase(
        IAccountRepository repository,
        ILogger<GetAccountUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves an account by its identifier.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <returns>The account if found; otherwise, null.</returns>
    public async Task<Account?> ExecuteAsync(string accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ArgumentException("Account ID cannot be null or empty", nameof(accountId));
        }

        var account = await _repository.GetByIdAsync(accountId);

        if (account == null)
        {
            _logger.LogWarning("Account not found: {AccountId}", accountId);
        }
        else
        {
            _logger.LogDebug("Retrieved account: {AccountId}", accountId);
        }

        return account;
    }
}

