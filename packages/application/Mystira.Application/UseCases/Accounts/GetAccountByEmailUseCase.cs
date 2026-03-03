using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Accounts;

/// <summary>
/// Use case for retrieving an account by email address
/// </summary>
public class GetAccountByEmailUseCase
{
    private readonly IAccountRepository _repository;
    private readonly ILogger<GetAccountByEmailUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAccountByEmailUseCase"/> class.
    /// </summary>
    /// <param name="repository">The account repository.</param>
    /// <param name="logger">The logger instance.</param>
    public GetAccountByEmailUseCase(
        IAccountRepository repository,
        ILogger<GetAccountByEmailUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves an account by email address.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>The account if found; otherwise, null.</returns>
    public async Task<Account?> ExecuteAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        }

        var account = await _repository.GetByEmailAsync(email);

        if (account == null)
        {
            _logger.LogWarning("Account not found for email: {Email}", email);
        }
        else
        {
            _logger.LogDebug("Retrieved account by email: {Email}", email);
        }

        return account;
    }
}

