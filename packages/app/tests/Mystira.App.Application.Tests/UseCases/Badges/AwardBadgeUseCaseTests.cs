using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Badges;
using Mystira.App.Domain.Models;
using Mystira.Contracts.App.Requests.Badges;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases.Badges;

public class AwardBadgeUseCaseTests
{
    private readonly Mock<IUserBadgeRepository> _badgeRepository;
    private readonly Mock<IUserProfileRepository> _profileRepository;
    private readonly Mock<IBadgeRepository> _newBadgeRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<AwardBadgeUseCase>> _logger;
    private readonly AwardBadgeUseCase _useCase;

    public AwardBadgeUseCaseTests()
    {
        _badgeRepository = new Mock<IUserBadgeRepository>();
        _profileRepository = new Mock<IUserProfileRepository>();
        _newBadgeRepository = new Mock<IBadgeRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<AwardBadgeUseCase>>();
        _useCase = new AwardBadgeUseCase(
            _badgeRepository.Object, _profileRepository.Object,
            _newBadgeRepository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_AwardsBadge()
    {
        var profile = new UserProfile { Id = "profile-1", Name = "Player" };
        var badge = new Badge { Id = "badge-config-1", Title = "Courage I", CompassAxisId = "courage", RequiredScore = 5.0f };

        _profileRepository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _newBadgeRepository.Setup(r => r.GetByIdAsync("badge-config-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(badge);
        _badgeRepository.Setup(r => r.GetByUserProfileIdAndBadgeConfigIdAsync("profile-1", "badge-config-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserBadge));

        var request = new AwardBadgeRequest
        {
            UserProfileId = "profile-1",
            BadgeConfigurationId = "badge-config-1",
            TriggerValue = 7.5f,
            GameSessionId = "gs-1",
            ScenarioId = "scen-1"
        };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result.UserProfileId.Should().Be("profile-1");
        result.BadgeConfigurationId.Should().Be("badge-config-1");
        _badgeRepository.Verify(r => r.AddAsync(It.IsAny<UserBadge>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsArgumentException()
    {
        var act = () => _useCase.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
