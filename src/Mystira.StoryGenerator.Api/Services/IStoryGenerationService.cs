using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Api.Services;

public interface IStoryGenerationService
{
    Task<GenerateJsonStoryResponse> GenerateJsonStoryAsync(GenerateJsonStoryRequest request, CancellationToken cancellationToken = default);
}
