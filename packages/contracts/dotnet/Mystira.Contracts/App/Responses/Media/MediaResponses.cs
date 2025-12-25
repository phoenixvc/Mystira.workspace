namespace Mystira.Contracts.App.Responses.Media;

public record AvatarResponse
{
    public Dictionary<string, List<string>> AgeGroupAvatars { get; set; } = new();
}

public record AvatarConfigurationResponse
{
    public string AgeGroup { get; set; } = string.Empty;
    public List<string> AvatarMediaIds { get; set; } = new();
}

public record MediaQueryResponse
{
    public List<MediaItem> Media { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public record MediaItem
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public string? Url { get; set; }
}
