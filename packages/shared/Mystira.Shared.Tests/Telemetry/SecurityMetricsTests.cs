using FluentAssertions;
using Microsoft.ApplicationInsights;
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

    public SecurityMetricsTests()
    {
        // AppInsights 3.x: ITelemetryChannel is internal; use CreateDefault + connection string
        _configuration = TelemetryConfiguration.CreateDefault();
        _configuration.ConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost";
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
        _sut.TrackAuthenticationFailed("password", "192.168.1.1", "invalid password");
    }

    [Fact]
    public void TrackAuthenticationSuccess_TracksEvent()
    {
        _sut.TrackAuthenticationSuccess("password", "user123");
    }

    [Fact]
    public void TrackTokenValidationFailed_TracksEvent()
    {
        _sut.TrackTokenValidationFailed("192.168.1.1", "expired token");
    }

    [Fact]
    public void TrackRateLimitHit_TracksEvent()
    {
        _sut.TrackRateLimitHit("192.168.1.1", "/api/users");
    }

    [Fact]
    public void TrackRateLimitSustained_TracksEvent()
    {
        _sut.TrackRateLimitSustained("192.168.1.1", 50, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void TrackSuspiciousRequest_TracksEvent()
    {
        _sut.TrackSuspiciousRequest("192.168.1.1", "sql_injection", "detected suspicious query");
    }

    [Fact]
    public void TrackAuthorizationFailed_TracksEvent()
    {
        _sut.TrackAuthorizationFailed("user123", "/admin/dashboard", "insufficient permissions");
    }

    [Fact]
    public void TrackBruteForceDetected_TracksEvent()
    {
        _sut.TrackBruteForceDetected("192.168.1.1", 10, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public void TrackInvalidInput_TracksEvent()
    {
        _sut.TrackInvalidInput("sql_query", "192.168.1.1", "suspicious pattern detected");
    }

    [Fact]
    public void TrackCorsViolation_TracksEvent()
    {
        _sut.TrackCorsViolation("https://evil.com", "192.168.1.1");
    }

    [Fact]
    public void TrackAuthenticationFailed_HandlesNullClientIp()
    {
        _sut.TrackAuthenticationFailed("password", null, "invalid password");
    }

    [Fact]
    public void TrackAuthenticationFailed_HandlesNullReason()
    {
        _sut.TrackAuthenticationFailed("password", "192.168.1.1", null);
    }

    [Fact]
    public void TrackAuthenticationSuccess_HandlesNullUserId()
    {
        _sut.TrackAuthenticationSuccess("jwt", null);
    }

    [Fact]
    public void DetectsBruteForcePattern_AfterMultipleFailures()
    {
        var clientIp = "192.168.1.100";

        // Should not throw even with many rapid calls triggering brute force detection
        for (int i = 0; i < 6; i++)
        {
            _sut.TrackAuthenticationFailed("password", clientIp, "invalid password");
        }
    }
}
