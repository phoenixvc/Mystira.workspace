using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.Avatars;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases.Avatars;

public class DeleteAvatarConfigurationUseCaseTests
{
    private readonly Mock<IAvatarConfigurationFileRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<DeleteAvatarConfigurationUseCase>> _logger;
    private readonly DeleteAvatarConfigurationUseCase _useCase;

    public DeleteAvatarConfigurationUseCaseTests()
    {
        _repository = new Mock<IAvatarConfigurationFileRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<DeleteAvatarConfigurationUseCase>>();
        _useCase = new DeleteAvatarConfigurationUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_DeletesConfiguration()
    {
        // Arrange
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AvatarConfigurationFile());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange - GetAsync returns null by default

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().BeFalse();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
