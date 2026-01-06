using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Mystira.Admin.Api.Tests.Infrastructure;
using Mystira.Contracts.App.Responses;

namespace Mystira.Admin.Api.Tests.Controllers;

/// <summary>
/// Integration tests for ApiInfoController endpoints.
/// Tests API version and compatibility information.
/// </summary>
[Collection("Api")]
public class ApiInfoControllerTests : ApiTestFixture
{
    private const string BaseUrl = "/api/apiinfo";

    public ApiInfoControllerTests(MystiraWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetApiInfo_ReturnsOk()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetApiInfo_ReturnsApiVersionInfo()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadFromJsonAsync<ApiVersionInfo>();

        // Assert
        content.Should().NotBeNull();
        content!.ApiVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetApiInfo_ContainsV1ApiVersion()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadFromJsonAsync<ApiVersionInfo>();

        // Assert
        content.Should().NotBeNull();
        content!.ApiVersion.Should().Be("v1");
    }

    [Fact]
    public async Task GetApiInfo_ContainsBuildVersion()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadFromJsonAsync<ApiVersionInfo>();

        // Assert
        content.Should().NotBeNull();
        content!.BuildVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetApiInfo_ContainsMasterDataVersion()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadFromJsonAsync<ApiVersionInfo>();

        // Assert
        content.Should().NotBeNull();
        content!.MasterDataVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetApiInfo_ContainsMasterDataEntities()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadFromJsonAsync<ApiVersionInfo>();

        // Assert
        content.Should().NotBeNull();
        content!.MasterDataEntities.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetApiInfo_MasterDataEntities_ContainExpectedTypes()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);
        var content = await response.Content.ReadFromJsonAsync<ApiVersionInfo>();

        // Assert
        content.Should().NotBeNull();
        var entityNames = content!.MasterDataEntities.Select(e => e.Name).ToList();

        entityNames.Should().Contain("CompassAxis");
        entityNames.Should().Contain("Archetype");
        entityNames.Should().Contain("EchoType");
        entityNames.Should().Contain("FantasyTheme");
        entityNames.Should().Contain("AgeGroup");
    }

    [Fact]
    public async Task GetApiInfo_DoesNotRequireAuthentication()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetApiInfo_ReturnsJsonContentType()
    {
        // Act
        var response = await AnonymousClient.GetAsync(BaseUrl);

        // Assert
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }
}
