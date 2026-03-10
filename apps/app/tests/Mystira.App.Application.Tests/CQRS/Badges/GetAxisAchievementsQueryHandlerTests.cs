using FluentAssertions;
using Moq;
using Mystira.App.Application.CQRS.Badges.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Badges;

public class GetAxisAchievementsQueryHandlerTests
{
    private readonly Mock<IAxisAchievementRepository> _achievementRepository;
    private readonly Mock<ICompassAxisRepository> _axisRepository;

    public GetAxisAchievementsQueryHandlerTests()
    {
        _achievementRepository = new Mock<IAxisAchievementRepository>();
        _axisRepository = new Mock<ICompassAxisRepository>();
    }

    [Fact]
    public async Task Handle_WithAchievementsAndAxes_ReturnsResponsesWithAxisNames()
    {
        var achievements = new List<AxisAchievement>
        {
            new() { Id = "ach-1", AgeGroupId = "6-9", CompassAxisId = "courage", AxesDirection = "positive", Description = "Brave choice" }
        };
        var axes = new List<CompassAxisDefinition>
        {
            new CompassAxisDefinition { Id = "courage", Name = "Courage" }
        };
        _achievementRepository.Setup(r => r.GetByAgeGroupAsync("6-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);
        _axisRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(axes);

        var result = await GetAxisAchievementsQueryHandler.Handle(
            new GetAxisAchievementsQuery("6-9"), _achievementRepository.Object,
            _axisRepository.Object, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].CompassAxisName.Should().Be("Courage");
        result[0].Description.Should().Be("Brave choice");
    }

    [Fact]
    public async Task Handle_WithNoAchievements_ReturnsEmptyList()
    {
        _achievementRepository.Setup(r => r.GetByAgeGroupAsync("6-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AxisAchievement>());
        _axisRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompassAxisDefinition>());

        var result = await GetAxisAchievementsQueryHandler.Handle(
            new GetAxisAchievementsQuery("6-9"), _achievementRepository.Object,
            _axisRepository.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithUnknownAxis_FallsBackToAxisId()
    {
        var achievements = new List<AxisAchievement>
        {
            new() { Id = "ach-1", AgeGroupId = "6-9", CompassAxisId = "unknown-axis", AxesDirection = "positive", Description = "Test" }
        };
        _achievementRepository.Setup(r => r.GetByAgeGroupAsync("6-9", It.IsAny<CancellationToken>()))
            .ReturnsAsync(achievements);
        _axisRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompassAxisDefinition>());

        var result = await GetAxisAchievementsQueryHandler.Handle(
            new GetAxisAchievementsQuery("6-9"), _achievementRepository.Object,
            _axisRepository.Object, CancellationToken.None);

        result[0].CompassAxisName.Should().Be("unknown-axis");
    }
}
