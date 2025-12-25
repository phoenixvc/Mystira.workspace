using Mystira.Contracts.App.Models;

namespace Mystira.Contracts.App.Requests.GameSessions;

/// <summary>
/// Request to start a new game session.
/// </summary>
public record StartGameSessionRequest
{
    /// <summary>
    /// The unique identifier of the scenario to play.
    /// </summary>
    public string ScenarioId { get; set; } = string.Empty;

    /// <summary>
    /// The account identifier of the user starting the session.
    /// </summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// The profile identifier of the player.
    /// </summary>
    public string ProfileId { get; set; } = string.Empty;

    /// <summary>
    /// Optional list of player names participating in the session.
    /// </summary>
    public List<string>? PlayerNames { get; set; }

    /// <summary>
    /// Optional list of character assignments for each player.
    /// </summary>
    public List<CharacterAssignmentDto>? CharacterAssignments { get; set; }

    /// <summary>
    /// The target age group for content filtering.
    /// </summary>
    public string TargetAgeGroup { get; set; } = string.Empty;
}

/// <summary>
/// Request to make a choice during a game session.
/// </summary>
public record MakeChoiceRequest
{
    /// <summary>
    /// The unique identifier of the current session.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the current scene.
    /// </summary>
    public string SceneId { get; set; } = string.Empty;

    /// <summary>
    /// The text of the choice made by the player.
    /// </summary>
    public string ChoiceText { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the next scene to navigate to.
    /// </summary>
    public string NextSceneId { get; set; } = string.Empty;

    /// <summary>
    /// Optional identifier of the player making the choice.
    /// </summary>
    public string? PlayerId { get; set; }

    /// <summary>
    /// Optional compass axis affected by this choice.
    /// </summary>
    public string? CompassAxis { get; set; }

    /// <summary>
    /// Optional direction on the compass axis.
    /// </summary>
    public string? CompassDirection { get; set; }

    /// <summary>
    /// Optional delta value for compass movement.
    /// </summary>
    public double? CompassDelta { get; set; }
}

/// <summary>
/// Request to progress to the next scene in a session.
/// </summary>
public record ProgressSceneRequest
{
    /// <summary>
    /// The unique identifier of the current session.
    /// </summary>
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the scene to progress to.
    /// </summary>
    public string SceneId { get; set; } = string.Empty;
}
