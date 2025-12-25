namespace Mystira.Contracts.App.Responses.Attribution;

public record ContentAttributionResponse
{
    public string ContentId { get; set; } = string.Empty;
    public string ContentTitle { get; set; } = string.Empty;
    public bool IsIpRegistered { get; set; }
    public string? IpAssetId { get; set; }
    public DateTime? RegisteredAt { get; set; }
    public List<CreatorCredit> Credits { get; set; } = new();
}

public record CreatorCredit
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public decimal? ContributionPercentage { get; set; }
}

public record IpVerificationResponse
{
    public string ContentId { get; set; } = string.Empty;
    public string ContentTitle { get; set; } = string.Empty;
    public bool IsRegistered { get; set; }
    public string? IpAssetId { get; set; }
    public DateTime? RegisteredAt { get; set; }
    public string? RegistrationTxHash { get; set; }
    public int ContributorCount { get; set; }
}
