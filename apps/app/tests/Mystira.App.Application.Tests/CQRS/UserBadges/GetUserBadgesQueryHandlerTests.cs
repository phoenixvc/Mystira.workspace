using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.UserBadges.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.UserBadges;

public class GetUserBadgesQueryHandlerTests
{
    private readonly Mock<IUserBadgeRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetUserBadgesQueryHandlerTests()
    {
        _repository = new Mock<IUserBadgeRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithExistingUserProfile_ReturnsBadges()
    {
        // Arrange
        var profileId = "profile-123";
        var expectedBadges = new List<UserBadge>
        {
            new UserBadge { Id = "badge-1", UserProfileId = profileId, BadgeConfigurationId = "config-1" },
            new UserBadge { Id = "badge-2", UserProfileId = profileId, BadgeConfigurationId = "config-2" }
        };

        var query = new GetUserBadgesQuery(profileId);

        _repository.Setup(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBadges);

        // Act
        var result = await GetUserBadgesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(b => b.UserProfileId.Should().Be(profileId));
    }

    [Fact]
    public async Task Handle_WithNoBadges_ReturnsEmptyList()
    {
        // Arrange
        var profileId = "profile-no-badges";
        var query = new GetUserBadgesQuery(profileId);

        _repository.Setup(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<UserBadge>());

        // Act
        var result = await GetUserBadgesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_LogsDebugWithBadgeCount()
    {
        // Arrange
        var profileId = "profile-456";
        var badges = new List<UserBadge>
        {
            new UserBadge { Id = "b1", UserProfileId = profileId },
            new UserBadge { Id = "b2", UserProfileId = profileId },
            new UserBadge { Id = "b3", UserProfileId = profileId }
        };

        var query = new GetUserBadgesQuery(profileId);

        _repository.Setup(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        // Act
        await GetUserBadgesQueryHandler.Handle(
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
    public async Task Handle_CallsRepositoryWithCorrectProfileId()
    {
        // Arrange
        var profileId = "specific-profile";
        var query = new GetUserBadgesQuery(profileId);

        _repository.Setup(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserBadge>());

        // Act
        await GetUserBadgesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCompleteBadge_ReturnsBadgesWithAllProperties()
    {
        // Arrange
        var profileId = "complete-profile";
        var now = DateTime.UtcNow;
        var badges = new List<UserBadge>
        {
            new UserBadge
            {
                Id = "badge-complete",
                UserProfileId = profileId,
                BadgeConfigurationId = "config-complete",
                EarnedAt = now,
                Axis = "courage",
                TriggerValue = 85.5f
            }
        };

        var query = new GetUserBadgesQuery(profileId);

        _repository.Setup(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        // Act
        var result = await GetUserBadgesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        var badge = result.Single();
        badge.Id.Should().Be("badge-complete");
        badge.UserProfileId.Should().Be(profileId);
        badge.BadgeConfigurationId.Should().Be("config-complete");
        badge.EarnedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        badge.Axis.Should().Be("courage");
        badge.TriggerValue.Should().Be(85.5f);
    }

    [Fact]
    public async Task Handle_WithManyBadges_ReturnsAllBadges()
    {
        // Arrange
        var profileId = "profile-many-badges";
        var badges = Enumerable.Range(1, 50)
            .Select(i => new UserBadge
            {
                Id = $"badge-{i}",
                UserProfileId = profileId,
                Axis = $"axis-{i % 5}"
            })
            .ToList();

        var query = new GetUserBadgesQuery(profileId);

        _repository.Setup(r => r.GetByUserProfileIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        // Act
        var result = await GetUserBadgesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(50);
    }
}
