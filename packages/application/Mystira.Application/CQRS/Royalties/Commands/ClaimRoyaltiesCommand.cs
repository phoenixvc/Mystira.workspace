namespace Mystira.Application.CQRS.Royalties.Commands;

/// <summary>
/// Command to claim accumulated royalties for a contributor
/// </summary>
/// <param name="IpAssetId">The unique identifier of the IP asset.</param>
/// <param name="ContributorWallet">The wallet address of the contributor claiming royalties.</param>
public record ClaimRoyaltiesCommand(
    string IpAssetId,
    string ContributorWallet) : ICommand<string>; // Returns transaction hash
