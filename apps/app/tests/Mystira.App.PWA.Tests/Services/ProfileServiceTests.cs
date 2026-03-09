using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.PWA.Models;
using Mystira.App.PWA.Services;
using Xunit;

namespace Mystira.App.PWA.Tests.Services;

public class ProfileServiceTests
{
    private readonly Mock<IApiClient> _apiClient;
    private readonly Mock<ILogger<ProfileService>> _logger;
    private readonly ProfileService _service;

    public ProfileServiceTests()
    {
        _apiClient = new Mock<IApiClient>();
        _logger = new Mock<ILogger<ProfileService>>();
        _service = new ProfileService(_apiClient.Object, _logger.Object);
    }

    #region GetUserProfilesAsync Tests

    [Fact]
    public async Task GetUserProfilesAsync_WithValidAccountId_ReturnsProfiles()
    {
        // Arrange
        var accountId = "account-123";
        var expectedProfiles = new List<UserProfile>
        {
            new UserProfile { Id = "profile-1", Name = "Player One" },
            new UserProfile { Id = "profile-2", Name = "Player Two" }
        };

        _apiClient.Setup(c => c.GetProfilesByAccountAsync(accountId))
            .ReturnsAsync(expectedProfiles);

        // Act
        var result = await _service.GetUserProfilesAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Player One");
    }

    [Fact]
    public async Task GetUserProfilesAsync_WhenApiReturnsNull_ReturnsEmptyList()
    {
        // Arrange
        var accountId = "account-no-profiles";

        _apiClient.Setup(c => c.GetProfilesByAccountAsync(accountId))
            .Returns(Task.FromResult<List<UserProfile>?>(null));

        // Act
        var result = await _service.GetUserProfilesAsync(accountId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserProfilesAsync_WhenApiThrows_ReturnsNullAndLogsError()
    {
        // Arrange
        var accountId = "account-error";

        _apiClient.Setup(c => c.GetProfilesByAccountAsync(accountId))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.GetUserProfilesAsync(accountId);

        // Assert
        result.Should().BeNull();
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion

    #region HasProfilesAsync Tests

    [Fact]
    public async Task HasProfilesAsync_WithProfiles_ReturnsTrue()
    {
        // Arrange
        var accountId = "account-with-profiles";
        var profiles = new List<UserProfile> { new UserProfile { Id = "p1" } };

        _apiClient.Setup(c => c.GetProfilesByAccountAsync(accountId))
            .ReturnsAsync(profiles);

        // Act
        var result = await _service.HasProfilesAsync(accountId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasProfilesAsync_WithNoProfiles_ReturnsFalse()
    {
        // Arrange
        var accountId = "account-no-profiles";

        _apiClient.Setup(c => c.GetProfilesByAccountAsync(accountId))
            .ReturnsAsync(new List<UserProfile>());

        // Act
        var result = await _service.HasProfilesAsync(accountId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasProfilesAsync_WhenApiReturnsNull_ReturnsFalse()
    {
        // Arrange
        var accountId = "account-null";

        _apiClient.Setup(c => c.GetProfilesByAccountAsync(accountId))
            .Returns(Task.FromResult<List<UserProfile>?>(null));

        // Act
        var result = await _service.HasProfilesAsync(accountId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasProfilesAsync_WhenApiThrows_ReturnsFalseAndLogsError()
    {
        // Arrange
        var accountId = "account-error";

        _apiClient.Setup(c => c.GetProfilesByAccountAsync(accountId))
            .ThrowsAsync(new Exception("Error"));

        // Act
        var result = await _service.HasProfilesAsync(accountId);

        // Assert
        result.Should().BeFalse();
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion

    #region CreateProfileAsync Tests

    [Fact]
    public async Task CreateProfileAsync_WithValidRequest_ReturnsCreatedProfile()
    {
        // Arrange
        var request = new CreateUserProfileRequest
        {
            AccountId = "account-1",
            Name = "New Player",
            AgeGroup = "6-9"
        };
        var expectedProfile = new UserProfile
        {
            Id = "new-profile-id",
            Name = "New Player",
            AgeGroup = "6-9"
        };

        _apiClient.Setup(c => c.CreateProfileAsync(request))
            .ReturnsAsync(expectedProfile);

        // Act
        var result = await _service.CreateProfileAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("new-profile-id");
        result.Name.Should().Be("New Player");
    }

    [Fact]
    public async Task CreateProfileAsync_WhenApiThrows_ReturnsNullAndLogsError()
    {
        // Arrange
        var request = new CreateUserProfileRequest { AccountId = "acc", Name = "Test" };

        _apiClient.Setup(c => c.CreateProfileAsync(request))
            .ThrowsAsync(new HttpRequestException("Failed"));

        // Act
        var result = await _service.CreateProfileAsync(request);

        // Assert
        result.Should().BeNull();
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion

    #region GetProfileAsync Tests

    [Fact]
    public async Task GetProfileAsync_WithValidProfileId_ReturnsProfile()
    {
        // Arrange
        var profileId = "profile-123";
        var expectedProfile = new UserProfile
        {
            Id = profileId,
            Name = "Test Player",
            AgeGroup = "10-12"
        };

        _apiClient.Setup(c => c.GetProfileAsync(profileId))
            .ReturnsAsync(expectedProfile);

        // Act
        var result = await _service.GetProfileAsync(profileId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(profileId);
        result.Name.Should().Be("Test Player");
    }

    [Fact]
    public async Task GetProfileAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var profileId = "non-existent";

        _apiClient.Setup(c => c.GetProfileAsync(profileId))
            .Returns(Task.FromResult<UserProfile?>(null));

        // Act
        var result = await _service.GetProfileAsync(profileId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProfileAsync_WhenApiThrows_ReturnsNullAndLogsError()
    {
        // Arrange
        var profileId = "error-profile";

        _apiClient.Setup(c => c.GetProfileAsync(profileId))
            .ThrowsAsync(new Exception("Network failure"));

        // Act
        var result = await _service.GetProfileAsync(profileId);

        // Assert
        result.Should().BeNull();
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion

    #region DeleteProfileAsync Tests

    [Fact]
    public async Task DeleteProfileAsync_WithValidProfileId_ReturnsTrue()
    {
        // Arrange
        var profileId = "profile-to-delete";

        _apiClient.Setup(c => c.DeleteProfileAsync(profileId))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteProfileAsync(profileId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProfileAsync_WhenApiFails_ReturnsFalse()
    {
        // Arrange
        var profileId = "profile-delete-fail";

        _apiClient.Setup(c => c.DeleteProfileAsync(profileId))
            .ReturnsAsync(false);

        // Act
        var result = await _service.DeleteProfileAsync(profileId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteProfileAsync_WhenApiThrows_ReturnsFalseAndLogsError()
    {
        // Arrange
        var profileId = "profile-error";

        _apiClient.Setup(c => c.DeleteProfileAsync(profileId))
            .ThrowsAsync(new HttpRequestException("Delete failed"));

        // Act
        var result = await _service.DeleteProfileAsync(profileId);

        // Assert
        result.Should().BeFalse();
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion

    #region UpdateProfileAsync Tests

    [Fact]
    public async Task UpdateProfileAsync_WithValidRequest_ReturnsUpdatedProfile()
    {
        // Arrange
        var profileId = "profile-to-update";
        var request = new UpdateUserProfileRequest
        {
            Name = "Updated Name",
            AgeGroup = "10-12"
        };
        var expectedProfile = new UserProfile
        {
            Id = profileId,
            Name = "Updated Name",
            AgeGroup = "10-12"
        };

        _apiClient.Setup(c => c.UpdateProfileAsync(profileId, request))
            .ReturnsAsync(expectedProfile);

        // Act
        var result = await _service.UpdateProfileAsync(profileId, request);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Name");
    }

    [Fact]
    public async Task UpdateProfileAsync_WhenApiThrows_ReturnsNullAndLogsError()
    {
        // Arrange
        var profileId = "profile-error";
        var request = new UpdateUserProfileRequest { Name = "Test" };

        _apiClient.Setup(c => c.UpdateProfileAsync(profileId, request))
            .ThrowsAsync(new Exception("Update failed"));

        // Act
        var result = await _service.UpdateProfileAsync(profileId, request);

        // Assert
        result.Should().BeNull();
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion
}
