using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.Core.UseCases.Scenarios;

/// <summary>
/// Use case for retrieving a single scenario by ID
/// </summary>
public class GetScenarioUseCase
{
    private readonly IScenarioRepository _repository;
    private readonly ILogger<GetScenarioUseCase> _logger;

    public GetScenarioUseCase(
        IScenarioRepository repository,
        ILogger<GetScenarioUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Scenario?> ExecuteAsync(string scenarioId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(scenarioId))
        {
            throw new ValidationException("scenarioId", "scenarioId is required");
        }

        var scenario = await _repository.GetByIdAsync(scenarioId, ct);

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

