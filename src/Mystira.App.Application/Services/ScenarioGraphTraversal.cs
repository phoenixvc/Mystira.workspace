using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Services;

/// <summary>
/// Performs depth-first traversal of scenario scene graphs.
/// Extracted from CalculateBadgeScoresQueryHandler to reduce complexity and improve testability.
/// </summary>
public static class ScenarioGraphTraversal
{
    /// <summary>
    /// Performs depth-first traversal of a scenario graph and returns all possible paths
    /// with their cumulative compass axis scores.
    /// </summary>
    public static List<Dictionary<string, double>> TraverseScenario(Scenario scenario)
    {
        var allPaths = new List<Dictionary<string, double>>();

        if (scenario.Scenes == null || !scenario.Scenes.Any())
        {
            return allPaths;
        }

        var sceneDict = scenario.Scenes.ToDictionary(s => s.Id, s => s);
        var startScene = scenario.Scenes.FirstOrDefault();
        if (startScene == null)
        {
            return allPaths;
        }

        var visited = new HashSet<string>();
        var currentPath = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        DepthFirstSearch(startScene, sceneDict, visited, currentPath, allPaths);

        return allPaths;
    }

    /// <summary>
    /// Recursively traverses a scenario graph depth-first, accumulating compass-axis scores.
    /// </summary>
    private static void DepthFirstSearch(
        Scene currentScene,
        Dictionary<string, Scene> sceneDict,
        HashSet<string> visited,
        Dictionary<string, double> currentPath,
        List<Dictionary<string, double>> allPaths)
    {
        if (visited.Contains(currentScene.Id))
        {
            // BUG-01 fix: Save accumulated scores when cycle is detected
            // rather than silently dropping the path
            if (currentPath.Any())
            {
                allPaths.Add(new Dictionary<string, double>(currentPath, StringComparer.OrdinalIgnoreCase));
            }
            return;
        }

        visited.Add(currentScene.Id);

        if (currentScene.Branches != null && currentScene.Branches.Any())
        {
            foreach (var branch in currentScene.Branches)
            {
                var branchPath = new Dictionary<string, double>(currentPath, StringComparer.OrdinalIgnoreCase);
                ApplyCompassChange(branchPath, branch);

                if (!string.IsNullOrWhiteSpace(branch.NextSceneId) && sceneDict.TryGetValue(branch.NextSceneId, out var nextScene))
                {
                    var branchVisited = new HashSet<string>(visited);
                    DepthFirstSearch(nextScene, sceneDict, branchVisited, branchPath, allPaths);
                }
                else if (branchPath.Any())
                {
                    allPaths.Add(branchPath);
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(currentScene.NextSceneId) &&
                 sceneDict.TryGetValue(currentScene.NextSceneId, out var nextScene))
        {
            DepthFirstSearch(nextScene, sceneDict, visited, currentPath, allPaths);
        }
        else if (currentPath.Any())
        {
            allPaths.Add(new Dictionary<string, double>(currentPath, StringComparer.OrdinalIgnoreCase));
        }
    }

    private static void ApplyCompassChange(Dictionary<string, double> path, Branch branch)
    {
        if (branch.CompassChange == null || string.IsNullOrWhiteSpace(branch.CompassChange.Axis))
            return;

        var axis = branch.CompassChange.Axis;
        if (!path.ContainsKey(axis))
        {
            path[axis] = 0;
        }
        path[axis] += branch.CompassChange.Delta;
    }
}
