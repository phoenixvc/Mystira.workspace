using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.UserBadges.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.UserBadges;

public class GetUserBadgesForAxisQueryHandlerTests
{
    private readonly Mock<IUserBadgeRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetUserBadgesForAxisQueryHandlerTests()
    {
        _repository = new Mock<IUserBadgeRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithMatchingAxis_ReturnsBadgesForAxis()
    {
        // Arrange
        var userProfileId = "profile-123";
        var axis = "courage";
        var badges = new List<UserBadge>
        {
            new UserBadge { Id = "badge-1", UserProfileId = userProfileId, Axis = "courage", TriggerValue = 10 },
            new UserBadge { Id = "badge-2", UserProfileId = userProfileId, Axis = "courage", TriggerValue = 25 },
            new UserBadge { Id = "badge-3", UserProfileId = userProfileId, Axis = "wisdom", TriggerValue = 15 }
        };

        var query = new GetUserBadgesForAxisQuery(userProfileId, axis);

        _repository.Setup(r => r.GetByUserProfileIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        // Act
        var result = await GetUserBadgesForAxisQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => b.Axis == "courage");
    }

    [Fact]
    public async Task Handle_WithCaseInsensitiveAxis_ReturnsBadges()
    {
        // Arrange
        var userProfileId = "profile-123";
        var axis = "COURAGE"; // Uppercase
        var badges = new List<UserBadge>
        {
            new UserBadge { Id = "badge-1", UserProfileId = userProfileId, Axis = "courage", TriggerValue = 10 },
            new UserBadge { Id = "badge-2", UserProfileId = userProfileId, Axis = "Courage", TriggerValue = 25 }
        };

        var query = new GetUserBadgesForAxisQuery(userProfileId, axis);

        _repository.Setup(r => r.GetByUserProfileIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        // Act
        var result = await GetUserBadgesForAxisQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoMatchingAxis_ReturnsEmptyList()
    {
        // Arrange
        var userProfileId = "profile-123";
        var axis = "nonexistent";
        var badges = new List<UserBadge>
        {
            new UserBadge { Id = "badge-1", UserProfileId = userProfileId, Axis = "courage", TriggerValue = 10 }
        };

        var query = new GetUserBadgesForAxisQuery(userProfileId, axis);

        _repository.Setup(r => r.GetByUserProfileIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        // Act
        var result = await GetUserBadgesForAxisQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNoBadges_ReturnsEmptyList()
    {
        // Arrange
        var userProfileId = "profile-no-badges";
        var axis = "courage";
        var query = new GetUserBadgesForAxisQuery(userProfileId, axis);

        _repository.Setup(r => r.GetByUserProfileIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserBadge>());

        // Act
        var result = await GetUserBadgesForAxisQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNullAxis_FiltersOutBadgesWithNullAxis()
    {
        // Arrange
        var userProfileId = "profile-123";
        var axis = "courage";
        var badges = new List<UserBadge>
        {
            new UserBadge { Id = "badge-1", UserProfileId = userProfileId, Axis = "courage", TriggerValue = 10 },
            new UserBadge { Id = "badge-2", UserProfileId = userProfileId, Axis = null!, TriggerValue = 5 }
        };

        var query = new GetUserBadgesForAxisQuery(userProfileId, axis);

        _repository.Setup(r => r.GetByUserProfileIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        // Act
        var result = await GetUserBadgesForAxisQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Id.Should().Be("badge-1");
    }

    [Fact]
    public async Task Handle_ReturnsBadgesWithAllProperties()
    {
        // Arrange
        var userProfileId = "profile-123";
        var axis = "courage";
        var earnedAt = DateTime.UtcNow.AddDays(-5);
        var badges = new List<UserBadge>
        {
            new UserBadge
            {
                Id = "badge-1",
                UserProfileId = userProfileId,
                Axis = "courage",
                TriggerValue = 25,
                Threshold = 20,
                EarnedAt = earnedAt,
                GameSessionId = "session-123",
                ScenarioId = "scenario-456"
            }
        };

        var query = new GetUserBadgesForAxisQuery(userProfileId, axis);

        _repository.Setup(r => r.GetByUserProfileIdAsync(userProfileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        // Act
        var result = await GetUserBadgesForAxisQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var badge = result[0];
        badge.Id.Should().Be("badge-1");
        badge.TriggerValue.Should().Be(25);
        badge.Threshold.Should().Be(20);
        badge.EarnedAt.Should().Be(earnedAt);
        badge.GameSessionId.Should().Be("session-123");
        badge.ScenarioId.Should().Be("scenario-456");
    }
}
