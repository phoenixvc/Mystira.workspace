using Mystira.Contracts.App.Requests.Scenarios;
using Mystira.Shared.Exceptions;
using NJsonSchema;

namespace Mystira.Core.Validation;

/// <summary>
/// Shared JSON schema validator for scenario requests.
/// Eliminates duplicated schema validation logic between CreateScenarioUseCase and UpdateScenarioUseCase.
/// </summary>
public static class ScenarioSchemaValidator
{
    private static readonly JsonSchema ScenarioJsonSchema =
        JsonSchema.FromJsonAsync(ScenarioSchemaDefinitions.StorySchema).GetAwaiter().GetResult();

    private static readonly System.Text.Json.JsonSerializerOptions SchemaSerializerOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    /// <summary>
    /// Validates a scenario request against the JSON schema.
    /// Throws ValidationException if validation fails.
    /// </summary>
    public static void ValidateAgainstSchema(CreateScenarioRequest request)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(request, SchemaSerializerOptions);
        var errors = ScenarioJsonSchema.Validate(json);

        if (errors.Count > 0)
        {
            var errorMessages = string.Join(", ", errors.Select(e => e.ToString()).ToList());
            throw new ValidationException("scenario", $"Scenario validation failed: {errorMessages}");
        }
    }
}
