namespace Mystira.Contracts.App.Requests.CharacterMaps;

/// <summary>
/// Request to create a new character map entry.
/// </summary>
public record CreateCharacterMapRequest
{
    /// <summary>
    /// The unique identifier for the character map.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The name of the character.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional URL or identifier for the character's image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Optional URL or identifier for the character's audio.
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// Optional metadata associated with the character map.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

/// <summary>
/// Request to update an existing character map entry.
/// </summary>
public record UpdateCharacterMapRequest
{
    /// <summary>
    /// The updated name of the character.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional updated URL or identifier for the character's image.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Optional updated URL or identifier for the character's audio.
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// Optional updated metadata associated with the character map.
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}
