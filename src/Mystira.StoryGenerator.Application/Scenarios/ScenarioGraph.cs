using System.Text;
using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Graph;
using Mystira.StoryGenerator.Domain.Stories;
using Mystira.StoryGenerator.GraphTheory.Algorithms;
using Mystira.StoryGenerator.GraphTheory.Graph;

namespace Mystira.StoryGenerator.Application.Scenarios;

public sealed class ScenarioGraph : IScenarioGraph
{
    private static Scenario _scenario;
    private readonly DirectedGraph<Scene, string> _inner;

    private ScenarioGraph(DirectedGraph<Scene, string> inner)
    {
        _inner = inner;
    }

    public static ScenarioGraph FromScenario(Scenario scenario)
    {
        _scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
        return new ScenarioGraph(scenario.ToGraph());
    }

    public IReadOnlyCollection<Scene> Nodes => _inner.Nodes;
    public IReadOnlyCollection<IEdge<Scene, string>> Edges => _inner.Edges;

    public IReadOnlyList<IEdge<Scene, string>> GetOutgoingEdges(Scene node) => _inner.GetOutgoingEdges(node);
    public IReadOnlyList<IEdge<Scene, string>> GetIncomingEdges(Scene node) => _inner.GetIncomingEdges(node);
    public IEnumerable<Scene> GetSuccessors(Scene node) => _inner.GetSuccessors(node);
    public IEnumerable<Scene> GetPredecessors(Scene node) => _inner.GetPredecessors(node);
    public int OutDegree(Scene node) => _inner.OutDegree(node);
    public int InDegree(Scene node) => _inner.InDegree(node);
    public IEnumerable<Scene> Roots() => _inner.Roots();
    public IEnumerable<Scene> Terminals() => _inner.Terminals();
    public IEnumerable<ScenarioPath> GetCompressedPaths()
    {
        var roots = Roots().ToArray();
        var root = roots.Length switch
        {
            0 => throw new InvalidOperationException("Could not find any starting scenes"),
            > 1 => throw new InvalidOperationException("Found more than one starting scene"),
            _ => roots[0]
        };

        var compressedPaths = this.CompressGraphPathsToEdgePaths(root, scene => scene.IsFinalScene());
        var paths = new List<ScenarioPath>();
        foreach (var path in compressedPaths)
        {
            var sb = new StringBuilder();
            // Introduce the characters in the first line
            sb.AppendLine(GetCharactersString());
            foreach (var edge in path)
            {
                // Add scene id: description, and an answer if it was a choice or roll scene
                sb.AppendLine($"Scene {edge.From.Id}: " + edge.From.Description);
                if (edge.From.Type is SceneType.Choice or SceneType.Roll) sb.AppendLine("Answer: " + edge.Label);
            }
            // Include the final scene
            sb.AppendLine($"Scene {path[^1].To.Id}: " + path[^1].To.Description);

            var story = sb.ToString();
            var sceneIds = new List<string> { root.Id };
            sceneIds.AddRange(path.Select(e => e.To.Id));

            paths.Add(new ScenarioPath(sceneIds, story));
        }

        return paths;
    }

    private string GetCharactersString()
    {
        return "The main player characters are: " +
               string.Join(",", _scenario.Characters.Select(x => x.Name));
    }
}
