using System.Text.Json.Serialization;

namespace Mystira.Contracts.StoryGenerator.Stories;

/// <summary>
/// Represents a branch (choice or outcome) from a scene.
/// </summary>
public class Branch
{
    /// <summary>
    /// Unique identifier for the branch.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Text displayed to the player for this choice.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// ID of the target scene this branch leads to.
    /// </summary>
    [JsonPropertyName("target_scene_id")]
    public string TargetSceneId { get; set; } = string.Empty;

    /// <summary>
    /// Condition that must be met for this branch to be available.
    /// </summary>
    [JsonPropertyName("condition")]
    public string? Condition { get; set; }

    /// <summary>
    /// For roll scenes, the required roll value or skill check.
    /// </summary>
    [JsonPropertyName("roll_requirement")]
    public RollRequirement? RollRequirement { get; set; }

    /// <summary>
    /// Compass changes that occur when taking this branch.
    /// </summary>
    [JsonPropertyName("compass_changes")]
    public List<CompassChange>? CompassChanges { get; set; }

    /// <summary>
    /// Whether this is a hidden option initially.
    /// </summary>
    [JsonPropertyName("is_hidden")]
    public bool IsHidden { get; set; }

    /// <summary>
    /// Order in which this branch appears.
    /// </summary>
    [JsonPropertyName("order")]
    public int Order { get; set; }

    /// <summary>
    /// Additional metadata for the branch.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Defines requirements for a dice roll or skill check.
/// </summary>
public class RollRequirement
{
    /// <summary>
    /// Type of roll (e.g., "skill", "luck", "combat").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Skill or attribute being tested.
    /// </summary>
    [JsonPropertyName("skill")]
    public string? Skill { get; set; }

    /// <summary>
    /// Target number to meet or exceed.
    /// </summary>
    [JsonPropertyName("target")]
    public int Target { get; set; }

    /// <summary>
    /// Number of dice to roll.
    /// </summary>
    [JsonPropertyName("dice_count")]
    public int DiceCount { get; set; } = 1;

    /// <summary>
    /// Number of sides on each die.
    /// </summary>
    [JsonPropertyName("dice_sides")]
    public int DiceSides { get; set; } = 6;

    /// <summary>
    /// Modifier to add to the roll.
    /// </summary>
    [JsonPropertyName("modifier")]
    public int Modifier { get; set; }
}
