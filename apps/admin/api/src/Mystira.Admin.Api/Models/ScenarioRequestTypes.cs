using Mystira.Domain.ValueObjects;

namespace Mystira.Admin.Api.Models;

/// <summary>
/// Request model for scene media references.
/// Used when mapping domain scene media to request format.
/// </summary>
public class SceneMediaRequest
{
    /// <summary>
    /// Image media reference ID
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Audio media reference ID
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// Video media reference ID
    /// </summary>
    public string? Video { get; set; }
}

/// <summary>
/// Request model for echo log entries.
/// Used when mapping domain echo logs to request format.
/// </summary>
public class EchoLogRequest
{
    /// <summary>
    /// The type of echo (e.g., "compassion", "courage")
    /// </summary>
    public string? EchoType { get; set; }

    /// <summary>
    /// Description of the echo effect
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Strength of the echo effect
    /// </summary>
    public float Strength { get; set; }
}

/// <summary>
/// Request model for compass changes.
/// Used when mapping domain compass changes to request format.
/// </summary>
public class CompassChangeRequest
{
    /// <summary>
    /// The compass axis affected
    /// </summary>
    public string? Axis { get; set; }

    /// <summary>
    /// The delta change value
    /// </summary>
    public float Delta { get; set; }
}

/// <summary>
/// Metadata for a character within a scenario.
/// Used when the Domain CharacterScenarioMetadata type is not available.
/// </summary>
public class CharacterScenarioMetadata
{
    /// <summary>
    /// Character roles
    /// </summary>
    public List<string>? Role { get; set; }

    /// <summary>
    /// Character archetypes
    /// </summary>
    public List<Archetype>? Archetype { get; set; }

    /// <summary>
    /// Character species
    /// </summary>
    public string? Species { get; set; }

    /// <summary>
    /// Character age
    /// </summary>
    public string? Age { get; set; }

    /// <summary>
    /// Character traits
    /// </summary>
    public List<string>? Traits { get; set; }

    /// <summary>
    /// Character backstory
    /// </summary>
    public string? Backstory { get; set; }
}

/// <summary>
/// Media references for a scene.
/// Used when the Domain SceneMedia type is not available.
/// </summary>
public class SceneMedia
{
    /// <summary>
    /// Image media reference ID
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Audio media reference ID
    /// </summary>
    public string? Audio { get; set; }

    /// <summary>
    /// Video media reference ID
    /// </summary>
    public string? Video { get; set; }
}

/// <summary>
/// Request model for scene branches.
/// Used when the Contracts BranchRequest type has incompatible properties.
/// </summary>
public class BranchRequest
{
    /// <summary>
    /// The choice text for this branch
    /// </summary>
    public string? Choice { get; set; }

    /// <summary>
    /// The next scene ID to navigate to
    /// </summary>
    public string? NextSceneId { get; set; }

    /// <summary>
    /// Echo log for this branch
    /// </summary>
    public EchoLogRequest? EchoLog { get; set; }

    /// <summary>
    /// Compass change for this branch
    /// </summary>
    public CompassChangeRequest? CompassChange { get; set; }
}

/// <summary>
/// Request model for echo reveals.
/// Used when the Contracts EchoRevealRequest type has incompatible properties.
/// </summary>
public class EchoRevealRequest
{
    /// <summary>
    /// The type of echo to reveal
    /// </summary>
    public string? EchoType { get; set; }

    /// <summary>
    /// Minimum strength required to trigger
    /// </summary>
    public float MinStrength { get; set; }

    /// <summary>
    /// Scene ID that triggers the reveal
    /// </summary>
    public string? TriggerSceneId { get; set; }

    /// <summary>
    /// Maximum age in scenes before reveal expires
    /// </summary>
    public int? MaxAgeScenes { get; set; }

    /// <summary>
    /// Mechanic used for the reveal
    /// </summary>
    public string? RevealMechanic { get; set; }

    /// <summary>
    /// Whether this reveal is required
    /// </summary>
    public bool Required { get; set; }
}
