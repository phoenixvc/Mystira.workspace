using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Application.Infrastructure.RateLimiting;
using Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.PrefixSummary;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.Entities;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;
using Microsoft.Extensions.Options;

namespace Mystira.StoryGenerator.Application.Services;

public sealed class ScenarioSrlAnalysisService : IScenarioSrlAnalysisService
{
    private readonly ISemanticRoleLabellingLlmService _srlLlm;
    private readonly int _requestsPerMinute;

    public ScenarioSrlAnalysisService(ISemanticRoleLabellingLlmService srlLlm, IOptions<LlmRateLimitOptions> rateOptions)
    {
        _srlLlm = srlLlm ?? throw new ArgumentNullException(nameof(srlLlm));
        if (rateOptions == null) throw new ArgumentNullException(nameof(rateOptions));
        var opts = rateOptions.Value ?? new LlmRateLimitOptions();
        _requestsPerMinute = Math.Max(1, opts.SrlRequestsPerMinute);
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
        var comparer = new SceneEntityComparer();
        var mustActive = PrefixSummaryAggregator.ComputeMustActivePerScene(prefixSummaries, comparer);
        var mustRemoved = PrefixSummaryAggregator.BuildPerSceneDefinitelyAbsent(prefixSummaries, comparer);
        var mayActive  = PrefixSummaryAggregator.ComputeMaybeActivePerScene(prefixSummaries, comparer);

        // 3. Derive all entities referenced in the scenario
        HashSet<SceneEntity> globalEntities = GetGlobalEntities(prefixSummaries, comparer);

        // 3. For each scene, call SRL LLM with appropriate sets, limited to configured requests per minute
        var rpmLimiter = new PerMinuteRateLimiter(_requestsPerMinute);

        var tasks = scenesById.Values.Select(async scene =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            mustActive.TryGetValue(scene.Id, out var knownActive);
            mustRemoved.TryGetValue(scene.Id, out var knownRemoved);
            mayActive.TryGetValue(scene.Id, out var mayActiveSet);

            knownActive  ??= new HashSet<SceneEntity>();
            knownRemoved ??= new HashSet<SceneEntity>();
            mayActiveSet ??= new HashSet<SceneEntity>();

            var candidateEntities = BuildCandidateEntitiesForScene(
                scene,
                mayActiveSet,
                knownActive,
                knownRemoved,
                globalEntities);

            // Enforce per-minute rate limit for SRL requests
            await rpmLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);

            var classification = await _srlLlm
                .ClassifyAsync(
                    scene,
                    candidateEntities,
                    knownActive,
                    knownRemoved,
                    cancellationToken)
                .ConfigureAwait(false);

            return (scene.Id, classification);
        });

        var resultsArray = await Task.WhenAll(tasks).ConfigureAwait(false);

        var results = resultsArray
            .Where(t => t.classification != null)
            .ToDictionary(t => t.Id, t => t.classification!, StringComparer.Ordinal);

        return results;
    }

    private static HashSet<SceneEntity> GetGlobalEntities(IReadOnlyList<ScenarioPathPrefixSummary> prefixSummaries, SceneEntityComparer comparer)
    {
        return prefixSummaries
            // Flatten all prefix summaries' entities (PrefixSummaryEntity)
            .SelectMany(ps => ps.Entities)
            // Map PrefixSummaryEntity -> SceneEntity
            .Select(e => new SceneEntity
            {
                Name = e.CanonicalName,
                Type = e.Type,
                IsProperNoun = e.IsProperNoun
                // map any other fields you care about
            })
            // Deduplicate using SceneEntityComparer
            .Distinct(comparer)
            // Materialize as HashSet<SceneEntity> with same comparer
            .ToHashSet(comparer);
    }

    private static IReadOnlyCollection<SceneEntity> BuildCandidateEntitiesForScene(
        Scene scene,
        HashSet<SceneEntity> mayActive,
        HashSet<SceneEntity> knownActive,
        HashSet<SceneEntity> knownRemoved,
        HashSet<SceneEntity> globalEntities)
    {
        // Strategy: err on the side of *including* too many entities.
        // False positives are cheap; false negatives are what you care about.
        var result = new HashSet<SceneEntity>(mayActive);

        result.UnionWith(globalEntities);
        result.UnionWith(knownActive);
        result.ExceptWith(knownRemoved);

        // Optional: also include characters from scenario metadata, locations from tags, etc.

        return result.ToList();
    }
}
