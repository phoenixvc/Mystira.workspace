using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.CharacterMaps;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.CharacterMaps;

public class ExportCharacterMapUseCaseTests
{
    private readonly Mock<ICharacterMapRepository> _repository;
    private readonly Mock<ILogger<ExportCharacterMapUseCase>> _logger;
    private readonly ExportCharacterMapUseCase _useCase;

    public ExportCharacterMapUseCaseTests()
    {
        _repository = new Mock<ICharacterMapRepository>();
        _logger = new Mock<ILogger<ExportCharacterMapUseCase>>();
        _useCase = new ExportCharacterMapUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingMaps_ReturnsYamlString()
    {
        // Arrange
        var maps = new List<CharacterMap>
        {
            new()
            {
                Id = "char-1",
                Name = "Elarion",
                Image = "media/images/elarion.jpg",
                Metadata = new CharacterMetadata
                {
                    Species = "elf",
                    Age = 250,
                    Roles = new List<string> { "mentor" }
                }
            }
        };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(maps);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().ContainEquivalentOf("elarion");
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMaps_ReturnsEmptyYaml()
    {
        // Arrange
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CharacterMap>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
    }
}
