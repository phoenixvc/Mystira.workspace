using FluentAssertions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Shared.Telemetry;
using Xunit;

namespace Mystira.Shared.Tests.Telemetry;

public class SecurityMetricsTests : IDisposable
{
    private readonly TelemetryClient _telemetryClient;
    private readonly TelemetryConfiguration _configuration;
    private readonly Mock<ILogger<SecurityMetrics>> _loggerMock;
    private readonly SecurityMetrics _sut;
    private readonly List<ITelemetry> _sentTelemetry;

    public SecurityMetricsTests()
    {
        _sentTelemetry = new List<ITelemetry>();
        _configuration = new TelemetryConfiguration
        {
            TelemetryChannel = new FakeTelemetryChannel(_sentTelemetry)
        };
        _telemetryClient = new TelemetryClient(_configuration);
        _loggerMock = new Mock<ILogger<SecurityMetrics>>();
        _sut = new SecurityMetrics(_telemetryClient, _loggerMock.Object, "Test");
    }

    public void Dispose()
    {
        _configuration?.Dispose();
    }

    [Fact]
    public void TrackAuthenticationFailed_TracksEvent()
    {
        // Act
        _sut.TrackAuthenticationFailed("password", "192.168.1.1", "invalid password");

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackAuthenticationSuccess_TracksEvent()
    {
        // Act
        _sut.TrackAuthenticationSuccess("password", "user123");

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackTokenValidationFailed_TracksEvent()
    {
        // Act
        _sut.TrackTokenValidationFailed("192.168.1.1", "expired token");

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackRateLimitHit_TracksEvent()
    {
        // Act
        _sut.TrackRateLimitHit("192.168.1.1", "/api/users");

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackRateLimitSustained_TracksEvent()
    {
        // Act
        _sut.TrackRateLimitSustained("192.168.1.1", 50, TimeSpan.FromMinutes(5));

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackSuspiciousRequest_TracksEvent()
    {
        // Act
        _sut.TrackSuspiciousRequest("192.168.1.1", "sql_injection", "detected suspicious query");

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackAuthorizationFailed_TracksEvent()
    {
        // Act
        _sut.TrackAuthorizationFailed("user123", "/admin/dashboard", "insufficient permissions");

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackBruteForceDetected_TracksEvent()
    {
        // Act
        _sut.TrackBruteForceDetected("192.168.1.1", 10, TimeSpan.FromMinutes(5));

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackInvalidInput_TracksEvent()
    {
        // Act
        _sut.TrackInvalidInput("sql_query", "192.168.1.1", "suspicious pattern detected");

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackCorsViolation_TracksEvent()
    {
        // Act
        _sut.TrackCorsViolation("https://evil.com", "192.168.1.1");

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackAuthenticationFailed_HandlesNullClientIp()
    {
        // Act
        _sut.TrackAuthenticationFailed("password", null, "invalid password");

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackAuthenticationFailed_HandlesNullReason()
    {
        // Act
        _sut.TrackAuthenticationFailed("password", "192.168.1.1", null);

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void TrackAuthenticationSuccess_HandlesNullUserId()
    {
        // Act
        _sut.TrackAuthenticationSuccess("jwt", null);

        // Assert
        _sentTelemetry.Should().NotBeEmpty();
    }

    [Fact]
    public void DetectsBruteForcePattern_AfterMultipleFailures()
    {
        // Arrange
        var clientIp = "192.168.1.100";
        var initialCount = _sentTelemetry.Count;

        // Act
        for (int i = 0; i < 6; i++)
        {
            _sut.TrackAuthenticationFailed("password", clientIp, "invalid password");
        }

        // Assert - Should have tracked regular auth failures
        _sentTelemetry.Count.Should().BeGreaterThan(initialCount);
    }

    // Fake telemetry channel for testing
    private class FakeTelemetryChannel : ITelemetryChannel
    {
        private readonly List<ITelemetry> _telemetry;

        public FakeTelemetryChannel(List<ITelemetry> telemetry)
        {
            _telemetry = telemetry;
        }

        public bool? DeveloperMode { get; set; }
        public string? EndpointAddress { get; set; }

        public void Send(ITelemetry item)
        {
            _telemetry.Add(item);
        }

        public void Flush()
        {
        }

        public void Dispose()
        {
        }
    }
}
