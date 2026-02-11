using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.CharacterMaps;
using Mystira.App.Domain.Models;
using System.Text;

namespace Mystira.App.Application.Tests.UseCases.CharacterMaps;

public class ImportCharacterMapUseCaseTests
{
    private readonly Mock<ICharacterMapRepository> _repository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<ImportCharacterMapUseCase>> _logger;
    private readonly ImportCharacterMapUseCase _useCase;

    public ImportCharacterMapUseCaseTests()
    {
        _repository = new Mock<ICharacterMapRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<ImportCharacterMapUseCase>>();
        _useCase = new ImportCharacterMapUseCase(_repository.Object, _unitOfWork.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidYaml_ImportsCharacterMaps()
    {
        // Arrange
        var yaml = @"characters:
- id: char-1
  name: Elarion
  image: media/images/elarion.jpg
  metadata:
    species: elf
    age: 250
- id: char-2
  name: Goblin Scout
  image: media/images/goblin.jpg
  metadata:
    species: goblin
    age: 30
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));

        _repository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CharacterMap));

        // Act
        var result = await _useCase.ExecuteAsync(stream);

        // Assert
        result.Should().HaveCount(2);
        _repository.Verify(r => r.AddAsync(It.IsAny<CharacterMap>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingCharacterMap_ReplacesExisting()
    {
        // Arrange
        var yaml = @"characters:
- id: char-1
  name: Updated Elarion
  image: new-image.jpg
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));

        _repository.Setup(r => r.GetByIdAsync("char-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CharacterMap { Id = "char-1", Name = "Old Elarion" });

        // Act
        var result = await _useCase.ExecuteAsync(stream);

        // Assert
        result.Should().HaveCount(1);
        _repository.Verify(r => r.DeleteAsync("char-1", It.IsAny<CancellationToken>()), Times.Once);
        _repository.Verify(r => r.AddAsync(It.IsAny<CharacterMap>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullStream_ThrowsArgumentNullException()
    {
        var act = () => _useCase.ExecuteAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidYaml_ThrowsArgumentException()
    {
        // Arrange - YAML without "characters" key
        var yaml = @"other_key:
- id: char-1
";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));

        // Act
        var act = () => _useCase.ExecuteAsync(stream);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*missing characters*");
    }
}
