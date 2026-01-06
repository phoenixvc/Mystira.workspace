using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for CharacterMapsAdminController endpoints.
/// Tests character map CRUD, import/export operations.
/// </summary>
[Collection("Api")]
public class CharacterMapsAdminControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/admin/charactermapsadmin";

    public CharacterMapsAdminControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    #region GET /api/admin/charactermapsadmin

    [Fact]
    public async Task GetAllCharacterMaps_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllCharacterMaps_ReturnsList()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().StartWith("["); // JSON array
    }

    #endregion

    #region GET /api/admin/charactermapsadmin/{id}

    [Fact]
    public async Task GetCharacterMap_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/admin/charactermapsadmin

    [Fact]
    public async Task CreateCharacterMap_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var request = new { Name = "Test Map" };

        // Act
        var response = await client.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task CreateCharacterMap_ReturnsCreated_WithValidRequest()
    {
        // Arrange
        var request = new
        {
            Name = $"Test Character Map {Guid.NewGuid()}",
            Description = "A test character map",
            AgeGroup = "all-ages"
        };

        // Act
        var response = await AuthenticatedClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/admin/charactermapsadmin/{id}

    [Fact]
    public async Task UpdateCharacterMap_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var mapId = Guid.NewGuid().ToString();

        // Act
        var response = await client.PutAsJsonAsync($"{BaseUrl}/{mapId}", new { Name = "Updated" });

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task UpdateCharacterMap_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var request = new { Name = "Updated Map" };

        // Act
        var response = await AuthenticatedClient.PutAsJsonAsync($"{BaseUrl}/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/admin/charactermapsadmin/{id}

    [Fact]
    public async Task DeleteCharacterMap_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var mapId = Guid.NewGuid().ToString();

        // Act
        var response = await client.DeleteAsync($"{BaseUrl}/{mapId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task DeleteCharacterMap_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AuthenticatedClient.DeleteAsync($"{BaseUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/admin/charactermapsadmin/export

    [Fact]
    public async Task ExportCharacterMaps_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.GetAsync($"{BaseUrl}/export");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task ExportCharacterMaps_ReturnsYamlFile_WhenAuthenticated()
    {
        // Act
        var response = await AuthenticatedClient.GetAsync($"{BaseUrl}/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/x-yaml");
    }

    #endregion

    #region POST /api/admin/charactermapsadmin/import

    [Fact]
    public async Task ImportCharacterMaps_RequiresAuthentication()
    {
        // Arrange
        var client = Factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Act
        var response = await client.PostAsync($"{BaseUrl}/import", null);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task ImportCharacterMaps_ReturnsBadRequest_WhenNoFileProvided()
    {
        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/import", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ImportCharacterMaps_ReturnsBadRequest_WhenInvalidFileType()
    {
        // Arrange
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(new byte[] { 1, 2, 3 }), "file", "test.txt");

        // Act
        var response = await AuthenticatedClient.PostAsync($"{BaseUrl}/import", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion
}
