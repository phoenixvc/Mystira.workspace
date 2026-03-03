namespace Mystira.Contracts.StoryGenerator.Entities;

public sealed class SceneEntityComparer : IEqualityComparer<SceneEntity>
{
    public bool Equals(SceneEntity? x, SceneEntity? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;

        return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase)
               && x.Type == y.Type
               && x.IsProperNoun == y.IsProperNoun;
    }

    public int GetHashCode(SceneEntity obj)
    {
        return HashCode.Combine(
            obj.Type,
            obj.IsProperNoun,
            obj.Name?.ToLowerInvariant()
        );
    }
}
