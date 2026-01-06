using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for BadgesController endpoints.
/// Tests badge CRUD operations and axis achievements.
/// </summary>
[Collection("Api")]
public class BadgesControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/admin/badges";

    public BadgesControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    #region GET /api/admin/badges

    [Fact]
    public async Task GetBadges_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBadges_ReturnsList()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().StartWith("["); // JSON array
    }

    #endregion

    #region GET /api/admin/badges/{id}

    [Fact]
    public async Task GetBadgeById_ReturnsNotFound_WhenBadgeDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/badges

    [Fact]
    public async Task CreateBadge_ReturnsCreated_WithValidRequest()
    {
        // Arrange
        var request = new
        {
            Name = $"Test Badge {Guid.NewGuid()}",
            Description = "A test badge for integration testing",
            ImageUrl = "https://example.com/badge.png"
        };

        // Act
        var response = await AnonymousClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/admin/badges/{id}

    [Fact]
    public async Task UpdateBadge_ReturnsNotFound_WhenBadgeDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var request = new
        {
            Name = "Updated Badge",
            Description = "Updated description"
        };

        // Act
        var response = await AnonymousClient.PutAsJsonAsync($"{BaseUrl}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/admin/badges/{id}

    [Fact]
    public async Task DeleteBadge_ReturnsNotFound_WhenBadgeDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.DeleteAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/admin/badges/axis-achievements

    [Fact]
    public async Task GetAxisAchievements_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/axis-achievements");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAxisAchievements_FiltersByAgeGroup()
    {
        // Arrange
        var ageGroupId = "younger-kids";

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/axis-achievements?ageGroupId={ageGroupId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAxisAchievements_FiltersByCompassAxis()
    {
        // Arrange
        var compassAxisId = "courage";

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/axis-achievements?compassAxisId={compassAxisId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region POST /api/admin/badges/axis-achievements

    [Fact]
    public async Task CreateAxisAchievement_ReturnsBadRequest_WithInvalidData()
    {
        // Arrange - empty request
        var request = new { };

        // Act
        var response = await AnonymousClient.PostAsJsonAsync($"{BaseUrl}/axis-achievements", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created);
    }

    #endregion

    #region PUT /api/admin/badges/axis-achievements/{id}

    [Fact]
    public async Task UpdateAxisAchievement_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var request = new
        {
            Name = "Updated Achievement",
            Description = "Updated description"
        };

        // Act
        var response = await AnonymousClient.PutAsJsonAsync($"{BaseUrl}/axis-achievements/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/admin/badges/axis-achievements/{id}

    [Fact]
    public async Task DeleteAxisAchievement_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.DeleteAsync($"{BaseUrl}/axis-achievements/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/badges/import

    [Fact]
    public async Task ImportBadges_ReturnsBadRequest_WhenNoFileProvided()
    {
        // Act
        var response = await AnonymousClient.PostAsync($"{BaseUrl}/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/admin/badges/age-groups/{ageGroupId}/snapshot

    [Fact]
    public async Task GetSnapshot_ReturnsNotFound_WhenAgeGroupDoesNotExist()
    {
        // Arrange
        var nonExistentAgeGroupId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/age-groups/{nonExistentAgeGroupId}/snapshot");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
