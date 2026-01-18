using Mystira.Contracts.StoryGenerator.Stories;
using Mystira.Contracts.StoryGenerator.StoryConsistency;

namespace Mystira.Contracts.StoryGenerator.Graph;

/// <summary>
/// Interface for navigating and analyzing the scenario graph structure.
/// </summary>
public interface IScenarioGraph
{
    /// <summary>
    /// Gets the scenario this graph represents.
    /// </summary>
    Scenario Scenario { get; }

    /// <summary>
    /// Gets all scenes in the graph.
    /// </summary>
    IReadOnlyList<Scene> Scenes { get; }

    /// <summary>
    /// Gets the starting scene.
    /// </summary>
    Scene? StartScene { get; }

    /// <summary>
    /// Gets all ending scenes.
    /// </summary>
    IReadOnlyList<Scene> EndScenes { get; }

    /// <summary>
    /// Gets a scene by ID.
    /// </summary>
    /// <param name="sceneId">The scene ID.</param>
    /// <returns>The scene, or null if not found.</returns>
    Scene? GetScene(string sceneId);

    /// <summary>
    /// Gets scenes that can be reached from the given scene.
    /// </summary>
    /// <param name="sceneId">The source scene ID.</param>
    /// <returns>Reachable scene IDs.</returns>
    IReadOnlyList<string> GetSuccessors(string sceneId);

    /// <summary>
    /// Gets scenes that lead to the given scene.
    /// </summary>
    /// <param name="sceneId">The target scene ID.</param>
    /// <returns>Predecessor scene IDs.</returns>
    IReadOnlyList<string> GetPredecessors(string sceneId);

    /// <summary>
    /// Gets the branches from a scene.
    /// </summary>
    /// <param name="sceneId">The scene ID.</param>
    /// <returns>Branches from the scene.</returns>
    IReadOnlyList<Branch> GetBranches(string sceneId);

    /// <summary>
    /// Checks if a scene is reachable from the start.
    /// </summary>
    /// <param name="sceneId">The scene ID.</param>
    /// <returns>True if the scene is reachable.</returns>
    bool IsReachable(string sceneId);

    /// <summary>
    /// Gets all paths from start to end scenes.
    /// </summary>
    /// <param name="maxPaths">Maximum number of paths to return.</param>
    /// <returns>All paths through the scenario.</returns>
    IReadOnlyList<ScenarioPath> GetAllPaths(int maxPaths = 1000);

    /// <summary>
    /// Gets paths from a specific scene to end scenes.
    /// </summary>
    /// <param name="startSceneId">The starting scene ID.</param>
    /// <param name="maxPaths">Maximum number of paths.</param>
    /// <returns>Paths from the scene to endings.</returns>
    IReadOnlyList<ScenarioPath> GetPathsFrom(string startSceneId, int maxPaths = 100);

    /// <summary>
    /// Gets the dominator tree for the scenario graph.
    /// </summary>
    /// <returns>Map of scene ID to dominator scene ID.</returns>
    IReadOnlyDictionary<string, string?> GetDominatorTree();

    /// <summary>
    /// Gets scenes that dominate the given scene.
    /// </summary>
    /// <param name="sceneId">The scene ID.</param>
    /// <returns>Dominator scene IDs.</returns>
    IReadOnlyList<string> GetDominators(string sceneId);

    /// <summary>
    /// Finds cycles in the graph.
    /// </summary>
    /// <returns>List of cycles (each cycle is a list of scene IDs).</returns>
    IReadOnlyList<IReadOnlyList<string>> FindCycles();

    /// <summary>
    /// Performs a topological sort of the scenes.
    /// </summary>
    /// <returns>Scenes in topological order, or null if cycles exist.</returns>
    IReadOnlyList<string>? TopologicalSort();

    /// <summary>
    /// Gets the depth of a scene (distance from start).
    /// </summary>
    /// <param name="sceneId">The scene ID.</param>
    /// <returns>Depth, or -1 if not reachable.</returns>
    int GetDepth(string sceneId);

    /// <summary>
    /// Gets orphaned scenes (not reachable from start).
    /// </summary>
    /// <returns>Orphaned scene IDs.</returns>
    IReadOnlyList<string> GetOrphanedScenes();

    /// <summary>
    /// Gets dead-end scenes (no path to any ending).
    /// </summary>
    /// <returns>Dead-end scene IDs.</returns>
    IReadOnlyList<string> GetDeadEndScenes();

    /// <summary>
    /// Validates the graph structure.
    /// </summary>
    /// <returns>List of structural issues.</returns>
    IReadOnlyList<GraphValidationIssue> ValidateStructure();
}

/// <summary>
/// A structural issue in the scenario graph.
/// </summary>
public class GraphValidationIssue
{
    /// <summary>
    /// Type of issue.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Affected scene IDs.
    /// </summary>
    public List<string> SceneIds { get; set; } = new();

    /// <summary>
    /// Description of the issue.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Severity of the issue.
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Suggested fix.
    /// </summary>
    public string? SuggestedFix { get; set; }
}
