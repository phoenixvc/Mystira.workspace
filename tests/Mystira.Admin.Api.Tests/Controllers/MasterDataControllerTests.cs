using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;
using Xunit;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for master data controllers (Archetypes, CompassAxes, etc.).
/// Tests CRUD operations for reference data entities.
/// </summary>
[Collection("Api")]
public class MasterDataControllerTests : ApiTestFixture
{
    public MasterDataControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    #region Archetypes Controller

    [Fact]
    public async Task Archetypes_GetAll_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync("/api/admin/archetypes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Archetypes_GetAll_ReturnsList()
    {
        // Act
        var response = await AnonymousClient.GetAsync("/api/admin/archetypes");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        content.Should().StartWith("["); // JSON array
    }

    [Fact]
    public async Task Archetypes_GetById_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"/api/admin/archetypes/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Archetypes_Create_ReturnsCreated_WithValidData()
    {
        // Arrange
        var request = new
        {
            Name = $"Test Archetype {Guid.NewGuid()}",
            Description = "A test archetype"
        };

        // Act
        var response = await AnonymousClient.PostAsJsonAsync("/api/admin/archetypes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Archetypes_Update_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var request = new { Name = "Updated", Description = "Updated" };

        // Act
        var response = await AnonymousClient.PutAsJsonAsync($"/api/admin/archetypes/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Archetypes_Delete_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.DeleteAsync($"/api/admin/archetypes/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region CompassAxes Controller

    [Fact]
    public async Task CompassAxes_GetAll_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync("/api/admin/compassaxes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CompassAxes_GetById_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"/api/admin/compassaxes/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompassAxes_Create_ReturnsCreated_WithValidData()
    {
        // Arrange
        var request = new
        {
            Name = $"Test Compass Axis {Guid.NewGuid()}",
            Description = "A test compass axis"
        };

        // Act
        var response = await AnonymousClient.PostAsJsonAsync("/api/admin/compassaxes", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CompassAxes_Update_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();
        var request = new { Name = "Updated", Description = "Updated" };

        // Act
        var response = await AnonymousClient.PutAsJsonAsync($"/api/admin/compassaxes/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompassAxes_Delete_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.DeleteAsync($"/api/admin/compassaxes/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region EchoTypes Controller

    [Fact]
    public async Task EchoTypes_GetAll_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync("/api/admin/echotypes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task EchoTypes_GetById_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"/api/admin/echotypes/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region FantasyThemes Controller

    [Fact]
    public async Task FantasyThemes_GetAll_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync("/api/admin/fantasythemes");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FantasyThemes_GetById_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"/api/admin/fantasythemes/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region AgeGroups Controller

    [Fact]
    public async Task AgeGroups_GetAll_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync("/api/admin/agegroups");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AgeGroups_GetById_ReturnsNotFound_WhenNotExists()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act
        var response = await AnonymousClient.GetAsync($"/api/admin/agegroups/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
