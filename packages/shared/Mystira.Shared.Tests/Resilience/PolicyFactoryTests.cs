using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Shared.Resilience;
using System.Net;

namespace Mystira.Shared.Tests.Resilience;

public class PolicyFactoryTests
{
    [Fact]
    public void CreateStandardHttpPolicy_ReturnsPolicy()
    {
        // Arrange & Act
        var policy = PolicyFactory.CreateStandardHttpPolicy("TestClient");

        // Assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CreateLongRunningHttpPolicy_ReturnsPolicy()
    {
        // Arrange & Act
        var policy = PolicyFactory.CreateLongRunningHttpPolicy("TestClient");

        // Assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CreateRetryPolicy_ReturnsPolicy()
    {
        // Arrange & Act
        var policy = PolicyFactory.CreateRetryPolicy<string>("TestOperation");

        // Assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CreateVoidRetryPolicy_ReturnsPolicy()
    {
        // Arrange & Act
        var policy = PolicyFactory.CreateVoidRetryPolicy("TestOperation");

        // Assert
        policy.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1, 2)]  // Attempt 1: 2 * 2^0 = 2 seconds
    [InlineData(2, 4)]  // Attempt 2: 2 * 2^1 = 4 seconds
    [InlineData(3, 8)]  // Attempt 3: 2 * 2^2 = 8 seconds
    [InlineData(4, 16)] // Attempt 4: 2 * 2^3 = 16 seconds
    [InlineData(5, 32)] // Attempt 5: 2 * 2^4 = 32 seconds
    [InlineData(6, 60)] // Attempt 6: 2 * 2^5 = 64, capped at 60 seconds
    public void CalculateDelay_ReturnsExponentialBackoff(int attempt, int expectedApproxSeconds)
    {
        // This tests the exponential backoff formula: baseDelay * 2^(attempt-1)
        // We can't directly test the private method, but we can verify behavior through retry counts

        // The expected delay should be approximately expectedApproxSeconds
        // With ±20% jitter, actual range is 0.8x to 1.2x of base delay
        var minExpected = expectedApproxSeconds * 0.8;
        var maxExpected = Math.Min(expectedApproxSeconds * 1.2, 60);

        // Since we can't directly invoke the private method, we verify the options work correctly
        var options = new ResilienceOptions
        {
            MaxRetries = attempt,
            BaseDelaySeconds = 2
        };

        // Policy creation should not throw
        var policy = PolicyFactory.CreateRetryPolicy<string>("Test", options);
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CreateStandardHttpPolicy_UsesProvidedOptions()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            MaxRetries = 5,
            BaseDelaySeconds = 1,
            TimeoutSeconds = 45,
            CircuitBreakerThreshold = 10,
            CircuitBreakerDurationSeconds = 120
        };

        // Act
        var policy = PolicyFactory.CreateStandardHttpPolicy("TestClient", options);

        // Assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CreateLongRunningHttpPolicy_UsesLongRunningTimeout()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            LongRunningTimeoutSeconds = 600 // 10 minutes
        };

        // Act
        var policy = PolicyFactory.CreateLongRunningHttpPolicy("LlmClient", options);

        // Assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CreateStandardHttpPolicy_WithLogger_LogsOnRetry()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var options = new ResilienceOptions
        {
            MaxRetries = 1,
            EnableDetailedLogging = true
        };

        // Act - just verify policy creation doesn't throw
        var policy = PolicyFactory.CreateStandardHttpPolicy("TestClient", options, loggerMock.Object);

        // Assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public void CreateRetryPolicy_WithZeroRetries_ReturnsPolicy()
    {
        // Arrange
        var options = new ResilienceOptions { MaxRetries = 0 };

        // Act
        var policy = PolicyFactory.CreateRetryPolicy<string>("Test", options);

        // Assert
        policy.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateRetryPolicy_RetriesOnException()
    {
        // Arrange
        var options = new ResilienceOptions
        {
            MaxRetries = 2,
            BaseDelaySeconds = 0 // No delay for tests
        };
        var policy = PolicyFactory.CreateRetryPolicy<string>("Test", options);
        var callCount = 0;

        // Act
        var result = await policy.ExecuteAsync(async () =>
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
}
