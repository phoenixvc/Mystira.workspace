using FluentAssertions;
using Mystira.Shared.Locking;

namespace Mystira.Shared.Tests.Locking;

public class DistributedLockOptionsTests
{
    [Fact]
    public void DefaultOptions_HaveCorrectValues()
    {
        // Arrange & Act
        var options = new DistributedLockOptions();

        // Assert
        options.DefaultExpirySeconds.Should().Be(30);
        options.DefaultWaitSeconds.Should().Be(10);
        options.RetryIntervalMs.Should().Be(100);
        options.KeyPrefix.Should().Be("lock:");
        options.EnableDetailedLogging.Should().BeTrue();
    }

    [Fact]
    public void SectionName_IsCorrect()
    {
        // Assert
        DistributedLockOptions.SectionName.Should().Be("DistributedLock");
    }

    [Fact]
    public void Options_AreConfigurable()
    {
        // Arrange & Act
        var options = new DistributedLockOptions
        {
            DefaultExpirySeconds = 60,
            DefaultWaitSeconds = 30,
            RetryIntervalMs = 200,
            KeyPrefix = "mylock:",
            EnableDetailedLogging = false
        };

        // Assert
        options.DefaultExpirySeconds.Should().Be(60);
        options.DefaultWaitSeconds.Should().Be(30);
        options.RetryIntervalMs.Should().Be(200);
        options.KeyPrefix.Should().Be("mylock:");
        options.EnableDetailedLogging.Should().BeFalse();
    }
}

public class DistributedLockExceptionTests
{
    [Fact]
    public void Constructor_WithResource_SetsMessage()
    {
        // Arrange & Act
        var exception = new DistributedLockException("my-resource");

        // Assert
        exception.Resource.Should().Be("my-resource");
        exception.Message.Should().Contain("my-resource");
    }

    [Fact]
    public void Constructor_WithResourceAndMessage_SetsProperties()
    {
        // Arrange & Act
        var exception = new DistributedLockException("my-resource", "Custom message");

        // Assert
        exception.Resource.Should().Be("my-resource");
        exception.Message.Should().Be("Custom message");
    }

    [Fact]
    public void Constructor_WithInnerException_SetsProperties()
    {
        // Arrange
        var inner = new InvalidOperationException("Inner error");

        // Act
        var exception = new DistributedLockException("my-resource", "Custom message", inner);

        // Assert
        exception.Resource.Should().Be("my-resource");
        exception.Message.Should().Be("Custom message");
        exception.InnerException.Should().Be(inner);
    }
}
