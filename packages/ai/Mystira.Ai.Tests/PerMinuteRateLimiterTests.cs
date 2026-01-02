using Mystira.Ai.RateLimiting;

namespace Mystira.Ai.Tests;

public class PerMinuteRateLimiterTests
{
    [Fact]
    public void Constructor_WithZeroRequests_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new PerMinuteRateLimiter(0));
    }

    [Fact]
    public void Constructor_WithNegativeRequests_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new PerMinuteRateLimiter(-1));
    }

    [Fact]
    public void Constructor_WithPositiveRequests_DoesNotThrow()
    {
        // Arrange & Act
        var limiter = new PerMinuteRateLimiter(60);

        // Assert
        Assert.NotNull(limiter);
    }

    [Fact]
    public async Task WaitAsync_FirstCall_ReturnsImmediately()
    {
        // Arrange
        var limiter = new PerMinuteRateLimiter(60);
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act & Assert - should complete almost immediately
        await limiter.WaitAsync(cts.Token);
    }

    [Fact]
    public async Task WaitAsync_RapidCalls_EnforcesRateLimit()
    {
        // Arrange - 60 requests per minute = 1 per second
        var limiter = new PerMinuteRateLimiter(60);
        var startTime = DateTimeOffset.UtcNow;

        // Act - make 3 rapid calls
        await limiter.WaitAsync();
        await limiter.WaitAsync();
        await limiter.WaitAsync();

        var elapsed = DateTimeOffset.UtcNow - startTime;

        // Assert - should take at least 2 seconds for 3 calls at 1/second rate
        // Allow some tolerance for timing variations
        Assert.True(elapsed.TotalMilliseconds >= 1900,
            $"Expected at least 1900ms delay, got {elapsed.TotalMilliseconds}ms");
    }

    [Fact]
    public async Task WaitAsync_WithCancellation_ThrowsOperationCanceled()
    {
        // Arrange - very slow rate to force delay
        var limiter = new PerMinuteRateLimiter(1); // 1 per minute
        await limiter.WaitAsync(); // Use up first permit

        using var cts = new CancellationTokenSource();

        // Act - start waiting then cancel
        var waitTask = limiter.WaitAsync(cts.Token);
        cts.Cancel();

        // Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() => waitTask);
    }
}
