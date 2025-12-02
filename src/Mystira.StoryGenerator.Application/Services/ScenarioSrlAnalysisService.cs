using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Services;

public sealed class ScenarioSrlAnalysisService : IScenarioSrlAnalysisService
{
    private readonly ISemanticRoleLabellingLlmService _srlLlm;

    public ScenarioSrlAnalysisService(ISemanticRoleLabellingLlmService srlLlm)
    {
        _srlLlm = srlLlm ?? throw new ArgumentNullException(nameof(srlLlm));
    }

    public async Task<IReadOnlyDictionary<string, SemanticRoleLabellingClassification>> ClassifyScenarioAsync(
        Scenario scenario,
        IReadOnlyList<ScenarioPathPrefixSummary> prefixSummaries,
        CancellationToken cancellationToken = default)
    {
        if (scenario == null) throw new ArgumentNullException(nameof(scenario));

        // 1. Build graph for scene lookup
        var graph = ScenarioGraph.FromScenario(scenario);
        var scenesById = graph.Nodes.ToDictionary(s => s.Id, s => s);

        // 2. Derive per-scene must/may active & removed from prefix summaries
        var mustActive = PrefixSummaryAggregator.ComputeMustActivePerScene(prefixSummaries, new SceneEntityComparer());
        var mustRemoved = PrefixSummaryAggregator.BuildPerSceneDefinitelyAbsent(prefixSummaries, new SceneEntityComparer());
        var mayActive  = PrefixSummaryAggregator.ComputeMaybeActivePerScene(prefixSummaries, new SceneEntityComparer());

        var results = new Dictionary<string, SemanticRoleLabellingClassification>();

        // 3. For each scene, call SRL LLM with appropriate sets
        foreach (var scene in scenesById.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();

            mustActive.TryGetValue(scene.Id, out var knownActive);
            mustRemoved.TryGetValue(scene.Id, out var knownRemoved);
            mayActive.TryGetValue(scene.Id, out var mayActiveSet);

            knownActive   ??= new HashSet<SceneEntity>();
            knownRemoved  ??= new HashSet<SceneEntity>();
            mayActiveSet  ??= new HashSet<SceneEntity>();

            // Candidate = "anything we think might reasonably appear here"
            // - entities that may be active
            // - optional: characters, items, locations from scenario metadata
            var candidateEntities = BuildCandidateEntitiesForScene(
                scene,
                mayActiveSet,
                knownActive,
                knownRemoved);

            var classification = await _srlLlm
                .ClassifyAsync(
                    scene,
                    candidateEntities,
                    knownActive,
                    knownRemoved,
                    cancellationToken)
                .ConfigureAwait(false);

            if (classification != null)
                results[scene.Id] = classification;
        }

        return results;
    }

    private static IReadOnlyCollection<SceneEntity> BuildCandidateEntitiesForScene(
        Scene scene,
        HashSet<SceneEntity> mayActive,
        HashSet<SceneEntity> knownActive,
        HashSet<SceneEntity> knownRemoved)
    {
        // Strategy: err on the side of *including* too many entities.
        // False positives are cheap; false negatives are what you care about.
        var result = new HashSet<SceneEntity>(mayActive);

        result.UnionWith(knownActive);
        result.ExceptWith(knownRemoved);

        // Optional: also include characters from scenario metadata, locations from tags, etc.

        return result.ToList();
    }
}
