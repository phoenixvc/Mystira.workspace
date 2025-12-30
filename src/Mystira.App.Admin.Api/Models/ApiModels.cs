using System.ComponentModel.DataAnnotations;

namespace Mystira.App.Admin.Api.Models;

// Note: Most request/response types have been moved to Mystira.Contracts package (0.5.0+)
// Import from:
//   - Mystira.Contracts.App.Requests.*
//   - Mystira.Contracts.App.Responses.*

// Character and CharacterMapFile are defined in MediaModels.cs

/// <summary>
/// Request to progress a game session to a new scene (admin operation)
/// </summary>
public class AdminProgressSceneRequest
{
    [Required]
    public string NewSceneId { get; set; } = string.Empty;
}
