using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Scenarios;

/// <summary>
/// Use case for retrieving a single scenario by ID
/// </summary>
public class GetScenarioUseCase
{
    private readonly IScenarioRepository _repository;
    private readonly ILogger<GetScenarioUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetScenarioUseCase"/> class.
    /// </summary>
    /// <param name="repository">The scenario repository.</param>
    /// <param name="logger">The logger instance.</param>
    public GetScenarioUseCase(
        IScenarioRepository repository,
        ILogger<GetScenarioUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a scenario by its unique identifier.
    /// </summary>
    /// <param name="scenarioId">The scenario identifier.</param>
    /// <returns>The scenario if found; otherwise, null.</returns>
    public async Task<Scenario?> ExecuteAsync(string scenarioId)
    {
        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            throw new ArgumentException("Scenario ID cannot be null or empty", nameof(scenarioId));
        }

        var scenario = await _repository.GetByIdAsync(scenarioId);

        if (scenario == null)
        {
            _logger.LogWarning("Scenario not found: {ScenarioId}", scenarioId);
        }
        else
        {
            _logger.LogDebug("Retrieved scenario: {ScenarioId}", scenarioId);
        }

        return scenario;
    }
}

