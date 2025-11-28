namespace Mystira.StoryGenerator.Domain.Stories;

public static class SceneExtensions
{
    public static bool IsFinalScene(this Scene scene)
    {
        var hasNext = !string.IsNullOrWhiteSpace(scene.NextSceneId);
        var hasBranches = scene.Branches is { Count: > 0 };
        return !hasNext && !hasBranches;
    }
}
