using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Controllers;
using Mystira.Admin.Api.Tests.Infrastructure;
using Xunit;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for AuthController endpoints.
/// </summary>
[Collection("Api")]
public class AuthControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/auth";

    public AuthControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    #region GET /api/auth/status

    [Fact]
    public async Task GetAuthStatus_ReturnsOk_WhenAnonymous()
    {
        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAuthStatus_ReturnsNotAuthenticated_WhenAnonymous()
    {
        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/status");
        var content = await response.Content.ReadFromJsonAsync<AuthStatusResponse>();

        // Assert
        content.Should().NotBeNull();
        content!.IsAuthenticated.Should().BeFalse();
        content.Username.Should().BeNull();
        content.Role.Should().BeNull();
    }

    [Fact]
    public async Task GetAuthStatus_ReturnsAuthenticated_WhenLoggedIn()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/status");
        var content = await response.Content.ReadFromJsonAsync<AuthStatusResponse>();

        // Assert
        content.Should().NotBeNull();
        content!.IsAuthenticated.Should().BeTrue();
        content.Username.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetAuthStatus_ReturnsCorrectRole_WhenAuthenticated()
    {
        // Arrange
        var clientWithRole = Factory.CreateClientWithRole("SuperAdmin");

        // Act
        var response = await clientWithRole.GetAsync($"{BaseUrl}/status");
        var content = await response.Content.ReadFromJsonAsync<AuthStatusResponse>();

        // Assert
        content.Should().NotBeNull();
        content!.Role.Should().Be("SuperAdmin");
    }

    #endregion

    #region POST /api/auth/login

    [Fact]
    public async Task Login_ReturnsUnauthorized_WithInvalidCredentials()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "invalid-user",
            Password = "invalid-password"
        };

        // Act
        var response = await AnonymousClient.PostAsJsonAsync($"{BaseUrl}/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ReturnsErrorMessage_WithInvalidCredentials()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "invalid-user",
            Password = "invalid-password"
        };

        // Act
        var response = await AnonymousClient.PostAsJsonAsync($"{BaseUrl}/login", loginRequest);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().Contain("Invalid").Or.Contain("not configured");
    }

    [Fact]
    public async Task Login_ReturnsBadRequest_WithEmptyBody()
    {
        // Arrange
        var emptyContent = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await AnonymousClient.PostAsync($"{BaseUrl}/login", emptyContent);

        // Assert
        // Should return unauthorized (empty credentials are invalid)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    #endregion

    #region POST /api/auth/logout

    [Fact]
    public async Task Logout_ReturnsOk_WhenCalled()
    {
        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_ReturnsSuccessMessage()
    {
        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/logout", null);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().Contain("success").Or.Contain("Logged out");
    }

    [Fact]
    public async Task Logout_WorksForAnonymousUser()
    {
        // Act
        var response = await AnonymousClient.PostAsync($"{BaseUrl}/logout", null);

        // Assert
        // Logout should succeed even if not logged in
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
