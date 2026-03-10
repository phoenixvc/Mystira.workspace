using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.App.Infrastructure.Data;
using Mystira.App.Infrastructure.Data.Repositories;

namespace Mystira.App.Infrastructure.Data.Tests.Repositories;

public class MediaAssetRepositoryTests : IDisposable
{
    private readonly MystiraAppDbContext _context;
    private readonly MediaAssetRepository _repository;

    public MediaAssetRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MystiraAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MystiraAppDbContext(options);
        _repository = new MediaAssetRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetByMediaIdAsync_WithExistingMediaId_ReturnsAsset()
    {
        // Arrange
        var asset = CreateMediaAsset("id-1", "media-1");
        await _context.MediaAssets.AddAsync(asset);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByMediaIdAsync("media-1");

        // Assert
        result.Should().NotBeNull();
        result!.MediaId.Should().Be("media-1");
    }

    [Fact]
    public async Task GetByMediaIdAsync_WithNonExistingMediaId_ReturnsNull()
    {
        var result = await _repository.GetByMediaIdAsync("nonexistent");
        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByMediaIdAsync_WithExistingMediaId_ReturnsTrue()
    {
        // Arrange
        var asset = CreateMediaAsset("id-1", "media-1");
        await _context.MediaAssets.AddAsync(asset);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsByMediaIdAsync("media-1");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByMediaIdAsync_WithNonExistingMediaId_ReturnsFalse()
    {
        var result = await _repository.ExistsByMediaIdAsync("nonexistent");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetMediaIdsAsync_ReturnsMatchingIds()
    {
        // Arrange
        await _context.MediaAssets.AddRangeAsync(
            CreateMediaAsset("id-1", "media-1"),
            CreateMediaAsset("id-2", "media-2"),
            CreateMediaAsset("id-3", "media-3"));
        await _context.SaveChangesAsync();

        // Act
        var result = (await _repository.GetMediaIdsAsync(new[] { "media-1", "media-3", "nonexistent" })).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain("media-1");
        result.Should().Contain("media-3");
    }

    [Fact]
    public async Task GetQueryable_ReturnsQueryableInterface()
    {
        // Arrange
        await _context.MediaAssets.AddRangeAsync(
            CreateMediaAsset("id-1", "media-1"),
            CreateMediaAsset("id-2", "media-2"));
        await _context.SaveChangesAsync();

        // Act
        var queryable = _repository.GetQueryable();

        // Assert
        queryable.Should().NotBeNull();
        var result = await queryable.Where(m => m.MediaType == "image").ToListAsync();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMediaIdsAsync_WithEmptyInput_ReturnsEmpty()
    {
        var result = await _repository.GetMediaIdsAsync(Array.Empty<string>());
        result.Should().BeEmpty();
    }

    private static MediaAsset CreateMediaAsset(string id, string mediaId)
    {
        return new MediaAsset
        {
            Id = id,
            MediaId = mediaId,
            Url = $"https://storage.example.com/{mediaId}.jpg",
            MediaType = "image",
            MimeType = "image/jpeg",
            FileSizeBytes = 1024,
            Tags = new List<string>(),
            Hash = "abc123",
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
