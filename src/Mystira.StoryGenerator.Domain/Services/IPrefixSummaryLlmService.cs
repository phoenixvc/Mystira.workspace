using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Domain.Services;

public interface IPrefixSummaryLlmService
{
    Task<ScenarioPathPrefixSummary?> SummarizeAsync(IEnumerable<Scene> scenePath,
        CancellationToken cancellationToken = default);
}
