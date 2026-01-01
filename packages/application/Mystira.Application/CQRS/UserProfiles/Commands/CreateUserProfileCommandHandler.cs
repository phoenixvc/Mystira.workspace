using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.ValueObjects;

namespace Mystira.Application.CQRS.UserProfiles.Commands;

/// <summary>
/// Wolverine handler for CreateUserProfileCommand.
/// Creates a new user profile with the specified details.
/// </summary>
public static class CreateUserProfileCommandHandler
{
    /// <summary>
    /// Handles the CreateUserProfileCommand by creating a new user profile.
    /// Wolverine injects dependencies as method parameters.
    /// </summary>
    public static async Task<UserProfile> Handle(
        CreateUserProfileCommand command,
        IUserProfileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger logger,
        CancellationToken ct)
    {
        var request = command.Request;

        // Validate request
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("Profile name is required");
        }

        // Enforce minimum length for name to provide a clear error from the API layer
        // This mirrors DataAnnotations on CreateUserProfileRequest (MinimumLength = 2)
        if (request.Name.Trim().Length < 2)
        {
            throw new ArgumentException("Profile name must be at least 2 characters long");
        }

        if (string.IsNullOrWhiteSpace(request.AgeGroup))
        {
            throw new ArgumentException("Age group is required");
        }

        var allAgeGroupIds = AgeGroup.All.Select(a => a.Id).ToList();
        if (!allAgeGroupIds.Contains(request.AgeGroup))
        {
            throw new ArgumentException($"Invalid age group: {request.AgeGroup}. Must be one of: {string.Join(", ", allAgeGroupIds)}");
        }

        var profile = new UserProfile
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            DateOfBirth = request.DateOfBirth.HasValue ? DateOnly.FromDateTime(request.DateOfBirth.Value) : null,
            IsGuest = request.IsGuest,
            IsNpc = request.IsNpc,
            AccountId = request.AccountId ?? string.Empty,
            Pronouns = request.Pronouns,
            Bio = request.Bio,
            PreferredFantasyThemes = request.PreferredFantasyThemes?.ToList() ?? new List<string>(),
            AvatarMediaId = request.SelectedAvatarMediaId,
            SelectedAvatarMediaId = request.SelectedAvatarMediaId,
            HasCompletedOnboarding = request.HasCompletedOnboarding,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AgeGroupId = request.AgeGroup
        };

        // Update age group from date of birth if provided
        if (request.DateOfBirth.HasValue)
        {
            profile.UpdateAgeGroupFromBirthDate();
        }

        // Add to repository
        await repository.AddAsync(profile);

        // Persist changes
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Created user profile {ProfileId} with name {Name}", profile.Id, profile.Name);

        return profile;
    }
}
