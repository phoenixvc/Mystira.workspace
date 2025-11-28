using Mystira.StoryGenerator.Domain.Stories;

namespace Mystira.StoryGenerator.Application.Extensions;

public static class SceneExtensions
{
    public static bool IsFinalScene(this Scene scene)
    {
    // A final scene has no direct next and no branches
    var hasNext = !string.IsNullOrWhiteSpace(scene.NextSceneId);
    var hasBranches = scene.Branches is { Count: > 0 };
    return !hasNext && !hasBranches;
    }
}
