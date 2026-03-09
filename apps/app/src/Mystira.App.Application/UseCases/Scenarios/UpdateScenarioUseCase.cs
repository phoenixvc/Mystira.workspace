using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Validation;
using Mystira.App.Application.Mappers;
using Mystira.Contracts.App.Requests.Scenarios;
using Mystira.App.Domain.Models;
using System.Threading;

namespace Mystira.App.Application.UseCases.Scenarios;

/// <summary>
/// Use case for updating an existing scenario
/// </summary>
public class UpdateScenarioUseCase
{
    private readonly IScenarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateScenarioUseCase> _logger;
    private readonly IValidateScenarioUseCase _validateScenarioUseCase;

    public UpdateScenarioUseCase(
        IScenarioRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateScenarioUseCase> logger,
        IValidateScenarioUseCase validateScenarioUseCase)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _validateScenarioUseCase = validateScenarioUseCase;
    }

    public async Task<Scenario?> ExecuteAsync(string id, CreateScenarioRequest request, CancellationToken ct = default)
    {
        var scenario = await _repository.GetByIdAsync(id, ct);
        if (scenario == null)
        {
            return null;
        }

        ScenarioSchemaValidator.ValidateAgainstSchema(request);

        scenario.Title = request.Title;
        scenario.Description = request.Description;
        scenario.Tags = request.Tags ?? new List<string>();
        scenario.Difficulty = ScenarioMapper.MapDifficultyLevel((int)request.Difficulty);
        scenario.SessionLength = ScenarioMapper.MapSessionLength((int)request.SessionLength);
        scenario.Archetypes = ScenarioMapper.ParseArchetypes(request.Archetypes);
        scenario.AgeGroup = request.AgeGroup;
        scenario.MinimumAge = request.MinimumAge;
        scenario.CoreAxes = ScenarioMapper.ParseCoreAxes(request.CoreAxes);
        scenario.Characters = request.Characters?.Select(ScenarioMapper.ToScenarioCharacter).ToList() ?? new List<ScenarioCharacter>();
        scenario.Scenes = request.Scenes?.Select(ScenarioMapper.ToScene).ToList() ?? new List<Scene>();

        await _validateScenarioUseCase.ExecuteAsync(scenario, ct);

        await _repository.UpdateAsync(scenario, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated scenario: {ScenarioId} - {Title}", scenario.Id, scenario.Title);
        return scenario;
    }
}

