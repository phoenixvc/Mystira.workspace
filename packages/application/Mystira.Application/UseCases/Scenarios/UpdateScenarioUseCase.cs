using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Validation;
using Mystira.Application.Mappers;
using Mystira.Contracts.App.Requests.Scenarios;
using Mystira.Domain.Models;
using NJsonSchema;

namespace Mystira.Application.UseCases.Scenarios;

/// <summary>
/// Use case for updating an existing scenario
/// </summary>
public class UpdateScenarioUseCase
{
    private readonly IScenarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateScenarioUseCase> _logger;
    private readonly ValidateScenarioUseCase _validateScenarioUseCase;

    private static readonly JsonSchema ScenarioJsonSchema = JsonSchema.FromJsonAsync(ScenarioSchemaDefinitions.StorySchema).GetAwaiter().GetResult();

    private static readonly System.Text.Json.JsonSerializerOptions SchemaSerializerOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateScenarioUseCase"/> class.
    /// </summary>
    /// <param name="repository">The scenario repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="validateScenarioUseCase">The scenario validation use case.</param>
    public UpdateScenarioUseCase(
        IScenarioRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateScenarioUseCase> logger,
        ValidateScenarioUseCase validateScenarioUseCase)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _validateScenarioUseCase = validateScenarioUseCase;
    }

    /// <summary>
    /// Updates an existing scenario with the provided request data.
    /// </summary>
    /// <param name="id">The scenario identifier.</param>
    /// <param name="request">The update request containing new scenario data.</param>
    /// <returns>The updated scenario if found; otherwise, null.</returns>
    public async Task<Scenario?> ExecuteAsync(string id, UpdateScenarioRequest request)
    {
        var scenario = await _repository.GetByIdAsync(id);
        if (scenario == null)
        {
            return null;
        }

        ValidateAgainstSchema(request);

        scenario.Title = request.Title;
        scenario.Description = request.Description;
        scenario.Tags = request.Tags ?? new List<string>();
        scenario.Difficulty = ScenarioMapper.MapDifficultyLevel((int)request.Difficulty);
        scenario.SessionLength = ScenarioMapper.MapSessionLength((int)request.SessionLength);
        scenario.Archetypes = ScenarioMapper.ParseArchetypes(request.Archetypes);
        scenario.AgeGroupId = request.AgeGroup;
        scenario.MinimumAge = request.MinimumAge;
        scenario.CoreAxes = ScenarioMapper.ParseCoreAxes(request.CoreAxes);
        scenario.Characters = request.Characters?.Select(ScenarioMapper.ToScenarioCharacter).ToList() ?? new List<ScenarioCharacter>();
        scenario.Scenes = request.Scenes?.Select(ScenarioMapper.ToScene).ToList() ?? new List<Scene>();
        scenario.Image = request.Image;
        scenario.ThumbnailUrl = request.ThumbnailUrl;

        // Only update IsFeatured if explicitly provided (admin-controlled)
        if (request.IsFeatured.HasValue)
        {
            scenario.IsFeatured = request.IsFeatured.Value;
        }

        await _validateScenarioUseCase.ExecuteAsync(scenario);

        await _repository.UpdateAsync(scenario);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated scenario: {ScenarioId} - {Title}", scenario.Id, scenario.Title);
        return scenario;
    }

    private void ValidateAgainstSchema(UpdateScenarioRequest request)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(request, SchemaSerializerOptions);
        var errors = ScenarioJsonSchema.Validate(json);

        if (errors.Count > 0)
        {
            var errorMessages = string.Join(", ", errors.Select(e => e.ToString()).ToList());
            throw new ArgumentException($"Scenario validation failed: {errorMessages}");
        }
    }

}

