using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Application.Ports.Data;
using Mystira.App.Application.UseCases.CharacterMaps;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.CharacterMaps;

public class GetCharacterMapUseCaseTests
{
    private readonly Mock<ICharacterMapRepository> _repository;
    private readonly Mock<ILogger<GetCharacterMapUseCase>> _logger;
    private readonly GetCharacterMapUseCase _useCase;

    public GetCharacterMapUseCaseTests()
    {
        _repository = new Mock<ICharacterMapRepository>();
        _logger = new Mock<ILogger<GetCharacterMapUseCase>>();
        _useCase = new GetCharacterMapUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingId_ReturnsCharacterMap()
    {
        // Arrange
        var expected = new CharacterMap
        {
            Id = "char-1",
            Name = "Elarion",
            Image = "media/images/elarion.jpg",
            Metadata = new CharacterMetadata
            {
                Species = "elf",
                Roles = new List<string> { "mentor" },
                Traits = new List<string> { "wise", "calm" }
            }
        };
        _repository.Setup(r => r.GetByIdAsync("char-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _useCase.ExecuteAsync("char-1");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("char-1");
        result.Name.Should().Be("Elarion");
        result.Metadata!.Species.Should().Be("elf");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CharacterMap));

        // Act
        var result = await _useCase.ExecuteAsync("missing");

        // Assert
        result.Should().BeNull();
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
