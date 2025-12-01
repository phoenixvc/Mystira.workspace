using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Domain.Services;

public interface IPrefixSummaryService
{
    Task<IReadOnlyList<ScenarioPathPrefixSummary>> GeneratePrefixSummariesAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default);
}
