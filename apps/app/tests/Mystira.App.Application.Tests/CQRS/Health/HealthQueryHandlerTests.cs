using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.Health.Queries;
using Mystira.Contracts.App.Ports.Health;

namespace Mystira.App.Application.Tests.CQRS.Health;

public class HealthQueryHandlerTests
{
    private readonly Mock<IHealthCheckPort> _healthCheckPort;

    public HealthQueryHandlerTests()
    {
        _healthCheckPort = new Mock<IHealthCheckPort>();
    }

    #region GetHealthCheckQueryHandler

    [Fact]
    public async Task GetHealthCheck_WithHealthyReport_ReturnsHealthyResult()
    {
        var report = new HealthReport
        {
            Status = HealthStatus.Healthy,
            TotalDuration = TimeSpan.FromMilliseconds(100),
            Entries = new Dictionary<string, HealthCheckEntry>
            {
                ["database"] = new()
                {
                    Status = HealthStatus.Healthy,
                    Description = "DB is healthy",
                    Duration = TimeSpan.FromMilliseconds(50)
                }
            }
        };
        _healthCheckPort.Setup(h => h.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var result = await GetHealthCheckQueryHandler.Handle(
            new GetHealthCheckQuery(),
            _healthCheckPort.Object,
            Mock.Of<ILogger<GetHealthCheckQuery>>(),
            CancellationToken.None);

        result.Status.Should().Be("Healthy");
        result.Results.Should().ContainKey("database");
    }

    [Fact]
    public async Task GetHealthCheck_WhenExceptionThrown_ReturnsUnhealthyResult()
    {
        _healthCheckPort.Setup(h => h.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB connection failed"));

        var result = await GetHealthCheckQueryHandler.Handle(
            new GetHealthCheckQuery(),
            _healthCheckPort.Object,
            Mock.Of<ILogger<GetHealthCheckQuery>>(),
            CancellationToken.None);

        result.Status.Should().Be("Unhealthy");
        result.Results.Should().ContainKey("error");
    }

    #endregion

    #region GetLivenessQueryHandler

    [Fact]
    public async Task GetLiveness_ReturnsAliveStatus()
    {
        var result = await GetLivenessQueryHandler.Handle(
            new GetLivenessQuery(),
            Mock.Of<ILogger<GetLivenessQuery>>(),
            CancellationToken.None);

        result.Status.Should().Be("alive");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region GetReadinessQueryHandler

    [Fact]
    public async Task GetReadiness_ReturnsReadyStatus()
    {
        var result = await GetReadinessQueryHandler.Handle(
            new GetReadinessQuery(),
            Mock.Of<ILogger<GetReadinessQuery>>(),
            CancellationToken.None);

        result.Status.Should().Be("ready");
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion
}
