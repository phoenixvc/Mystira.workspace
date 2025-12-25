namespace Mystira.Contracts.App.Responses.Attribution;

/// <summary>
/// Response containing creator credit information.
/// </summary>
public record CreatorCreditResponse
{
    /// <summary>
    /// The unique identifier of the creator.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// The name of the creator.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The role of the creator (e.g., Author, Artist, Editor).
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// The percentage of contribution for revenue sharing.
    /// </summary>
    public decimal? ContributionPercentage { get; init; }

    /// <summary>
    /// The wallet address of the creator for payments.
    /// </summary>
    public string? WalletAddress { get; init; }

    /// <summary>
    /// The profile identifier of the creator if registered.
    /// </summary>
    public string? ProfileId { get; init; }

    /// <summary>
    /// URL to the creator's avatar or profile image.
    /// </summary>
    public string? AvatarUrl { get; init; }

    /// <summary>
    /// The date and time when the creator was added.
    /// </summary>
    public DateTime? AddedAt { get; init; }
}
