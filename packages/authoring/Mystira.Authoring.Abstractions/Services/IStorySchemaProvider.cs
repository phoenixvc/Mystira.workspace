namespace Mystira.Authoring.Abstractions.Services;

/// <summary>
/// Provider for story JSON schemas.
/// </summary>
public interface IStorySchemaProvider
{
    /// <summary>
    /// Gets the full story schema as JSON.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The JSON schema string.</returns>
    Task<string> GetSchemaAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the schema for a specific component (e.g., "scene", "character").
    /// </summary>
    /// <param name="componentName">Name of the component.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The JSON schema string for the component.</returns>
    Task<string?> GetComponentSchemaAsync(string componentName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the schema version.
    /// </summary>
    string Version { get; }
}
