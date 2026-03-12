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
/// Use case for updating subscription details
/// </summary>
public class UpdateSubscriptionUseCase
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSubscriptionUseCase> _logger;

    public UpdateSubscriptionUseCase(
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateSubscriptionUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Account> ExecuteAsync(string accountId, UpdateSubscriptionRequest request, CancellationToken ct = default)
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

        await _repository.UpdateAsync(account, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated subscription for account: {AccountId}", PiiMask.HashId(accountId));
        return account;
    }
}

