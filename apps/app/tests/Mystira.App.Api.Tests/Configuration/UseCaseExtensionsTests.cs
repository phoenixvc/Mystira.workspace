using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Core;
using Mystira.Core.Ports;
using Mystira.Core.UseCases.Accounts;
using Mystira.Core.UseCases.Contributors;
using Mystira.Core.UseCases.GameSessions;
using Mystira.Core.UseCases.Media;
using Mystira.Core.UseCases.Scenarios;
using Mystira.Core.UseCases.UserProfiles;

namespace Mystira.App.Api.Tests.Configuration;

public class ApplicationServicesRegistrationTests
{
    [Fact]
    public void AddCoreApplicationServices_RegistersScenarioUseCases()
    {
        var services = new ServiceCollection();

        services.AddCoreApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(GetScenariosUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(GetScenarioUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(CreateScenarioUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(DeleteScenarioUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(ValidateScenarioUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(IValidateScenarioUseCase));
    }

    [Fact]
    public void AddCoreApplicationServices_RegistersGameSessionUseCases()
    {
        var services = new ServiceCollection();

        services.AddCoreApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(ICreateGameSessionUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(MakeChoiceUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(EndGameSessionUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(PauseGameSessionUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(ResumeGameSessionUseCase));
    }

    [Fact]
    public void AddCoreApplicationServices_RegistersAccountUseCases()
    {
        var services = new ServiceCollection();

        services.AddCoreApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(ICreateAccountUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(GetAccountUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(AddCompletedScenarioUseCase));
    }

    [Fact]
    public void AddCoreApplicationServices_RegistersUserProfileUseCases()
    {
        var services = new ServiceCollection();

        services.AddCoreApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(CreateUserProfileUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(UpdateUserProfileUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(DeleteUserProfileUseCase));
    }

    [Fact]
    public void AddCoreApplicationServices_RegistersMediaUseCases()
    {
        var services = new ServiceCollection();

        services.AddCoreApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(GetMediaUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(UploadMediaUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(DeleteMediaUseCase));
    }

    [Fact]
    public void AddCoreApplicationServices_RegistersContributorUseCases()
    {
        var services = new ServiceCollection();

        services.AddCoreApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(RegisterScenarioIpAssetUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(RegisterBundleIpAssetUseCase));
    }

    [Fact]
    public void AddCoreApplicationServices_DoesNotRegisterStoryProtocol_RegisteredByChainServices()
    {
        // IStoryProtocolService is registered by AddChainServices() in the API host,
        // not by AddCoreApplicationServices() - this is intentional for feature flag support.
        var services = new ServiceCollection();

        services.AddCoreApplicationServices();

        services.Should().NotContain(d => d.ServiceType == typeof(IStoryProtocolService));
    }

    [Fact]
    public void AddCoreApplicationServices_RegistersUseCasesAsScopedLifetime()
    {
        var services = new ServiceCollection();

        services.AddCoreApplicationServices();

        var descriptor = services.Single(d => d.ServiceType == typeof(AddCompletedScenarioUseCase));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddCoreApplicationServices_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddCoreApplicationServices();

        result.Should().BeSameAs(services);
    }
}
