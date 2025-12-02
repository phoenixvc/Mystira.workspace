using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Application.Infrastructure.RateLimiting;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;
using Microsoft.Extensions.Options;

namespace Mystira.StoryGenerator.Application.Services;

public class ScenarioPrefixSummaryService : IPrefixSummaryService
{
    private readonly IPrefixSummaryLlmService _llm;
    private readonly int _requestsPerMinute;

    public ScenarioPrefixSummaryService(IPrefixSummaryLlmService llm, IOptions<LlmRateLimitOptions> rateOptions)
    {
        _llm = llm ?? throw new ArgumentNullException(nameof(llm));
        if (rateOptions == null) throw new ArgumentNullException(nameof(rateOptions));
        var opts = rateOptions.Value ?? new LlmRateLimitOptions();
        _requestsPerMinute = Math.Max(1, opts.PrefixSummaryRequestsPerMinute);
    }

    public async Task<IReadOnlyList<ScenarioPathPrefixSummary>> GeneratePrefixSummariesAsync(
        Scenario scenario,
        CancellationToken cancellationToken = default)
    {
        if (scenario == null) throw new ArgumentNullException(nameof(scenario));

        // 1. Build graph from scenario
        var graph = ScenarioGraph.FromScenario(scenario);

        // 2. Get compressed paths (each is a path of Scene nodes)
        var compressedPaths = graph.GetCompressedPaths().ToList();

        // 3. For fast lookup: scene id -> Scene
        var scenesById = scenario.Scenes.ToDictionary(s => s.Id);

        var results = new List<ScenarioPathPrefixSummary>();

        // Throttle LLM calls globally to a max requests-per-minute budget
        var rpmLimiter = new PerMinuteRateLimiter(_requestsPerMinute);

        var allTasks = new List<Task<ScenarioPathPrefixSummary?>>();

        foreach (var compressedPath in compressedPaths)
        {
            // Each ScenarioPath already has ordered scene ids; if not, reconstruct
            var sceneIds = compressedPath.SceneIds; // assuming your ScenarioPath exposes this
            var orderedScenes = sceneIds.Select(id => scenesById[id]).ToList();

            // 4. Generate *prefixes* along this path with bounded parallelism
            for (var prefixLength = 1; prefixLength <= orderedScenes.Count; prefixLength++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Materialize to avoid deferred execution issues when running in parallel
                var prefixScenes = orderedScenes.Take(prefixLength).ToList();

                var task = SummarizeWithLimitAsync(rpmLimiter, prefixScenes, cancellationToken);

                allTasks.Add(task);
            }
        }

        var allSummaries = await Task.WhenAll(allTasks).ConfigureAwait(false);
        foreach (var summary in allSummaries)
        {
            if (summary is null) continue;
            results.Add(summary);
        }

        return results;
    }

    private async Task<ScenarioPathPrefixSummary?> SummarizeWithLimitAsync(
        PerMinuteRateLimiter limiter,
        List<Scene> prefixScenes,
        CancellationToken cancellationToken)
    {
        await limiter.WaitAsync(cancellationToken).ConfigureAwait(false);
        return await _llm.SummarizeAsync(prefixScenes, cancellationToken).ConfigureAwait(false);
    }
}
