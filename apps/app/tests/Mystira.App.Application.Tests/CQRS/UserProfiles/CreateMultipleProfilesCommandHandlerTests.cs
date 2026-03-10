using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.UserProfiles.Commands;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.UserProfiles;
using Wolverine;

namespace Mystira.App.Application.Tests.CQRS.UserProfiles;

public class CreateMultipleProfilesCommandHandlerTests
{
    private readonly Mock<IMessageBus> _messageBus;
    private readonly Mock<ILogger> _logger;

    public CreateMultipleProfilesCommandHandlerTests()
    {
        _messageBus = new Mock<IMessageBus>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithMultipleProfiles_CreatesAll()
    {
        var request = new CreateMultipleProfilesRequest
        {
            Profiles = new List<CreateUserProfileRequest>
            {
                new() { Name = "Child 1", AgeGroup = "6-9" },
                new() { Name = "Child 2", AgeGroup = "10-12" }
            }
        };
        var profile1 = new UserProfile { Id = "p1", Name = "Child 1" };
        var profile2 = new UserProfile { Id = "p2", Name = "Child 2" };

        _messageBus.SetupSequence(m => m.InvokeAsync<UserProfile>(It.IsAny<CreateUserProfileCommand>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(profile1)
            .ReturnsAsync(profile2);

        var result = await CreateMultipleProfilesCommandHandler.Handle(
            new CreateMultipleProfilesCommand(request),
            _messageBus.Object, _logger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Child 1");
        result[1].Name.Should().Be("Child 2");
    }

    [Fact]
    public async Task Handle_WithPartialFailure_ReturnsSuccessfulProfiles()
    {
        var request = new CreateMultipleProfilesRequest
        {
            Profiles = new List<CreateUserProfileRequest>
            {
                new() { Name = "Good Profile", AgeGroup = "6-9" },
                new() { Name = "Bad Profile", AgeGroup = "invalid" },
                new() { Name = "Another Good", AgeGroup = "10-12" }
            }
        };
        var profile1 = new UserProfile { Id = "p1", Name = "Good Profile" };
        var profile3 = new UserProfile { Id = "p3", Name = "Another Good" };

        _messageBus.SetupSequence(m => m.InvokeAsync<UserProfile>(It.IsAny<CreateUserProfileCommand>(), It.IsAny<CancellationToken>(), It.IsAny<TimeSpan?>()))
            .ReturnsAsync(profile1)
            .ThrowsAsync(new ArgumentException("Invalid age group"))
            .ReturnsAsync(profile3);

        var result = await CreateMultipleProfilesCommandHandler.Handle(
            new CreateMultipleProfilesCommand(request),
            _messageBus.Object, _logger.Object, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Good Profile");
        result.Should().Contain(p => p.Name == "Another Good");
    }

    [Fact]
    public async Task Handle_WithEmptyProfilesList_ReturnsEmptyList()
    {
        var request = new CreateMultipleProfilesRequest
        {
            Profiles = new List<CreateUserProfileRequest>()
        };

        var result = await CreateMultipleProfilesCommandHandler.Handle(
            new CreateMultipleProfilesCommand(request),
            _messageBus.Object, _logger.Object, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
