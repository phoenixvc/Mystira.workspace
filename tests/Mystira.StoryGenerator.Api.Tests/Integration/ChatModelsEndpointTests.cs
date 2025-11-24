using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Mystira.StoryGenerator.Api;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Api.Tests.Integration;

public class ChatModelsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ChatModelsEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetModels_ReturnsSuccessResponse()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/chat/models");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ChatModelsResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Providers);
        Assert.True(result.TotalModels >= 0);
    }

    [Fact]
    public async Task GetModels_WithMockServices_ReturnsExpectedStructure()
    {
        // Arrange
        var mockFactory = new Mock<ILLMServiceFactory>();
        
        var providerModels = new List<ProviderModels>
        {
            new()
            {
                Provider = "test-provider",
                Available = true,
                Models = new List<ChatModelInfo>
                {
                    new()
                    {
                        Id = "test-model",
                        DisplayName = "Test Model",
                        MaxTokens = 1000,
                        DefaultTemperature = 0.5
                    }
                }
            }
        };
        
        mockFactory.Setup(x => x.GetAvailableModels()).Returns(providerModels);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => mockFactory.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/chat/models");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ChatModelsResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.Single(result.Providers);
        Assert.Equal("test-provider", result.Providers[0].Provider);
        Assert.True(result.Providers[0].Available);
        Assert.Single(result.Providers[0].Models);
        Assert.Equal("test-model", result.Providers[0].Models[0].Id);
        Assert.Equal(1, result.TotalModels);
    }

    [Fact]
    public async Task GetModels_WithMultipleProviders_ReturnsAllProviders()
    {
        // Arrange
        var mockFactory = new Mock<ILLMServiceFactory>();
        
        var providerModels = new List<ProviderModels>
        {
            new()
            {
                Provider = "provider-1",
                Available = true,
                Models = new List<ChatModelInfo>
                {
                    new() { Id = "model-1", DisplayName = "Model 1" },
                    new() { Id = "model-2", DisplayName = "Model 2" }
                }
            },
            new()
            {
                Provider = "provider-2",
                Available = false,
                Models = new List<ChatModelInfo>()
            }
        };
        
        mockFactory.Setup(x => x.GetAvailableModels()).Returns(providerModels);

        var client = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => mockFactory.Object);
            });
        }).CreateClient();

        // Act
        var response = await client.GetAsync("/api/chat/models");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ChatModelsResponse>(content, _jsonOptions);
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Providers.Count);
        Assert.Equal(2, result.TotalModels); // Only count models from available providers
        
        var provider1 = result.Providers.FirstOrDefault(p => p.Provider == "provider-1");
        Assert.NotNull(provider1);
        Assert.True(provider1.Available);
        Assert.Equal(2, provider1.Models.Count);
        
        var provider2 = result.Providers.FirstOrDefault(p => p.Provider == "provider-2");
        Assert.NotNull(provider2);
        Assert.False(provider2.Available);
        Assert.Empty(provider2.Models);
    }
}