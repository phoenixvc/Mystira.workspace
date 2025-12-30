using Mystira.App.Domain.Models;

namespace Mystira.App.Admin.Api.Models;

// Note: Most request/response types have been moved to Mystira.Contracts package (0.5.0+)
// Import from:
//   - Mystira.Contracts.Requests.*
//   - Mystira.Contracts.Responses.*

// Admin-specific models that remain local

/// <summary>
/// Character model for admin operations (wraps domain Character with admin-specific fields)
/// </summary>
public class Character
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageId { get; set; }
    public string? AudioId { get; set; }
    public CharacterMetadata? Metadata { get; set; }
}

/// <summary>
/// Character map file model for admin bulk operations
/// </summary>
public class CharacterMapFile
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<Character> Characters { get; set; } = new();
    public DateTime LastModified { get; set; }
}
