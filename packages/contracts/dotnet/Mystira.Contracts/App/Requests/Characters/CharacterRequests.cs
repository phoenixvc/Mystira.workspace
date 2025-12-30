namespace Mystira.Contracts.App.Requests.Characters;

/// <summary>
/// Request to select a character for a game session.
/// </summary>
public record SelectCharacterRequest
{
    /// <summary>
    /// The unique identifier of the character to select.
    /// </summary>
    public string CharacterId { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the game session.
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// The unique identifier of the player making the selection.
    /// </summary>
    public string? PlayerId { get; set; }

    /// <summary>
    /// The unique identifier of the scenario.
    /// </summary>
    public string? ScenarioId { get; set; }
}
