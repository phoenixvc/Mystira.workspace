using Mystira.StoryGenerator.Domain.Stories;
using Mystira.StoryGenerator.GraphTheory.Graph;

namespace Mystira.StoryGenerator.Application.Extensions;

public static class ScenarioExtensions
{
    public static DirectedGraph<Scene, string> ToGraph(this Scenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        var nodes = scenario.Scenes ?? new List<Scene>();

        var byId = nodes
            .Where(s => !string.IsNullOrWhiteSpace(s.Id))
            .ToDictionary(s => s.Id, s => s, StringComparer.Ordinal);

        var edges = ExtractEdges(scenario, byId);
        return DirectedGraph<Scene, string>.FromEdges(edges, nodes);

        static IEnumerable<Edge<Scene, string>> ExtractEdges(Scenario scen, Dictionary<string, Scene> byId)
        {
            foreach (var scene in scen.Scenes)
            {
                if (string.IsNullOrWhiteSpace(scene.Id))
                    continue;

                if (!string.IsNullOrWhiteSpace(scene.NextSceneId))
                {
                    if (byId.TryGetValue(scene.NextSceneId!, out var to))
                        yield return new Edge<Scene, string>(scene, to, "next");
                    continue;
                }

                if (scene.Branches is { Count: > 0 })
                {
                    foreach (var br in scene.Branches)
                    {
                        if (string.IsNullOrWhiteSpace(br?.NextSceneId))
                            continue;
                        if (byId.TryGetValue(br!.NextSceneId, out var to))
                        {
                            var label = string.IsNullOrWhiteSpace(br.Choice) ? "choice" : br.Choice;
                            yield return new Edge<Scene, string>(scene, to, label);
                        }
                    }
                }
            }
        }
    }

    public static Scene GetFirstScene(this Scenario scenario)
    {
        var graph = scenario.ToGraph();
        var roots = graph.Roots().ToArray();
        return roots.Length switch
        {
            0 => throw new InvalidOperationException("Could not find a first scene in the scenario."),
            > 1 => throw new InvalidOperationException("Found more than one starting scene in the scenario."),
            _ => roots[0]
        };
    }
}
