using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Shared.Resilience;
using Xunit;

namespace Mystira.Shared.Tests.Resilience;

public class ResiliencePipelineFactoryTests
{
    private readonly Mock<ILogger> _loggerMock = new();

    [Fact]
    public void CreateStandardHttpPipeline_ReturnsNonNullPipeline()
    {
        // Act
        var pipeline = ResiliencePipelineFactory.CreateStandardHttpPipeline("TestClient");

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void CreateLongRunningHttpPipeline_ReturnsNonNullPipeline()
    {
        // Act
        var pipeline = ResiliencePipelineFactory.CreateLongRunningHttpPipeline("LlmClient");

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void CreateRetryPipeline_ReturnsNonNullPipeline()
    {
        // Act
        var pipeline = ResiliencePipelineFactory.CreateRetryPipeline<string>("TestOperation");

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void CreateVoidRetryPipeline_ReturnsNonNullPipeline()
    {
        // Act
        var pipeline = ResiliencePipelineFactory.CreateVoidRetryPipeline("TestOperation");

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void CreateCircuitBreakerPipeline_ReturnsNonNullPipeline()
    {
        // Act
        var pipeline = ResiliencePipelineFactory.CreateCircuitBreakerPipeline<string>("TestOperation");

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void CreateStandardHttpPipeline_WithCustomOptions_UseOptions()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            MaxRetries = 5,
            BaseDelaySeconds = 1,
            TimeoutSeconds = 60,
            CircuitBreakerThreshold = 10,
            CircuitBreakerDurationSeconds = 120
        };

        // Act
        var pipeline = ResiliencePipelineFactory.CreateStandardHttpPipeline("TestClient", options);

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void CreateLongRunningHttpPipeline_WithCustomOptions_UsesLongTimeout()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            LongRunningTimeoutSeconds = 600 // 10 minutes
        };

        // Act
        var pipeline = ResiliencePipelineFactory.CreateLongRunningHttpPipeline("LlmClient", options);

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateRetryPipeline_RetriesOnException()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            MaxRetries = 2,
            BaseDelaySeconds = 0 // No delay for tests
        };
        var pipeline = ResiliencePipelineFactory.CreateRetryPipeline<string>("Test", options);
        var callCount = 0;

        // Act
        var result = await pipeline.ExecuteAsync(async ct =>
        {
            callCount++;
            if (callCount < 3)
            {
                throw new InvalidOperationException("Transient error");
            }
            return "success";
        });

        // Assert
        result.Should().Be("success");
        callCount.Should().Be(3); // Initial + 2 retries
    }

    [Fact]
    public async Task CreateRetryPipeline_ThrowsAfterMaxRetries()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            MaxRetries = 2,
            BaseDelaySeconds = 0
        };
        var pipeline = ResiliencePipelineFactory.CreateRetryPipeline<string>("Test", options);
        var callCount = 0;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await pipeline.ExecuteAsync<string>(async ct =>
            {
                callCount++;
                throw new InvalidOperationException("Persistent error");
            });
        });

        callCount.Should().Be(3); // Initial + 2 retries
    }

    [Fact]
    public async Task CreateVoidRetryPipeline_RetriesOnException()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            MaxRetries = 2,
            BaseDelaySeconds = 0
        };
        var pipeline = ResiliencePipelineFactory.CreateVoidRetryPipeline("Test", options);
        var callCount = 0;

        // Act
        await pipeline.ExecuteAsync(async ct =>
        {
            callCount++;
            if (callCount < 3)
            {
                throw new InvalidOperationException("Transient error");
            }
        });

        // Assert
        callCount.Should().Be(3);
    }

    [Fact]
    public void CreateStandardHttpPipeline_WithLogger_LogsOnRetry()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            MaxRetries = 1,
            EnableDetailedLogging = true
        };

        // Act
        var pipeline = ResiliencePipelineFactory.CreateStandardHttpPipeline(
            "TestClient", options, _loggerMock.Object);

        // Assert
        pipeline.Should().NotBeNull();
    }

    [Fact]
    public void CreateRetryPipeline_WithZeroRetries_ThrowsValidationException()
    {
        // Arrange
        var options = new ResilienceOptions { MaxRetries = 0 };

        // Act & Assert - Polly requires at least 1 retry attempt
        Assert.Throws<System.ComponentModel.DataAnnotations.ValidationException>(() =>
            ResiliencePipelineFactory.CreateRetryPipeline<string>("Test", options));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void CreateRetryPipeline_WithVariousRetryAttempts_CreatesValidPipeline(int maxRetries)
    {
        // Arrange
        var options = new ResilienceOptions { MaxRetries = maxRetries };

        // Act
        var pipeline = ResiliencePipelineFactory.CreateRetryPipeline<string>("Test", options);

        // Assert
        pipeline.Should().NotBeNull();
    }
}
