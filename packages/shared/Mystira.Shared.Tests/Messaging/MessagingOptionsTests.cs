using FluentAssertions;
using Mystira.Shared.Messaging;

namespace Mystira.Shared.Tests.Messaging;

public class MessagingOptionsTests
{
    [Fact]
    public void DefaultOptions_HasCorrectDefaults()
    {
        // Arrange & Act
        var options = new MessagingOptions();

        // Assert
        options.Enabled.Should().BeTrue();
        options.MaxRetries.Should().Be(3);
        options.InitialRetryDelaySeconds.Should().Be(5);
        options.AutoProvision.Should().BeTrue();
        options.DurabilityMode.Should().Be(DurabilityMode.Balanced);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void MaxRetries_CanBeConfigured(int maxRetries)
    {
        // Arrange
        var options = new MessagingOptions { MaxRetries = maxRetries };

        // Assert
        options.MaxRetries.Should().Be(maxRetries);
    }

    [Theory]
    [InlineData(DurabilityMode.Solo)]
    [InlineData(DurabilityMode.Balanced)]
    [InlineData(DurabilityMode.MediatorOnly)]
    [InlineData(DurabilityMode.Serverless)]
    public void DurabilityMode_CanBeConfigured(DurabilityMode mode)
    {
        // Arrange
        var options = new MessagingOptions { DurabilityMode = mode };

        // Assert
        options.DurabilityMode.Should().Be(mode);
    }
}
