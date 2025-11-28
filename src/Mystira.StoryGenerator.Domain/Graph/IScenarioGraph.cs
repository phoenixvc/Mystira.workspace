using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Domain.Graph;

/// <summary>
/// Marker interface for scenario graphs over Scene nodes with string edge labels.
/// Concrete implementation should live outside Domain (e.g., in Application).
/// </summary>
public interface IScenarioGraph : IDirectedGraph<Scene, string>
{
    IEnumerable<string> GetCompressedPaths();
}
