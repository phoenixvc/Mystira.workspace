using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;
using System.Threading;

namespace Mystira.App.Application.UseCases.Accounts;

/// <summary>
/// Use case for retrieving an account by email address
/// </summary>
public class GetAccountByEmailUseCase
{
    private readonly IAccountRepository _repository;
    private readonly ILogger<GetAccountByEmailUseCase> _logger;

    public GetAccountByEmailUseCase(
        IAccountRepository repository,
        ILogger<GetAccountByEmailUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Account?> ExecuteAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        }

        var account = await _repository.GetByEmailAsync(email, ct);

        if (account == null)
        {
            _logger.LogWarning("Account not found for email: {Email}", PiiMask.MaskEmail(email));
        }
        else
        {
            _logger.LogDebug("Retrieved account by email: {Email}", PiiMask.MaskEmail(email));
        }

        return account;
    }
}

