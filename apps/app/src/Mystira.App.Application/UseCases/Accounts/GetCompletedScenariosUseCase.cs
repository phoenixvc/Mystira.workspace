using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.Accounts;

/// <summary>
/// Use case for retrieving completed scenarios for an account
/// </summary>
public class GetCompletedScenariosUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IScenarioRepository _scenarioRepository;
    private readonly ILogger<GetCompletedScenariosUseCase> _logger;

    public GetCompletedScenariosUseCase(
        IAccountRepository accountRepository,
        IScenarioRepository scenarioRepository,
        ILogger<GetCompletedScenariosUseCase> logger)
    {
        _accountRepository = accountRepository;
        _scenarioRepository = scenarioRepository;
        _logger = logger;
    }

    public async Task<List<Scenario>> ExecuteAsync(string accountId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ValidationException("accountId", "accountId is required");
        }

        var account = await _accountRepository.GetByIdAsync(accountId, ct);
        if (account == null)
        {
            throw new NotFoundException("Account", accountId);
        }

        var scenarios = new List<Scenario>();
        if (account.CompletedScenarioIds != null && account.CompletedScenarioIds.Any())
        {
            foreach (var scenarioId in account.CompletedScenarioIds)
            {
                var scenario = await _scenarioRepository.GetByIdAsync(scenarioId, ct);
                if (scenario != null)
                {
                    scenarios.Add(scenario);
                }
            }
        }

        _logger.LogInformation("Retrieved {Count} completed scenarios for account {AccountId}", scenarios.Count, PiiMask.HashId(accountId));
        return scenarios;
    }
}

