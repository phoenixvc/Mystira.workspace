using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.CharacterMaps;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.UseCases.CharacterMaps;

public class GetCharacterMapsUseCaseTests
{
    private readonly Mock<ICharacterMapRepository> _repository;
    private readonly Mock<ILogger<GetCharacterMapsUseCase>> _logger;
    private readonly GetCharacterMapsUseCase _useCase;

    public GetCharacterMapsUseCaseTests()
    {
        _repository = new Mock<ICharacterMapRepository>();
        _logger = new Mock<ILogger<GetCharacterMapsUseCase>>();
        _useCase = new GetCharacterMapsUseCase(_repository.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingMaps_ReturnsList()
    {
        // Arrange
        var maps = new List<CharacterMap>
        {
            new() { Id = "char-1", Name = "Elarion" },
            new() { Id = "char-2", Name = "Goblin Scout" },
            new() { Id = "char-3", Name = "Dragon Elder" }
        };
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(maps);

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(m => m.Name).Should().Contain("Elarion");
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMaps_ReturnsEmptyList()
    {
        // Arrange
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CharacterMap>());

        // Act
        var result = await _useCase.ExecuteAsync();

        // Assert
        result.Should().BeEmpty();
    }
}
