using Mystira.Domain.Entities;
using Mystira.Domain.ValueObjects;

namespace Mystira.Domain.Models;

/// <summary>
/// Represents a player's score on a specific scenario.
/// </summary>
public class PlayerScenarioScore : Entity
{
    /// <summary>
    /// Gets or sets the user profile ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scenario ID.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the best score achieved.
    /// </summary>
    public int BestScore { get; set; }

    /// <summary>
    /// Gets or sets the total attempts.
    /// </summary>
    public int TotalAttempts { get; set; }

    /// <summary>
    /// Gets or sets the number of completions.
    /// </summary>
    public int Completions { get; set; }

    /// <summary>
    /// Gets or sets the first completion date.
    /// </summary>
    public DateTime? FirstCompletedAt { get; set; }

    /// <summary>
    /// Gets or sets the last played date.
    /// </summary>
    public DateTime? LastPlayedAt { get; set; }

    /// <summary>
    /// Gets or sets the total play time in seconds.
    /// </summary>
    public int TotalPlayTimeSeconds { get; set; }

    /// <summary>
    /// Gets or sets the best ending achieved.
    /// </summary>
    public string? BestEnding { get; set; }

    /// <summary>
    /// Gets or sets all endings discovered as JSON array.
    /// </summary>
    public string? EndingsDiscoveredJson { get; set; }

    /// <summary>
    /// Gets or sets the percentage of scenes visited.
    /// </summary>
    public decimal ScenesVisitedPercentage { get; set; }

    /// <summary>
    /// Gets or sets the percentage of echoes discovered.
    /// </summary>
    public decimal EchoesDiscoveredPercentage { get; set; }

    /// <summary>
    /// Navigation to the user profile.
    /// </summary>
    public virtual UserProfile? User { get; set; }

    /// <summary>
    /// Navigation to the scenario.
    /// </summary>
    public virtual Scenario? Scenario { get; set; }

    /// <summary>
    /// Gets the total play time as a TimeSpan.
    /// </summary>
    public TimeSpan TotalPlayTime => TimeSpan.FromSeconds(TotalPlayTimeSeconds);

    /// <summary>
    /// Records a new attempt.
    /// </summary>
    /// <param name="score">The score achieved.</param>
    /// <param name="completed">Whether the scenario was completed.</param>
    /// <param name="ending">The ending achieved if completed.</param>
    /// <param name="playTimeSeconds">The play time in seconds.</param>
    public void RecordAttempt(int score, bool completed, string? ending = null, int playTimeSeconds = 0)
    {
        TotalAttempts++;
        TotalPlayTimeSeconds += playTimeSeconds;
        LastPlayedAt = DateTime.UtcNow;

        if (score > BestScore)
        {
            BestScore = score;
        }

        if (completed)
        {
            Completions++;
            if (!FirstCompletedAt.HasValue)
            {
                FirstCompletedAt = DateTime.UtcNow;
            }

            if (!string.IsNullOrEmpty(ending))
            {
                BestEnding ??= ending;
            }
        }
    }
}

/// <summary>
/// Represents a player's overall compass progress.
/// </summary>
public class PlayerCompassProgress : Entity
{
    /// <summary>
    /// Gets or sets the user profile ID.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the overall compass values as JSON.
    /// </summary>
    public string CompassValuesJson { get; set; } = "{}";

    /// <summary>
    /// Gets or sets the dominant axis ID.
    /// </summary>
    public string? DominantAxisId { get; set; }

    /// <summary>
    /// Gets or sets the dominant archetype ID.
    /// </summary>
    public string? DominantArchetypeId { get; set; }

    /// <summary>
    /// Gets or sets the total choices made.
    /// </summary>
    public int TotalChoices { get; set; }

    /// <summary>
    /// Gets or sets the total sessions played.
    /// </summary>
    public int TotalSessions { get; set; }

    /// <summary>
    /// Gets or sets the last updated time.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the dominant axis.
    /// </summary>
    public CoreAxis? DominantAxis => CoreAxis.FromValue(DominantAxisId);

    /// <summary>
    /// Gets the dominant archetype.
    /// </summary>
    public Archetype? DominantArchetype => Archetype.FromValue(DominantArchetypeId);

    /// <summary>
    /// Navigation to the user profile.
    /// </summary>
    public virtual UserProfile? User { get; set; }

    /// <summary>
    /// Updates the compass from a completed session.
    /// </summary>
    /// <param name="compassTracking">The session's compass tracking.</param>
    public void UpdateFromSession(CompassTracking compassTracking)
    {
        TotalSessions++;
        TotalChoices += compassTracking.History.Count;
        LastUpdatedAt = DateTime.UtcNow;

        // In a real implementation, this would merge the compass values
        // and recalculate the dominant axis and archetype
    }
}
