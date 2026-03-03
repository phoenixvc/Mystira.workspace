using Mystira.Contracts.StoryGenerator.Chat;

namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Provides story JSON schema for validation and LLM structured output.
/// </summary>
public interface IStorySchemaProvider
{
    /// <summary>
    /// Gets the JSON schema as a string.
    /// </summary>
    /// <returns>The JSON schema string.</returns>
    string GetSchemaJson();

    /// <summary>
    /// Gets the JSON schema formatted for LLM response format.
    /// </summary>
    /// <returns>The JSON schema response format.</returns>
    JsonSchemaResponseFormat GetSchemaResponseFormat();

    /// <summary>
    /// Gets the schema version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets a description of the schema for documentation purposes.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Validates that a JSON string conforms to the schema.
    /// </summary>
    /// <param name="json">The JSON to validate.</param>
    /// <returns>Validation result with any errors.</returns>
    (bool IsValid, IReadOnlyList<string> Errors) ValidateJson(string json);
}
