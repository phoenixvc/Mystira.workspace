using FluentAssertions;
using Mystira.Shared.Caching;

namespace Mystira.Shared.Tests.Caching;

public class CacheOptionsTests
{
    [Fact]
    public void DefaultOptions_HasCorrectDefaults()
    {
        // Arrange & Act
        var options = new CacheOptions();

        // Assert
        options.Enabled.Should().BeTrue();
        options.DefaultExpirationMinutes.Should().Be(60);
        options.UseSlidingExpiration.Should().BeFalse();
        options.InstanceName.Should().Be("mystira:");
    }

    [Fact]
    public void SectionName_IsCorrect()
    {
        // Assert
        CacheOptions.SectionName.Should().Be("Cache");
    }

    [Fact]
    public void RedisConnectionString_CanBeConfigured()
    {
        // Arrange
        var options = new CacheOptions
        {
            RedisConnectionString = "localhost:6379,password=test"
        };

        // Assert
        options.RedisConnectionString.Should().Be("localhost:6379,password=test");
    }
}
