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

public class GetBadgesForAccountByEmailQueryHandlerTests
{
    private readonly Mock<IMessageBus> _messageBus;
    private readonly Mock<ILogger> _logger;

    public GetBadgesForAccountByEmailQueryHandlerTests()
    {
        _messageBus = new Mock<IMessageBus>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithAccountAndBadges_ReturnsCombinedBadges()
    {
        var account = new Account { Id = "acc-1", Email = "test@example.com" };
        var profiles = new List<UserProfile>
        {
            new() { Id = "profile-1", Name = "Child 1" },
            new() { Id = "profile-2", Name = "Child 2" }
        };
        var badges1 = new List<UserBadge> { new() { Id = "b1", UserProfileId = "profile-1", BadgeName = "Brave" } };
        var badges2 = new List<UserBadge> { new() { Id = "b2", UserProfileId = "profile-2", BadgeName = "Kind" } };

        _messageBus.Setup(m => m.InvokeAsync<Account?>(It.IsAny<GetAccountByEmailQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(account);
        _messageBus.Setup(m => m.InvokeAsync<List<UserProfile>>(It.IsAny<GetProfilesByAccountQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(profiles);
        _messageBus.SetupSequence(m => m.InvokeAsync<List<UserBadge>>(It.IsAny<GetUserBadgesQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(badges1)
            .ReturnsAsync(badges2);

        var result = await GetBadgesForAccountByEmailQueryHandler.Handle(
            new GetBadgesForAccountByEmailQuery("test@example.com"),
            _messageBus.Object, _logger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(b => b.BadgeName == "Brave");
        result.Should().Contain(b => b.BadgeName == "Kind");
    }

    [Fact]
    public async Task Handle_WhenAccountNotFound_ReturnsEmptyList()
    {
        _messageBus.Setup(m => m.InvokeAsync<Account?>(It.IsAny<GetAccountByEmailQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(default(Account));

        var result = await GetBadgesForAccountByEmailQueryHandler.Handle(
            new GetBadgesForAccountByEmailQuery("unknown@example.com"),
            _messageBus.Object, _logger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenNoProfiles_ReturnsEmptyList()
    {
        var account = new Account { Id = "acc-1", Email = "test@example.com" };
        _messageBus.Setup(m => m.InvokeAsync<Account?>(It.IsAny<GetAccountByEmailQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(account);
        _messageBus.Setup(m => m.InvokeAsync<List<UserProfile>>(It.IsAny<GetProfilesByAccountQuery>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(new List<UserProfile>());

        var result = await GetBadgesForAccountByEmailQueryHandler.Handle(
            new GetBadgesForAccountByEmailQuery("test@example.com"),
            _messageBus.Object, _logger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
