using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Services;

public interface IScenarioSrlAnalysisService
{
    Task<IReadOnlyDictionary<string, SemanticRoleLabellingClassification>> ClassifyScenarioAsync(
        Scenario scenario,
        IReadOnlyList<ScenarioPathPrefixSummary> prefixSummaries,
        CancellationToken cancellationToken = default);
}
