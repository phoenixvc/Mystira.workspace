namespace Mystira.Contracts.App.Requests.Scenarios;

/// <summary>
/// Request model representing a character definition in a scenario.
/// </summary>
public record CharacterRequest
{
    /// <summary>
    /// Optional unique identifier for the character.
    /// </summary>
    public string? Id { get; set; }

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
    /// Optional list of traits associated with the character.
    /// </summary>
    public List<string>? Traits { get; set; }

    /// <summary>
    /// Whether this character is a player character.
    /// </summary>
    public bool IsPlayerCharacter { get; set; } = true;

    /// <summary>
    /// Additional metadata for the character.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Request model representing a scene definition in a scenario.
/// </summary>
public record SceneRequest
{
    /// <summary>
    /// Optional unique identifier for the scene.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// The title of the scene.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// A description of the scene.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The narrative content of the scene.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// The order of this scene in the scenario.
    /// </summary>
    public int Order { get; set; }

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
    /// Additional metadata for the scene.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Request model representing a choice within a scene.
/// </summary>
public record ChoiceRequest
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
