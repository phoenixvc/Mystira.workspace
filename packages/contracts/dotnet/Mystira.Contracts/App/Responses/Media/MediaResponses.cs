namespace Mystira.Contracts.App.Responses.Media;

/// <summary>
/// Response containing available avatars grouped by age group.
/// </summary>
public record AvatarResponse
{
    /// <summary>
    /// Dictionary mapping age groups to lists of available avatar identifiers.
    /// </summary>
    public Dictionary<string, List<string>> AgeGroupAvatars { get; set; } = new();
}

/// <summary>
/// Response containing avatar configuration for a specific age group.
/// </summary>
public record AvatarConfigurationResponse
{
    /// <summary>
    /// The age group this configuration applies to.
    /// </summary>
    public string AgeGroup { get; set; } = string.Empty;

    /// <summary>
    /// The list of available avatar media identifiers for this age group.
    /// </summary>
    public List<string> AvatarMediaIds { get; set; } = new();
}

/// <summary>
/// Response containing paginated media query results.
/// </summary>
public record MediaQueryResponse
{
    /// <summary>
    /// The list of media items matching the query.
    /// </summary>
    public List<MediaItem> Media { get; set; } = new();

    /// <summary>
    /// The total number of items matching the query.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of pages available.
    /// </summary>
    public int TotalPages { get; set; }
}

/// <summary>
/// Represents a media item in the system.
/// </summary>
public record MediaItem
{
    /// <summary>
    /// The unique identifier of the media item.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The original file name of the media.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The type of media (e.g., image, audio, video).
    /// </summary>
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// The URL to access the media content.
    /// </summary>
    public string? Url { get; set; }
}
