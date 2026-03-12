using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Contracts.App.Requests.UserProfiles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.UserProfiles;

/// <summary>
/// Use case for creating a new user profile
/// </summary>
public class CreateUserProfileUseCase
{
    private readonly IUserProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateUserProfileUseCase> _logger;

    public CreateUserProfileUseCase(
        IUserProfileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<CreateUserProfileUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserProfile> ExecuteAsync(CreateUserProfileRequest request, CancellationToken ct = default)
    {
        // Check if profile already exists (using Id from request)
        var existingProfile = await _repository.GetByIdAsync(request.Id, ct);
        if (existingProfile != null)
        {
            throw new ConflictException($"Profile already exists for name: {request.Name}");
        }

        // Validate fantasy themes
        var invalidThemes = (request.PreferredFantasyThemes ?? Enumerable.Empty<string>()).Where(t => FantasyTheme.Parse(t) == null).ToList();
        if (invalidThemes.Any())
        {
            throw new ValidationException("preferredFantasyThemes", $"Invalid fantasy themes: {string.Join(", ", invalidThemes)}");
        }

        // Validate age group
        if (!AgeGroupConstants.GetAll().Contains(request.AgeGroup))
        {
            throw new ValidationException("ageGroup", $"Invalid age group: {request.AgeGroup}. Must be one of: {string.Join(", ", AgeGroupConstants.GetAll())}");
        }

        var profile = new UserProfile
        {
            Id = request.Id,
            Name = request.Name,
            AccountId = request.AccountId,
            PreferredFantasyThemes = request.PreferredFantasyThemes?.Where(t => FantasyTheme.Parse(t) != null).ToList() ?? new List<string>(),
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

        await _repository.AddAsync(profile, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Created new user profile: {ProfileId} - {Name} (Guest: {IsGuest}, NPC: {IsNPC})",
            PiiMask.HashId(profile.Id), PiiMask.HashId(profile.Name), profile.IsGuest, profile.IsNpc);
        return profile;
    }
}

