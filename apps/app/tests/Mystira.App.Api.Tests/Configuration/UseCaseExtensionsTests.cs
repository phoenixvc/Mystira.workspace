using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Mystira.App.Application;
using Mystira.Core.Ports;
using Mystira.App.Application.UseCases.Accounts;
using Mystira.App.Application.UseCases.Contributors;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.App.Application.UseCases.Media;
using Mystira.App.Application.UseCases.Scenarios;
using Mystira.App.Application.UseCases.UserProfiles;

namespace Mystira.App.Api.Tests.Configuration;

public class ApplicationServicesRegistrationTests
{
    [Fact]
    public void AddApplicationServices_RegistersScenarioUseCases()
    {
        var services = new ServiceCollection();

        services.AddApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(GetScenariosUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(GetScenarioUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(CreateScenarioUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(DeleteScenarioUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(ValidateScenarioUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(IValidateScenarioUseCase));
    }

    [Fact]
    public void AddApplicationServices_RegistersGameSessionUseCases()
    {
        var services = new ServiceCollection();

        services.AddApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(ICreateGameSessionUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(MakeChoiceUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(EndGameSessionUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(PauseGameSessionUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(ResumeGameSessionUseCase));
    }

    [Fact]
    public void AddApplicationServices_RegistersAccountUseCases()
    {
        var services = new ServiceCollection();

        services.AddApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(ICreateAccountUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(GetAccountUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(AddCompletedScenarioUseCase));
    }

    [Fact]
    public void AddApplicationServices_RegistersUserProfileUseCases()
    {
        var services = new ServiceCollection();

        services.AddApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(CreateUserProfileUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(UpdateUserProfileUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(DeleteUserProfileUseCase));
    }

    [Fact]
    public void AddApplicationServices_RegistersMediaUseCases()
    {
        var services = new ServiceCollection();

        services.AddApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(GetMediaUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(UploadMediaUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(DeleteMediaUseCase));
    }

    [Fact]
    public void AddApplicationServices_RegistersContributorUseCases()
    {
        var services = new ServiceCollection();

        services.AddApplicationServices();

        services.Should().Contain(d => d.ServiceType == typeof(RegisterScenarioIpAssetUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(RegisterBundleIpAssetUseCase));
    }

    [Fact]
    public void AddApplicationServices_DoesNotRegisterStoryProtocol_RegisteredByChainServices()
    {
        // IStoryProtocolService is registered by AddChainServices() in the API host,
        // not by AddApplicationServices() - this is intentional for feature flag support.
        var services = new ServiceCollection();

        services.AddApplicationServices();

        services.Should().NotContain(d => d.ServiceType == typeof(IStoryProtocolService));
    }

    [Fact]
    public void AddApplicationServices_RegistersUseCasesAsScopedLifetime()
    {
        var services = new ServiceCollection();

        services.AddApplicationServices();

        var descriptor = services.Single(d => d.ServiceType == typeof(AddCompletedScenarioUseCase));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddApplicationServices_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddApplicationServices();

        result.Should().BeSameAs(services);
    }
}
