using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.UserProfiles.Queries;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.UserProfiles;

public class GetProfilesByAccountQueryHandlerTests
{
    private readonly Mock<IUserProfileRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetProfilesByAccountQueryHandlerTests()
    {
        _repository = new Mock<IUserProfileRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithValidAccountId_ReturnsProfiles()
    {
        // Arrange
        var accountId = "account-123";
        var profiles = new List<UserProfile>
        {
            new UserProfile
            {
                Id = "profile-1",
                AccountId = accountId,
                Name = "Child One",
                AgeGroupId = "middle_childhood"
            },
            new UserProfile
            {
                Id = "profile-2",
                AccountId = accountId,
                Name = "Child Two",
                AgeGroupId = "preteen"
            }
        };

        var query = new GetProfilesByAccountQuery(accountId);

        _repository.Setup(r => r.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profiles);

        // Act
        var result = await GetProfilesByAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result[0].Id.Should().Be("profile-1");
        result[0].Name.Should().Be("Child One");
        result[1].Id.Should().Be("profile-2");
        result[1].Name.Should().Be("Child Two");
    }

    [Fact]
    public async Task Handle_WithEmptyAccountId_ThrowsValidationException()
    {
        // Arrange
        var query = new GetProfilesByAccountQuery("");

        // Act
        var act = async () => await GetProfilesByAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*AccountId is required*");
    }

    [Fact]
    public async Task Handle_WithNullAccountId_ThrowsValidationException()
    {
        // Arrange
        var query = new GetProfilesByAccountQuery(null!);

        // Act
        var act = async () => await GetProfilesByAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WithNoProfiles_ReturnsEmptyList()
    {
        // Arrange
        var accountId = "account-no-profiles";
        var query = new GetProfilesByAccountQuery(accountId);

        _repository.Setup(r => r.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserProfile>());

        // Act
        var result = await GetProfilesByAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReturnsProfilesWithAllProperties()
    {
        // Arrange
        var accountId = "account-123";
        var createdAt = DateTime.UtcNow.AddDays(-30);
        var profiles = new List<UserProfile>
        {
            new UserProfile
            {
                Id = "profile-1",
                AccountId = accountId,
                Name = "Test Child",
                AgeGroupId = "middle_childhood",
                AvatarMediaId = "avatar-001",
                IsGuest = false,
                HasCompletedOnboarding = true,
                CreatedAt = createdAt
            }
        };

        var query = new GetProfilesByAccountQuery(accountId);

        _repository.Setup(r => r.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profiles);

        // Act
        var result = await GetProfilesByAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        var profile = result[0];
        profile.Id.Should().Be("profile-1");
        profile.AccountId.Should().Be(accountId);
        profile.Name.Should().Be("Test Child");
        profile.AgeGroupId.Should().Be("middle_childhood");
        profile.AvatarMediaId.Should().Be("avatar-001");
        profile.IsGuest.Should().BeFalse();
        profile.HasCompletedOnboarding.Should().BeTrue();
        profile.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public async Task Handle_WithGuestProfiles_IncludesThemInResults()
    {
        // Arrange
        var accountId = "account-123";
        var profiles = new List<UserProfile>
        {
            new UserProfile
            {
                Id = "profile-1",
                AccountId = accountId,
                Name = "Regular Child",
                IsGuest = false
            },
            new UserProfile
            {
                Id = "profile-guest",
                AccountId = accountId,
                Name = "Guest Player",
                IsGuest = true
            }
        };

        var query = new GetProfilesByAccountQuery(accountId);

        _repository.Setup(r => r.GetByAccountIdAsync(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profiles);

        // Act
        var result = await GetProfilesByAccountQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => !p.IsGuest);
        result.Should().Contain(p => p.IsGuest);
    }
}
