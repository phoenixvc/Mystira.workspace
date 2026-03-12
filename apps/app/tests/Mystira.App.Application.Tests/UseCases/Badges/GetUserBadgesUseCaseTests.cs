using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.Core.UseCases.Badges;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.Badges;

public class GetUserBadgesUseCaseTests
{
    private readonly Mock<IUserBadgeRepository> _repository;
    private readonly Mock<ILogger<GetUserBadgesUseCase>> _logger;
    private readonly GetUserBadgesUseCase _useCase;

    public GetUserBadgesUseCaseTests()
    {
        _repository = new Mock<IUserBadgeRepository>();
        _logger = new Mock<ILogger<GetUserBadgesUseCase>>();
        _useCase = new GetUserBadgesUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingProfile_ReturnsBadges()
    {
        var badges = new List<UserBadge>
        {
            new() { Id = "b1", UserProfileId = "profile-1", BadgeName = "Courage I" },
            new() { Id = "b2", UserProfileId = "profile-1", BadgeName = "Honesty I" }
        };
        _repository.Setup(r => r.GetByUserProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badges);

        var result = await _useCase.ExecuteAsync("profile-1");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoBadges_ReturnsEmptyList()
    {
        _repository.Setup(r => r.GetByUserProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserBadge>());

        var result = await _useCase.ExecuteAsync("profile-1");

        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyProfileId_ThrowsValidationException(string? profileId)
    {
        var act = () => _useCase.ExecuteAsync(profileId!);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
