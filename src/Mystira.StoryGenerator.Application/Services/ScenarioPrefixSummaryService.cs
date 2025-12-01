using Mystira.StoryGenerator.Application.Scenarios;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Services;

public class ScenarioPrefixSummaryService : IPrefixSummaryService
{
    private readonly IPrefixSummaryLlmService _llm;

    public ScenarioPrefixSummaryService(IPrefixSummaryLlmService llm)
    {
        _llm = llm ?? throw new ArgumentNullException(nameof(llm));
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

        foreach (var compressedPath in compressedPaths)
        {
            // Each ScenarioPath already has ordered scene ids; if not, reconstruct
            var sceneIds = compressedPath.SceneIds; // assuming your ScenarioPath exposes this
            var orderedScenes = sceneIds.Select(id => scenesById[id]).ToList();

            // 4. Generate *prefixes* along this path
            for (var prefixLength = 1; prefixLength <= orderedScenes.Count; prefixLength++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var prefixScenes = orderedScenes.Take(prefixLength);

                var summary = await _llm
                    .SummarizeAsync(prefixScenes, cancellationToken)
                    .ConfigureAwait(false);

                if (summary is null)
                    continue;

                results.Add(summary);
            }
        }

        return results;
    }
}
