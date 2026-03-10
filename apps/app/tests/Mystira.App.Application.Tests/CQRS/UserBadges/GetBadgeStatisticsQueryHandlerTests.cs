using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.UserBadges.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.UserBadges;

public class GetBadgeStatisticsQueryHandlerTests
{
    private readonly Mock<IUserBadgeRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetBadgeStatisticsQueryHandlerTests()
    {
        _repository = new Mock<IUserBadgeRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithBadgesOnMultipleAxes_ReturnsGroupedCounts()
    {
        var badges = new List<UserBadge>
        {
            new() { Id = "b1", UserProfileId = "profile-1", Axis = "courage", BadgeName = "Brave" },
            new() { Id = "b2", UserProfileId = "profile-1", Axis = "courage", BadgeName = "Bold" },
            new() { Id = "b3", UserProfileId = "profile-1", Axis = "honesty", BadgeName = "Truthful" }
        };
        _repository.Setup(r => r.GetByUserProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        var result = await GetBadgeStatisticsQueryHandler.Handle(
            new GetBadgeStatisticsQuery("profile-1"),
            _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
        result["courage"].Should().Be(2);
        result["honesty"].Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNoBadges_ReturnsEmptyDictionary()
    {
        _repository.Setup(r => r.GetByUserProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserBadge>());

        var result = await GetBadgeStatisticsQueryHandler.Handle(
            new GetBadgeStatisticsQuery("profile-1"),
            _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_FiltersOutBadgesWithEmptyAxis()
    {
        var badges = new List<UserBadge>
        {
            new() { Id = "b1", UserProfileId = "profile-1", Axis = "courage", BadgeName = "Brave" },
            new() { Id = "b2", UserProfileId = "profile-1", Axis = "", BadgeName = "NoAxis" },
            new() { Id = "b3", UserProfileId = "profile-1", Axis = null!, BadgeName = "NullAxis" }
        };
        _repository.Setup(r => r.GetByUserProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        var result = await GetBadgeStatisticsQueryHandler.Handle(
            new GetBadgeStatisticsQuery("profile-1"),
            _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().HaveCount(1);
        result["courage"].Should().Be(1);
    }
}
