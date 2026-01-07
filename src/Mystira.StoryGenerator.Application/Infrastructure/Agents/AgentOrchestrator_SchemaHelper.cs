using System.Text.Json;

namespace Mystira.StoryGenerator.Application.Infrastructure.Agents;

/// <summary>
/// Partial class containing helper methods for JSON schema response format.
/// </summary>
public partial class AgentOrchestrator
{
    /// <summary>
    /// Builds a BinaryData response format for structured JSON output based on the story schema.
    /// Returns null if schema cannot be loaded.
    /// </summary>
    private async Task<BinaryData?> BuildResponseFormatAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var schemaJson = await _schemaProvider.GetSchemaJsonAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(schemaJson))
            {
                _logger.LogWarning("Story schema is empty or null, agents will run without structured output enforcement");
                return null;
            }

            // Create ResponseFormatJsonSchema object
            var responseFormatSchema = new
            {
                type = "json_schema",
                json_schema = new
                {
                    name = "mystira_story_schema",
                    description = "JSON schema for Mystira interactive story generation with branching narratives",
                    schema = JsonSerializer.Deserialize<JsonElement>(schemaJson),
                    strict = _schemaProvider.IsStrict
                }
            };

            var responseFormatJson = JsonSerializer.Serialize(responseFormatSchema);
            return BinaryData.FromString(responseFormatJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build response format from story schema");
            return null;
        }
    }
}
