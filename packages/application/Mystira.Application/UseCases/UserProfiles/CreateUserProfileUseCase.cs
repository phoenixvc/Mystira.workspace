using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Contracts.App.Requests.UserProfiles;
using Mystira.Domain.Models;
using Mystira.Domain.ValueObjects;

namespace Mystira.Application.UseCases.UserProfiles;

/// <summary>
/// Use case for creating a new user profile
/// </summary>
public class CreateUserProfileUseCase
{
    private readonly IUserProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserProfileUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserProfileUseCase"/> class.
    /// </summary>
    /// <param name="repository">The user profile repository.</param>
    /// <param name="unitOfWork">The unit of work for transaction management.</param>
    /// <param name="logger">The logger instance.</param>
    public CreateUserProfileUseCase(
        IUserProfileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateUserProfileUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new user profile based on the provided request.
    /// </summary>
    /// <param name="request">The request containing user profile data.</param>
    /// <returns>The newly created user profile.</returns>
    public async Task<UserProfile> ExecuteAsync(CreateUserProfileRequest request)
    {
        // Check if profile already exists (using Id from request)
        var existingProfile = await _repository.GetByIdAsync(request.Id);
        if (existingProfile != null)
        {
            throw new ArgumentException($"Profile already exists for name: {request.Name}");
        }

        // Validate fantasy themes
        var invalidThemes = (request.PreferredFantasyThemes ?? Enumerable.Empty<string>()).Where(t => FantasyTheme.Parse(t) == null).ToList();
        if (invalidThemes.Any())
        {
            throw new ArgumentException($"Invalid fantasy themes: {string.Join(", ", invalidThemes)}");
        }

        // Validate age group
        var allAgeGroupIds = AgeGroup.All.Select(a => a.Id).ToList();
        if (!allAgeGroupIds.Contains(request.AgeGroup))
        {
            throw new ArgumentException($"Invalid age group: {request.AgeGroup}. Must be one of: {string.Join(", ", allAgeGroupIds)}");
        }

        var profile = new UserProfile
        {
            Id = request.Id,
            Name = request.Name,
            AccountId = request.AccountId ?? string.Empty,
            PreferredFantasyThemes = request.PreferredFantasyThemes?.ToList() ?? new List<string>(),
            AgeGroupId = request.AgeGroup,
            DateOfBirth = request.DateOfBirth.HasValue ? DateOnly.FromDateTime(request.DateOfBirth.Value) : null,
            IsGuest = request.IsGuest,
            IsNpc = request.IsNpc,
            HasCompletedOnboarding = request.HasCompletedOnboarding,
            Pronouns = request.Pronouns,
            Bio = request.Bio,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            AvatarMediaId = request.SelectedAvatarMediaId,
            SelectedAvatarMediaId = request.SelectedAvatarMediaId
        };

        // If date of birth is provided, update age group automatically
        if (profile.DateOfBirth.HasValue)
        {
            profile.UpdateAgeGroupFromBirthDate();
        }

        await _repository.AddAsync(profile);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created new user profile: {ProfileId} - {Name} (Guest: {IsGuest}, NPC: {IsNPC})",
            profile.Id, profile.Name, profile.IsGuest, profile.IsNpc);
        return profile;
    }
}

