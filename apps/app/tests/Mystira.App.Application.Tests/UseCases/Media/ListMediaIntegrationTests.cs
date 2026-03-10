using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.UseCases.Media;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.App.Infrastructure.Data;
using Mystira.App.Infrastructure.Data.Repositories;
using Mystira.Contracts.App.Requests.Media;

namespace Mystira.App.Application.Tests.UseCases.Media;

/// <summary>
/// Integration tests for ListMediaUseCase using EF Core InMemory provider
/// to properly support IQueryable with async LINQ operations.
/// </summary>
public class ListMediaIntegrationTests : IDisposable
{
    private readonly MystiraAppDbContext _context;
    private readonly MediaAssetRepository _repository;
    private readonly ListMediaUseCase _useCase;

    public ListMediaIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<MystiraAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MystiraAppDbContext(options);
        _repository = new MediaAssetRepository(_context);
        _useCase = new ListMediaUseCase(_repository, new Mock<ILogger<ListMediaUseCase>>().Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task ExecuteAsync_WithNoFilters_ReturnsAllMedia()
    {
        // Arrange
        await SeedMedia(3);
        var request = new MediaQueryRequest { Page = 1, PageSize = 10 };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(3);
        result.Media.Should().HaveCount(3);
        result.Page.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        await SeedMedia(5);
        var request = new MediaQueryRequest { Page = 1, PageSize = 2 };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.TotalCount.Should().Be(5);
        result.Media.Should().HaveCount(2);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_WithMediaTypeFilter_ReturnsFiltered()
    {
        // Arrange
        await _context.MediaAssets.AddRangeAsync(
            CreateMediaAsset("id-1", "m1", "image"),
            CreateMediaAsset("id-2", "m2", "audio"),
            CreateMediaAsset("id-3", "m3", "image"));
        await _context.SaveChangesAsync();

        var request = new MediaQueryRequest { Page = 1, PageSize = 10, MediaType = "image" };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Media.Should().OnlyContain(m => m.MediaType == "image");
    }

    [Fact]
    public async Task ExecuteAsync_WithSearchFilter_ReturnsMatching()
    {
        // Arrange
        await _context.MediaAssets.AddRangeAsync(
            CreateMediaAsset("id-1", "elarion-portrait", "image", "Elarion portrait"),
            CreateMediaAsset("id-2", "goblin-sound", "audio", "Goblin battle cry"),
            CreateMediaAsset("id-3", "elarion-theme", "audio", "Elarion theme music"));
        await _context.SaveChangesAsync();

        var request = new MediaQueryRequest { Page = 1, PageSize = 10, Search = "elarion" };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoMatches_ReturnsEmptyResponse()
    {
        // Arrange
        await SeedMedia(2);
        var request = new MediaQueryRequest { Page = 1, PageSize = 10, MediaType = "video" };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.TotalCount.Should().Be(0);
        result.Media.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_PageBeyondRange_ReturnsEmptyMedia()
    {
        // Arrange
        await SeedMedia(3);
        var request = new MediaQueryRequest { Page = 5, PageSize = 10 };

        // Act
        var result = await _useCase.ExecuteAsync(request);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Media.Should().BeEmpty();
    }

    private async Task SeedMedia(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await _context.MediaAssets.AddAsync(
                CreateMediaAsset($"id-{i}", $"media-{i}", "image"));
        }
        await _context.SaveChangesAsync();
    }

    private static MediaAsset CreateMediaAsset(string id, string mediaId, string mediaType, string? description = null)
    {
        var mimeType = mediaType switch
        {
            "image" => "image/jpeg",
            "audio" => "audio/mpeg",
            "video" => "video/mp4",
            _ => "application/octet-stream"
        };

        return new MediaAsset
        {
            Id = id,
            MediaId = mediaId,
            Url = $"https://storage.example.com/{mediaId}.jpg",
            MediaType = mediaType,
            MimeType = mimeType,
            FileSizeBytes = 1024,
            Description = description,
            Tags = new List<string>(),
            Hash = "abc123",
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
