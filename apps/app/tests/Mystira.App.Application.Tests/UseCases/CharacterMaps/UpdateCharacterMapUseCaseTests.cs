using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.UseCases.CharacterMaps;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.CharacterMaps;

namespace Mystira.App.Application.Tests.UseCases.CharacterMaps;

public class UpdateCharacterMapUseCaseTests
{
    private readonly Mock<ICharacterMapRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<UpdateCharacterMapUseCase>> _logger;
    private readonly UpdateCharacterMapUseCase _useCase;

    public UpdateCharacterMapUseCaseTests()
    {
        _repository = new Mock<ICharacterMapRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<UpdateCharacterMapUseCase>>();
        _useCase = new UpdateCharacterMapUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_UpdatesCharacterMap()
    {
        // Arrange
        var existing = new CharacterMap
        {
            Id = "char-1",
            Name = "Old Name",
            Image = "old-image.jpg",
            Metadata = new CharacterMetadata { Species = "elf" }
        };
        _repository.Setup(r => r.GetByIdAsync("char-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var request = new UpdateCharacterMapRequest
        {
            Name = "New Name",
            Image = "new-image.jpg"
        };

        // Act
        var result = await _useCase.ExecuteAsync("char-1", request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Name");
        result.Image.Should().Be("new-image.jpg");
        _repository.Verify(r => r.UpdateAsync(It.IsAny<CharacterMap>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingId_ThrowsValidationException()
    {
        // Arrange
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CharacterMap));

        var request = new UpdateCharacterMapRequest { Name = "Test" };

        // Act
        var act = () => _useCase.ExecuteAsync("missing", request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*not found*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_WithNullOrEmptyId_ThrowsValidationException(string? id)
    {
        var request = new UpdateCharacterMapRequest { Name = "Test" };
        var act = () => _useCase.ExecuteAsync(id!, request);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsValidationException()
    {
        var act = () => _useCase.ExecuteAsync("char-1", null!);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_PartialUpdate_OnlyChangesProvidedFields()
    {
        // Arrange
        var existing = new CharacterMap
        {
            Id = "char-1",
            Name = "Original",
            Image = "original.jpg",
            Audio = "original.mp3"
        };
        _repository.Setup(r => r.GetByIdAsync("char-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var request = new UpdateCharacterMapRequest { Name = "Updated" };

        // Act
        var result = await _useCase.ExecuteAsync("char-1", request);

        // Assert
        result.Name.Should().Be("Updated");
        result.Image.Should().Be("original.jpg");
        result.Audio.Should().Be("original.mp3");
    }
}
