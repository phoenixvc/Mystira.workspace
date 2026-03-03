using Mystira.Authoring.Abstractions.Models.Scenario;
using Mystira.Shared.GraphTheory;

namespace Mystira.Authoring.Abstractions.Graph;

/// <summary>
/// Represents a path through a scenario as a sequence of scene IDs.
/// </summary>
public class ScenarioPath
{
    /// <summary>
    /// The ordered list of scene IDs in this path.
    /// </summary>
    public List<string> SceneIds { get; set; } = new();

    /// <summary>
    /// Gets the path as a formatted string (e.g., "scene_1 -> scene_2 -> scene_3").
    /// </summary>
    public string ToPathString() => string.Join(" -> ", SceneIds);
}

/// <summary>
/// Graph representation of a scenario, where nodes are scenes and edges are transitions.
/// </summary>
public interface IScenarioGraph : IDirectedGraph<Scene, string>
{
    /// <summary>
    /// Gets all compressed paths through the scenario graph.
    /// A compressed path represents a complete playthrough from start to end.
    /// </summary>
    /// <returns>Collection of scenario paths.</returns>
    IEnumerable<ScenarioPath> GetCompressedPaths();
}
