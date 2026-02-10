using Mystira.App.Application.UseCases.Accounts;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.App.Application.UseCases.Media;
using Mystira.App.Application.UseCases.Scenarios;
using Mystira.App.Application.UseCases.UserProfiles;

namespace Mystira.App.Api.Configuration;

public static class UseCaseExtensions
{
    public static IServiceCollection AddMystiraUseCases(this IServiceCollection services)
    {
        // Scenario Use Cases
        services.AddScoped<GetScenariosUseCase>();
        services.AddScoped<GetScenarioUseCase>();
        services.AddScoped<CreateScenarioUseCase>();
        services.AddScoped<UpdateScenarioUseCase>();
        services.AddScoped<DeleteScenarioUseCase>();
        services.AddScoped<ValidateScenarioUseCase>();

        // GameSession Use Cases
        services.AddScoped<CreateGameSessionUseCase>();
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
        services.AddScoped<CreateAccountUseCase>();
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

        return services;
    }
}
