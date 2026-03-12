using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.UserBadges.Commands;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.Badges;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.UserBadges;

public class AwardBadgeCommandHandlerTests
{
    private readonly Mock<IUserBadgeRepository> _badgeRepository;
    private readonly Mock<IBadgeConfigurationRepository> _badgeConfigRepository;
    private readonly Mock<IUserProfileRepository> _profileRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;

    public AwardBadgeCommandHandlerTests()
    {
        _badgeRepository = new Mock<IUserBadgeRepository>();
        _badgeConfigRepository = new Mock<IBadgeConfigurationRepository>();
        _profileRepository = new Mock<IUserProfileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesUserBadge()
    {
        // Arrange
        var badgeConfig = new BadgeConfiguration
        {
            Id = "badge-config-123",
            Name = "Honesty Champion",
            Message = "You showed great honesty!",
            AxisId = "honesty",
            Threshold = 10,
            ImageId = "img-123"
        };

        var request = new AwardBadgeRequest
        {
            UserProfileId = "profile-123",
            BadgeConfigurationId = "badge-config-123",
            TriggerValue = 15,
            GameSessionId = "session-456",
            ScenarioId = "scenario-789"
        };

        _badgeConfigRepository.Setup(r => r.GetByIdAsync("badge-config-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badgeConfig);

        _badgeRepository.Setup(r => r.GetByUserProfileIdAndBadgeConfigIdAsync(
                "profile-123", "badge-config-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserBadge));

        var profile = new UserProfile
        {
            Id = "profile-123",
            AccountId = "account-1",
            Name = "Test",
            EarnedBadges = new List<UserBadge>()
        };
        _profileRepository.Setup(r => r.GetByIdAsync("profile-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var command = new AwardBadgeCommand(request);

        // Act
        var result = await AwardBadgeCommandHandler.Handle(
            command,
            _badgeRepository.Object,
            _badgeConfigRepository.Object,
            _profileRepository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserProfileId.Should().Be("profile-123");
        result.BadgeConfigurationId.Should().Be("badge-config-123");
        result.BadgeId.Should().Be("badge-config-123");
        result.BadgeName.Should().Be("Honesty Champion");
        result.BadgeMessage.Should().Be("You showed great honesty!");
        result.Axis.Should().Be("honesty");
        result.TriggerValue.Should().Be(15);
        result.Threshold.Should().Be(10);
        result.GameSessionId.Should().Be("session-456");
        result.ScenarioId.Should().Be("scenario-789");
        result.ImageId.Should().Be("img-123");
        result.EarnedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _badgeRepository.Verify(r => r.AddAsync(It.IsAny<UserBadge>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateBadge_ReturnsExistingBadge()
    {
        // Arrange
        var existingBadge = new UserBadge
        {
            Id = "existing-badge-id",
            UserProfileId = "profile-123",
            BadgeConfigurationId = "badge-config-123",
            BadgeName = "Honesty Champion",
            EarnedAt = DateTime.UtcNow.AddDays(-1)
        };

        _badgeRepository.Setup(r => r.GetByUserProfileIdAndBadgeConfigIdAsync(
                "profile-123", "badge-config-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBadge);

        var request = new AwardBadgeRequest
        {
            UserProfileId = "profile-123",
            BadgeConfigurationId = "badge-config-123"
        };

        var command = new AwardBadgeCommand(request);

        // Act
        var result = await AwardBadgeCommandHandler.Handle(
            command,
            _badgeRepository.Object,
            _badgeConfigRepository.Object,
            _profileRepository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeSameAs(existingBadge);
        _badgeRepository.Verify(r => r.AddAsync(It.IsAny<UserBadge>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdatesUserProfileEarnedBadges()
    {
        // Arrange
        var badgeConfig = new BadgeConfiguration
        {
            Id = "badge-config-123",
            Name = "Courage Badge",
            AxisId = "courage"
        };

        var profile = new UserProfile
        {
            Id = "profile-123",
            AccountId = "account-1",
            Name = "Test",
            EarnedBadges = new List<UserBadge>()
        };

        _badgeRepository.Setup(r => r.GetByUserProfileIdAndBadgeConfigIdAsync(
                "profile-123", "badge-config-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserBadge));

        _badgeConfigRepository.Setup(r => r.GetByIdAsync("badge-config-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badgeConfig);

        _profileRepository.Setup(r => r.GetByIdAsync("profile-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var request = new AwardBadgeRequest
        {
            UserProfileId = "profile-123",
            BadgeConfigurationId = "badge-config-123"
        };

        var command = new AwardBadgeCommand(request);

        // Act
        await AwardBadgeCommandHandler.Handle(
            command,
            _badgeRepository.Object,
            _badgeConfigRepository.Object,
            _profileRepository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        profile.EarnedBadges.Should().HaveCount(1);
        _profileRepository.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMissingProfile_StillCreatesBadge()
    {
        // Arrange
        var badgeConfig = new BadgeConfiguration
        {
            Id = "badge-config-123",
            Name = "Courage Badge",
            AxisId = "courage"
        };

        _badgeRepository.Setup(r => r.GetByUserProfileIdAndBadgeConfigIdAsync(
                "profile-orphan", "badge-config-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserBadge));

        _badgeConfigRepository.Setup(r => r.GetByIdAsync("badge-config-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badgeConfig);

        _profileRepository.Setup(r => r.GetByIdAsync("profile-orphan", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        var request = new AwardBadgeRequest
        {
            UserProfileId = "profile-orphan",
            BadgeConfigurationId = "badge-config-123"
        };

        var command = new AwardBadgeCommand(request);

        // Act
        var result = await AwardBadgeCommandHandler.Handle(
            command,
            _badgeRepository.Object,
            _badgeConfigRepository.Object,
            _profileRepository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.BadgeName.Should().Be("Courage Badge");
        _badgeRepository.Verify(r => r.AddAsync(It.IsAny<UserBadge>(), It.IsAny<CancellationToken>()), Times.Once);
        _profileRepository.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyUserProfileId_ThrowsValidationException()
    {
        // Arrange
        var request = new AwardBadgeRequest
        {
            UserProfileId = "",
            BadgeConfigurationId = "badge-config-123"
        };

        var command = new AwardBadgeCommand(request);

        // Act
        var act = () => AwardBadgeCommandHandler.Handle(
            command,
            _badgeRepository.Object,
            _badgeConfigRepository.Object,
            _profileRepository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*UserProfileId*required*");
    }

    [Fact]
    public async Task Handle_WithNullUserProfileId_ThrowsValidationException()
    {
        // Arrange
        var request = new AwardBadgeRequest
        {
            UserProfileId = null!,
            BadgeConfigurationId = "badge-config-123"
        };

        var command = new AwardBadgeCommand(request);

        // Act
        var act = () => AwardBadgeCommandHandler.Handle(
            command,
            _badgeRepository.Object,
            _badgeConfigRepository.Object,
            _profileRepository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*UserProfileId*required*");
    }

    [Fact]
    public async Task Handle_WithEmptyBadgeConfigurationId_ThrowsValidationException()
    {
        // Arrange
        var request = new AwardBadgeRequest
        {
            UserProfileId = "profile-123",
            BadgeConfigurationId = ""
        };

        var command = new AwardBadgeCommand(request);

        // Act
        var act = () => AwardBadgeCommandHandler.Handle(
            command,
            _badgeRepository.Object,
            _badgeConfigRepository.Object,
            _profileRepository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*BadgeConfigurationId*required*");
    }

    [Fact]
    public async Task Handle_WithNonexistentBadgeConfig_ThrowsValidationException()
    {
        // Arrange
        var request = new AwardBadgeRequest
        {
            UserProfileId = "profile-123",
            BadgeConfigurationId = "nonexistent-badge"
        };

        _badgeRepository.Setup(r => r.GetByUserProfileIdAndBadgeConfigIdAsync(
                "profile-123", "nonexistent-badge", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserBadge));

        _badgeConfigRepository.Setup(r => r.GetByIdAsync("nonexistent-badge", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(BadgeConfiguration));

        var command = new AwardBadgeCommand(request);

        // Act
        var act = () => AwardBadgeCommandHandler.Handle(
            command,
            _badgeRepository.Object,
            _badgeConfigRepository.Object,
            _profileRepository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*BadgeConfiguration*not found*");
    }

    [Fact]
    public async Task Handle_GeneratesUniqueId()
    {
        // Arrange
        var badgeConfig = new BadgeConfiguration
        {
            Id = "badge-config-456",
            Name = "Courage Badge",
            AxisId = "courage"
        };

        var request = new AwardBadgeRequest
        {
            UserProfileId = "profile-456",
            BadgeConfigurationId = "badge-config-456"
        };

        _badgeRepository.Setup(r => r.GetByUserProfileIdAndBadgeConfigIdAsync(
                "profile-456", "badge-config-456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserBadge));

        _badgeConfigRepository.Setup(r => r.GetByIdAsync("badge-config-456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badgeConfig);

        var command = new AwardBadgeCommand(request);

        // Act
        var result = await AwardBadgeCommandHandler.Handle(
            command,
            _badgeRepository.Object,
            _badgeConfigRepository.Object,
            _profileRepository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Id.Should().NotBeNullOrEmpty();
        Guid.TryParse(result.Id, out _).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithoutOptionalFields_CreatesValidBadge()
    {
        // Arrange
        var badgeConfig = new BadgeConfiguration
        {
            Id = "badge-config-789",
            Name = "Kindness Badge",
            AxisId = "kindness"
        };

        var request = new AwardBadgeRequest
        {
            UserProfileId = "profile-789",
            BadgeConfigurationId = "badge-config-789"
        };

        _badgeRepository.Setup(r => r.GetByUserProfileIdAndBadgeConfigIdAsync(
                "profile-789", "badge-config-789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserBadge));

        _badgeConfigRepository.Setup(r => r.GetByIdAsync("badge-config-789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badgeConfig);

        var command = new AwardBadgeCommand(request);

        // Act
        var result = await AwardBadgeCommandHandler.Handle(
            command,
            _badgeRepository.Object,
            _badgeConfigRepository.Object,
            _profileRepository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.BadgeName.Should().Be("Kindness Badge");
        result.Axis.Should().Be("kindness");
    }

    [Theory]
    [InlineData("honesty")]
    [InlineData("courage")]
    [InlineData("kindness")]
    [InlineData("compassion")]
    [InlineData("wisdom")]
    public async Task Handle_CopiesAxisFromBadgeConfig(string axis)
    {
        // Arrange
        var badgeConfig = new BadgeConfiguration
        {
            Id = $"badge-{axis}",
            Name = $"{axis} Badge",
            AxisId = axis
        };

        var request = new AwardBadgeRequest
        {
            UserProfileId = "profile-test",
            BadgeConfigurationId = $"badge-{axis}"
        };

        _badgeRepository.Setup(r => r.GetByUserProfileIdAndBadgeConfigIdAsync(
                "profile-test", $"badge-{axis}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserBadge));

        _badgeConfigRepository.Setup(r => r.GetByIdAsync($"badge-{axis}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badgeConfig);

        var command = new AwardBadgeCommand(request);

        // Act
        var result = await AwardBadgeCommandHandler.Handle(
            command,
            _badgeRepository.Object,
            _badgeConfigRepository.Object,
            _profileRepository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Axis.Should().Be(axis);
    }
}
