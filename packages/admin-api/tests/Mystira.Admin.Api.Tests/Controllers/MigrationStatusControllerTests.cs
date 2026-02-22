using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;
using Xunit;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for MigrationStatusController endpoints.
/// Tests migration status and recommendations for the hybrid data strategy.
/// </summary>
[Collection("Api")]
public class MigrationStatusControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/admin/migration";

    public MigrationStatusControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    #region Authentication Tests

    [Fact]
    public async Task GetMigrationStatus_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync($"{BaseUrl}/status");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRecommendations_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync($"{BaseUrl}/recommendations");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect, HttpStatusCode.Forbidden);
    }

    #endregion

    #region GET /api/admin/migration/status

    [Fact]
    public async Task GetMigrationStatus_ReturnsOk_WhenAdminAuthenticated()
    {
        // Arrange
        var adminClient = Factory.CreateClientWithRole("Admin");

        // Act
        var response = await adminClient.GetAsync($"{BaseUrl}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetMigrationStatus_ContainsCurrentPhase()
    {
        // Arrange
        var adminClient = Factory.CreateClientWithRole("Admin");

        // Act
        var response = await adminClient.GetAsync($"{BaseUrl}/status");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        (content.Contains("currentPhase") || content.Contains("CurrentPhase")).Should().BeTrue();
    }

    [Fact]
    public async Task GetMigrationStatus_ContainsInfrastructureInfo()
    {
        // Arrange
        var adminClient = Factory.CreateClientWithRole("Admin");

        // Act
        var response = await adminClient.GetAsync($"{BaseUrl}/status");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        (content.Contains("infrastructure") || content.Contains("Infrastructure")).Should().BeTrue();
    }

    [Fact]
    public async Task GetMigrationStatus_ContainsTimestamp()
    {
        // Arrange
        var adminClient = Factory.CreateClientWithRole("Admin");

        // Act
        var response = await adminClient.GetAsync($"{BaseUrl}/status");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        (content.Contains("timestamp") || content.Contains("Timestamp")).Should().BeTrue();
    }

    #endregion

    #region GET /api/admin/migration/recommendations

    [Fact]
    public async Task GetRecommendations_ReturnsOk_WhenAdminAuthenticated()
    {
        // Arrange
        var adminClient = Factory.CreateClientWithRole("Admin");

        // Act
        var response = await adminClient.GetAsync($"{BaseUrl}/recommendations");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetRecommendations_ContainsRecommendationsList()
    {
        // Arrange
        var adminClient = Factory.CreateClientWithRole("Admin");

        // Act
        var response = await adminClient.GetAsync($"{BaseUrl}/recommendations");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        (content.Contains("recommendations") || content.Contains("Recommendations")).Should().BeTrue();
    }

    [Fact]
    public async Task GetRecommendations_ContainsWarningsList()
    {
        // Arrange
        var adminClient = Factory.CreateClientWithRole("Admin");

        // Act
        var response = await adminClient.GetAsync($"{BaseUrl}/recommendations");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        (content.Contains("warnings") || content.Contains("Warnings")).Should().BeTrue();
    }

    [Fact]
    public async Task GetRecommendations_ContainsReadyForNextPhaseFlag()
    {
        // Arrange
        var adminClient = Factory.CreateClientWithRole("Admin");

        // Act
        var response = await adminClient.GetAsync($"{BaseUrl}/recommendations");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        (content.Contains("readyForNextPhase") || content.Contains("ReadyForNextPhase")).Should().BeTrue();
    }

    #endregion

    #region Authorization Tests

    [Fact]
    public async Task GetMigrationStatus_AllowsSuperAdmin()
    {
        // Arrange
        var superAdminClient = Factory.CreateClientWithRole("SuperAdmin");

        // Act
        var response = await superAdminClient.GetAsync($"{BaseUrl}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
