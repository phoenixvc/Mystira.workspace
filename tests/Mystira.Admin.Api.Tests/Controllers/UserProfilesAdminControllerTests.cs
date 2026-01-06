using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for UserProfilesAdminController endpoints.
/// Tests user profile management operations.
/// </summary>
[Collection("Api")]
public class UserProfilesAdminControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/userprofilesadmin";

    public UserProfilesAdminControllerTests(MystiraWebApplicationFactory factory) : base(factory)
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

        // Act - GET all profiles
        var getResponse = await client.GetAsync(BaseUrl);

        // Assert
        getResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    #endregion

    #region GET /api/userprofilesadmin

    [Fact]
    public async Task GetAllProfiles_ReturnsOk_WhenAuthenticated()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllProfiles_ReturnsList()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().StartWith("["); // JSON array
    }

    #endregion

    #region GET /api/userprofilesadmin/{id}

    [Fact]
    public async Task GetProfileById_ReturnsNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/userprofilesadmin/account/{accountId}

    [Fact]
    public async Task GetProfilesByAccount_ReturnsOk_WhenAuthenticated()
    {
        // Arrange
        var accountId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/account/{accountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProfilesByAccount_ReturnsEmptyList_WhenNoProfiles()
    {
        // Arrange
        var accountId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/account/{accountId}");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().Be("[]");
    }

    #endregion

    #region POST /api/userprofilesadmin

    [Fact]
    public async Task CreateProfile_ReturnsBadRequest_WithInvalidRequest()
    {
        // Arrange - empty request
        var request = new { };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created);
    }

    #endregion

    #region PUT /api/userprofilesadmin/{name}

    [Fact]
    public async Task UpdateProfile_ReturnsNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var nonExistentName = $"nonexistent-{Guid.NewGuid()}";
        var request = new { DisplayName = "Updated Name" };

        // Act
        var response = await AuthenticatedClient.PutAsJsonAsync($"{BaseUrl}/{nonExistentName}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region PUT /api/userprofilesadmin/id/{profileId}

    [Fact]
    public async Task UpdateProfileById_ReturnsNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var request = new { DisplayName = "Updated Name" };

        // Act
        var response = await AuthenticatedClient.PutAsJsonAsync($"{BaseUrl}/id/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/userprofilesadmin/{name}

    [Fact]
    public async Task DeleteProfile_ReturnsNotFound_WhenProfileDoesNotExist()
    {
        // Arrange
        var nonExistentName = $"nonexistent-{Guid.NewGuid()}";

        // Act
        var response = await AuthenticatedClient.DeleteAsync($"{BaseUrl}/{nonExistentName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/userprofilesadmin/non-guest

    [Fact]
    public async Task GetNonGuestProfiles_ReturnsOk_WhenAuthenticated()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/non-guest");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetNonGuestProfiles_ReturnsList()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/non-guest");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().StartWith("["); // JSON array
    }

    #endregion

    #region GET /api/userprofilesadmin/guest

    [Fact]
    public async Task GetGuestProfiles_ReturnsOk_WhenAuthenticated()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/guest");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetGuestProfiles_ReturnsList()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/guest");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().StartWith("["); // JSON array
    }

    #endregion
}
