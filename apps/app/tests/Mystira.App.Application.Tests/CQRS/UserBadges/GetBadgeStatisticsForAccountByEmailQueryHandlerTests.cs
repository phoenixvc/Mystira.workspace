using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Accounts.Queries;
using Mystira.App.Application.CQRS.UserBadges.Queries;
using Mystira.App.Application.CQRS.UserProfiles.Queries;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Wolverine;

namespace Mystira.App.Application.Tests.CQRS.UserBadges;

public class GetBadgeStatisticsForAccountByEmailQueryHandlerTests
{
    private readonly Mock<IMessageBus> _messageBus;
    private readonly Mock<ILogger> _logger;

    public GetBadgeStatisticsForAccountByEmailQueryHandlerTests()
    {
        _messageBus = new Mock<IMessageBus>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithAccountAndProfiles_ReturnsAggregatedStatistics()
    {
        var account = new Account { Id = "acc-1", Email = "test@example.com" };
        var profiles = new List<UserProfile>
        {
            new() { Id = "profile-1", Name = "Child 1" },
            new() { Id = "profile-2", Name = "Child 2" }
        };
        var stats1 = new Dictionary<string, int> { ["courage"] = 2, ["honesty"] = 1 };
        var stats2 = new Dictionary<string, int> { ["courage"] = 1, ["kindness"] = 3 };

        _messageBus.Setup(m => m.InvokeAsync<Account?>(It.IsAny<GetAccountByEmailQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(account);
        _messageBus.Setup(m => m.InvokeAsync<List<UserProfile>>(It.IsAny<GetProfilesByAccountQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(profiles);
        _messageBus.SetupSequence(m => m.InvokeAsync<Dictionary<string, int>>(It.IsAny<GetBadgeStatisticsQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(stats1)
            .ReturnsAsync(stats2);

        var result = await GetBadgeStatisticsForAccountByEmailQueryHandler.Handle(
            new GetBadgeStatisticsForAccountByEmailQuery("test@example.com"),
            _messageBus.Object, _logger.Object, CancellationToken.None);

        result["courage"].Should().Be(3);
        result["honesty"].Should().Be(1);
        result["kindness"].Should().Be(3);
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ReturnsEmptyDictionary()
    {
        _messageBus.Setup(m => m.InvokeAsync<Account?>(It.IsAny<GetAccountByEmailQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(default(Account));

        var result = await GetBadgeStatisticsForAccountByEmailQueryHandler.Handle(
            new GetBadgeStatisticsForAccountByEmailQuery("unknown@example.com"),
            _messageBus.Object, _logger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNoProfiles_ReturnsEmptyDictionary()
    {
        var account = new Account { Id = "acc-1", Email = "test@example.com" };
        _messageBus.Setup(m => m.InvokeAsync<Account?>(It.IsAny<GetAccountByEmailQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(account);
        _messageBus.Setup(m => m.InvokeAsync<List<UserProfile>>(It.IsAny<GetProfilesByAccountQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(new List<UserProfile>());

        var result = await GetBadgeStatisticsForAccountByEmailQueryHandler.Handle(
            new GetBadgeStatisticsForAccountByEmailQuery("test@example.com"),
            _messageBus.Object, _logger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
