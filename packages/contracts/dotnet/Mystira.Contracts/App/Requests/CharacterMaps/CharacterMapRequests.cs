namespace Mystira.Contracts.App.Requests.CharacterMaps;

public record CreateCharacterMapRequest
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? Audio { get; set; }
}

public record UpdateCharacterMapRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? Audio { get; set; }
}
