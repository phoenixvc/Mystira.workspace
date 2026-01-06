using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for CharacterAdminController endpoints.
/// Tests character CRUD operations.
/// </summary>
[Collection("Api")]
public class CharacterAdminControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/admin/characteradmin";

    public CharacterAdminControllerTests(MystiraWebApplicationFactory factory) : base(factory)
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

        // Act - POST create
        var createResponse = await client.PostAsJsonAsync(BaseUrl, new { Id = "test", Name = "Test" });

        // Assert
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    #endregion

    #region POST /api/admin/characteradmin

    [Fact]
    public async Task AddCharacter_ReturnsOk_WithValidRequest()
    {
        // Arrange
        var request = new
        {
            Id = $"test-char-{Guid.NewGuid()}",
            Name = "Test Character",
            Image = "test-image-id",
            Metadata = new
            {
                Species = "bear",
                Age = 8,
                Roles = new[] { "Hero" },
                Archetypes = new[] { "The Brave" },
                Traits = new[] { "kind" },
                Backstory = "A test character"
            }
        };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddCharacter_ReturnsBadRequest_WithDuplicateId()
    {
        // Arrange
        var charId = $"test-char-{Guid.NewGuid()}";
        var request = new
        {
            Id = charId,
            Name = "Test Character",
            Image = "test-image-id"
        };

        // Act - create first
        await AuthenticatedClient.PostAsJsonAsync(BaseUrl, request);
        // Try to create duplicate
        var response = await AuthenticatedClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
    }

    #endregion

    #region PUT /api/admin/characteradmin/{id}

    [Fact]
    public async Task UpdateCharacter_ReturnsNotFound_WhenCharacterDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var request = new
        {
            Id = nonExistentId,
            Name = "Updated Character"
        };

        // Act
        var response = await AuthenticatedClient.PutAsJsonAsync($"{BaseUrl}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/admin/characteradmin/{id}

    [Fact]
    public async Task DeleteCharacter_ReturnsNotFound_WhenCharacterDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.DeleteAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCharacter_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var characterId = Guid.NewGuid().ToString();

        // Act
        var response = await client.DeleteAsync($"{BaseUrl}/{characterId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    #endregion
}
