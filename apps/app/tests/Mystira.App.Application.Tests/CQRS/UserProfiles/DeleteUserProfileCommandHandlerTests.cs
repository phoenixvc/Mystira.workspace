using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.UserProfiles.Commands;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.UserProfiles;

public class DeleteUserProfileCommandHandlerTests
{
    private readonly Mock<IUserProfileRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger> _logger;

    public DeleteUserProfileCommandHandlerTests()
    {
        _repository = new Mock<IUserProfileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithExistingProfile_DeletesAndReturnsTrue()
    {
        // Arrange
        var profileId = "profile-123";
        var profile = new UserProfile
        {
            Id = profileId,
            AccountId = "account-123",
            Name = "Test Child"
        };

        var command = new DeleteUserProfileCommand(profileId);

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _repository.Setup(r => r.DeleteAsync(profileId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await DeleteUserProfileCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync(profileId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingProfile_ReturnsFalse()
    {
        // Arrange
        var profileId = "non-existent";
        var command = new DeleteUserProfileCommand(profileId);

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<UserProfile?>(null));

        // Act
        var result = await DeleteUserProfileCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CallsDeleteWithCorrectId()
    {
        // Arrange
        var profileId = "profile-to-delete";
        var profile = new UserProfile { Id = profileId };
        var command = new DeleteUserProfileCommand(profileId);

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _repository.Setup(r => r.DeleteAsync(profileId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await DeleteUserProfileCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.DeleteAsync(profileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SavesChangesAfterDeletion()
    {
        // Arrange
        var profileId = "profile-123";
        var profile = new UserProfile { Id = profileId };
        var command = new DeleteUserProfileCommand(profileId);
        var callOrder = new List<string>();

        _repository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
        _repository.Setup(r => r.DeleteAsync(profileId, It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("delete"))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => callOrder.Add("save"))
            .ReturnsAsync(1);

        // Act
        await DeleteUserProfileCommandHandler.Handle(
            command,
            _repository.Object,
            _unitOfWork.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        callOrder.Should().ContainInOrder("delete", "save");
    }
}
