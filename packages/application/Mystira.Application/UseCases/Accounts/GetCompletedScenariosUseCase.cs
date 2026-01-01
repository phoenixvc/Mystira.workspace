using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Accounts;

/// <summary>
/// Use case for retrieving completed scenarios for an account
/// </summary>
public class GetCompletedScenariosUseCase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IScenarioRepository _scenarioRepository;
    private readonly ILogger<GetCompletedScenariosUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetCompletedScenariosUseCase"/> class.</summary>
    /// <param name="accountRepository">The account repository.</param>
    /// <param name="scenarioRepository">The scenario repository.</param>
    /// <param name="logger">The logger.</param>
    public GetCompletedScenariosUseCase(
        IAccountRepository accountRepository,
        IScenarioRepository scenarioRepository,
        ILogger<GetCompletedScenariosUseCase> logger)
    {
        _accountRepository = accountRepository;
        _scenarioRepository = scenarioRepository;
        _logger = logger;
    }

    /// <summary>Retrieves all completed scenarios for the specified account.</summary>
    /// <param name="accountId">The account identifier.</param>
    /// <returns>A list of completed scenarios.</returns>
    public async Task<List<Scenario>> ExecuteAsync(string accountId)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new ArgumentException("Account ID cannot be null or empty", nameof(accountId));
        }

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            throw new ArgumentException($"Account not found: {accountId}", nameof(accountId));
        }

        var scenarios = new List<Scenario>();
        if (account.CompletedScenarioIds != null && account.CompletedScenarioIds.Any())
        {
            foreach (var scenarioId in account.CompletedScenarioIds)
            {
                var scenario = await _scenarioRepository.GetByIdAsync(scenarioId);
                if (scenario != null)
                {
                    scenarios.Add(scenario);
                }
            }
        }

        _logger.LogInformation("Retrieved {Count} completed scenarios for account {AccountId}", scenarios.Count, accountId);
        return scenarios;
    }
}

