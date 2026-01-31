using Mystira.StoryGenerator.Contracts.StoryConsistency;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Domain.Graph;

/// <summary>
/// Graph representing a scenario
/// </summary>
public interface IScenarioGraph : IDirectedGraph<Scene, string>
{
    IEnumerable<ScenarioPath> GetCompressedPaths();

    /// <summary>
    /// Gets paths that represent the dominator structure of the scenario.
    /// These paths connect the root to each terminal node via its immediate dominators,
    /// providing a representative set of paths that cover all critical decision points.
    /// </summary>
    /// <param name="compress">If true, returns compressed paths that cover all branches between dominators.</param>
    IEnumerable<ScenarioPath> GetDominatorPaths(bool compress = false);
}
