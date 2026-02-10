using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Mystira.App.Api.Configuration;
using Mystira.App.Application.Ports.Data;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Api.Tests.Configuration;

public class RepositoryExtensionsTests
{
    [Fact]
    public void AddMystiraRepositories_RegistersAllRepositories()
    {
        var services = new ServiceCollection();

        services.AddMystiraRepositories();

        // Verify key repository registrations exist as descriptors
        services.Should().Contain(d => d.ServiceType == typeof(IAccountRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IUserProfileRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IGameSessionRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IScenarioRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IContentBundleRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IUserBadgeRepository));
        services.Should().Contain(d => d.ServiceType == typeof(ICharacterMapRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IMediaAssetRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IBadgeRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IBadgeConfigurationRepository));
        services.Should().Contain(d => d.ServiceType == typeof(ICompassAxisRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IArchetypeRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IEchoTypeRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IFantasyThemeRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IAgeGroupRepository));
        services.Should().Contain(d => d.ServiceType == typeof(IUnitOfWork));
    }

    [Fact]
    public void AddMystiraRepositories_RegistersAsScopedLifetime()
    {
        var services = new ServiceCollection();

        services.AddMystiraRepositories();

        var accountDescriptor = services.Single(d => d.ServiceType == typeof(IAccountRepository));
        accountDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);

        var unitOfWorkDescriptor = services.Single(d => d.ServiceType == typeof(IUnitOfWork));
        unitOfWorkDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMystiraRepositories_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMystiraRepositories();

        result.Should().BeSameAs(services);
    }
}
