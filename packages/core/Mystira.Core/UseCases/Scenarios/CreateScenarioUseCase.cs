using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Core.Validation;
using Mystira.Core.Mappers;
using Mystira.Contracts.App.Requests.Scenarios;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using System.Threading;

namespace Mystira.Core.UseCases.Scenarios;

/// <summary>
/// Use case for creating a new scenario
/// </summary>
public class CreateScenarioUseCase
{
    private readonly IScenarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateScenarioUseCase> _logger;
    private readonly IValidateScenarioUseCase _validateScenarioUseCase;

    public CreateScenarioUseCase(
        IScenarioRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateScenarioUseCase> logger,
        IValidateScenarioUseCase validateScenarioUseCase)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _validateScenarioUseCase = validateScenarioUseCase;
    }

    public async Task<Scenario> ExecuteAsync(CreateScenarioRequest request, CancellationToken ct = default)
    {
        ScenarioSchemaValidator.ValidateAgainstSchema(request);

        var scenario = new Scenario
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Description = request.Description,
            Tags = request.Tags ?? new List<string>(),
            Difficulty = ScenarioMapper.MapDifficultyLevel((int)request.Difficulty),
            SessionLength = ScenarioMapper.MapSessionLength((int)request.SessionLength),
            Archetypes = ScenarioMapper.ParseArchetypes(request.Archetypes),
            AgeGroupId = request.AgeGroup,
            MinimumAge = request.MinimumAge,
            CoreAxes = ScenarioMapper.ParseCoreAxes(request.CoreAxes),
            Characters = request.Characters?.Select(ScenarioMapper.ToScenarioCharacter).ToList() ?? new List<ScenarioCharacter>(),
            Scenes = request.Scenes?.Select(ScenarioMapper.ToScene).ToList() ?? new List<Scene>(),
            CreatedAt = DateTime.UtcNow
        };

        await _validateScenarioUseCase.ExecuteAsync(scenario, ct);

        await _repository.AddAsync(scenario, ct);

        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving scenario: {ScenarioId}", scenario.Id);
            throw;
        }

        _logger.LogInformation("Created new scenario: {ScenarioId} - {Title}", scenario.Id, scenario.Title);
        return scenario;
    }
}

