using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Mystira.Shared.Caching;
using System.Text;
using System.Text.Json;

namespace Mystira.Shared.Tests.Caching;

public class DistributedCacheServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<DistributedCacheService>> _loggerMock;
    private readonly CacheOptions _options;
    private readonly DistributedCacheService _sut;

    public DistributedCacheServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<DistributedCacheService>>();
        _options = new CacheOptions { Enabled = true };

        _sut = new DistributedCacheService(
            _cacheMock.Object,
            Options.Create(_options),
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_ReturnsDefault_WhenCacheDisabled()
    {
        // Arrange
        var options = new CacheOptions { Enabled = false };
        var sut = new DistributedCacheService(
            _cacheMock.Object,
            Options.Create(options),
            _loggerMock.Object);

        // Act
        var result = await sut.GetAsync<string>("key");

        // Assert
        result.Should().BeNull();
        _cacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_ReturnsDefault_WhenKeyNotFound()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _sut.GetAsync<string>("missing");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        var expected = new TestDto { Id = 1, Name = "Test" };
        var json = JsonSerializer.Serialize(expected, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(json));

        // Act
        var result = await _sut.GetAsync<TestDto>("key");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task TryGetAsync_ReturnsFalse_WhenCacheDisabled()
    {
        // Arrange
        var options = new CacheOptions { Enabled = false };
        var sut = new DistributedCacheService(
            _cacheMock.Object,
            Options.Create(options),
            _loggerMock.Object);

        // Act
        var (found, value) = await sut.TryGetAsync<int>("key");

        // Assert
        found.Should().BeFalse();
        value.Should().Be(default);
    }

    [Fact]
    public async Task TryGetAsync_ReturnsFalse_WhenKeyNotFound()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var (found, value) = await _sut.TryGetAsync<int>("missing");

        // Assert
        found.Should().BeFalse();
        value.Should().Be(default);
    }

    [Fact]
    public async Task TryGetAsync_ReturnsTrue_WithValueTypeZero()
    {
        // Arrange - This tests the critical value type bug fix
        var json = JsonSerializer.Serialize(0, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _cacheMock.Setup(c => c.GetAsync("counter", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(json));

        // Act
        var (found, value) = await _sut.TryGetAsync<int>("counter");

        // Assert - Should find the value even though it's 0
        found.Should().BeTrue();
        value.Should().Be(0);
    }

    [Fact]
    public async Task TryGetAsync_ReturnsTrue_WithValueTypeFalse()
    {
        // Arrange - Boolean false should be distinguishable from "not found"
        var json = JsonSerializer.Serialize(false, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _cacheMock.Setup(c => c.GetAsync("flag", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(json));

        // Act
        var (found, value) = await _sut.TryGetAsync<bool>("flag");

        // Assert
        found.Should().BeTrue();
        value.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrCreateAsync_UsesCache_WhenValueExists()
    {
        // Arrange
        var json = JsonSerializer.Serialize(42, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        _cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes(json));

        var factoryCalled = false;

        // Act
        var result = await _sut.GetOrCreateAsync("key", async ct =>
        {
            factoryCalled = true;
            return 100;
        });

        // Assert
        result.Should().Be(42);
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrCreateAsync_CallsFactory_WhenCacheMiss()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _sut.GetOrCreateAsync("key", async ct => 100);

        // Assert
        result.Should().Be(100);
        _cacheMock.Verify(c => c.SetAsync(
            "key",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateAsync_CachesZeroValue()
    {
        // Arrange - Zero should be cached to avoid repeated factory calls
        _cacheMock.Setup(c => c.GetAsync("key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _sut.GetOrCreateAsync("key", async ct => 0);

        // Assert
        result.Should().Be(0);
        _cacheMock.Verify(c => c.SetAsync(
            "key",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_UsesSlidingExpiration_WhenConfigured()
    {
        // Arrange
        var options = new CacheOptions { Enabled = true, UseSlidingExpiration = true };
        var sut = new DistributedCacheService(
            _cacheMock.Object,
            Options.Create(options),
            _loggerMock.Object);

        DistributedCacheEntryOptions? capturedOptions = null;
        _cacheMock.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (key, value, opts, ct) => capturedOptions = opts);

        // Act
        await sut.SetAsync("key", "value", TimeSpan.FromMinutes(10));

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.SlidingExpiration.Should().Be(TimeSpan.FromMinutes(10));
        capturedOptions.AbsoluteExpirationRelativeToNow.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_UsesAbsoluteExpiration_ByDefault()
    {
        // Arrange
        var options = new CacheOptions { Enabled = true, UseSlidingExpiration = false };
        var sut = new DistributedCacheService(
            _cacheMock.Object,
            Options.Create(options),
            _loggerMock.Object);

        DistributedCacheEntryOptions? capturedOptions = null;
        _cacheMock.Setup(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()))
            .Callback<string, byte[], DistributedCacheEntryOptions, CancellationToken>(
                (key, value, opts, ct) => capturedOptions = opts);

        // Act
        await sut.SetAsync("key", "value", TimeSpan.FromMinutes(10));

        // Assert
        capturedOptions.Should().NotBeNull();
        capturedOptions!.AbsoluteExpirationRelativeToNow.Should().Be(TimeSpan.FromMinutes(10));
        capturedOptions.SlidingExpiration.Should().BeNull();
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
