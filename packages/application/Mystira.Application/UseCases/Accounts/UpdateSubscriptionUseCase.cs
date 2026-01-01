using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Requests.Accounts;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Accounts;

/// <summary>
/// Use case for updating subscription details
/// </summary>
public class UpdateSubscriptionUseCase
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSubscriptionUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateSubscriptionUseCase"/> class.
    /// </summary>
    /// <param name="repository">The account repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="logger">The logger instance.</param>
    public UpdateSubscriptionUseCase(
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateSubscriptionUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Updates subscription details for an account.
    /// </summary>
    /// <param name="accountId">The account identifier.</param>
    /// <param name="request">The request containing subscription update details.</param>
    /// <returns>The updated account.</returns>
    public async Task<Account> ExecuteAsync(string accountId, UpdateSubscriptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ArgumentException("Account ID cannot be null or empty", nameof(accountId));
        }

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var account = await _repository.GetByIdAsync(accountId);
        if (account == null)
        {
            throw new ArgumentException($"Account not found: {accountId}", nameof(accountId));
        }

        // Update subscription properties - convert from Contracts enum to Domain enum
        account.Subscription.Type = (SubscriptionType)(int)request.Type;

        if (request.ProductId != null)
        {
            account.Subscription.ProductId = request.ProductId;
        }

        if (request.ValidUntil.HasValue)
        {
            account.Subscription.ValidUntil = request.ValidUntil;
        }

        if (request.PurchaseToken != null)
        {
            account.Subscription.PurchaseToken = request.PurchaseToken;
        }

        if (request.PurchasedScenarios != null)
        {
            account.Subscription.PurchasedScenarios = request.PurchasedScenarios;
        }

        account.Subscription.LastVerified = DateTime.UtcNow;

        await _repository.UpdateAsync(account);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated subscription for account: {AccountId}", accountId);
        return account;
    }
}

