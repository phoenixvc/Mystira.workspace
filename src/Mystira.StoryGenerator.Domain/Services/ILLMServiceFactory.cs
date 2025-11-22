using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Domain.Services;

/// <summary>
/// Factory for LLM service instances
/// </summary>
public interface ILLMServiceFactory
{
    ILLMService? GetService(string providerName);
    ILLMService? GetDefaultService();
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
}
