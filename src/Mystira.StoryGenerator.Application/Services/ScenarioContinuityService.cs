using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.ContinuityAnalyzer;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.PrefixSummary;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Services;

public sealed class StoryContinuityService
{
    private readonly IPrefixSummaryService _prefixSummaryService;
    private readonly IScenarioSrlAnalysisService _srlService;

    public StoryContinuityService(
        IPrefixSummaryService prefixSummaryService,
        IScenarioSrlAnalysisService srlService)
    {
        _prefixSummaryService = prefixSummaryService;
        _srlService = srlService;
    }

    public async Task<IReadOnlyList<EntityContinuityIssue>> AnalyzeAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default)
    {
        // 1) Heavyweight pass: prefix summaries across compressed paths
        var prefixSummaries = await _prefixSummaryService
            .GeneratePrefixSummariesAsync(scenario, cancellationToken)
            .ConfigureAwait(false);

        // 2) Merge prefixes into per-scene must-active entity sets
        var mustActiveByScene = PrefixSummaryMerger
            .ComputeMustActiveEntitiesByScene(prefixSummaries);

        // 3) Lightweight pass: per-scene SRL classification
        var srlResults = await _srlService.ClassifyScenarioAsync(
                scenario, prefixSummaries, cancellationToken)
            .ConfigureAwait(false);

        // Index SRL results by scene id
        var srlByScene = srlResults.ToDictionary(
            r => r.Value.SceneId,
            r => r.Value,
            StringComparer.Ordinal);

        // 4) Glue: compare SRL vs. must-active and emit issues
        var issues = EntityContinuityAnalyzer.FindIssues(
            mustActiveByScene,
            srlByScene);

        return issues;
    }
}

