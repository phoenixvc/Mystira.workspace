using Mystira.StoryGenerator.Contracts.Stories;
using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Domain.Graph;

/// <summary>
/// Graph representing a scenario
/// </summary>
public interface IScenarioGraph : IDirectedGraph<Scene, string>
{
    IEnumerable<ScenarioPath> GetCompressedPaths();
}
