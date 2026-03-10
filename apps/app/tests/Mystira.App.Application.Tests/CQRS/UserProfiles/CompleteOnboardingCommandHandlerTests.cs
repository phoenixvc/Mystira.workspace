using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.UserProfiles.Commands;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.UserProfiles;

public class CompleteOnboardingCommandHandlerTests
{
    private readonly Mock<IUserProfileRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;

    public CompleteOnboardingCommandHandlerTests()
    {
        _repository = new Mock<IUserProfileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithValidProfile_MarksOnboardingComplete()
    {
        var profile = new UserProfile { Id = "profile-1", HasCompletedOnboarding = false };
        var command = new CompleteOnboardingCommand("profile-1");

        _repository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        var result = await CompleteOnboardingCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeTrue();
        profile.HasCompletedOnboarding.Should().BeTrue();
        _repository.Verify(r => r.UpdateAsync(profile, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ProfileNotFound_ReturnsFalse()
    {
        var command = new CompleteOnboardingCommand("missing");

        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));

        var result = await CompleteOnboardingCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        result.Should().BeFalse();
        _repository.Verify(r => r.UpdateAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SetsUpdatedAtTimestamp()
    {
        var profile = new UserProfile { Id = "profile-1", HasCompletedOnboarding = false };
        var command = new CompleteOnboardingCommand("profile-1");
        var beforeCall = DateTime.UtcNow;

        _repository.Setup(r => r.GetByIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);

        await CompleteOnboardingCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object, _logger.Object, CancellationToken.None);

        profile.UpdatedAt.Should().BeOnOrAfter(beforeCall);
        profile.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var profile = new UserProfile { Id = "profile-1" };
        var command = new CompleteOnboardingCommand("profile-1");

        _repository.Setup(r => r.GetByIdAsync("profile-1", ct)).ReturnsAsync(profile);

        await CompleteOnboardingCommandHandler.Handle(
            command, _repository.Object, _unitOfWork.Object, _logger.Object, ct);

        _repository.Verify(r => r.GetByIdAsync("profile-1", ct), Times.Once);
        _repository.Verify(r => r.UpdateAsync(profile, ct), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(ct), Times.Once);
    }
}
