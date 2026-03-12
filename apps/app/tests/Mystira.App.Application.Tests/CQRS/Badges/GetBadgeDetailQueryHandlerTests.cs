using FluentAssertions;
using Moq;
using Mystira.Core.CQRS.Badges.Queries;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Badges;

public class GetBadgeDetailQueryHandlerTests
{
    private readonly Mock<IBadgeRepository> _repository;

    public GetBadgeDetailQueryHandlerTests()
    {
        _repository = new Mock<IBadgeRepository>();
    }

    [Fact]
    public async Task Handle_WithExistingBadge_ReturnsBadgeResponse()
    {
        var badge = new Badge
        {
            Id = "badge-1",
            AgeGroupId = "6-9",
            CompassAxisId = "courage",
            Tier = "Bronze",
            TierOrder = 1,
            Title = "Brave Explorer",
            Description = "First courageous choice",
            RequiredScore = 10,
            ImageId = "img-1"
        };
        _repository.Setup(r => r.GetByIdAsync("badge-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);

        var result = await GetBadgeDetailQueryHandler.Handle(
            new GetBadgeDetailQuery("badge-1"), _repository.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Brave Explorer");
        result.Tier.Should().Be("Bronze");
        result.RequiredScore.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithNonExistingBadge_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Badge));

        var result = await GetBadgeDetailQueryHandler.Handle(
            new GetBadgeDetailQuery("missing"), _repository.Object, CancellationToken.None);

        result.Should().BeNull();
    }
}
