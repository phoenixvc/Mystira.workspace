using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Domain.Services;

public interface IStoryContinuityService
{
    Task<IReadOnlyList<EntityContinuityIssue>> AnalyzeAsync(
        Scenario scenario,
        Func<SrlEntityClassification, bool>? includeEntityFilter = null,
        CancellationToken cancellationToken = default);
}
