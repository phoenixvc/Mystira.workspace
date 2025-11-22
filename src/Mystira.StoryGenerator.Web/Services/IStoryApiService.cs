using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Web.Services;

public interface IStoryApiService
{
    Task<ValidationResponse> ValidateStoryAsync(string storyContent, string format = "yaml");
    Task<GenerateJsonStoryResponse> GenerateJsonStoryAsync(GenerateJsonStoryRequest request);
    Task<ChatCompletionResponse> SetupStoryAsync(ChatCompletionRequest request);
    // Route regular chat turns to the chat orchestration endpoint
    Task<ChatCompletionResponse> CompleteChatAsync(ChatCompletionRequest request);
    Task<RandomStoryParametersResponse> RandomizeStoryParametersAsync(RandomStoryParametersRequest request);
}
