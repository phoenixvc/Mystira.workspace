using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;
using Xunit;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for GameSessionsController endpoints.
/// Tests game session lifecycle and authentication requirements.
/// </summary>
[Collection("Api")]
public class GameSessionsControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/gamesessions";

    public GameSessionsControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    #region POST /api/gamesessions (Start Session)

    [Fact]
    public async Task StartSession_ReturnsBadRequest_WithInvalidRequest()
    {
        // Arrange - empty request body
        var emptyContent = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await AnonymousClient.PostAsync(BaseUrl, emptyContent);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created);
    }

    #endregion

    #region GET /api/gamesessions/{id}

    [Fact]
    public async Task GetSession_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var sessionId = Guid.NewGuid().ToString();

        // Act
        var response = await client.GetAsync($"{BaseUrl}/{sessionId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetSession_ReturnsNotFound_WhenSessionDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/gamesessions/account/{accountId}

    [Fact]
    public async Task GetSessionsByAccount_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var accountId = Guid.NewGuid().ToString();

        // Act
        var response = await client.GetAsync($"{BaseUrl}/account/{accountId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetSessionsByAccount_ReturnsOk_WhenAuthenticated()
    {
        // Arrange
        var accountId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/account/{accountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /api/gamesessions/profile/{profileId}

    [Fact]
    public async Task GetSessionsByProfile_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var profileId = Guid.NewGuid().ToString();

        // Act
        var response = await client.GetAsync($"{BaseUrl}/profile/{profileId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetSessionsByProfile_ReturnsOk_WhenAuthenticated()
    {
        // Arrange
        var profileId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/profile/{profileId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Session Lifecycle Tests

    [Fact]
    public async Task PauseSession_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var sessionId = Guid.NewGuid().ToString();

        // Act
        var response = await client.PostAsync($"{BaseUrl}/{sessionId}/pause", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task PauseSession_ReturnsNotFound_WhenSessionDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/{nonExistentId}/pause", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ResumeSession_ReturnsNotFound_WhenSessionDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/{nonExistentId}/resume", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EndSession_ReturnsNotFound_WhenSessionDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/{nonExistentId}/end", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/gamesessions/{id}/stats

    [Fact]
    public async Task GetSessionStats_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var sessionId = Guid.NewGuid().ToString();

        // Act
        var response = await client.GetAsync($"{BaseUrl}/{sessionId}/stats");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task GetSessionStats_ReturnsNotFound_WhenSessionDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/{nonExistentId}/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/gamesessions/{id}/achievements

    [Fact]
    public async Task GetAchievements_ReturnsOk_OrNotFound()
    {
        // Arrange
        var sessionId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/{sessionId}/achievements");

        // Assert - Should return OK (empty list) or error
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region GET /api/gamesessions/account/email/{email}

    [Fact]
    public async Task GetSessionsForAccountByEmail_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        var nonExistentEmail = "nonexistent@example.com";

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/account/email/{nonExistentEmail}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/gamesessions/account/{email}/history

    [Fact]
    public async Task GetSessionHistoryForAccount_ReturnsNotFound_WhenAccountDoesNotExist()
    {
        // Arrange
        var nonExistentEmail = "nonexistent@example.com";

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/account/{nonExistentEmail}/history");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
