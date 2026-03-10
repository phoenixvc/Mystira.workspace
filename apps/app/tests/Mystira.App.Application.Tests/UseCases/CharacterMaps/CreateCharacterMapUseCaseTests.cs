using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.CharacterMaps;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.CharacterMaps;

namespace Mystira.App.Application.Tests.UseCases.CharacterMaps;

public class CreateCharacterMapUseCaseTests
{
    private readonly Mock<ICharacterMapRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<CreateCharacterMapUseCase>> _logger;
    private readonly CreateCharacterMapUseCase _useCase;

    public CreateCharacterMapUseCaseTests()
    {
        _repository = new Mock<ICharacterMapRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<CreateCharacterMapUseCase>>();
        _useCase = new CreateCharacterMapUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesCharacterMap()
    {
        // Arrange
        var request = new CreateCharacterMapRequest
        {
            Id = "char-1",
            Name = "Elarion",
            Image = "media/images/elarion.jpg",
            Audio = "media/audio/elarion_voice.mp3",
            Metadata = new Dictionary<string, object>
            {
                { "species", "elf" },
                { "age", 250 },
                { "backstory", "An ancient guardian" }
            }
        };

        _repository.Setup(r => r.GetByIdAsync("char-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CharacterMap));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("char-1");
        result.Name.Should().Be("Elarion");
        result.Image.Should().Be("media/images/elarion.jpg");
        result.Audio.Should().Be("media/audio/elarion_voice.mp3");
        _repository.Verify(r => r.AddAsync(It.IsAny<CharacterMap>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingId_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreateCharacterMapRequest { Id = "char-1", Name = "Test" };
        _repository.Setup(r => r.GetByIdAsync("char-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CharacterMap { Id = "char-1" });

        // Act
        var act = () => _useCase.ExecuteAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ThrowsValidationException()
    {
        var act = () => _useCase.ExecuteAsync(null!);
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullMetadata_CreatesCharacterMapWithDefaultMetadata()
    {
        // Arrange
        var request = new CreateCharacterMapRequest
        {
            Id = "char-2",
            Name = "Goblin",
            Metadata = null
        };

        _repository.Setup(r => r.GetByIdAsync("char-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CharacterMap));

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Should().NotBeNull();
        _repository.Verify(r => r.AddAsync(It.IsAny<CharacterMap>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
