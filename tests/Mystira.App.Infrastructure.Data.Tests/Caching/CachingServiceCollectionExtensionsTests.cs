using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mystira.App.Infrastructure.Data.Caching;

namespace Mystira.App.Infrastructure.Data.Tests.Caching;

public class CachingServiceCollectionExtensionsTests
{
    [Fact]
    public void AddRedisCaching_WithNoConnectionString_FallsBackToInMemoryCache()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Caching:Enabled"] = "true"
            })
            .Build();

        services.AddRedisCaching(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var cache = serviceProvider.GetService<IDistributedCache>();

        cache.Should().NotBeNull();
        // Without a Redis connection string, should use in-memory
        cache.Should().BeOfType<MemoryDistributedCache>();
    }

    [Fact]
    public void AddRedisCaching_ShouldConfigureCacheOptions()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Caching:Enabled"] = "true",
                ["Caching:DefaultSlidingExpirationMinutes"] = "60",
                ["Caching:DefaultAbsoluteExpirationMinutes"] = "240",
                ["Caching:KeyPrefix"] = "test:",
                ["Caching:InstanceName"] = "test-app"
            })
            .Build();

        services.AddRedisCaching(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CacheOptions>>().Value;

        options.DefaultSlidingExpirationMinutes.Should().Be(60);
        options.DefaultAbsoluteExpirationMinutes.Should().Be(240);
        options.KeyPrefix.Should().Be("test:");
        options.InstanceName.Should().Be("test-app");
    }

    [Fact]
    public void AddRedisCaching_WithDefaultConfig_UsesDefaultValues()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddRedisCaching(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<CacheOptions>>().Value;

        options.Enabled.Should().BeTrue();
        options.DefaultSlidingExpirationMinutes.Should().Be(30);
        options.DefaultAbsoluteExpirationMinutes.Should().Be(120);
        options.KeyPrefix.Should().Be("mystira:");
        options.InstanceName.Should().Be("mystira-app");
    }

    [Fact]
    public void AddRedisCaching_WhenDisabled_StillRegistersInMemoryCache()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Caching:Enabled"] = "false"
            })
            .Build();

        services.AddRedisCaching(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var cache = serviceProvider.GetService<IDistributedCache>();

        cache.Should().NotBeNull();
    }
}
