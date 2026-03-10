using FluentAssertions;
using Moq;
using Mystira.App.Application.CQRS.Badges.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Badges;

public class GetBadgesByAgeGroupQueryHandlerTests
{
    private readonly Mock<IBadgeRepository> _repository;

    public GetBadgesByAgeGroupQueryHandlerTests()
    {
        _repository = new Mock<IBadgeRepository>();
    }

    [Fact]
    public async Task Handle_WithExistingAgeGroup_ReturnsBadges()
    {
        // Arrange
        var ageGroupId = "6-9";
        var expectedBadges = new List<Badge>
        {
            new Badge
            {
                Id = "badge-1",
                AgeGroupId = ageGroupId,
                CompassAxisId = "kindness",
                Tier = "Bronze",
                TierOrder = 1,
                Title = "Kind Explorer",
                Description = "First step in kindness",
                RequiredScore = 10,
                ImageId = "badge-kindness-1"
            },
            new Badge
            {
                Id = "badge-2",
                AgeGroupId = ageGroupId,
                CompassAxisId = "kindness",
                Tier = "Silver",
                TierOrder = 2,
                Title = "Kind Adventurer",
                Description = "Growing in kindness",
                RequiredScore = 25,
                ImageId = "badge-kindness-2"
            }
        };

        var query = new GetBadgesByAgeGroupQuery(ageGroupId);

        _repository.Setup(r => r.GetByAgeGroupAsync(ageGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBadges);

        // Act
        var result = await GetBadgesByAgeGroupQueryHandler.Handle(
            query,
            _repository.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(b => b.AgeGroupId.Should().Be(ageGroupId));
    }

    [Fact]
    public async Task Handle_WithNoBadges_ReturnsEmptyList()
    {
        // Arrange
        var ageGroupId = "3-5";
        var query = new GetBadgesByAgeGroupQuery(ageGroupId);

        _repository.Setup(r => r.GetByAgeGroupAsync(ageGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Badge>());

        // Act
        var result = await GetBadgesByAgeGroupQueryHandler.Handle(
            query,
            _repository.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_OrdersBadgesByCompassAxisThenTierOrder()
    {
        // Arrange
        var ageGroupId = "6-9";
        var unsortedBadges = new List<Badge>
        {
            new Badge { Id = "b3", CompassAxisId = "bravery", TierOrder = 1 },
            new Badge { Id = "b1", CompassAxisId = "kindness", TierOrder = 2 },
            new Badge { Id = "b2", CompassAxisId = "kindness", TierOrder = 1 },
            new Badge { Id = "b4", CompassAxisId = "bravery", TierOrder = 2 }
        };

        var query = new GetBadgesByAgeGroupQuery(ageGroupId);

        _repository.Setup(r => r.GetByAgeGroupAsync(ageGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(unsortedBadges);

        // Act
        var result = await GetBadgesByAgeGroupQueryHandler.Handle(
            query,
            _repository.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(4);
        // Ordered by CompassAxisId then TierOrder
        result[0].CompassAxisId.Should().Be("bravery");
        result[0].TierOrder.Should().Be(1);
        result[1].CompassAxisId.Should().Be("bravery");
        result[1].TierOrder.Should().Be(2);
        result[2].CompassAxisId.Should().Be("kindness");
        result[2].TierOrder.Should().Be(1);
        result[3].CompassAxisId.Should().Be("kindness");
        result[3].TierOrder.Should().Be(2);
    }

    [Fact]
    public async Task Handle_MapsBadgePropertiesToResponse()
    {
        // Arrange
        var ageGroupId = "10-12";
        var badge = new Badge
        {
            Id = "badge-complete",
            AgeGroupId = ageGroupId,
            CompassAxisId = "honesty",
            Tier = "Gold",
            TierOrder = 3,
            Title = "Truth Seeker",
            Description = "Master of honesty",
            RequiredScore = 50,
            ImageId = "badge-honesty-gold"
        };

        var query = new GetBadgesByAgeGroupQuery(ageGroupId);

        _repository.Setup(r => r.GetByAgeGroupAsync(ageGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { badge });

        // Act
        var result = await GetBadgesByAgeGroupQueryHandler.Handle(
            query,
            _repository.Object,
            CancellationToken.None);

        // Assert
        var response = result.Single();
        response.Id.Should().Be("badge-complete");
        response.AgeGroupId.Should().Be(ageGroupId);
        response.CompassAxisId.Should().Be("honesty");
        response.Tier.Should().Be("Gold");
        response.TierOrder.Should().Be(3);
        response.Title.Should().Be("Truth Seeker");
        response.Description.Should().Be("Master of honesty");
        response.RequiredScore.Should().Be(50);
        response.ImageId.Should().Be("badge-honesty-gold");
    }

    [Fact]
    public async Task Handle_CallsRepositoryWithCorrectAgeGroupId()
    {
        // Arrange
        var ageGroupId = "specific-age-group";
        var query = new GetBadgesByAgeGroupQuery(ageGroupId);

        _repository.Setup(r => r.GetByAgeGroupAsync(ageGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Badge>());

        // Act
        await GetBadgesByAgeGroupQueryHandler.Handle(
            query,
            _repository.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.GetByAgeGroupAsync(ageGroupId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("3-5")]
    [InlineData("6-9")]
    [InlineData("10-12")]
    public async Task Handle_WorksWithDifferentAgeGroups(string ageGroupId)
    {
        // Arrange
        var badge = new Badge { Id = $"badge-{ageGroupId}", AgeGroupId = ageGroupId };
        var query = new GetBadgesByAgeGroupQuery(ageGroupId);

        _repository.Setup(r => r.GetByAgeGroupAsync(ageGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { badge });

        // Act
        var result = await GetBadgesByAgeGroupQueryHandler.Handle(
            query,
            _repository.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].AgeGroupId.Should().Be(ageGroupId);
    }
}
