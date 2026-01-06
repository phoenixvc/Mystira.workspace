using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;
using Mystira.Contracts.App.Responses.Common;
using Mystira.Contracts.App.Responses.Scenarios;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for ScenariosController endpoints.
/// </summary>
[Collection("Api")]
public class ScenariosControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/scenarios";

    public ScenariosControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    #region GET /api/scenarios

    [Fact]
    public async Task GetScenarios_ReturnsOk_WhenCalled()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetScenarios_ReturnsScenarioListResponse()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadFromJsonAsync<ScenarioListResponse>();

        // Assert
        content.Should().NotBeNull();
        content!.Scenarios.Should().NotBeNull();
    }

    [Fact]
    public async Task GetScenarios_RespectsPageSize_WhenProvided()
    {
        // Arrange
        var query = TestDataBuilder.ScenarioQuery()
            .WithPageSize(5)
            .Build();

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}?pageSize={query.PageSize}");
        var content = await response.Content.ReadFromJsonAsync<ScenarioListResponse>();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().NotBeNull();
    }

    [Fact]
    public async Task GetScenarios_FiltersByAgeGroup_WhenProvided()
    {
        // Arrange
        const string ageGroup = "younger-kids";

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}?ageGroup={ageGroup}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /api/scenarios/{id}

    [Fact]
    public async Task GetScenarioById_ReturnsNotFound_WhenScenarioDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetScenarioById_ReturnsErrorResponse_WhenNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/{nonExistentId}");
        var content = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        // Assert
        content.Should().NotBeNull();
        content!.Message.Should().Contain("not found");
    }

    #endregion

    #region GET /api/scenarios/featured

    [Fact]
    public async Task GetFeaturedScenarios_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/featured");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetFeaturedScenarios_ReturnsList()
    {
        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/featured");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().StartWith("["); // Should be a JSON array
    }

    #endregion

    #region GET /api/scenarios/age-group/{ageGroup}

    [Fact]
    public async Task GetScenariosByAgeGroup_ReturnsOk_WithValidAgeGroup()
    {
        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/age-group/all-ages");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetScenariosByAgeGroup_ReturnsList()
    {
        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/age-group/younger-kids");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().StartWith("["); // Should be a JSON array
    }

    #endregion
}
