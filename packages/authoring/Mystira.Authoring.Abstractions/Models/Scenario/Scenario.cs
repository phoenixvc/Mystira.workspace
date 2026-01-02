namespace Mystira.Authoring.Abstractions.Models.Scenario;

/// <summary>
/// Represents a complete interactive story scenario.
/// </summary>
public class Scenario
{
    /// <summary>
    /// Unique identifier for the scenario.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Title of the scenario.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Description of the scenario.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Path or URL to the scenario's cover image.
    /// </summary>
    public string Image { get; set; } = string.Empty;

    /// <summary>
    /// Tags for categorization and discovery.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Difficulty level of the scenario.
    /// </summary>
    public DifficultyLevel Difficulty { get; set; }

    /// <summary>
    /// Expected session length.
    /// </summary>
    public SessionLength SessionLength { get; set; }

    /// <summary>
    /// Character archetypes present in the scenario.
    /// </summary>
    public List<string> Archetypes { get; set; } = new();

    /// <summary>
    /// Target age group identifier.
    /// </summary>
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// Minimum recommended age.
    /// </summary>
    public int MinimumAge { get; set; }

    /// <summary>
    /// Core developmental/moral axes tracked in the scenario.
    /// </summary>
    public List<string> CoreAxes { get; set; } = new();

    /// <summary>
    /// Characters in the scenario.
    /// </summary>
    public List<ScenarioCharacter> Characters { get; set; } = new();

    /// <summary>
    /// Scenes that make up the scenario.
    /// </summary>
    public List<Scene> Scenes { get; set; } = new();

    /// <summary>
    /// When the scenario was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Difficulty level for a scenario.
/// </summary>
public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

/// <summary>
/// Expected session length for a scenario.
/// </summary>
public enum SessionLength
{
    Short,
    Medium,
    Long
}
