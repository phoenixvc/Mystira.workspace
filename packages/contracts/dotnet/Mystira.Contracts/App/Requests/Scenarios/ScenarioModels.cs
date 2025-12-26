namespace Mystira.Contracts.App.Requests.Scenarios;

/// <summary>
/// Request model representing character metadata.
/// </summary>
public class CharacterMetadataRequest
{
    /// <summary>
    /// The roles of the character in the story.
    /// </summary>
    public List<string>? Role { get; set; }

    /// <summary>
    /// The archetype classifications of the character.
    /// </summary>
    public List<string>? Archetype { get; set; }

    /// <summary>
    /// The species of the character.
    /// </summary>
    public string? Species { get; set; }

    /// <summary>
    /// The age of the character.
    /// </summary>
    public int? Age { get; set; }

    /// <summary>
    /// List of character traits.
    /// </summary>
    public List<string>? Traits { get; set; }

    /// <summary>
    /// The backstory of the character.
    /// </summary>
    public string? Backstory { get; set; }
}

/// <summary>
/// Request model representing media references.
/// </summary>
public class MediaReferencesRequest
{
    /// <summary>
    /// Image URL or identifier.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Audio URL or identifier.
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// Video URL or identifier.
    /// </summary>
    public string? Video { get; set; }
}

/// <summary>
/// Request model representing a branch (choice) in a scene.
/// </summary>
public class BranchRequest
{
    /// <summary>
    /// The text displayed for this branch/choice.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The identifier of the scene this branch leads to.
    /// </summary>
    public string? NextSceneId { get; set; }

    /// <summary>
    /// The compass axis affected by this choice.
    /// </summary>
    public string? CompassAxis { get; set; }

    /// <summary>
    /// The direction on the compass axis (positive/negative).
    /// </summary>
    public string? CompassDirection { get; set; }

    /// <summary>
    /// The delta value for the compass axis.
    /// </summary>
    public double? CompassDelta { get; set; }
}

/// <summary>
/// Request model representing an echo reveal condition.
/// </summary>
public class EchoRevealRequest
{
    /// <summary>
    /// The condition that triggers the echo reveal.
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// The message to display when revealed.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// The tone of the echo reveal.
    /// </summary>
    public string? Tone { get; set; }
}

/// <summary>
/// Request model representing a character definition in a scenario.
/// </summary>
public class CharacterRequest
{
    /// <summary>
    /// Unique identifier for the character.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The name of the character.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A description of the character.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The role of the character in the story.
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// The archetype classification of the character.
    /// </summary>
    public string? Archetype { get; set; }

    /// <summary>
    /// Optional URL or identifier for the character's image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Optional URL or identifier for the character's audio.
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// Character metadata including role, archetype, species, etc.
    /// </summary>
    public CharacterMetadataRequest? Metadata { get; set; }
  
    /// Optional list of traits associated with the character.
    /// </summary>
    public List<string>? Traits { get; set; }

    /// <summary>
    /// Whether this character is a player character.
    /// </summary>
    public bool IsPlayerCharacter { get; set; } = true;
}

/// <summary>
/// Request model representing a scene definition in a scenario.
/// </summary>
public class SceneRequest
{
    /// <summary>
    /// Unique identifier for the scene.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The title of the scene.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The type of scene (e.g., "narrative", "choice", "ending").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// A description of the scene.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The narrative content of the scene.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// The order of this scene in the scenario.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The identifier of the next scene (for linear progression).
    /// </summary>
    public string? NextSceneId { get; set; }

    /// <summary>
    /// The difficulty level of this scene.
    /// </summary>
    public string? Difficulty { get; set; }

    /// <summary>
    /// The active character in this scene.
    /// </summary>
    public string? ActiveCharacter { get; set; }

    /// <summary>
    /// Media references for this scene.
    /// </summary>
    public MediaReferencesRequest? Media { get; set; }

    /// <summary>
    /// Optional URL or identifier for the scene's background image.
    /// </summary>
    public string? BackgroundImage { get; set; }

    /// <summary>
    /// Optional URL or identifier for the scene's background music.
    /// </summary>
    public string? BackgroundMusic { get; set; }

    /// <summary>
    /// Optional list of choices available in this scene.
    /// </summary>
    public List<ChoiceRequest>? Choices { get; set; }

    /// <summary>
    /// Optional list of branches (choices) in this scene.
    /// </summary>
    public List<BranchRequest>? Branches { get; set; }

    /// <summary>
    /// Optional list of echo reveals in this scene.
    /// </summary>
    public List<EchoRevealRequest>? EchoReveals { get; set; }
}

/// <summary>
/// Request model representing a choice within a scene.
/// </summary>
public class ChoiceRequest
{
    /// <summary>
    /// Optional unique identifier for the choice.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The text displayed for this choice.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Optional identifier of the scene this choice leads to.
    /// </summary>
    public string? NextSceneId { get; set; }

    /// <summary>
    /// Optional compass axis impacts for this choice.
    /// </summary>
    public Dictionary<string, double>? CompassImpacts { get; set; }

    /// <summary>
    /// Additional metadata for the choice.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
