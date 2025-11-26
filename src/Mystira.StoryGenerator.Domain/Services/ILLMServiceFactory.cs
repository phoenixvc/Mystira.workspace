using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Domain.Services;

/// <summary>
/// Factory for LLM service instances
/// </summary>
public interface ILLMServiceFactory
{
    /// <summary>
    /// Get an LLM service by provider name, with optional deployment name/model ID override.
    /// </summary>
    /// <param name="providerName">The provider name (e.g., "azure-openai")</param>
    /// <param name="deploymentNameOrModelId">Optional deployment name or model ID to use for this provider</param>
    /// <returns>An LLM service instance, or null if not available</returns>
    ILLMService? GetService(string providerName, string? deploymentNameOrModelId = null);
    ILLMService? GetDefaultService();
    IEnumerable<ProviderModels> GetAvailableModels();
}

/// <summary>
/// Interface for Large Language Model providers
/// </summary>
public interface ILLMService
{
    /// <summary>
    /// The name of the provider (e.g., "azure-openai", "google-gemini")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Optional deployment name or model ID being used by this service instance
    /// </summary>
    string? DeploymentNameOrModelId { get; }

    /// <summary>
    /// Generate a chat completion using this LLM provider
    /// </summary>
    /// <param name="request">The chat completion request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The chat completion response</returns>
    Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if this provider is properly configured and available
    /// </summary>
    /// <returns>True if the provider is available</returns>
    bool IsAvailable();

    /// <summary>
    /// Get the list of available models for this provider
    /// </summary>
    /// <returns>List of available models</returns>
    IEnumerable<ChatModelInfo> GetAvailableModels();
}
