using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Core.Services;
using Mystira.Core.UseCases.Accounts;
using Mystira.Core.UseCases.Avatars;
using Mystira.Core.UseCases.Badges;
using Mystira.Core.UseCases.CharacterMaps;
using Mystira.Core.UseCases.ContentBundles;
using Mystira.Core.UseCases.Contributors;
using Mystira.Core.UseCases.GameSessions;
using Mystira.Core.UseCases.Media;
using Mystira.Core.UseCases.Scenarios;
using Mystira.Core.UseCases.UserProfiles;

namespace Mystira.Core;

/// <summary>
/// Extension methods for registering Core application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Core application services including validators, use cases,
    /// and application services.
    /// </summary>
    public static IServiceCollection AddCoreApplicationServices(this IServiceCollection services)
    {
        // Register all validators from this assembly
        services.AddValidatorsFromAssemblyContaining<Validators.StartGameSessionCommandValidator>();

        // Application services
        services.AddScoped<IAxisScoringService, AxisScoringService>();
        services.AddScoped<IBadgeAwardingService, BadgeAwardingService>();
        services.AddSingleton<IQueryCacheInvalidationService, QueryCacheInvalidationService>();

        // Email builders
        services.AddSingleton<ConsentEmailBuilder>();
        services.AddSingleton<MagicSignupEmailBuilder>();

        // Use Cases
        services.AddCoreUseCases();

        return services;
    }

    private static IServiceCollection AddCoreUseCases(this IServiceCollection services)
    {
        // Scenario Use Cases
        services.AddScoped<GetScenariosUseCase>();
        services.AddScoped<GetScenarioUseCase>();
        services.AddScoped<CreateScenarioUseCase>();
        services.AddScoped<UpdateScenarioUseCase>();
        services.AddScoped<DeleteScenarioUseCase>();
        services.AddScoped<ValidateScenarioUseCase>();
        services.AddScoped<IValidateScenarioUseCase>(sp => sp.GetRequiredService<ValidateScenarioUseCase>());

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
        services.AddScoped<UpdateAccountSettingsUseCase>();
        services.AddScoped<UpdateSubscriptionUseCase>();
        services.AddScoped<AddUserProfileToAccountUseCase>();
        services.AddScoped<RemoveUserProfileFromAccountUseCase>();
        services.AddScoped<AddCompletedScenarioUseCase>();
        services.AddScoped<GetCompletedScenariosUseCase>();

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

        // Avatar Use Cases
        services.AddScoped<GetAvatarConfigurationsUseCase>();
        services.AddScoped<GetAvatarsByAgeGroupUseCase>();
        services.AddScoped<CreateAvatarConfigurationUseCase>();
        services.AddScoped<UpdateAvatarConfigurationUseCase>();
        services.AddScoped<DeleteAvatarConfigurationUseCase>();
        services.AddScoped<AssignAvatarToAgeGroupUseCase>();

        // Badge Use Cases
        services.AddScoped<GetBadgeUseCase>();
        services.AddScoped<GetBadgesByAxisUseCase>();
        services.AddScoped<GetUserBadgesUseCase>();
        services.AddScoped<AwardBadgeUseCase>();
        services.AddScoped<RevokeBadgeUseCase>();

        // CharacterMap Use Cases
        services.AddScoped<GetCharacterMapsUseCase>();
        services.AddScoped<GetCharacterMapUseCase>();
        services.AddScoped<CreateCharacterMapUseCase>();
        services.AddScoped<UpdateCharacterMapUseCase>();
        services.AddScoped<DeleteCharacterMapUseCase>();
        services.AddScoped<ImportCharacterMapUseCase>();
        services.AddScoped<ExportCharacterMapUseCase>();

        // ContentBundle Use Cases
        services.AddScoped<GetContentBundlesUseCase>();
        services.AddScoped<GetContentBundleUseCase>();
        services.AddScoped<GetContentBundlesByAgeGroupUseCase>();
        services.AddScoped<CreateContentBundleUseCase>();
        services.AddScoped<UpdateContentBundleUseCase>();
        services.AddScoped<DeleteContentBundleUseCase>();
        services.AddScoped<AddScenarioToBundleUseCase>();
        services.AddScoped<RemoveScenarioFromBundleUseCase>();
        services.AddScoped<CheckBundleAccessUseCase>();

        // Contributor Use Cases
        services.AddScoped<RegisterScenarioIpAssetUseCase>();
        services.AddScoped<RegisterBundleIpAssetUseCase>();
        services.AddScoped<SetScenarioContributorsUseCase>();
        services.AddScoped<SetBundleContributorsUseCase>();

        return services;
    }
}
