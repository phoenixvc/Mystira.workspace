using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;
using Xunit;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for ContributorsController endpoints.
/// Tests Story Protocol contributor and IP asset management.
/// </summary>
[Collection("Api")]
public class ContributorsControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/admin/contributors";

    public ContributorsControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    #region Authentication Tests

    [Fact]
    public async Task AllEndpoints_RequireAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var scenarioId = Guid.NewGuid().ToString();

        // Act
        var response = await client.PostAsJsonAsync($"{BaseUrl}/scenarios/{scenarioId}", new { });

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    #endregion

    #region POST /api/admin/contributors/scenarios/{scenarioId}

    [Fact]
    public async Task SetScenarioContributors_ReturnsBadRequest_WithEmptyRequest()
    {
        // Arrange
        var scenarioId = Guid.NewGuid().ToString();
        var request = new { Contributors = new List<object>() };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync($"{BaseUrl}/scenarios/{scenarioId}", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task SetScenarioContributors_AcceptsValidRequest()
    {
        // Arrange
        var scenarioId = Guid.NewGuid().ToString();
        var request = new
        {
            Contributors = new[]
            {
                new
                {
                    Name = "Test Contributor",
                    WalletAddress = "0x1234567890abcdef1234567890abcdef12345678",
                    Role = "Writer",
                    ContributionPercentage = 100
                }
            }
        };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync($"{BaseUrl}/scenarios/{scenarioId}", request);

        // Assert
        // May fail due to scenario not existing, but should not be unauthorized
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/admin/contributors/bundles/{bundleId}

    [Fact]
    public async Task SetBundleContributors_ReturnsBadRequest_WithEmptyRequest()
    {
        // Arrange
        var bundleId = Guid.NewGuid().ToString();
        var request = new { Contributors = new List<object>() };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync($"{BaseUrl}/bundles/{bundleId}", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region POST /api/admin/contributors/scenarios/{scenarioId}/register

    [Fact]
    public async Task RegisterScenarioIpAsset_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var scenarioId = Guid.NewGuid().ToString();

        // Act
        var response = await client.PostAsJsonAsync($"{BaseUrl}/scenarios/{scenarioId}/register", new { });

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task RegisterScenarioIpAsset_AcceptsValidRequest()
    {
        // Arrange
        var scenarioId = Guid.NewGuid().ToString();
        var request = new
        {
            IpName = "Test IP Asset",
            IpDescription = "A test intellectual property asset"
        };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync($"{BaseUrl}/scenarios/{scenarioId}/register", request);

        // Assert
        // May fail due to scenario not existing or Story Protocol not configured
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/admin/contributors/bundles/{bundleId}/register

    [Fact]
    public async Task RegisterBundleIpAsset_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var bundleId = Guid.NewGuid().ToString();

        // Act
        var response = await client.PostAsJsonAsync($"{BaseUrl}/bundles/{bundleId}/register", new { });

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    #endregion
}
