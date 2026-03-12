using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.Accounts;

/// <summary>
/// Use case for retrieving an account by ID
/// </summary>
public class GetAccountUseCase
{
    private readonly IAccountRepository _repository;
    private readonly ILogger<GetAccountUseCase> _logger;

    public GetAccountUseCase(
        IAccountRepository repository,
        ILogger<GetAccountUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Account?> ExecuteAsync(string accountId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ValidationException("accountId", "accountId is required");
        }

        var account = await _repository.GetByIdAsync(accountId, ct);

        if (account == null)
        {
            _logger.LogWarning("Account not found: {AccountId}", PiiMask.HashId(accountId));
        }
        else
        {
            _logger.LogDebug("Retrieved account: {AccountId}", PiiMask.HashId(accountId));
        }

        return account;
    }
}

