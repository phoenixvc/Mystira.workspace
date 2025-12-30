using Mystira.App.Domain.Models;
using Mystira.Contracts.App.Requests.UserProfiles;

namespace Mystira.App.Admin.Api.Services;

public class UserProfileApiService : IUserProfileApiService
{
    private readonly IUserProfileService _userProfileService;

    public UserProfileApiService(IUserProfileService userProfileService)
    {
        _userProfileService = userProfileService;
    }

    public async Task<UserProfile> CreateProfileAsync(CreateUserProfileRequest request) => await _userProfileService.CreateProfileAsync(request);
    public async Task<UserProfile> CreateGuestProfileAsync(CreateGuestProfileRequest request) => await _userProfileService.CreateGuestProfileAsync(request);
    public async Task<List<UserProfile>> CreateMultipleProfilesAsync(CreateMultipleProfilesRequest request) => await _userProfileService.CreateMultipleProfilesAsync(request);
    public async Task<UserProfile?> GetProfileAsync(string name) => await _userProfileService.GetProfileAsync(name);
    public async Task<UserProfile?> GetProfileByIdAsync(string id) => await _userProfileService.GetProfileByIdAsync(id);
    public async Task<UserProfile?> UpdateProfileAsync(string name, UpdateUserProfileRequest request) => await _userProfileService.UpdateProfileAsync(name, request);
    public async Task<UserProfile?> UpdateProfileByIdAsync(string id, UpdateUserProfileRequest request) => await _userProfileService.UpdateProfileByIdAsync(id, request);
    public async Task<bool> DeleteProfileAsync(string name) => await _userProfileService.DeleteProfileAsync(name);
    public async Task<bool> CompleteOnboardingAsync(string name) => await _userProfileService.CompleteOnboardingAsync(name);
    public async Task<List<UserProfile>> GetAllProfilesAsync() => await _userProfileService.GetAllProfilesAsync();
    public async Task<List<UserProfile>> GetNonGuestProfilesAsync() => await _userProfileService.GetNonGuestProfilesAsync();
    public async Task<List<UserProfile>> GetGuestProfilesAsync() => await _userProfileService.GetGuestProfilesAsync();
    public async Task<bool> AssignCharacterToProfileAsync(string profileId, string characterId, bool isNpc = false) => await _userProfileService.AssignCharacterToProfileAsync(profileId, characterId, isNpc);
}
