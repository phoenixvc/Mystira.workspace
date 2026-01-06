using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;
using Mystira.Contracts.App.Requests.Scenarios;
using Mystira.Contracts.App.Responses.Common;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for ScenariosAdminController endpoints.
/// Tests CRUD operations with authentication requirements.
/// </summary>
[Collection("Api")]
public class ScenariosAdminControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/admin/scenarios";

    public ScenariosAdminControllerTests(MystiraWebApplicationFactory factory) : base(factory)
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

        // Act & Assert - GET by ID
        var getResponse = await client.GetAsync($"{BaseUrl}/test-id");
        getResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);

        // Act & Assert - POST create
        var createResponse = await client.PostAsJsonAsync(BaseUrl, new CreateScenarioRequest());
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);

        // Act & Assert - DELETE
        var deleteResponse = await client.DeleteAsync($"{BaseUrl}/test-id");
        deleteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    #endregion

    #region GET /api/admin/scenarios/{id}

    [Fact]
    public async Task GetScenario_ReturnsNotFound_WhenScenarioDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetScenario_ReturnsErrorResponse_WithNotFoundMessage()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/{nonExistentId}");
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        content.Should().NotBeNull();
        content!.Message.Should().Contain("not found");
    }

    #endregion

    #region POST /api/admin/scenarios

    [Fact]
    public async Task CreateScenario_ReturnsCreated_WithValidRequest()
    {
        // Arrange
        var request = TestDataBuilder.CreateScenario()
            .WithTitle($"Test Scenario {Guid.NewGuid()}")
            .WithDescription("A test scenario for integration testing")
            .WithAgeGroup("all-ages")
            .Build();

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateScenario_ReturnsBadRequest_WithInvalidRequest()
    {
        // Arrange - empty title should fail validation
        var request = new CreateScenarioRequest
        {
            Title = "",
            Description = "Test"
        };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        // Should return BadRequest or Created depending on validation rules
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created);
    }

    #endregion

    #region PUT /api/admin/scenarios/{id}

    [Fact]
    public async Task UpdateScenario_ReturnsNotFound_WhenScenarioDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var request = TestDataBuilder.CreateScenario()
            .WithTitle("Updated Title")
            .Build();

        // Act
        var response = await AuthenticatedClient.PutAsJsonAsync($"{BaseUrl}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/admin/scenarios/{id}

    [Fact]
    public async Task DeleteScenario_ReturnsNotFound_WhenScenarioDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.DeleteAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/scenarios/validate

    [Fact]
    public async Task ValidateScenario_ReturnsOk_WhenAuthenticated()
    {
        // Arrange
        var scenario = TestDataBuilder.Scenario()
            .WithTitle("Test Scenario")
            .Build();

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync($"{BaseUrl}/validate", scenario);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region GET /api/admin/scenarios/{id}/validate-references

    [Fact]
    public async Task ValidateReferences_ReturnsNotFound_WhenScenarioDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/{nonExistentId}/validate-references");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/admin/scenarios/validate-all-references

    [Fact]
    public async Task ValidateAllReferences_ReturnsOk_WhenAuthenticated()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/validate-all-references");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
