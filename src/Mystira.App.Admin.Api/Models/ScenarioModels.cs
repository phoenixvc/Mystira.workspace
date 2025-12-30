namespace Mystira.App.Admin.Api.Models;

/// <summary>
/// Paginated response for scenario listing
/// </summary>
public class ScenarioListResponse
{
    public List<ScenarioSummary> Scenarios { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasNextPage { get; set; }
}

/// <summary>
/// Summary information for a scenario in list views
/// </summary>
public class ScenarioSummary
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public int Difficulty { get; set; }
    public int SessionLength { get; set; }
    public List<string> Archetypes { get; set; } = new();
    public int MinimumAge { get; set; }
    public string AgeGroup { get; set; } = string.Empty;
    public List<string> CoreAxes { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string? Image { get; set; }
}

/// <summary>
/// Validation result for scenario references
/// </summary>
public class ScenarioReferenceValidation
{
    public string ScenarioId { get; set; } = string.Empty;
    public string ScenarioTitle { get; set; } = string.Empty;
    public bool IsValid => MissingReferences.Count == 0;
    public List<MediaReference> MediaReferences { get; set; } = new();
    public List<CharacterReference> CharacterReferences { get; set; } = new();
    public List<MissingReference> MissingReferences { get; set; } = new();
}

/// <summary>
/// Media reference validation info
/// </summary>
public class MediaReference
{
    public string SceneId { get; set; } = string.Empty;
    public string SceneTitle { get; set; } = string.Empty;
    public string MediaId { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public bool MediaExists { get; set; }
    public bool HasMetadata { get; set; }
}

/// <summary>
/// Character reference validation info
/// </summary>
public class CharacterReference
{
    public string SceneId { get; set; } = string.Empty;
    public string SceneTitle { get; set; } = string.Empty;
    public string CharacterId { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public bool CharacterExists { get; set; }
    public bool HasMetadata { get; set; }
}

/// <summary>
/// Information about a missing reference
/// </summary>
public class MissingReference
{
    public string ReferenceId { get; set; } = string.Empty;
    public string ReferenceType { get; set; } = string.Empty;
    public string SceneId { get; set; } = string.Empty;
    public string SceneTitle { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
