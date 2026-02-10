using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.Validation;
using Mystira.App.Application.Mappers;
using Mystira.Contracts.App.Requests.Scenarios;
using Mystira.App.Domain.Models;
using System.Threading;

namespace Mystira.App.Application.UseCases.Scenarios;

/// <summary>
/// Use case for creating a new scenario
/// </summary>
public class CreateScenarioUseCase
{
    private readonly IScenarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateScenarioUseCase> _logger;
    private readonly ValidateScenarioUseCase _validateScenarioUseCase;

    public CreateScenarioUseCase(
        IScenarioRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateScenarioUseCase> logger,
        ValidateScenarioUseCase validateScenarioUseCase)
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
            AgeGroup = request.AgeGroup,
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

