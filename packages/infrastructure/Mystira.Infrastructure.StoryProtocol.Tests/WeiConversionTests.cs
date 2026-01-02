using FluentAssertions;
using Mystira.Infrastructure.StoryProtocol.Services.Grpc;
using Xunit;

namespace Mystira.Infrastructure.StoryProtocol.Tests;

/// <summary>
/// Unit tests for wei conversion utilities in <see cref="GrpcStoryProtocolService"/>.
/// </summary>
public class WeiConversionTests
{
    #region ConvertToWei Tests

    [Fact]
    public void ConvertToWei_WithOneToken_ReturnsCorrectWei()
    {
        // 1 token = 10^18 wei
        var result = GrpcStoryProtocolService.ConvertToWei(1m);

        result.Should().Be("1000000000000000000");
    }

    [Fact]
    public void ConvertToWei_WithZero_ReturnsZero()
    {
        var result = GrpcStoryProtocolService.ConvertToWei(0m);

        result.Should().Be("0");
    }

    [Fact]
    public void ConvertToWei_WithFractionalAmount_ReturnsCorrectWei()
    {
        // 0.5 tokens = 5 * 10^17 wei
        var result = GrpcStoryProtocolService.ConvertToWei(0.5m);

        result.Should().Be("500000000000000000");
    }

    [Fact]
    public void ConvertToWei_WithSmallFraction_ReturnsCorrectWei()
    {
        // 0.000000000000000001 tokens = 1 wei
        var result = GrpcStoryProtocolService.ConvertToWei(0.000000000000000001m);

        result.Should().Be("1");
    }

    [Fact]
    public void ConvertToWei_WithLargeAmount_HandlesWithoutOverflow()
    {
        // 100 tokens = 100 * 10^18 wei (exceeds long.MaxValue)
        var result = GrpcStoryProtocolService.ConvertToWei(100m);

        result.Should().Be("100000000000000000000");
    }

    [Fact]
    public void ConvertToWei_WithVeryLargeAmount_HandlesWithoutOverflow()
    {
        // 1000000 tokens = 10^24 wei
        var result = GrpcStoryProtocolService.ConvertToWei(1_000_000m);

        result.Should().Be("1000000000000000000000000");
    }

    [Fact]
    public void ConvertToWei_WithMixedWholeAndFractional_ReturnsCorrectWei()
    {
        // 1.5 tokens = 1.5 * 10^18 wei
        var result = GrpcStoryProtocolService.ConvertToWei(1.5m);

        result.Should().Be("1500000000000000000");
    }

    [Theory]
    [InlineData(0.001, "1000000000000000")]         // 0.001 tokens
    [InlineData(0.1, "100000000000000000")]          // 0.1 tokens
    [InlineData(10, "10000000000000000000")]         // 10 tokens
    [InlineData(999.999, "999999000000000000000")]   // 999.999 tokens
    public void ConvertToWei_WithVariousAmounts_ReturnsCorrectValues(decimal amount, string expectedWei)
    {
        var result = GrpcStoryProtocolService.ConvertToWei(amount);

        result.Should().Be(expectedWei);
    }

    #endregion

    #region ConvertFromWei Tests

    [Fact]
    public void ConvertFromWei_WithOneTokenWei_ReturnsOneToken()
    {
        var result = GrpcStoryProtocolService.ConvertFromWei("1000000000000000000");

        result.Should().Be(1m);
    }

    [Fact]
    public void ConvertFromWei_WithZero_ReturnsZero()
    {
        var result = GrpcStoryProtocolService.ConvertFromWei("0");

        result.Should().Be(0m);
    }

    [Fact]
    public void ConvertFromWei_WithNull_ReturnsZero()
    {
        var result = GrpcStoryProtocolService.ConvertFromWei(null!);

        result.Should().Be(0m);
    }

    [Fact]
    public void ConvertFromWei_WithEmptyString_ReturnsZero()
    {
        var result = GrpcStoryProtocolService.ConvertFromWei("");

        result.Should().Be(0m);
    }

    [Fact]
    public void ConvertFromWei_WithWhitespace_ReturnsZero()
    {
        var result = GrpcStoryProtocolService.ConvertFromWei("   ");

        result.Should().Be(0m);
    }

    [Fact]
    public void ConvertFromWei_WithInvalidString_ReturnsZero()
    {
        var result = GrpcStoryProtocolService.ConvertFromWei("not-a-number");

        result.Should().Be(0m);
    }

    [Fact]
    public void ConvertFromWei_WithFractionalTokenWei_ReturnsFractionalToken()
    {
        // 5 * 10^17 wei = 0.5 tokens
        var result = GrpcStoryProtocolService.ConvertFromWei("500000000000000000");

        result.Should().Be(0.5m);
    }

    [Fact]
    public void ConvertFromWei_WithLargeWei_HandlesWithoutOverflow()
    {
        // 100 * 10^18 wei = 100 tokens
        var result = GrpcStoryProtocolService.ConvertFromWei("100000000000000000000");

        result.Should().Be(100m);
    }

    [Fact]
    public void ConvertFromWei_WithVeryLargeWei_HandlesWithoutOverflow()
    {
        // 1000000 * 10^18 wei = 1000000 tokens
        var result = GrpcStoryProtocolService.ConvertFromWei("1000000000000000000000000");

        result.Should().Be(1_000_000m);
    }

    [Fact]
    public void ConvertFromWei_WithOneWei_ReturnsSmallestFraction()
    {
        // 1 wei = 10^-18 tokens
        var result = GrpcStoryProtocolService.ConvertFromWei("1");

        result.Should().Be(0.000000000000000001m);
    }

    [Theory]
    [InlineData("1000000000000000", 0.001)]          // 0.001 tokens
    [InlineData("100000000000000000", 0.1)]           // 0.1 tokens
    [InlineData("10000000000000000000", 10)]          // 10 tokens
    public void ConvertFromWei_WithVariousAmounts_ReturnsCorrectValues(string weiString, decimal expectedTokens)
    {
        var result = GrpcStoryProtocolService.ConvertFromWei(weiString);

        result.Should().Be(expectedTokens);
    }

    #endregion

    #region Round-Trip Tests

    [Theory]
    [InlineData(0.000000000000000001)]  // 1 wei
    [InlineData(0.5)]                    // 0.5 tokens
    [InlineData(1)]                      // 1 token
    [InlineData(1.5)]                    // 1.5 tokens
    [InlineData(100)]                    // 100 tokens
    [InlineData(1000000)]                // 1M tokens
    public void ConvertToWei_AndConvertFromWei_RoundTripsCorrectly(decimal originalAmount)
    {
        // Act
        var weiString = GrpcStoryProtocolService.ConvertToWei(originalAmount);
        var roundTrippedAmount = GrpcStoryProtocolService.ConvertFromWei(weiString);

        // Assert
        roundTrippedAmount.Should().Be(originalAmount);
    }

    #endregion
}
