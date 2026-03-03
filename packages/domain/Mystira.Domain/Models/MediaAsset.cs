using Mystira.Domain.Entities;

namespace Mystira.Domain.Models;

/// <summary>
/// Represents a media asset (image, audio, video).
/// </summary>
public class MediaAsset : Entity
{
    /// <summary>
    /// Gets or sets the asset name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the asset key/path.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the media type (image, audio, video).
    /// </summary>
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME type.
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Gets or sets the URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CDN URL.
    /// </summary>
    public string? CdnUrl { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail URL.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Gets or sets the uploader's user ID.
    /// </summary>
    public string? UploaderId { get; set; }

    /// <summary>
    /// Gets or sets the content hash for integrity.
    /// </summary>
    public string? ContentHash { get; set; }

    /// <summary>
    /// Gets or sets alt text for accessibility.
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Gets or sets the image width (for images).
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Gets or sets the image height (for images).
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Gets or sets the duration in seconds (for audio/video).
    /// </summary>
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets whether the asset is public.
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// Gets or sets tags as JSON.
    /// </summary>
    public string? TagsJson { get; set; }

    /// <summary>
    /// Gets or sets additional metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Gets or sets the media ID (alias for Id for DTO compatibility).
    /// </summary>
    public string MediaId
    {
        get => Id;
        set => Id = value;
    }

    /// <summary>
    /// Gets or sets the file size in bytes (alias for SizeBytes for DTO compatibility).
    /// </summary>
    public long FileSizeBytes
    {
        get => SizeBytes;
        set => SizeBytes = value;
    }

    /// <summary>
    /// Gets or sets the description (alias for AltText for DTO compatibility).
    /// </summary>
    public string? Description
    {
        get => AltText;
        set => AltText = value;
    }

    /// <summary>
    /// Gets or sets the content hash (alias for ContentHash for DTO compatibility).
    /// </summary>
    public string? Hash
    {
        get => ContentHash;
        set => ContentHash = value;
    }

    /// <summary>
    /// Gets or sets the asset version.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// Gets or sets the tags list (for DTO compatibility, backed by TagsJson).
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets the file extension.
    /// </summary>
    public string? Extension => Key.Contains('.') ? Key.Split('.').Last() : null;

    /// <summary>
    /// Gets the size in a human-readable format.
    /// </summary>
    public string SizeFormatted
    {
        get
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            return SizeBytes switch
            {
                >= GB => $"{SizeBytes / (double)GB:F2} GB",
                >= MB => $"{SizeBytes / (double)MB:F2} MB",
                >= KB => $"{SizeBytes / (double)KB:F2} KB",
                _ => $"{SizeBytes} bytes"
            };
        }
    }
}

/// <summary>
/// Represents avatar configuration options.
/// </summary>
public class AvatarConfiguration : Entity
{
    /// <summary>
    /// Gets or sets the configuration name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the age group ID this is for.
    /// </summary>
    public string? AgeGroupId { get; set; }

    /// <summary>
    /// Gets or sets the avatar image URL.
    /// </summary>
    public string AvatarUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the avatar thumbnail URL.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Gets or sets the avatar category.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets whether this is a premium avatar.
    /// </summary>
    public bool IsPremium { get; set; }

    /// <summary>
    /// Gets or sets whether this avatar is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the display order.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets the available media IDs for this configuration.
    /// </summary>
    public string? AvatarMediaIdsJson { get; set; }

    /// <summary>
    /// Gets or sets customization options as JSON.
    /// </summary>
    public string? CustomizationOptionsJson { get; set; }
}

/// <summary>
/// Represents an onboarding step.
/// </summary>
public class OnboardingStep : Entity
{
    /// <summary>
    /// Gets or sets the step key.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the step title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the step description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the step order.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets whether this step is required.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this step is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the target age group ID.
    /// </summary>
    public string? AgeGroupId { get; set; }

    /// <summary>
    /// Gets or sets the step content as JSON.
    /// </summary>
    public string? ContentJson { get; set; }
}
