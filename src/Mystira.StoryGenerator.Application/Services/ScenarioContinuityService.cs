using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.ContinuityAnalyzer;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.PrefixSummary;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;
using System.Text.Json;

namespace Mystira.StoryGenerator.Application.Services;

public sealed class StoryContinuityService : IStoryContinuityService
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
        var scenarioWithIntro = CreateScenarioWithAdditionalSceneForCharacterIntroduction(scenario);

        // 1) Heavyweight pass: prefix summaries across compressed paths
        var prefixSummaries = await _prefixSummaryService
            .GeneratePrefixSummariesAsync(scenarioWithIntro, cancellationToken)
            .ConfigureAwait(false);

        // 2) Merge prefixes into per-scene must-active entity sets
        var mustActiveByScene = PrefixSummaryMerger
            .ComputeMustActiveEntitiesByScene(prefixSummaries);

        // 3) Lightweight pass: per-scene SRL classification
        var srlResults = await _srlService.ClassifyScenarioAsync(
                scenarioWithIntro, prefixSummaries, cancellationToken)
            .ConfigureAwait(false);

        // Index SRL results by scene id and de-duplicate individual entity
        // classifications within each scene by (Name, Type)
        var srlByScene = srlResults
            .ToDictionary(
                kvp => kvp.Value.SceneId,
                kvp =>
                {
                    var cls = kvp.Value ?? new SemanticRoleLabellingClassification { SceneId = kvp.Key };
                    var list = cls.EntityClassifications ?? new List<SrlEntityClassification>();
                    cls.EntityClassifications = list
                        .Distinct(SrlEntityClassificationComparer.Instance)
                        .ToList();
                    return cls;
                },
                StringComparer.Ordinal);

        // 4) Glue: compare SRL vs. must-active and emit issues
        var issues = EntityContinuityAnalyzer.FindIssues(
            mustActiveByScene,
            srlByScene);

        return issues;
    }

    private static Scenario CreateScenarioWithAdditionalSceneForCharacterIntroduction(Scenario scenario)
    {
        // Find the first scene in the scenario
        var graph = scenario.ToGraph();
        var roots = graph.Roots().ToArray();
        if (roots.Length != 1) throw new InvalidOperationException("Scenario must have exactly one starting scene.");
        var firstSceneId = roots[0].Id;

        // Deep copy the scenario to avoid mutating the original during continuity checking
        var scenarioForContinuityChecking =
            JsonSerializer.Deserialize<Scenario>(JsonSerializer.Serialize(scenario))
            ?? throw new InvalidOperationException("Failed to clone scenario for continuity checking.");

        // Add a scene that introduces the characters
        scenarioForContinuityChecking.Scenes.Add(new Scene
        {
            Id = "introduced-scene-id-for-story-context",
            NextSceneId = firstSceneId,
            Description = "The main characters in this story are: " +
                          string.Join(", ", scenario.Characters.Select(c => c.Name)),
            Type = SceneType.Narrative
        });
        return scenarioForContinuityChecking;
    }
}

// Note: Entity-level de-duplication is applied inside each
// SemanticRoleLabellingClassification using SrlEntityClassificationComparer.
