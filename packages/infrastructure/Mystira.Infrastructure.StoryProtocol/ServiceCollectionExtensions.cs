using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Mystira.Application.Configuration.StoryProtocol;
using Mystira.Application.Ports;
using Mystira.Infrastructure.StoryProtocol.HealthChecks;
using Mystira.Infrastructure.StoryProtocol.Services.Grpc;
using Mystira.Infrastructure.StoryProtocol.Services.Mock;

namespace Mystira.Infrastructure.StoryProtocol;

/// <summary>
/// Extension methods for registering Story Protocol services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Story Protocol gRPC client services to the service collection.
    /// Provider selection is configuration-driven via ChainServiceOptions.UseGrpc.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddStoryProtocolServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind ChainService configuration
        var chainSection = configuration.GetSection(ChainServiceOptions.SectionName);
        services.Configure<ChainServiceOptions>(chainSection);

        var options = chainSection.Get<ChainServiceOptions>() ?? new ChainServiceOptions();

        // Register appropriate implementation based on configuration
        if (options.UseGrpc)
        {
            // Use gRPC implementation for production
            services.AddSingleton<GrpcStoryProtocolService>();
            services.AddSingleton<IStoryProtocolService>(sp =>
                sp.GetRequiredService<GrpcStoryProtocolService>());
        }
        else
        {
            // Use mock implementation for development/testing
            services.AddSingleton<IStoryProtocolService, MockStoryProtocolService>();
        }

        return services;
    }

    /// <summary>
    /// Adds Story Protocol gRPC client services with custom options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddStoryProtocolServices(
        this IServiceCollection services,
        Action<ChainServiceOptions> configureOptions)
    {
        var options = new ChainServiceOptions();
        configureOptions(options);

        services.Configure<ChainServiceOptions>(opts =>
        {
            opts.GrpcEndpoint = options.GrpcEndpoint;
            opts.TimeoutSeconds = options.TimeoutSeconds;
            opts.EnableRetry = options.EnableRetry;
            opts.MaxRetryAttempts = options.MaxRetryAttempts;
            opts.RetryBaseDelayMs = options.RetryBaseDelayMs;
            opts.WipTokenAddress = options.WipTokenAddress;
            opts.UseGrpc = options.UseGrpc;
            opts.UseTls = options.UseTls;
            opts.ApiKey = options.ApiKey;
            opts.EnableHealthChecks = options.EnableHealthChecks;
            opts.ApiKeyHeaderName = options.ApiKeyHeaderName;
        });

        if (options.UseGrpc)
        {
            services.AddSingleton<GrpcStoryProtocolService>();
            services.AddSingleton<IStoryProtocolService>(sp =>
                sp.GetRequiredService<GrpcStoryProtocolService>());
        }
        else
        {
            services.AddSingleton<IStoryProtocolService, MockStoryProtocolService>();
        }

        return services;
    }

    /// <summary>
    /// Adds Chain service health check to the health checks builder.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="name">Optional name for the health check.</param>
    /// <param name="failureStatus">Optional failure status.</param>
    /// <param name="tags">Optional tags for the health check.</param>
    /// <returns>The health checks builder for chaining.</returns>
    public static IHealthChecksBuilder AddChainServiceHealthCheck(
        this IHealthChecksBuilder builder,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        return builder.AddCheck<ChainServiceHealthCheck>(
            name ?? "chain_service",
            failureStatus ?? HealthStatus.Degraded,
            tags ?? new[] { "blockchain", "grpc", "ready" });
    }
}
