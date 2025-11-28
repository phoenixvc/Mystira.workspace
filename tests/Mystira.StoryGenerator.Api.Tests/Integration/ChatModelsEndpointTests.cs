using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Mystira.StoryGenerator.Domain.Services;
using Mystira.StoryGenerator.Contracts.Chat;

namespace Mystira.StoryGenerator.Api.Tests.Integration;

public class ChatModelsEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    // xUnit requires the test class constructor used for fixtures to be public.
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
        var mockFactory = new Mock<ILlmServiceFactory>();

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
    public async Task GetModels_WithMultipleDeployments_ReturnsAllDeployments()
    {
        // Arrange
        var mockFactory = new Mock<ILlmServiceFactory>();

        var providerModels = new List<ProviderModels>
        {
            new()
            {
                Provider = "azure-openai",
                Available = true,
                Models = new List<ChatModelInfo>
                {
                    new() { Id = "model-1", DisplayName = "Model 1", MaxTokens = 1000 },
                    new() { Id = "model-2", DisplayName = "Model 2", MaxTokens = 2000 }
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
        Assert.Equal(2, result.TotalModels); // Both models from single provider

        var provider = result.Providers.FirstOrDefault(p => p.Provider == "azure-openai");
        Assert.NotNull(provider);
        Assert.True(provider.Available);
        Assert.Equal(2, provider.Models.Count);

        var model1 = provider.Models.FirstOrDefault(m => m.Id == "model-1");
        Assert.NotNull(model1);
        Assert.Equal("Model 1", model1.DisplayName);
        Assert.Equal(1000, model1.MaxTokens);

        var model2 = provider.Models.FirstOrDefault(m => m.Id == "model-2");
        Assert.NotNull(model2);
        Assert.Equal("Model 2", model2.DisplayName);
        Assert.Equal(2000, model2.MaxTokens);
    }
}
