namespace Mystira.Contracts.StoryGenerator.Services;

/// <summary>
/// Factory interface for creating LLM service instances.
/// </summary>
public interface ILlmServiceFactory
{
    /// <summary>
    /// Creates an LLM service for the specified provider.
    /// </summary>
    /// <param name="provider">The provider identifier (e.g., "azure-openai", "anthropic").</param>
    /// <returns>The LLM service instance.</returns>
    ILLMService Create(string provider);

    /// <summary>
    /// Creates an LLM service for the specified provider and model.
    /// </summary>
    /// <param name="provider">The provider identifier.</param>
    /// <param name="modelId">The logical model identifier.</param>
    /// <returns>The LLM service instance.</returns>
    ILLMService Create(string provider, string modelId);

    /// <summary>
    /// Gets the default LLM service based on configuration.
    /// </summary>
    /// <returns>The default LLM service instance.</returns>
    ILLMService GetDefault();

    /// <summary>
    /// Gets available provider names.
    /// </summary>
    /// <returns>Collection of available provider names.</returns>
    IEnumerable<string> GetAvailableProviders();
}
