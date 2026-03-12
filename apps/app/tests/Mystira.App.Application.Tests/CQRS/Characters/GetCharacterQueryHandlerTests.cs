using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Characters.Queries;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Characters;

public class GetCharacterQueryHandlerTests
{
    private readonly Mock<ICharacterMapFileRepository> _repository;
    private readonly Mock<ILogger<GetCharacterQuery>> _logger;

    public GetCharacterQueryHandlerTests()
    {
        _repository = new Mock<ICharacterMapFileRepository>();
        _logger = new Mock<ILogger<GetCharacterQuery>>();
    }

    [Fact]
    public async Task Handle_WithExistingCharacter_ReturnsCharacter()
    {
        var file = new CharacterMapFile
        {
            Characters = new List<CharacterMapFileCharacter>
            {
                new()
                {
                    Id = "elarion",
                    Name = "Elarion the Wise",
                    Image = "elarion.jpg",
                    Metadata = new CharacterMetadata
                    {
                        Roles = new List<string> { "mentor" },
                        Archetypes = new List<string> { "guardian" },
                        Species = "elf",
                        Age = 312,
                        Traits = new List<string> { "wise" },
                        Backstory = "A sage from the Verdant Isles."
                    }
                }
            }
        };
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(file);

        var result = await GetCharacterQueryHandler.Handle(
            new GetCharacterQuery("elarion"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Elarion the Wise");
        result.Metadata.Species.Should().Be("elf");
    }

    [Fact]
    public async Task Handle_WithNonExistingCharacter_ReturnsNull()
    {
        var file = new CharacterMapFile { Characters = new List<CharacterMapFileCharacter>() };
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(file);

        var result = await GetCharacterQueryHandler.Handle(
            new GetCharacterQuery("missing"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNullFile_ReturnsNull()
    {
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CharacterMapFile));

        var result = await GetCharacterQueryHandler.Handle(
            new GetCharacterQuery("elarion"), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
    }
}
