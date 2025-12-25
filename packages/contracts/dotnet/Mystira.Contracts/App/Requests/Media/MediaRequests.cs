namespace Mystira.Contracts.App.Requests.Media;

/// <summary>
/// Request to query media items with filtering and pagination.
/// </summary>
public record MediaQueryRequest
{
    /// <summary>
    /// Optional filter by media type (e.g., image, audio, video).
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    /// Optional list of tags to filter media items.
    /// </summary>
    public List<string>? Tags { get; set; }

    /// <summary>
    /// The page number for pagination (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Request to update an existing media item.
/// </summary>
public record MediaUpdateRequest
{
    /// <summary>
    /// Optional updated description for the media item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional updated list of tags for the media item.
    /// </summary>
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Request to upload a new media item.
/// </summary>
public record UploadMediaRequest
{
    /// <summary>
    /// Optional pre-assigned media identifier.
    /// </summary>
    public string? MediaId { get; set; }

    /// <summary>
    /// The original file name of the uploaded media.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Optional MIME content type of the file.
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// The type of media (e.g., image, audio, video).
    /// </summary>
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// The file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Optional description for the media item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional list of tags for categorization.
    /// </summary>
    public List<string>? Tags { get; set; }
}
