namespace Mystira.Contracts.App.Responses.Avatars;

/// <summary>
/// Response containing avatar information.
/// </summary>
public record AvatarResponse
{
    /// <summary>
    /// The unique identifier of the avatar.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the avatar.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The URL or identifier for the avatar image.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// The media identifier for the avatar image.
    /// </summary>
    public string? MediaId { get; set; }

    /// <summary>
    /// The category of the avatar (e.g., "default", "premium", "earned").
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Whether the avatar is available to the user.
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Whether the avatar is currently selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// The age group this avatar is appropriate for.
    /// </summary>
    public string? AgeGroup { get; set; }

    /// <summary>
    /// Sort order for display purposes.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Optional tags for categorization.
    /// </summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Response containing avatar configuration and available options.
/// </summary>
public record AvatarConfigurationResponse
{
    /// <summary>
    /// List of available avatars.
    /// </summary>
    public List<AvatarResponse> Avatars { get; set; } = new();

    /// <summary>
    /// List of avatar categories.
    /// </summary>
    public List<AvatarCategoryResponse> Categories { get; set; } = new();

    /// <summary>
    /// The currently selected avatar ID.
    /// </summary>
    public string? SelectedAvatarId { get; set; }

    /// <summary>
    /// Whether custom avatars are enabled.
    /// </summary>
    public bool AllowCustomAvatars { get; set; }

    /// <summary>
    /// Maximum file size for custom avatars in bytes.
    /// </summary>
    public long? MaxCustomAvatarSize { get; set; }

    /// <summary>
    /// Allowed file types for custom avatars.
    /// </summary>
    public List<string>? AllowedFileTypes { get; set; }
}

/// <summary>
/// Response containing avatar category information.
/// </summary>
public record AvatarCategoryResponse
{
    /// <summary>
    /// The unique identifier of the category.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the category.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the category.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Sort order for display purposes.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Number of avatars in this category.
    /// </summary>
    public int AvatarCount { get; set; }
}
