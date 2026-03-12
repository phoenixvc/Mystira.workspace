using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.Core.CQRS.CharacterMediaMetadata.Queries;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.CharacterMediaMetadata;

public class CharacterMediaMetadataQueryHandlerTests
{
    private readonly Mock<ICharacterMediaMetadataFileRepository> _repository;

    public CharacterMediaMetadataQueryHandlerTests()
    {
        _repository = new Mock<ICharacterMediaMetadataFileRepository>();
    }

    #region GetCharacterMediaMetadataEntryQueryHandler Tests

    [Fact]
    public async Task GetEntry_WithExistingEntry_ReturnsEntry()
    {
        var file = new CharacterMediaMetadataFile
        {
            Id = "metadata-1",
            Entries = new List<CharacterMediaMetadataEntry>
            {
                new() { Id = "entry-1", Title = "Elarion Portrait", FileName = "elarion.jpg", Type = "image" }
            }
        };
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(file);

        var result = await GetCharacterMediaMetadataEntryQueryHandler.Handle(
            new GetCharacterMediaMetadataEntryQuery("entry-1"), _repository.Object,
            Mock.Of<ILogger<GetCharacterMediaMetadataEntryQuery>>(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Elarion Portrait");
    }

    [Fact]
    public async Task GetEntry_WithNonExistingEntry_ReturnsNull()
    {
        var file = new CharacterMediaMetadataFile
        {
            Id = "metadata-1",
            Entries = new List<CharacterMediaMetadataEntry>()
        };
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(file);

        var result = await GetCharacterMediaMetadataEntryQueryHandler.Handle(
            new GetCharacterMediaMetadataEntryQuery("missing"), _repository.Object,
            Mock.Of<ILogger<GetCharacterMediaMetadataEntryQuery>>(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEntry_WithNullFile_ReturnsNull()
    {
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CharacterMediaMetadataFile));

        var result = await GetCharacterMediaMetadataEntryQueryHandler.Handle(
            new GetCharacterMediaMetadataEntryQuery("entry-1"), _repository.Object,
            Mock.Of<ILogger<GetCharacterMediaMetadataEntryQuery>>(), CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion

    #region GetCharacterMediaMetadataFileQueryHandler Tests

    [Fact]
    public async Task GetFile_WithExistingFile_ReturnsFile()
    {
        var file = new CharacterMediaMetadataFile
        {
            Id = "metadata-1",
            Entries = new List<CharacterMediaMetadataEntry>
            {
                new() { Id = "entry-1", Title = "Test", FileName = "test.jpg", Type = "image" }
            },
            Version = "1"
        };
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(file);

        var result = await GetCharacterMediaMetadataFileQueryHandler.Handle(
            new GetCharacterMediaMetadataFileQuery(), _repository.Object,
            Mock.Of<ILogger<GetCharacterMediaMetadataFileQuery>>(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Entries.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetFile_WithNullFile_ReturnsNull()
    {
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(CharacterMediaMetadataFile));

        var result = await GetCharacterMediaMetadataFileQueryHandler.Handle(
            new GetCharacterMediaMetadataFileQuery(), _repository.Object,
            Mock.Of<ILogger<GetCharacterMediaMetadataFileQuery>>(), CancellationToken.None);

        result.Should().BeNull();
    }

    #endregion
}
