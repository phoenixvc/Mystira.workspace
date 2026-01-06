using System.Net;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;

namespace Mystira.Admin.Api.Tests;

/// <summary>
/// Integration tests for health check endpoints.
/// </summary>
[Collection("Api")]
public class HealthCheckTests : ApiTestFixture
{
    public HealthCheckTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task HealthLive_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthReady_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthLive_DoesNotRequireAuthentication()
    {
        // Act
        var response = await AnonymousClient.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }
}
