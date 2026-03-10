using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.MediaMetadata.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.MediaMetadata;

public class GetMediaMetadataFileQueryHandlerTests
{
    private readonly Mock<IMediaMetadataFileRepository> _repository;
    private readonly Mock<ILogger<GetMediaMetadataFileQuery>> _logger;

    public GetMediaMetadataFileQueryHandlerTests()
    {
        _repository = new Mock<IMediaMetadataFileRepository>();
        _logger = new Mock<ILogger<GetMediaMetadataFileQuery>>();
    }

    [Fact]
    public async Task Handle_WithExistingFile_ReturnsFile()
    {
        var file = new MediaMetadataFile
        {
            Id = "media-metadata-1",
            Entries = new List<MediaMetadataEntry>
            {
                new()
                {
                    Id = "entry-1",
                    Title = "Background Music",
                    FileName = "bg.mp3",
                    Type = "audio",
                    ClassificationTags = new List<ClassificationTag>(),
                    Modifiers = new List<MetadataModifier>()
                }
            },
            Version = "1"
        };
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(file);

        var result = await GetMediaMetadataFileQueryHandler.Handle(
            new GetMediaMetadataFileQuery(), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Entries.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithNullFile_ReturnsNull()
    {
        _repository.Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(MediaMetadataFile));

        var result = await GetMediaMetadataFileQueryHandler.Handle(
            new GetMediaMetadataFileQuery(), _repository.Object, _logger.Object, CancellationToken.None);

        result.Should().BeNull();
    }
}
