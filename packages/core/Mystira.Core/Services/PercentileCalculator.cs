namespace Mystira.Core.Services;

/// <summary>
/// Utility for calculating percentile values from score distributions.
/// Extracted from CalculateBadgeScoresQueryHandler to reduce complexity and improve testability.
/// </summary>
public static class PercentileCalculator
{
    /// <summary>
    /// Computes the requested percentile values from a collection of numeric scores.
    /// </summary>
    public static Dictionary<double, double> CalculatePercentiles(List<double> scores, List<double> percentiles)
    {
        var result = new Dictionary<double, double>();

        if (!scores.Any())
        {
            return result;
        }

        var sortedScores = scores.OrderBy(s => s).ToList();

        foreach (var percentile in percentiles)
        {
            result[percentile] = CalculatePercentile(sortedScores, percentile);
        }

        return result;
    }

    /// <summary>
    /// Computes the value at the specified percentile from sorted scores using linear interpolation.
    /// </summary>
    public static double CalculatePercentile(List<double> sortedScores, double percentile)
    {
        if (!sortedScores.Any())
            return 0;

        if (sortedScores.Count == 1)
            return sortedScores[0];

        var position = (percentile / 100.0) * (sortedScores.Count - 1);
        var lowerIndex = (int)Math.Floor(position);
        var upperIndex = (int)Math.Ceiling(position);

        if (lowerIndex < 0)
            return sortedScores[0];

        if (upperIndex >= sortedScores.Count)
            return sortedScores[^1];

        if (lowerIndex == upperIndex)
            return sortedScores[lowerIndex];

        var fraction = position - lowerIndex;
        return sortedScores[lowerIndex] + (sortedScores[upperIndex] - sortedScores[lowerIndex]) * fraction;
    }
}
