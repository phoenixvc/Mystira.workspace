using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mystira.App.Application.Configuration.StoryProtocol;
using Mystira.App.Application.Ports;
using Mystira.App.Application.Services;
using Mystira.App.Infrastructure.Chain.Services;

namespace Mystira.App.Infrastructure.Chain;

/// <summary>
/// DI registration for Story Protocol / Chain service integration.
/// Uses feature flag to switch between stub and real gRPC adapter.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers Story Protocol services with feature flag support.
    /// When ChainService:UseGrpc is true, uses GrpcChainServiceAdapter.
    /// Otherwise, falls back to StubStoryProtocolService.
    /// </summary>
    public static IServiceCollection AddChainServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration
        services.Configure<StoryProtocolOptions>(configuration.GetSection(StoryProtocolOptions.SectionName));
        services.Configure<ChainServiceOptions>(configuration.GetSection(ChainServiceOptions.SectionName));

        var chainConfig = configuration.GetSection(ChainServiceOptions.SectionName).Get<ChainServiceOptions>();
        var useGrpc = chainConfig?.UseGrpc ?? false;

        if (useGrpc)
        {
            // Singleton: GrpcChannel manages HTTP/2 connection pool internally
            services.AddSingleton<IStoryProtocolService, GrpcChainServiceAdapter>();
        }
        else
        {
            // Stub for development/testing (no blockchain calls)
            services.AddScoped<IStoryProtocolService, StubStoryProtocolService>();
        }

        return services;
    }
}
