namespace Mystira.Contracts.App.Requests.Contributors;

public record RegisterIpAssetRequest
{
    public string MetadataUri { get; set; } = string.Empty;
    public string MetadataHash { get; set; } = string.Empty;
}

public record SetContributorsRequest
{
    public List<ContributorRequest> Contributors { get; set; } = new();
}

public record ContributorRequest
{
    public string Name { get; set; } = string.Empty;
    public string? WalletAddress { get; set; }
    public string Role { get; set; } = string.Empty;
    public decimal ContributionPercentage { get; set; }
}
