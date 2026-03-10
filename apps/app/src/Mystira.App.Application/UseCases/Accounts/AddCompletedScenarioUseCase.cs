using Microsoft.Extensions.Logging;
using Mystira.App.Application.Helpers;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.Accounts;

/// <summary>
/// Use case for marking a scenario as completed for an account
/// </summary>
public class AddCompletedScenarioUseCase
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddCompletedScenarioUseCase> _logger;

    public AddCompletedScenarioUseCase(
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<AddCompletedScenarioUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Account> ExecuteAsync(string accountId, string scenarioId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ValidationException("accountId", "accountId is required");
        }

        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            throw new ValidationException("scenarioId", "scenarioId is required");
        }

        var account = await _repository.GetByIdAsync(accountId, ct);
        if (account == null)
        {
            throw new NotFoundException("Account", accountId);
        }

        if (account.CompletedScenarioIds == null)
        {
            account.CompletedScenarioIds = new List<string>();
        }

        if (!account.CompletedScenarioIds.Contains(scenarioId))
        {
            account.CompletedScenarioIds.Add(scenarioId);
            await _repository.UpdateAsync(account, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Added completed scenario {ScenarioId} to account {AccountId}", scenarioId, LogAnonymizer.HashId(accountId));
        }
        else
        {
            _logger.LogDebug("Scenario {ScenarioId} already marked as completed for account {AccountId}", scenarioId, LogAnonymizer.HashId(accountId));
        }

        return account;
    }
}

