using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.UserBadges.Queries;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.UserBadges;

public class HasUserEarnedBadgeQueryHandlerTests
{
    private readonly Mock<IUserBadgeRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public HasUserEarnedBadgeQueryHandlerTests()
    {
        _repository = new Mock<IUserBadgeRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WhenBadgeEarned_ReturnsTrue()
    {
        var badges = new List<UserBadge>
        {
            new() { Id = "b1", UserProfileId = "profile-1", BadgeConfigurationId = "badge-config-1" }
        };
        _repository.Setup(r => r.GetByUserProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        var result = await HasUserEarnedBadgeQueryHandler.Handle(
            new HasUserEarnedBadgeQuery("profile-1", "badge-config-1"),
            _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenBadgeNotEarned_ReturnsFalse()
    {
        var badges = new List<UserBadge>
        {
            new() { Id = "b1", UserProfileId = "profile-1", BadgeConfigurationId = "badge-config-1" }
        };
        _repository.Setup(r => r.GetByUserProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        var result = await HasUserEarnedBadgeQueryHandler.Handle(
            new HasUserEarnedBadgeQuery("profile-1", "badge-config-999"),
            _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenNoBadges_ReturnsFalse()
    {
        _repository.Setup(r => r.GetByUserProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserBadge>());

        var result = await HasUserEarnedBadgeQueryHandler.Handle(
            new HasUserEarnedBadgeQuery("profile-1", "badge-config-1"),
            _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
    }
}
