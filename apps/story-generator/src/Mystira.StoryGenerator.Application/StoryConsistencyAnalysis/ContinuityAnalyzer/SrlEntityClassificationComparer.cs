using Mystira.StoryGenerator.Contracts.StoryConsistency;

namespace Mystira.StoryGenerator.Application.StoryConsistencyAnalysis.ContinuityAnalyzer;

/// <summary>
/// Compares SRL entity classifications by their identity fields: (Name, Type).
/// Ordinal comparison; nulls treated as empty strings.
/// </summary>
internal sealed class SrlEntityClassificationComparer : IEqualityComparer<SrlEntityClassification>
{
    public static readonly SrlEntityClassificationComparer Instance = new();

    public bool Equals(SrlEntityClassification? x, SrlEntityClassification? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;

        return string.Equals(x.Name ?? string.Empty, y.Name ?? string.Empty, StringComparison.Ordinal)
            && string.Equals(x.Type ?? string.Empty, y.Type ?? string.Empty, StringComparison.Ordinal);
    }

    public int GetHashCode(SrlEntityClassification obj)
    {
        var hc = new HashCode();
        hc.Add(obj.Name ?? string.Empty, StringComparer.Ordinal);
        hc.Add(obj.Type ?? string.Empty, StringComparer.Ordinal);
        return hc.ToHashCode();
    }
}
