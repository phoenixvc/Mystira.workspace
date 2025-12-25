namespace Mystira.Contracts.App.Models;

public record CharacterAssignmentDto
{
    public string CharacterId { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string? Audio { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Archetype { get; set; } = string.Empty;
    public PlayerAssignmentDto? PlayerAssignment { get; set; }
}

public record PlayerAssignmentDto
{
    public string Type { get; set; } = string.Empty;
    public string? ProfileId { get; set; }
    public string? ProfileName { get; set; }
    public string? GuestName { get; set; }
}
