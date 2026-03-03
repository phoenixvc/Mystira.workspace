using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Application.Configuration.StoryProtocol;
using Mystira.Application.Ports;
using Mystira.Infrastructure.StoryProtocol.Services.Grpc;
using Mystira.Infrastructure.StoryProtocol.Services.Mock;
using Xunit;

namespace Mystira.Infrastructure.StoryProtocol.Tests;

/// <summary>
/// Unit tests for <see cref="ServiceCollectionExtensions"/>.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    #region AddStoryProtocolServices with IConfiguration Tests

    [Fact]
    public void AddStoryProtocolServices_WithUseGrpcFalse_RegistersMockService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ChainService:UseGrpc"] = "false"
        });

        // Act
        services.AddStoryProtocolServices(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<IStoryProtocolService>();
        service.Should().NotBeNull();
        service.Should().BeOfType<MockStoryProtocolService>();
    }

    [Fact]
    public void AddStoryProtocolServices_WithDefaultConfiguration_RegistersMockService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = CreateConfiguration(new Dictionary<string, string?>());

        // Act
        services.AddStoryProtocolServices(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<IStoryProtocolService>();
        service.Should().BeOfType<MockStoryProtocolService>();
    }

    [Fact]
    public void AddStoryProtocolServices_WithUseGrpcTrue_RegistersGrpcService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ChainService:UseGrpc"] = "true",
            ["ChainService:GrpcEndpoint"] = "https://localhost:50051"
        });

        // Act
        services.AddStoryProtocolServices(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<IStoryProtocolService>();
        service.Should().NotBeNull();
        service.Should().BeOfType<GrpcStoryProtocolService>();
    }

    [Fact]
    public void AddStoryProtocolServices_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ChainService:GrpcEndpoint"] = "https://custom:8080",
            ["ChainService:TimeoutSeconds"] = "60",
            ["ChainService:MaxRetryAttempts"] = "5",
            ["ChainService:ApiKey"] = "test-api-key"
        });

        // Act
        services.AddStoryProtocolServices(configuration);
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<Microsoft.Extensions.Options.IOptions<ChainServiceOptions>>();
        options.Should().NotBeNull();
        options!.Value.GrpcEndpoint.Should().Be("https://custom:8080");
        options.Value.TimeoutSeconds.Should().Be(60);
        options.Value.MaxRetryAttempts.Should().Be(5);
        options.Value.ApiKey.Should().Be("test-api-key");
    }

    #endregion

    #region AddStoryProtocolServices with Action<ChainServiceOptions> Tests

    [Fact]
    public void AddStoryProtocolServices_WithActionConfigurer_RegistersMockService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddStoryProtocolServices(options =>
        {
            options.UseGrpc = false;
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<IStoryProtocolService>();
        service.Should().BeOfType<MockStoryProtocolService>();
    }

    [Fact]
    public void AddStoryProtocolServices_WithActionConfigurer_RegistersGrpcService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddStoryProtocolServices(options =>
        {
            options.UseGrpc = true;
            options.GrpcEndpoint = "https://localhost:50051";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<IStoryProtocolService>();
        service.Should().BeOfType<GrpcStoryProtocolService>();
    }

    [Fact]
    public void AddStoryProtocolServices_WithActionConfigurer_ConfiguresAllOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddStoryProtocolServices(options =>
        {
            options.GrpcEndpoint = "https://test:9000";
            options.TimeoutSeconds = 30;
            options.EnableRetry = false;
            options.MaxRetryAttempts = 10;
            options.RetryBaseDelayMs = 500;
            options.WipTokenAddress = "0xCustomToken";
            options.UseGrpc = false;
            options.UseTls = false;
            options.ApiKey = "custom-key";
            options.EnableHealthChecks = false;
            options.ApiKeyHeaderName = "Authorization";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<Microsoft.Extensions.Options.IOptions<ChainServiceOptions>>();
        options.Should().NotBeNull();
        options!.Value.GrpcEndpoint.Should().Be("https://test:9000");
        options.Value.TimeoutSeconds.Should().Be(30);
        options.Value.EnableRetry.Should().BeFalse();
        options.Value.MaxRetryAttempts.Should().Be(10);
        options.Value.RetryBaseDelayMs.Should().Be(500);
        options.Value.WipTokenAddress.Should().Be("0xCustomToken");
        options.Value.UseGrpc.Should().BeFalse();
        options.Value.UseTls.Should().BeFalse();
        options.Value.ApiKey.Should().Be("custom-key");
        options.Value.EnableHealthChecks.Should().BeFalse();
        options.Value.ApiKeyHeaderName.Should().Be("Authorization");
    }

    #endregion

    #region GrpcEndpoint Validation Tests

    [Fact]
    public void AddStoryProtocolServices_WithUseGrpcAndNullEndpointConfig_UsesDefaultEndpoint()
    {
        // Arrange
        // When GrpcEndpoint is null in config, the default value ("https://localhost:50051") is used
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ChainService:UseGrpc"] = "true",
            ["ChainService:GrpcEndpoint"] = null // Null means "use default"
        });

        // Act & Assert - should not throw because default is a valid URI
        var act = () => services.AddStoryProtocolServices(configuration);
        act.Should().NotThrow();
    }

    [Fact]
    public void AddStoryProtocolServices_WithUseGrpcAndEmptyEndpoint_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ChainService:UseGrpc"] = "true",
            ["ChainService:GrpcEndpoint"] = ""
        });

        // Act & Assert
        var act = () => services.AddStoryProtocolServices(configuration);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GrpcEndpoint is required*");
    }

    [Fact]
    public void AddStoryProtocolServices_WithUseGrpcAndInvalidUri_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ChainService:UseGrpc"] = "true",
            ["ChainService:GrpcEndpoint"] = "not-a-valid-uri"
        });

        // Act & Assert
        var act = () => services.AddStoryProtocolServices(configuration);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*is not a valid URI*");
    }

    [Fact]
    public void AddStoryProtocolServices_WithUseGrpcAndInvalidScheme_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ChainService:UseGrpc"] = "true",
            ["ChainService:GrpcEndpoint"] = "ftp://localhost:50051"
        });

        // Act & Assert
        var act = () => services.AddStoryProtocolServices(configuration);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*scheme*is not supported*");
    }

    [Fact]
    public void AddStoryProtocolServices_WithActionAndUseGrpcAndNullEndpoint_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act & Assert
        var act = () => services.AddStoryProtocolServices(options =>
        {
            options.UseGrpc = true;
            options.GrpcEndpoint = null!;
        });
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*GrpcEndpoint is required*");
    }

    [Theory]
    [InlineData("http://localhost:50051")]
    [InlineData("https://chain.example.com:443")]
    [InlineData("http://192.168.1.1:8080")]
    public void AddStoryProtocolServices_WithValidGrpcEndpoints_DoesNotThrow(string endpoint)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = CreateConfiguration(new Dictionary<string, string?>
        {
            ["ChainService:UseGrpc"] = "true",
            ["ChainService:GrpcEndpoint"] = endpoint
        });

        // Act & Assert
        var act = () => services.AddStoryProtocolServices(configuration);
        act.Should().NotThrow();
    }

    #endregion

    #region Fluent Chaining Tests

    [Fact]
    public void AddStoryProtocolServices_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = CreateConfiguration(new Dictionary<string, string?>());

        // Act
        var result = services.AddStoryProtocolServices(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddStoryProtocolServices_WithAction_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var result = services.AddStoryProtocolServices(options => { });

        // Assert
        result.Should().BeSameAs(services);
    }

    #endregion

    #region Helper Methods

    private static IConfiguration CreateConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }

    #endregion
}
