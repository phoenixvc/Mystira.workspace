using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Web.Services;

public interface IStoryApiService
{
    Task<ValidationResponse> ValidateStoryAsync(string storyContent, string format = "yaml");
    Task<GenerateYamlStoryResponse> GenerateYamlStoryAsync(GenerateYamlStoryRequest request);
    Task<ChatCompletionResponse> SetupStoryAsync(ChatCompletionRequest request);
    Task<RandomStoryParametersResponse> RandomizeStoryParametersAsync(RandomStoryParametersRequest request);
}
