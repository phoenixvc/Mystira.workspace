using System.Text;
using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Graph;
using Mystira.StoryGenerator.Domain.Stories;
using Mystira.StoryGenerator.GraphTheory.Algorithms;
using Mystira.StoryGenerator.GraphTheory.Graph;

namespace Mystira.StoryGenerator.Application.Scenarios;

public sealed class ScenarioGraph : IScenarioGraph
{
    private static Scenario? _scenario;
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
        return CreateScenarioPaths(root, compressedPaths);
    }

    /// <summary>
    /// Generates paths through the scenario based on dominator analysis.
    /// This method supports graphs with cycles.
    /// </summary>
    /// <param name="compress">If true, compresses paths by removing redundant suffixes.</param>
    public IEnumerable<ScenarioPath> GetDominatorPaths(bool compress = false)
    {
        var roots = Roots().ToArray();
        Scene root;
        if (roots.Length == 1)
        {
            root = roots[0];
        }
        else if (roots.Length == 0 && Nodes.Any())
        {
            // In a pure cycle, there are no roots. Pick the first node as a synthetic root.
            root = Nodes.First();
        }
        else
        {
            throw new InvalidOperationException(roots.Length == 0
                ? "Could not find any scenes in the scenario"
                : "Found more than one starting scene");
        }

        var idoms = this.GetImmediateDominators(root);
        var terminalNodes = Terminals().ToList();

        if (terminalNodes.Count == 0 && Nodes.Count > 0)
        {
            // If no terminals (e.g. pure cycle), use all nodes as potential endpoints for coverage
            terminalNodes = Nodes.ToList();
        }

        var dominatorPaths = new List<List<IEdge<Scene, string>>>();

        foreach (var terminal in terminalNodes)
        {
            var currentTerminalPaths = new List<List<IEdge<Scene, string>>> { new List<IEdge<Scene, string>>() };
            var current = terminal;

            while (!current.Equals(root))
            {
                if (!idoms.TryGetValue(current, out var dominator) || dominator.Equals(current))
                {
                    // Should not happen in a connected graph from root, but safety first
                    break;
                }

                // Find paths from dominator to current.
                var subPaths = compress
                    ? FindAllPathsBetweenDominators(dominator, current)
                    : new List<List<IEdge<Scene, string>>> { FindShortestPathEdges(dominator, current) ?? new List<IEdge<Scene, string>>() };

                var nextTerminalPaths = new List<List<IEdge<Scene, string>>>();
                foreach (var subPath in subPaths)
                {
                    foreach (var existingPath in currentTerminalPaths)
                    {
                        var newPath = new List<IEdge<Scene, string>>(subPath);
                        newPath.AddRange(existingPath);
                        nextTerminalPaths.Add(newPath);
                    }
                }
                currentTerminalPaths = nextTerminalPaths;
                current = dominator;
            }

            dominatorPaths.AddRange(currentTerminalPaths.Where(p => p.Count > 0));
        }

        if (dominatorPaths.Count == 0 && Nodes.Count > 0)
        {
            // If we still have no paths (e.g. root is the only node or root is part of a pure cycle and it was its own terminal)
            // Just return the root as a single scene path
            return new[] { new ScenarioPath(new[] { root.Id ?? "root" }, root.Description ?? "") };
        }

        var finalPaths = compress
            ? PathAlgorithms.CompressBySharedSuffixes(dominatorPaths.Select(p =>
            {
                var nodes = new List<Scene> { root };
                nodes.AddRange(p.Select(e => e.To));
                return (IReadOnlyList<Scene>)nodes;
            }).ToList())
            : null;

        if (compress && finalPaths != null)
        {
            // Convert back to edge paths
            var edgePaths = new List<List<IEdge<Scene, string>>>();
            foreach (var nodePath in finalPaths)
            {
                var edgePath = new List<IEdge<Scene, string>>();
                for (int i = 0; i < nodePath.Count - 1; i++)
                {
                    var from = nodePath[i];
                    var to = nodePath[i + 1];
                    var edge = GetOutgoingEdges(from).First(e => e.To.Equals(to));
                    edgePath.Add(edge);
                }
                if (edgePath.Count > 0)
                    edgePaths.Add(edgePath);
            }
            return CreateScenarioPaths(root, edgePaths);
        }

        return CreateScenarioPaths(root, dominatorPaths);
    }

    private List<List<IEdge<Scene, string>>> FindAllPathsBetweenDominators(Scene start, Scene end)
    {
        // We want all paths from start to end that don't pass through other dominators of 'end'
        // Actually, immediate dominator means there are no other dominators in between.
        // But there might be many paths.

        var results = new List<List<IEdge<Scene, string>>>();
        var currentPath = new List<IEdge<Scene, string>>();
        var onStack = new HashSet<Scene>();

        void Backtrack(Scene current)
        {
            if (current.Equals(end))
            {
                results.Add(new List<IEdge<Scene, string>>(currentPath));
                return;
            }

            onStack.Add(current);
            foreach (var edge in GetOutgoingEdges(current))
            {
                // To support cycles, we avoid revisiting nodes on the current recursion stack.
                // This ensures we don't loop infinitely while still finding all simple paths.
                if (!onStack.Contains(edge.To))
                {
                    currentPath.Add(edge);
                    Backtrack(edge.To);
                    currentPath.RemoveAt(currentPath.Count - 1);
                }
            }
            onStack.Remove(current);
        }

        Backtrack(start);
        return results;
    }

    private List<IEdge<Scene, string>>? FindShortestPathEdges(Scene start, Scene end)
    {
        var queue = new Queue<(Scene Node, List<IEdge<Scene, string>> Path)>();
        queue.Enqueue((start, new List<IEdge<Scene, string>>()));
        var visited = new HashSet<Scene> { start };

        while (queue.Count > 0)
        {
            var (current, path) = queue.Dequeue();
            if (current.Equals(end))
                return path;

            foreach (var edge in GetOutgoingEdges(current))
            {
                if (visited.Add(edge.To))
                {
                    var newPath = new List<IEdge<Scene, string>>(path) { edge };
                    queue.Enqueue((edge.To, newPath));
                }
            }
        }

        return null;
    }

    private IEnumerable<ScenarioPath> CreateScenarioPaths(Scene root, IEnumerable<IEnumerable<IEdge<Scene, string>>> edgePaths)
    {
        var paths = new List<ScenarioPath>();
        foreach (var path in edgePaths)
        {
            var pathList = path.ToList();
            if (pathList.Count == 0)
                continue;

            var sb = new StringBuilder();
            sb.AppendLine(GetCharactersString());

            foreach (var edge in pathList)
            {
                sb.AppendLine($"Scene {edge.From.Id}: " + edge.From.Description);
                if (edge.From.Type is SceneType.Choice or SceneType.Roll)
                    sb.AppendLine("Answer: " + edge.Label);
            }

            sb.AppendLine($"Scene {pathList[^1].To.Id}: " + pathList[^1].To.Description);

            var story = sb.ToString();
            var sceneIds = new List<string> { root.Id };
            sceneIds.AddRange(pathList.Select(e => e.To.Id));

            paths.Add(new ScenarioPath(sceneIds, story));
        }

        return paths;
    }

    private string GetCharactersString()
    {
        if (_scenario == null)
            return "The main player characters are undefined";

        var characterNames = _scenario.Characters?
            .Where(x => x != null)
            .Select(x => x.Name ?? "Unknown")
            .ToList() ?? new List<string>();

        return "The main player characters are: " + string.Join(",", characterNames);
    }
}
