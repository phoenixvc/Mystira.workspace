using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mystira.App.Application.Ports;
using Mystira.App.Application.Services;
using Mystira.App.Application.UseCases.Accounts;
using Mystira.App.Application.UseCases.Contributors;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.App.Application.UseCases.Media;
using Mystira.App.Application.UseCases.Scenarios;
using Mystira.App.Application.UseCases.UserProfiles;

namespace Mystira.App.Application;

/// <summary>
/// Extension methods for registering Application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer services including validators, use cases,
    /// and application services.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register all validators from this assembly
        services.AddValidatorsFromAssemblyContaining<Validators.StartGameSessionCommandValidator>();

        // Application services
        services.AddScoped<IAxisScoringService, AxisScoringService>();
        services.AddScoped<IBadgeAwardingService, BadgeAwardingService>();
        services.AddSingleton<IQueryCacheInvalidationService, QueryCacheInvalidationService>();

        // Note: IStoryProtocolService is registered by AddChainServices() in the API host
        // with feature flag support (stub vs real gRPC adapter)

        // Use Cases (PERF-4: moved from API Configuration to Application layer)
        services.AddMystiraUseCases();

        return services;
    }

    /// <summary>
    /// Registers all Use Case classes in the DI container.
    /// </summary>
    private static IServiceCollection AddMystiraUseCases(this IServiceCollection services)
    {
        // Scenario Use Cases
        services.AddScoped<GetScenariosUseCase>();
        services.AddScoped<GetScenarioUseCase>();
        services.AddScoped<CreateScenarioUseCase>();
        services.AddScoped<UpdateScenarioUseCase>();
        services.AddScoped<DeleteScenarioUseCase>();
        services.AddScoped<ValidateScenarioUseCase>();

        // GameSession Use Cases
        services.AddScoped<ICreateGameSessionUseCase, CreateGameSessionUseCase>();
        services.AddScoped<GetGameSessionUseCase>();
        services.AddScoped<GetGameSessionsByAccountUseCase>();
        services.AddScoped<GetGameSessionsByProfileUseCase>();
        services.AddScoped<GetInProgressSessionsUseCase>();
        services.AddScoped<MakeChoiceUseCase>();
        services.AddScoped<ProgressSceneUseCase>();
        services.AddScoped<PauseGameSessionUseCase>();
        services.AddScoped<ResumeGameSessionUseCase>();
        services.AddScoped<EndGameSessionUseCase>();
        services.AddScoped<SelectCharacterUseCase>();
        services.AddScoped<GetSessionStatsUseCase>();
        services.AddScoped<CheckAchievementsUseCase>();
        services.AddScoped<DeleteGameSessionUseCase>();

        // Account Use Cases
        services.AddScoped<GetAccountByEmailUseCase>();
        services.AddScoped<GetAccountUseCase>();
        services.AddScoped<ICreateAccountUseCase, CreateAccountUseCase>();
        services.AddScoped<UpdateAccountUseCase>();
        services.AddScoped<AddUserProfileToAccountUseCase>();
        services.AddScoped<RemoveUserProfileFromAccountUseCase>();
        services.AddScoped<AddCompletedScenarioUseCase>();

        // UserProfile Use Cases
        services.AddScoped<CreateUserProfileUseCase>();
        services.AddScoped<UpdateUserProfileUseCase>();
        services.AddScoped<GetUserProfileUseCase>();
        services.AddScoped<DeleteUserProfileUseCase>();

        // Media Use Cases
        services.AddScoped<GetMediaUseCase>();
        services.AddScoped<GetMediaByFilenameUseCase>();
        services.AddScoped<ListMediaUseCase>();
        services.AddScoped<UploadMediaUseCase>();
        services.AddScoped<UpdateMediaMetadataUseCase>();
        services.AddScoped<DeleteMediaUseCase>();
        services.AddScoped<DownloadMediaUseCase>();

        // Contributor Use Cases
        services.AddScoped<RegisterScenarioIpAssetUseCase>();
        services.AddScoped<RegisterBundleIpAssetUseCase>();

        return services;
    }
}
