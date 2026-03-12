using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Contracts.App.Requests.UserProfiles;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.Core.UseCases.UserProfiles;

/// <summary>
/// Use case for updating an existing user profile
/// </summary>
public class UpdateUserProfileUseCase
{
    private readonly IUserProfileRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateUserProfileUseCase> _logger;

    public UpdateUserProfileUseCase(
        IUserProfileRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateUserProfileUseCase> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UserProfile?> ExecuteAsync(string id, UpdateUserProfileRequest request, CancellationToken ct = default)
    {
        var profile = await _repository.GetByIdAsync(id, ct);
        if (profile == null)
        {
            return null;
        }

        // Apply updates
        if (request.PreferredFantasyThemes != null)
        {
            // Validate fantasy themes
            var invalidThemes = request.PreferredFantasyThemes.Where(t => FantasyTheme.FromValue(t) == null).ToList();
            if (invalidThemes.Any())
            {
                throw new ValidationException("preferredFantasyThemes", $"Invalid fantasy themes: {string.Join(", ", invalidThemes)}");
            }

            profile.PreferredFantasyThemes = request.PreferredFantasyThemes;
        }

        if (request.AgeGroup != null)
        {
            // Validate age group
            if (!AgeGroupConstants.GetAll().Contains(request.AgeGroup))
            {
                throw new ValidationException("ageGroup", $"Invalid age group: {request.AgeGroup}. Must be one of: {string.Join(", ", AgeGroupConstants.GetAll())}");
            }

            profile.AgeGroupId = request.AgeGroup;
        }

        if (request.DateOfBirth.HasValue)
        {
            profile.DateOfBirth = DateOnly.FromDateTime(request.DateOfBirth.Value);
            // Update age group automatically if date of birth is provided
            profile.UpdateAgeGroupFromBirthDate();
        }

        if (request.HasCompletedOnboarding.HasValue)
        {
            profile.HasCompletedOnboarding = request.HasCompletedOnboarding.Value;
        }

        if (request.IsGuest.HasValue)
        {
            profile.IsGuest = request.IsGuest.Value;
        }

        if (request.IsNpc.HasValue)
        {
            profile.IsNpc = request.IsNpc.Value;
        }

        if (request.AccountId != null)
        {
            profile.AccountId = request.AccountId;
        }

        if (request.Pronouns != null)
        {
            profile.Pronouns = request.Pronouns;
        }

        if (request.Bio != null)
        {
            profile.Bio = request.Bio;
        }

        profile.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(profile, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Updated user profile: {ProfileId} - {Name}", PiiMask.HashId(profile.Id), PiiMask.HashId(profile.Name));
        return profile;
    }
}

