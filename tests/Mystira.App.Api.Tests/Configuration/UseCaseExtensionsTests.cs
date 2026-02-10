using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Mystira.App.Api.Configuration;
using Mystira.App.Application.UseCases.Accounts;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.App.Application.UseCases.Media;
using Mystira.App.Application.UseCases.Scenarios;
using Mystira.App.Application.UseCases.UserProfiles;

namespace Mystira.App.Api.Tests.Configuration;

public class UseCaseExtensionsTests
{
    [Fact]
    public void AddMystiraUseCases_RegistersScenarioUseCases()
    {
        var services = new ServiceCollection();

        services.AddMystiraUseCases();

        services.Should().Contain(d => d.ServiceType == typeof(GetScenariosUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(GetScenarioUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(CreateScenarioUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(DeleteScenarioUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(ValidateScenarioUseCase));
    }

    [Fact]
    public void AddMystiraUseCases_RegistersGameSessionUseCases()
    {
        var services = new ServiceCollection();

        services.AddMystiraUseCases();

        services.Should().Contain(d => d.ServiceType == typeof(CreateGameSessionUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(MakeChoiceUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(EndGameSessionUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(PauseGameSessionUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(ResumeGameSessionUseCase));
    }

    [Fact]
    public void AddMystiraUseCases_RegistersAccountUseCases()
    {
        var services = new ServiceCollection();

        services.AddMystiraUseCases();

        services.Should().Contain(d => d.ServiceType == typeof(CreateAccountUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(GetAccountUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(AddCompletedScenarioUseCase));
    }

    [Fact]
    public void AddMystiraUseCases_RegistersUserProfileUseCases()
    {
        var services = new ServiceCollection();

        services.AddMystiraUseCases();

        services.Should().Contain(d => d.ServiceType == typeof(CreateUserProfileUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(UpdateUserProfileUseCase));
        services.Should().Contain(d => d.ServiceType == typeof(DeleteUserProfileUseCase));
    }

    [Fact]
    public void AddMystiraUseCases_RegistersAsScopedLifetime()
    {
        var services = new ServiceCollection();

        services.AddMystiraUseCases();

        var descriptor = services.Single(d => d.ServiceType == typeof(CreateAccountUseCase));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddMystiraUseCases_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMystiraUseCases();

        result.Should().BeSameAs(services);
    }
}
