using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.Core.UseCases.Badges;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases;

public class RevokeBadgeUseCaseTests
{
    private readonly Mock<IUserBadgeRepository> _badgeRepository;
    private readonly Mock<IUserProfileRepository> _profileRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<RevokeBadgeUseCase>> _logger;
    private readonly RevokeBadgeUseCase _useCase;

    public RevokeBadgeUseCaseTests()
    {
        _badgeRepository = new Mock<IUserBadgeRepository>();
        _profileRepository = new Mock<IUserProfileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<RevokeBadgeUseCase>>();

        _useCase = new RevokeBadgeUseCase(
            _badgeRepository.Object, _profileRepository.Object,
            _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingBadge_RevokesAndReturnsTrue()
    {
        var badge = new UserBadge
        {
            Id = "badge-1",
            UserProfileId = "profile-1",
            BadgeName = "Courage Badge"
        };
        var profile = new UserProfile
        {
            Id = "profile-1",
            EarnedBadges = new List<UserBadge> { badge }
        };

        _badgeRepository.Setup(r => r.GetByIdAsync("badge-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);
        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _useCase.ExecuteAsync("badge-1");

        result.Should().BeTrue();
        profile.EarnedBadges.Should().NotContain(b => b.Id == "badge-1");

        _profileRepository.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
        _badgeRepository.Verify(r => r.DeleteAsync("badge-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentBadge_ReturnsFalse()
    {
        _badgeRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserBadge));

        var result = await _useCase.ExecuteAsync("missing");

        result.Should().BeFalse();
        _badgeRepository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyBadgeId_ThrowsValidationException(string? badgeId)
    {
        var act = () => _useCase.ExecuteAsync(badgeId!);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingUserProfile_StillDeletesBadge()
    {
        var badge = new UserBadge
        {
            Id = "badge-1",
            UserProfileId = "orphan-profile"
        };

        _badgeRepository.Setup(r => r.GetByIdAsync("badge-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);
        _profileRepository.Setup(r => r.GetByIdAsync("orphan-profile", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        var result = await _useCase.ExecuteAsync("badge-1");

        result.Should().BeTrue();
        _badgeRepository.Verify(r => r.DeleteAsync("badge-1", It.IsAny<CancellationToken>()), Times.Once);
        _profileRepository.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithBadgeNotInProfileList_StillDeletesBadge()
    {
        var badge = new UserBadge
        {
            Id = "badge-1",
            UserProfileId = "profile-1"
        };
        var profile = new UserProfile
        {
            Id = "profile-1",
            EarnedBadges = new List<UserBadge>() // Badge not in list
        };

        _badgeRepository.Setup(r => r.GetByIdAsync("badge-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);
        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await _useCase.ExecuteAsync("badge-1");

        result.Should().BeTrue();
        _badgeRepository.Verify(r => r.DeleteAsync("badge-1", It.IsAny<CancellationToken>()), Times.Once);
        _profileRepository.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
