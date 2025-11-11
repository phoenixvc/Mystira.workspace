using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Api.Services;

public interface IStoryValidationService
{
    Task<ValidationResponse> ValidateStoryAsync(string content, string format = "yaml");
    Task<ValidationResponse> ValidateStoryAsync(ValidateStoryRequest request);
}
