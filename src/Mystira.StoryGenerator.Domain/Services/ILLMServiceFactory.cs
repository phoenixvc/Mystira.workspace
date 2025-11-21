using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Domain.Services;

public interface ILLMServiceFactory
{
    ILLMService? GetService(string providerName);
    ILLMService? GetDefaultService();
}

public interface ILLMService
{
    string ProviderName { get; }
    Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default);
}
