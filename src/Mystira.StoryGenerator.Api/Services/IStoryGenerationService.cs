using Mystira.StoryGenerator.Contracts.Stories;

namespace Mystira.StoryGenerator.Api.Services;

public interface IStoryGenerationService
{
    Task<GenerateYamlStoryResponse> GenerateYamlStoryAsync(GenerateYamlStoryRequest request, CancellationToken cancellationToken = default);
}
