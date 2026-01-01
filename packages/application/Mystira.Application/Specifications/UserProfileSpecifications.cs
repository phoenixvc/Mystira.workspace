using Ardalis.Specification;
using Mystira.Domain.Models;

namespace Mystira.Application.Specifications;

/// <summary>
/// Specification to get a user profile by ID.
/// </summary>
public sealed class UserProfileByIdSpec : SingleEntitySpecification<UserProfile>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileByIdSpec"/> class.
    /// </summary>
    /// <param name="id">The user profile identifier.</param>
    public UserProfileByIdSpec(string id)
    {
        Query.Where(p => p.Id == id);
    }
}

/// <summary>
/// Specification to filter profiles by account ID.
/// Migrated from ProfilesByAccountSpecification.
/// </summary>
public sealed class ProfilesByAccountSpec : BaseEntitySpecification<UserProfile>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilesByAccountSpec"/> class.
    /// </summary>
    /// <param name="accountId">The account identifier to filter by.</param>
    public ProfilesByAccountSpec(string accountId)
    {
        Query.Where(p => p.AccountId == accountId)
             .OrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification to filter guest profiles.
/// Migrated from GuestProfilesSpecification.
/// </summary>
public sealed class GuestProfilesSpec : BaseEntitySpecification<UserProfile>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GuestProfilesSpec"/> class.
    /// </summary>
    public GuestProfilesSpec()
    {
        Query.Where(p => p.IsGuest)
             .OrderByDescending(p => p.CreatedAt);
    }
}

/// <summary>
/// Specification to filter non-guest profiles.
/// Migrated from NonGuestProfilesSpecification.
/// </summary>
public sealed class NonGuestProfilesSpec : BaseEntitySpecification<UserProfile>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NonGuestProfilesSpec"/> class.
    /// </summary>
    public NonGuestProfilesSpec()
    {
        Query.Where(p => !p.IsGuest)
             .OrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification to filter NPC profiles.
/// Migrated from NpcProfilesSpecification.
/// </summary>
public sealed class NpcProfilesSpec : BaseEntitySpecification<UserProfile>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NpcProfilesSpec"/> class.
    /// </summary>
    public NpcProfilesSpec()
    {
        Query.Where(p => p.IsNpc)
             .OrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification to filter profiles that have completed onboarding.
/// Migrated from OnboardedProfilesSpecification.
/// </summary>
public sealed class OnboardedProfilesSpec : BaseEntitySpecification<UserProfile>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OnboardedProfilesSpec"/> class.
    /// </summary>
    public OnboardedProfilesSpec()
    {
        Query.Where(p => p.HasCompletedOnboarding)
             .OrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification to filter profiles by age group.
/// Migrated from ProfilesByAgeGroupSpecification.
/// </summary>
public sealed class ProfilesByAgeGroupSpec : BaseEntitySpecification<UserProfile>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilesByAgeGroupSpec"/> class.
    /// </summary>
    /// <param name="ageGroup">The age group to filter by.</param>
    public ProfilesByAgeGroupSpec(string ageGroup)
    {
        Query.Where(p => p.AgeGroupName == ageGroup)
             .OrderBy(p => p.Name);
    }
}

/// <summary>
/// Specification to get profiles with pagination.
/// </summary>
public sealed class ProfilesPaginatedSpec : BaseEntitySpecification<UserProfile>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilesPaginatedSpec"/> class.
    /// </summary>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <param name="accountId">Optional account identifier to filter by.</param>
    /// <param name="isGuest">Optional guest status to filter by.</param>
    public ProfilesPaginatedSpec(int skip, int take, string? accountId = null, bool? isGuest = null)
    {
        var query = Query.AsTracking();

        if (!string.IsNullOrWhiteSpace(accountId))
        {
            query = query.Where(p => p.AccountId == accountId);
        }

        if (isGuest.HasValue)
        {
            query = query.Where(p => p.IsGuest == isGuest.Value);
        }

        query.OrderBy(p => p.Name)
             .Skip(skip)
             .Take(take);
    }
}

/// <summary>
/// Specification to search profiles by name pattern.
/// </summary>
public sealed class ProfilesByNamePatternSpec : BaseEntitySpecification<UserProfile>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProfilesByNamePatternSpec"/> class.
    /// </summary>
    /// <param name="namePattern">The name pattern to search for.</param>
    public ProfilesByNamePatternSpec(string namePattern)
    {
        Query.Where(p => p.Name.ToLower().Contains(namePattern.ToLower()))
             .OrderBy(p => p.Name);
    }
}
