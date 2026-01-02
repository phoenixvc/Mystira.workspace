using Mystira.Contracts.StoryGenerator.Chat;

namespace Mystira.Ai.Abstractions;

/// <summary>
/// Factory for LLM service instances.
/// </summary>
public interface ILlmServiceFactory
{
    /// <summary>
    /// Get an LLM service by provider name, with optional deployment name/model ID override.
    /// </summary>
    /// <param name="providerName">The provider name (e.g., "azure-openai").</param>
    /// <param name="deploymentNameOrModelId">Optional deployment name or model ID.</param>
    /// <returns>An LLM service instance, or null if not available.</returns>
    ILLMService? GetService(string providerName, string? deploymentNameOrModelId = null);

    /// <summary>
    /// Get the default LLM service.
    /// </summary>
    /// <returns>The default LLM service, or null if none available.</returns>
    ILLMService? GetDefaultService();

    /// <summary>
    /// Get all available models across all providers.
    /// </summary>
    /// <returns>Available models grouped by provider.</returns>
    IEnumerable<ProviderModels> GetAvailableModels();
}

/// <summary>
/// Models available for a specific provider.
/// </summary>
public record ProviderModels(string ProviderName, IEnumerable<ChatModelInfo> Models);
