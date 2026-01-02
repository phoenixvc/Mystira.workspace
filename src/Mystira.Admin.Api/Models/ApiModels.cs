namespace Mystira.Admin.Api.Models;

// Note: Most request/response types have been moved to Mystira.Contracts package (0.5.0+)
// Import from:
//   - Mystira.Contracts.App.Requests.*
//   - Mystira.Contracts.App.Responses.*

// Character and CharacterMapFile are defined in MediaModels.cs

// AdminProgressSceneRequest is now in Mystira.Contracts.App.Requests.GameSessions

/// <summary>
/// Simple key-value modifier for media metadata.
/// Locally defined as it's not in the shared Domain package.
/// </summary>
public class Modifier
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
