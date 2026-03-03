using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Domain.Enums;
using Mystira.Domain.Models;
using Mystira.Infrastructure.StoryProtocol.Services.Mock;
using Xunit;

namespace Mystira.Infrastructure.StoryProtocol.Tests;

/// <summary>
/// Unit tests for <see cref="MockStoryProtocolService"/>.
/// </summary>
public class MockStoryProtocolServiceTests
{
    private readonly Mock<ILogger<MockStoryProtocolService>> _loggerMock;
    private readonly MockStoryProtocolService _service;

    public MockStoryProtocolServiceTests()
    {
        _loggerMock = new Mock<ILogger<MockStoryProtocolService>>();
        _service = new MockStoryProtocolService(_loggerMock.Object);
    }

    #region RegisterIpAssetAsync Tests

    [Fact]
    public async Task RegisterIpAssetAsync_WithValidInput_ReturnsRegisteredAsset()
    {
        // Arrange
        var contentId = "scenario-123";
        var contentTitle = "Test Scenario";
        var contributors = CreateTestContributors();

        // Act
        var result = await _service.RegisterIpAssetAsync(contentId, contentTitle, contributors);

        // Assert
        result.Should().NotBeNull();
        result.IpAssetId.Should().StartWith("0x");
        result.TransactionHash.Should().StartWith("0x");
        result.IsRegistered.Should().BeTrue();
        result.RegisteredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Contributors.Should().HaveCount(2);
    }

    [Fact]
    public async Task RegisterIpAssetAsync_WithMetadataAndLicense_IncludesInResult()
    {
        // Arrange
        var contributors = CreateTestContributors();
        var metadataUri = "ipfs://QmTest123";
        var licenseTermsId = "PIL-001";

        // Act
        var result = await _service.RegisterIpAssetAsync(
            "content-1", "Test", contributors, metadataUri, licenseTermsId);

        // Assert
        result.LicenseTermsId.Should().Be(licenseTermsId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task RegisterIpAssetAsync_WithInvalidContentId_ThrowsArgumentException(string? contentId)
    {
        // Arrange
        var contributors = CreateTestContributors();

        // Act
        var act = () => _service.RegisterIpAssetAsync(contentId!, "Title", contributors);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("contentId");
    }

    [Fact]
    public async Task RegisterIpAssetAsync_WithEmptyContributors_ThrowsArgumentException()
    {
        // Act
        var act = () => _service.RegisterIpAssetAsync("id", "title", new List<Contributor>());

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*At least one contributor*");
    }

    [Fact]
    public async Task RegisterIpAssetAsync_WithNullContributors_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _service.RegisterIpAssetAsync("id", "title", null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("contributors");
    }

    #endregion

    #region IsRegisteredAsync Tests

    [Fact]
    public async Task IsRegisteredAsync_AfterRegistration_ReturnsTrue()
    {
        // Arrange
        var contentId = "registered-content";
        await _service.RegisterIpAssetAsync(contentId, "Title", CreateTestContributors());

        // Act
        var result = await _service.IsRegisteredAsync(contentId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsRegisteredAsync_BeforeRegistration_ReturnsFalse()
    {
        // Act
        var result = await _service.IsRegisteredAsync("unregistered-content");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task IsRegisteredAsync_WithInvalidContentId_ThrowsArgumentException(string? contentId)
    {
        // Act
        var act = () => _service.IsRegisteredAsync(contentId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region PayRoyaltyAsync Tests

    [Fact]
    public async Task PayRoyaltyAsync_WithValidInput_DistributesToContributors()
    {
        // Arrange
        var registration = await _service.RegisterIpAssetAsync(
            "content-1", "Title", CreateTestContributors());
        var paymentAmount = 100m;

        // Act
        var result = await _service.PayRoyaltyAsync(registration.IpAssetId!, paymentAmount, "order-123");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Amount.Should().Be(paymentAmount);
        result.IpAssetId.Should().Be(registration.IpAssetId);
        result.PayerReference.Should().Be("order-123");
        result.TransactionHash.Should().StartWith("0x");
        result.Distributions.Should().HaveCount(2);

        // Verify distribution amounts (60% + 40% = 100%)
        var totalDistributed = result.Distributions.Sum(d => d.Amount);
        totalDistributed.Should().Be(paymentAmount);
    }

    [Fact]
    public async Task PayRoyaltyAsync_UpdatesClaimableBalances()
    {
        // Arrange
        var registration = await _service.RegisterIpAssetAsync(
            "content-1", "Title", CreateTestContributors());
        var paymentAmount = 100m;

        // Act
        await _service.PayRoyaltyAsync(registration.IpAssetId!, paymentAmount);
        var balance = await _service.GetClaimableRoyaltiesAsync(registration.IpAssetId!);

        // Assert
        balance.TotalClaimable.Should().Be(paymentAmount);
        balance.TotalReceived.Should().Be(paymentAmount);
        balance.ContributorBalances.Should().HaveCount(2);
        balance.ContributorBalances.Sum(cb => cb.ClaimableAmount).Should().Be(paymentAmount);
    }

    [Fact]
    public async Task PayRoyaltyAsync_WithZeroAmount_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var registration = await _service.RegisterIpAssetAsync(
            "content-1", "Title", CreateTestContributors());

        // Act
        var act = () => _service.PayRoyaltyAsync(registration.IpAssetId!, 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithParameterName("amount");
    }

    [Fact]
    public async Task PayRoyaltyAsync_WithNegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _service.PayRoyaltyAsync("0xtest", -100);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    #endregion

    #region ClaimRoyaltiesAsync Tests

    [Fact]
    public async Task ClaimRoyaltiesAsync_WithClaimableBalance_ReturnsTransactionHash()
    {
        // Arrange
        var contributors = CreateTestContributors();
        var registration = await _service.RegisterIpAssetAsync("content-1", "Title", contributors);
        await _service.PayRoyaltyAsync(registration.IpAssetId!, 100m);

        var walletAddress = contributors[0].WalletAddress!;

        // Act
        var txHash = await _service.ClaimRoyaltiesAsync(registration.IpAssetId!, walletAddress);

        // Assert
        txHash.Should().StartWith("0x");
    }

    [Fact]
    public async Task ClaimRoyaltiesAsync_UpdatesBalances()
    {
        // Arrange
        var contributors = CreateTestContributors();
        var registration = await _service.RegisterIpAssetAsync("content-1", "Title", contributors);
        await _service.PayRoyaltyAsync(registration.IpAssetId!, 100m);

        var walletAddress = contributors[0].WalletAddress!;
        var expectedClaimed = 60m; // 60% share

        // Act
        await _service.ClaimRoyaltiesAsync(registration.IpAssetId!, walletAddress);
        var balance = await _service.GetClaimableRoyaltiesAsync(registration.IpAssetId!);

        // Assert
        balance.TotalClaimed.Should().Be(expectedClaimed);
        balance.TotalClaimable.Should().Be(40m); // Remaining 40%

        var contributorBalance = balance.ContributorBalances.First(cb => cb.WalletAddress == walletAddress);
        contributorBalance.ClaimableAmount.Should().Be(0);
        contributorBalance.ClaimedAmount.Should().Be(expectedClaimed);
    }

    [Theory]
    [InlineData(null, "0xwallet")]
    [InlineData("", "0xwallet")]
    [InlineData("0xasset", null)]
    [InlineData("0xasset", "")]
    public async Task ClaimRoyaltiesAsync_WithInvalidInput_ThrowsArgumentException(
        string? ipAssetId, string? contributorWallet)
    {
        // Act
        var act = () => _service.ClaimRoyaltiesAsync(ipAssetId!, contributorWallet!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region GetRoyaltyConfigurationAsync Tests

    [Fact]
    public async Task GetRoyaltyConfigurationAsync_WithRegisteredAsset_ReturnsConfiguration()
    {
        // Arrange
        var contributors = CreateTestContributors();
        var registration = await _service.RegisterIpAssetAsync("content-1", "Title", contributors);

        // Act
        var config = await _service.GetRoyaltyConfigurationAsync(registration.IpAssetId!);

        // Assert
        config.Should().NotBeNull();
        config!.IpAssetId.Should().Be(registration.IpAssetId);
        config.Contributors.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRoyaltyConfigurationAsync_WithUnregisteredAsset_ReturnsNull()
    {
        // Act
        var config = await _service.GetRoyaltyConfigurationAsync("0xunknown");

        // Assert
        config.Should().BeNull();
    }

    #endregion

    #region UpdateRoyaltySplitAsync Tests

    [Fact]
    public async Task UpdateRoyaltySplitAsync_WithValidInput_UpdatesContributors()
    {
        // Arrange
        var registration = await _service.RegisterIpAssetAsync(
            "content-1", "Title", CreateTestContributors());

        var newContributors = new List<Contributor>
        {
            new() { Name = "New Author", WalletAddress = "0xnew1", ContributionPercentage = 100 }
        };

        // Act
        var updated = await _service.UpdateRoyaltySplitAsync(registration.IpAssetId!, newContributors);

        // Assert
        updated.Should().NotBeNull();
        updated.IpAssetId.Should().Be(registration.IpAssetId);
        updated.Contributors.Should().HaveCount(1);
        updated.TransactionHash.Should().StartWith("0x");
    }

    [Fact]
    public async Task UpdateRoyaltySplitAsync_WithUnknownIpAsset_ThrowsInvalidOperationException()
    {
        // Act
        var act = () => _service.UpdateRoyaltySplitAsync("0xunknown", CreateTestContributors());

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region IsHealthyAsync Tests

    [Fact]
    public async Task IsHealthyAsync_AlwaysReturnsTrue()
    {
        // Act
        var result = await _service.IsHealthyAsync();

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private static List<Contributor> CreateTestContributors()
    {
        return new List<Contributor>
        {
            new()
            {
                Id = "contrib-1",
                Name = "Alice Author",
                WalletAddress = "0xabc123",
                Role = ContributorRole.Author,
                ContributionPercentage = 60
            },
            new()
            {
                Id = "contrib-2",
                Name = "Bob Artist",
                WalletAddress = "0xdef456",
                Role = ContributorRole.Artist,
                ContributionPercentage = 40
            }
        };
    }

    #endregion
}
