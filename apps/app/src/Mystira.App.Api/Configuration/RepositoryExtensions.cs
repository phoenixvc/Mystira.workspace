using Mystira.App.Application.Ports;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Infrastructure.Data.Repositories;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Api.Configuration;

public static class RepositoryExtensions
{
    public static IServiceCollection AddMystiraRepositories(this IServiceCollection services)
    {
        services.AddScoped<IGameSessionRepository, GameSessionRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IScenarioRepository, ScenarioRepository>();
        services.AddScoped<ICharacterMapRepository, CharacterMapRepository>();
        services.AddScoped<IContentBundleRepository, ContentBundleRepository>();
        services.AddScoped<IUserBadgeRepository, UserBadgeRepository>();
        services.AddScoped<IPlayerScenarioScoreRepository, PlayerScenarioScoreRepository>();
        services.AddScoped<IBadgeRepository, BadgeRepository>();
        services.AddScoped<IBadgeImageRepository, BadgeImageRepository>();
        services.AddScoped<IAxisAchievementRepository, AxisAchievementRepository>();
        services.AddScoped<IBadgeConfigurationRepository, BadgeConfigurationRepository>();
        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddScoped<IMediaMetadataFileRepository, MediaMetadataFileRepository>();
        services.AddScoped<ICharacterMediaMetadataFileRepository, CharacterMediaMetadataFileRepository>();
        services.AddScoped<ICharacterMapFileRepository, CharacterMapFileRepository>();
        services.AddScoped<IAvatarConfigurationFileRepository, AvatarConfigurationFileRepository>();
        services.AddScoped<ICompassAxisRepository, CompassAxisRepository>();
        services.AddScoped<IArchetypeRepository, ArchetypeRepository>();
        services.AddScoped<IEchoTypeRepository, EchoTypeRepository>();
        services.AddScoped<IFantasyThemeRepository, FantasyThemeRepository>();
        services.AddScoped<IAgeGroupRepository, AgeGroupRepository>();
        services.AddScoped<IUnitOfWork, Infrastructure.Data.UnitOfWork.UnitOfWork>();

        // COPPA compliance repositories
        services.AddScoped<ICoppaConsentRepository, CoppaConsentRepository>();
        services.AddScoped<IDataDeletionRepository, DataDeletionRepository>();
        services.AddScoped<IPendingSignupRepository, PendingSignupRepository>();

        return services;
    }
}
