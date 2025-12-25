namespace Mystira.Contracts.App.Requests.Media;

public record MediaQueryRequest
{
    public string? MediaType { get; set; }
    public List<string>? Tags { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public record MediaUpdateRequest
{
    public string? Description { get; set; }
    public List<string>? Tags { get; set; }
}

public record UploadMediaRequest
{
    public string? MediaId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public string MediaType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; }
    public List<string>? Tags { get; set; }
}
