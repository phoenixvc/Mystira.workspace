using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Royalties.Commands;
using Mystira.App.Application.CQRS.Royalties.Queries;
using Mystira.Application.Ports;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Royalties;

public class RoyaltyHandlerTests
{
    private readonly Mock<IStoryProtocolService> _storyProtocolService;

    public RoyaltyHandlerTests()
    {
        _storyProtocolService = new Mock<IStoryProtocolService>();
    }

    #region ClaimRoyaltiesCommandHandler

    [Fact]
    public async Task ClaimRoyalties_WithValidInput_ReturnsTransactionHash()
    {
        _storyProtocolService.Setup(s => s.ClaimRoyaltiesAsync("ip-1", "0xWallet123", It.IsAny<CancellationToken>()))
            .ReturnsAsync("0xTxHash456");

        var result = await ClaimRoyaltiesCommandHandler.Handle(
            new ClaimRoyaltiesCommand("ip-1", "0xWallet123"),
            _storyProtocolService.Object,
            Mock.Of<ILogger<ClaimRoyaltiesCommand>>(),
            CancellationToken.None);

        result.Should().Be("0xTxHash456");
    }

    [Fact]
    public async Task ClaimRoyalties_WithEmptyIpAssetId_ThrowsValidationException()
    {
        var act = () => ClaimRoyaltiesCommandHandler.Handle(
            new ClaimRoyaltiesCommand("", "0xWallet123"),
            _storyProtocolService.Object,
            Mock.Of<ILogger<ClaimRoyaltiesCommand>>(),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ClaimRoyalties_WithEmptyWallet_ThrowsValidationException()
    {
        var act = () => ClaimRoyaltiesCommandHandler.Handle(
            new ClaimRoyaltiesCommand("ip-1", ""),
            _storyProtocolService.Object,
            Mock.Of<ILogger<ClaimRoyaltiesCommand>>(),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region PayRoyaltyCommandHandler

    [Fact]
    public async Task PayRoyalty_WithValidInput_ReturnsPaymentResult()
    {
        var paymentResult = new RoyaltyPaymentResult
        {
            PaymentId = "pay-1",
            IpAssetId = "ip-1",
            TransactionHash = "0xTx789",
            Amount = 100m,
            Success = true
        };
        _storyProtocolService.Setup(s => s.PayRoyaltyAsync("ip-1", 100m, "ref-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentResult);

        var result = await PayRoyaltyCommandHandler.Handle(
            new PayRoyaltyCommand("ip-1", 100m, "ref-1"),
            _storyProtocolService.Object,
            Mock.Of<ILogger<PayRoyaltyCommand>>(),
            CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Amount.Should().Be(100m);
        result.TransactionHash.Should().Be("0xTx789");
    }

    [Fact]
    public async Task PayRoyalty_WithEmptyIpAssetId_ThrowsValidationException()
    {
        var act = () => PayRoyaltyCommandHandler.Handle(
            new PayRoyaltyCommand("", 100m),
            _storyProtocolService.Object,
            Mock.Of<ILogger<PayRoyaltyCommand>>(),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PayRoyalty_WithZeroAmount_ThrowsValidationException()
    {
        var act = () => PayRoyaltyCommandHandler.Handle(
            new PayRoyaltyCommand("ip-1", 0m),
            _storyProtocolService.Object,
            Mock.Of<ILogger<PayRoyaltyCommand>>(),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task PayRoyalty_WithNegativeAmount_ThrowsValidationException()
    {
        var act = () => PayRoyaltyCommandHandler.Handle(
            new PayRoyaltyCommand("ip-1", -50m),
            _storyProtocolService.Object,
            Mock.Of<ILogger<PayRoyaltyCommand>>(),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region GetClaimableRoyaltiesQueryHandler

    [Fact]
    public async Task GetClaimableRoyalties_WithValidIpAssetId_ReturnsBalance()
    {
        var balance = new RoyaltyBalance
        {
            IpAssetId = "ip-1",
            TotalClaimable = 500m,
            TotalClaimed = 200m,
            TotalReceived = 700m
        };
        _storyProtocolService.Setup(s => s.GetClaimableRoyaltiesAsync("ip-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(balance);

        var result = await GetClaimableRoyaltiesQueryHandler.Handle(
            new GetClaimableRoyaltiesQuery("ip-1"),
            _storyProtocolService.Object,
            Mock.Of<ILogger<GetClaimableRoyaltiesQuery>>(),
            CancellationToken.None);

        result.TotalClaimable.Should().Be(500m);
        result.TotalReceived.Should().Be(700m);
    }

    [Fact]
    public async Task GetClaimableRoyalties_WithEmptyIpAssetId_ThrowsValidationException()
    {
        var act = () => GetClaimableRoyaltiesQueryHandler.Handle(
            new GetClaimableRoyaltiesQuery(""),
            _storyProtocolService.Object,
            Mock.Of<ILogger<GetClaimableRoyaltiesQuery>>(),
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion
}
