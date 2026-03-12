using FluentAssertions;
using Moq;
using Mystira.App.Application.CQRS.Badges.Queries;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Badges;

public class GetProfileBadgeProgressQueryHandlerTests
{
    private readonly Mock<IBadgeRepository> _badgeRepository;
    private readonly Mock<ICompassAxisRepository> _axisRepository;
    private readonly Mock<IUserBadgeRepository> _userBadgeRepository;
    private readonly Mock<IUserProfileRepository> _profileRepository;

    public GetProfileBadgeProgressQueryHandlerTests()
    {
        _badgeRepository = new Mock<IBadgeRepository>();
        _axisRepository = new Mock<ICompassAxisRepository>();
        _userBadgeRepository = new Mock<IUserBadgeRepository>();
        _profileRepository = new Mock<IUserProfileRepository>();
    }

    [Fact]
    public async Task Handle_WithValidProfile_ReturnsProgress()
    {
        // Arrange
        var profileId = "profile-123";
        var profile = new UserProfile
        {
            Id = profileId,
            AgeGroupId = "6-9"
        };

        var badges = new List<Badge>
        {
            new Badge
            {
                Id = "badge-1",
                CompassAxisId = "courage",
                Tier = "bronze",
                TierOrder = 1,
                Title = "Courage Bronze",
                RequiredScore = 10,
                AgeGroupId = "6-9"
            }
        };

        var axes = new List<CompassAxisDefinition>
        {
            new CompassAxisDefinition { Id = "courage", Name = "Courage" }
        };

        var query = new GetProfileBadgeProgressQuery(profileId);

        _profileRepository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _badgeRepository.Setup(r => r.GetByAgeGroupAsync("6-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);
        _axisRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(axes);
        _userBadgeRepository.Setup(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserBadge>());

        // Act
        var result = await GetProfileBadgeProgressQueryHandler.Handle(
            query,
            _badgeRepository.Object,
            _axisRepository.Object,
            _userBadgeRepository.Object,
            _profileRepository.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.AgeGroupId.Should().Be("6-9");
        result.AxisProgresses.Should().HaveCount(1);
        result.AxisProgresses[0].CompassAxisId.Should().Be("courage");
    }

    [Fact]
    public async Task Handle_WithNonExistingProfile_ReturnsNull()
    {
        // Arrange
        var query = new GetProfileBadgeProgressQuery("non-existent");

        _profileRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        // Act
        var result = await GetProfileBadgeProgressQueryHandler.Handle(
            query,
            _badgeRepository.Object,
            _axisRepository.Object,
            _userBadgeRepository.Object,
            _profileRepository.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithEarnedBadges_MarksThemAsEarned()
    {
        // Arrange
        var profileId = "profile-123";
        var profile = new UserProfile
        {
            Id = profileId,
            AgeGroupId = "6-9"
        };

        var badges = new List<Badge>
        {
            new Badge
            {
                Id = "badge-1",
                CompassAxisId = "courage",
                Tier = "bronze",
                TierOrder = 1,
                Title = "Courage Bronze",
                RequiredScore = 10,
                AgeGroupId = "6-9"
            },
            new Badge
            {
                Id = "badge-2",
                CompassAxisId = "courage",
                Tier = "silver",
                TierOrder = 2,
                Title = "Courage Silver",
                RequiredScore = 25,
                AgeGroupId = "6-9"
            }
        };

        var userBadges = new List<UserBadge>
        {
            new UserBadge
            {
                Id = "ub-1",
                UserProfileId = profileId,
                Axis = "courage",
                TriggerValue = 15,
                Threshold = 10,
                EarnedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        var axes = new List<CompassAxisDefinition>
        {
            new CompassAxisDefinition { Id = "courage", Name = "Courage" }
        };

        var query = new GetProfileBadgeProgressQuery(profileId);

        _profileRepository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _badgeRepository.Setup(r => r.GetByAgeGroupAsync("6-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);
        _axisRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(axes);
        _userBadgeRepository.Setup(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userBadges);

        // Act
        var result = await GetProfileBadgeProgressQueryHandler.Handle(
            query,
            _badgeRepository.Object,
            _axisRepository.Object,
            _userBadgeRepository.Object,
            _profileRepository.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var axisProgress = result!.AxisProgresses.First();
        axisProgress.Tiers.Should().HaveCount(2);

        // Bronze badge should be earned (score 15 >= required 10)
        axisProgress.Tiers[0].IsEarned.Should().BeTrue();
        axisProgress.Tiers[0].EarnedAt.Should().NotBeNull();

        // Silver badge should not be earned (score 15 < required 25)
        axisProgress.Tiers[1].IsEarned.Should().BeFalse();
        axisProgress.Tiers[1].EarnedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CalculatesCurrentScore_FromUserBadges()
    {
        // Arrange
        var profileId = "profile-123";
        var profile = new UserProfile
        {
            Id = profileId,
            AgeGroupId = "6-9"
        };

        var badges = new List<Badge>
        {
            new Badge
            {
                Id = "badge-1",
                CompassAxisId = "courage",
                Tier = "bronze",
                TierOrder = 1,
                RequiredScore = 10,
                AgeGroupId = "6-9"
            }
        };

        var userBadges = new List<UserBadge>
        {
            new UserBadge
            {
                UserProfileId = profileId,
                Axis = "courage",
                TriggerValue = 25,
                Threshold = 10,
                EarnedAt = DateTime.UtcNow
            }
        };

        var axes = new List<CompassAxisDefinition>
        {
            new CompassAxisDefinition { Id = "courage", Name = "Courage" }
        };

        var query = new GetProfileBadgeProgressQuery(profileId);

        _profileRepository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _badgeRepository.Setup(r => r.GetByAgeGroupAsync("6-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);
        _axisRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(axes);
        _userBadgeRepository.Setup(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userBadges);

        // Act
        var result = await GetProfileBadgeProgressQueryHandler.Handle(
            query,
            _badgeRepository.Object,
            _axisRepository.Object,
            _userBadgeRepository.Object,
            _profileRepository.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var axisProgress = result!.AxisProgresses.First();
        axisProgress.CurrentScore.Should().Be(25); // Max of TriggerValue (25) and Threshold (10)
    }

    [Fact]
    public async Task Handle_WithNoBadges_ReturnsEmptyAxisProgresses()
    {
        // Arrange
        var profileId = "profile-123";
        var profile = new UserProfile
        {
            Id = profileId,
            AgeGroupId = "6-9"
        };

        var query = new GetProfileBadgeProgressQuery(profileId);

        _profileRepository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _badgeRepository.Setup(r => r.GetByAgeGroupAsync("6-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Badge>());
        _axisRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompassAxisDefinition>());
        _userBadgeRepository.Setup(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserBadge>());

        // Act
        var result = await GetProfileBadgeProgressQueryHandler.Handle(
            query,
            _badgeRepository.Object,
            _axisRepository.Object,
            _userBadgeRepository.Object,
            _profileRepository.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.AxisProgresses.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UsesDefaultAgeGroup_WhenProfileHasNoAgeGroup()
    {
        // Arrange
        var profileId = "profile-123";
        var profile = new UserProfile
        {
            Id = profileId,
            AgeGroupId = null! // Intentionally null to test default age group fallback
        };

        var query = new GetProfileBadgeProgressQuery(profileId);

        _profileRepository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _badgeRepository.Setup(r => r.GetByAgeGroupAsync("middle_childhood", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Badge>());
        _axisRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompassAxisDefinition>());
        _userBadgeRepository.Setup(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserBadge>());

        // Act
        var result = await GetProfileBadgeProgressQueryHandler.Handle(
            query,
            _badgeRepository.Object,
            _axisRepository.Object,
            _userBadgeRepository.Object,
            _profileRepository.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.AgeGroupId.Should().Be("middle_childhood"); // Default age group
        _badgeRepository.Verify(r => r.GetByAgeGroupAsync("middle_childhood", It.IsAny<CancellationToken>()), Times.Once);
    }
}
