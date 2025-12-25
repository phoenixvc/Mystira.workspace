namespace Mystira.Contracts.Requests.Scenarios;

/// <summary>
/// Request to create a new scenario.
/// </summary>
public class CreateScenarioRequest
{
    /// <summary>
    /// The title of the scenario.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// A description of the scenario content and objectives.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The difficulty level of the scenario.
    /// </summary>
    public string Difficulty { get; set; } = string.Empty;

    /// <summary>
    /// The expected duration of a session.
    /// </summary>
    public string SessionLength { get; set; } = string.Empty;

    /// <summary>
    /// Optional list of tags for categorization.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Optional list of character archetypes available in this scenario.
    /// </summary>
    public List<string>? Archetypes { get; set; }

    /// <summary>
    /// The target age group for this scenario.
    /// </summary>
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// The minimum recommended age for players.
    /// </summary>
    public int MinimumAge { get; set; }

    /// <summary>
    /// Optional list of core moral compass axes explored in this scenario.
    /// </summary>
    public List<string>? CoreAxes { get; set; }
}
