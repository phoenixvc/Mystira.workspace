using Mystira.Authoring.Abstractions.Models.Scenario;
using Mystira.Shared.GraphTheory;
using Mystira.Shared.GraphTheory.Algorithms;

namespace Mystira.Authoring.Graph;

/// <summary>
/// Builds a directed graph from a scenario for path analysis.
/// </summary>
public class ScenarioGraphBuilder
{
    /// <summary>
    /// Builds a directed graph of scene transitions from a scenario.
    /// </summary>
    /// <param name="scenario">The scenario to analyze.</param>
    /// <returns>A directed graph with scene IDs as nodes.</returns>
    public IDirectedGraph<string, SceneTransition> Build(Scenario scenario)
    {
        var edges = new List<Edge<string, SceneTransition>>();

        foreach (var scene in scenario.Scenes)
        {
            // Add edge for next_scene (linear progression)
            if (!string.IsNullOrEmpty(scene.NextSceneId))
            {
                var transition = new SceneTransition
                {
                    FromSceneId = scene.Id,
                    ToSceneId = scene.NextSceneId,
                    TransitionType = TransitionType.Linear
                };
                edges.Add(new Edge<string, SceneTransition>(scene.Id, scene.NextSceneId, transition));
            }

            // Add edges for branches
            foreach (var branch in scene.Branches)
            {
                if (string.IsNullOrEmpty(branch.NextSceneId)) continue;

                var transition = new SceneTransition
                {
                    FromSceneId = scene.Id,
                    ToSceneId = branch.NextSceneId,
                    TransitionType = TransitionType.Branch,
                    ChoiceText = branch.Choice
                };
                edges.Add(new Edge<string, SceneTransition>(scene.Id, branch.NextSceneId, transition));
            }
        }

        return DirectedGraph<string, SceneTransition>.FromEdges(edges);
    }

    /// <summary>
    /// Finds the starting scene of a scenario.
    /// </summary>
    /// <param name="scenario">The scenario.</param>
    /// <returns>The starting scene ID, or null if not found.</returns>
    public string? FindStartScene(Scenario scenario)
    {
        if (scenario.Scenes.Count == 0) return null;

        var allTargets = new HashSet<string>();

        foreach (var scene in scenario.Scenes)
        {
            if (!string.IsNullOrEmpty(scene.NextSceneId))
                allTargets.Add(scene.NextSceneId);
            foreach (var branch in scene.Branches)
            {
                if (!string.IsNullOrEmpty(branch.NextSceneId))
                    allTargets.Add(branch.NextSceneId);
            }
        }

        // Start scene is one that is never targeted
        foreach (var scene in scenario.Scenes)
        {
            if (!allTargets.Contains(scene.Id))
                return scene.Id;
        }

        // Fallback: first scene
        return scenario.Scenes[0].Id;
    }

    /// <summary>
    /// Finds all ending scenes (scenes with no outgoing transitions).
    /// </summary>
    /// <param name="scenario">The scenario.</param>
    /// <returns>List of ending scene IDs.</returns>
    public List<string> FindEndingScenes(Scenario scenario)
    {
        var endings = new List<string>();

        foreach (var scene in scenario.Scenes)
        {
            var hasNextScene = !string.IsNullOrEmpty(scene.NextSceneId);
            var hasBranches = scene.Branches.Any(b => !string.IsNullOrEmpty(b.NextSceneId));

            if (!hasNextScene && !hasBranches)
            {
                endings.Add(scene.Id);
            }
        }

        return endings;
    }

    /// <summary>
    /// Enumerates all paths through the scenario from start to endings.
    /// </summary>
    /// <param name="scenario">The scenario.</param>
    /// <param name="maxPaths">Maximum number of paths to enumerate.</param>
    /// <returns>List of paths (each path is a list of scene IDs).</returns>
    public List<List<string>> EnumerateAllPaths(Scenario scenario, int maxPaths = 100)
    {
        var graph = Build(scenario);
        var start = FindStartScene(scenario);
        var endings = new HashSet<string>(FindEndingScenes(scenario));

        if (start == null || endings.Count == 0)
            return new List<List<string>>();

        var allPaths = new List<List<string>>();

        // Use path enumeration from GraphTheory
        // EnumeratePaths finds paths from start to terminal nodes
        var paths = graph.EnumeratePaths(start, node => endings.Contains(node), maxPaths);
        foreach (var path in paths)
        {
            allPaths.Add(path.ToList());
            if (allPaths.Count >= maxPaths)
                break;
        }

        return allPaths;
    }
}

/// <summary>
/// Represents a transition between scenes.
/// </summary>
public class SceneTransition
{
    /// <summary>
    /// Source scene ID.
    /// </summary>
    public string FromSceneId { get; set; } = string.Empty;

    /// <summary>
    /// Target scene ID.
    /// </summary>
    public string ToSceneId { get; set; } = string.Empty;

    /// <summary>
    /// Type of transition.
    /// </summary>
    public TransitionType TransitionType { get; set; }

    /// <summary>
    /// Choice text (if branch transition).
    /// </summary>
    public string? ChoiceText { get; set; }
}

/// <summary>
/// Type of scene transition.
/// </summary>
public enum TransitionType
{
    /// <summary>
    /// Linear progression (next_scene).
    /// </summary>
    Linear,

    /// <summary>
    /// Branch from a choice or roll.
    /// </summary>
    Branch
}
