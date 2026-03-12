using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.CharacterMaps;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.CharacterMaps;

public class DeleteCharacterMapUseCaseTests
{
    private readonly Mock<ICharacterMapRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<DeleteCharacterMapUseCase>> _logger;
    private readonly DeleteCharacterMapUseCase _useCase;

    public DeleteCharacterMapUseCaseTests()
    {
        _repository = new Mock<ICharacterMapRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<DeleteCharacterMapUseCase>>();
        _useCase = new DeleteCharacterMapUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingCharacterMap_DeletesAndReturnsTrue()
    {
        // Arrange
        _repository.Setup(r => r.GetByIdAsync("char-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CharacterMap { Id = "char-1", Name = "Test" });

        // Act
        var result = await _useCase.ExecuteAsync("char-1");

        // Assert
        result.Should().BeTrue();
        _repository.Verify(r => r.DeleteAsync("char-1", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingCharacterMap_ReturnsFalse()
    {
        // Arrange
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CharacterMap));

        // Act
        var result = await _useCase.ExecuteAsync("missing");

        // Assert
        result.Should().BeFalse();
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsValidationException(string? id)
    {
        var act = () => _useCase.ExecuteAsync(id!);
        await act.Should().ThrowAsync<ValidationException>();
    }
}
