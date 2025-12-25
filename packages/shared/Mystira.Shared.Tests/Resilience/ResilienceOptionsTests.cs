using FluentAssertions;
using Mystira.Shared.Resilience;
using Xunit;

namespace Mystira.Shared.Tests.Resilience;

public class ResilienceOptionsTests
{
    [Fact]
    public void DefaultOptions_HasCorrectDefaults()
    {
        // Arrange & Act
        var options = new ResilienceOptions();

        // Assert
        options.MaxRetries.Should().Be(3);
        options.BaseDelaySeconds.Should().Be(2);
        options.TimeoutSeconds.Should().Be(30);
        options.LongRunningTimeoutSeconds.Should().Be(300);
        options.CircuitBreakerThreshold.Should().Be(5);
        options.CircuitBreakerDurationSeconds.Should().Be(30);
        options.EnableDetailedLogging.Should().BeTrue();
    }

    [Fact]
    public void SectionName_IsCorrect()
    {
        // Assert
        ResilienceOptions.SectionName.Should().Be("Resilience");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void MaxRetries_CanBeConfigured(int maxRetries)
    {
        // Arrange & Act
        var options = new ResilienceOptions { MaxRetries = maxRetries };

        // Assert
        options.MaxRetries.Should().Be(maxRetries);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(120)]
    public void TimeoutSeconds_CanBeConfigured(int timeout)
    {
        // Arrange & Act
        var options = new ResilienceOptions { TimeoutSeconds = timeout };

        // Assert
        options.TimeoutSeconds.Should().Be(timeout);
    }
}
