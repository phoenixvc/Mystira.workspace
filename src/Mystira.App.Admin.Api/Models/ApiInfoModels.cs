namespace Mystira.App.Admin.Api.Models;

/// <summary>
/// Legacy value mappings for backward compatibility (admin-specific)
/// </summary>
public class LegacyMappings
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string> CoreAxisMappings { get; set; } = new();
    public Dictionary<string, string> ArchetypeMappings { get; set; } = new();
}
