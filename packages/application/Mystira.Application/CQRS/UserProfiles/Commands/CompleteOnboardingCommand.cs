namespace Mystira.Application.CQRS.UserProfiles.Commands;

/// <summary>
/// Command to mark onboarding as complete for a user profile
/// </summary>
/// <param name="ProfileId">The unique identifier of the user profile.</param>
public record CompleteOnboardingCommand(string ProfileId) : ICommand<bool>;
