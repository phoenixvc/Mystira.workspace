using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.UserProfiles.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.UserProfiles;

public class GetUserProfileQueryHandlerTests
{
    private readonly Mock<IUserProfileRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetUserProfileQueryHandlerTests()
    {
        _repository = new Mock<IUserProfileRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithExistingProfileId_ReturnsProfile()
    {
        // Arrange
        var profileId = "profile-123";
        var expectedProfile = new UserProfile
        {
            Id = profileId,
            Name = "Test User",
            AccountId = "account-1",
            AgeGroupName = "6-9"
        };

        var query = new GetUserProfileQuery(profileId);

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProfile);

        // Act
        var result = await GetUserProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(profileId);
        result.Name.Should().Be("Test User");
        result.AccountId.Should().Be("account-1");
        result.AgeGroupName.Should().Be("6-9");
    }

    [Fact]
    public async Task Handle_WithNonExistingProfileId_ReturnsNull()
    {
        // Arrange
        var profileId = "non-existent-profile";
        var query = new GetUserProfileQuery(profileId);

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        // Act
        var result = await GetUserProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenProfileNotFound_LogsDebug()
    {
        // Arrange
        var profileId = "missing-profile";
        var query = new GetUserProfileQuery(profileId);

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        // Act
        await GetUserProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProfileFound_LogsDebug()
    {
        // Arrange
        var profileId = "found-profile";
        var query = new GetUserProfileQuery(profileId);

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile { Id = profileId });

        // Act
        await GetUserProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsProfileWithAllProperties()
    {
        // Arrange
        var profileId = "complete-profile";
        var now = DateTime.UtcNow;
        var expectedProfile = new UserProfile
        {
            Id = profileId,
            Name = "Complete User",
            AccountId = "account-456",
            AgeGroupName = "10-12",
            AvatarMediaId = "avatar-1",
            IsGuest = false,
            HasCompletedOnboarding = true,
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now
        };

        var query = new GetUserProfileQuery(profileId);

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProfile);

        // Act
        var result = await GetUserProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(profileId);
        result.Name.Should().Be("Complete User");
        result.AccountId.Should().Be("account-456");
        result.AgeGroupName.Should().Be("10-12");
        result.AvatarMediaId.Should().Be("avatar-1");
        result.IsGuest.Should().BeFalse();
        result.HasCompletedOnboarding.Should().BeTrue();
        result.CreatedAt.Should().BeCloseTo(now.AddDays(-30), TimeSpan.FromSeconds(1));
        result.UpdatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectProfileId()
    {
        // Arrange
        var profileId = "specific-profile-id";
        var query = new GetUserProfileQuery(profileId);

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfile { Id = profileId });

        // Act
        await GetUserProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsGuestProfile()
    {
        // Arrange
        var profileId = "guest-profile";
        var guestProfile = new UserProfile
        {
            Id = profileId,
            Name = "Guest Player",
            IsGuest = true,
            HasCompletedOnboarding = false
        };

        var query = new GetUserProfileQuery(profileId);

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(guestProfile);

        // Act
        var result = await GetUserProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsGuest.Should().BeTrue();
        result.HasCompletedOnboarding.Should().BeFalse();
    }

    [Theory]
    [InlineData("3-5")]
    [InlineData("6-9")]
    [InlineData("10-12")]
    public async Task Handle_ReturnsProfilesWithDifferentAgeGroups(string ageGroup)
    {
        // Arrange
        var profileId = $"profile-{ageGroup}";
        var profile = new UserProfile
        {
            Id = profileId,
            AgeGroupName = ageGroup
        };

        var query = new GetUserProfileQuery(profileId);

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        // Act
        var result = await GetUserProfileQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.AgeGroupName.Should().Be(ageGroup);
    }
}
