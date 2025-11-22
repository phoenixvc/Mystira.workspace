using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Domain.Services;

public interface IStoryValidationService
{
    Task<ValidationResponse> ValidateStoryAsync(ValidateStoryRequest request);
}
