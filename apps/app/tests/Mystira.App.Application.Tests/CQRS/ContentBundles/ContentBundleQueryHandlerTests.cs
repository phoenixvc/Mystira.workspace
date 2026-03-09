using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.ContentBundles.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.ContentBundles;

public class ContentBundleQueryHandlerTests
{
    private readonly Mock<IContentBundleRepository> _repository;
    private readonly Mock<ILogger<GetAllContentBundlesQuery>> _logger;

    public ContentBundleQueryHandlerTests()
    {
        _repository = new Mock<IContentBundleRepository>();
        _logger = new Mock<ILogger<GetAllContentBundlesQuery>>();
    }

    #region GetAllContentBundlesQueryHandler Tests

    [Fact]
    public async Task GetAllContentBundles_ReturnsAllBundles()
    {
        // Arrange
        var expectedBundles = new List<ContentBundle>
        {
            new ContentBundle
            {
                Id = "bundle-1",
                Title = "Adventure Pack",
                Description = "Exciting adventures for young explorers",
                AgeGroup = "6-9",
                IsFree = false,
                ScenarioIds = new List<string> { "scenario-1", "scenario-2" }
            },
            new ContentBundle
            {
                Id = "bundle-2",
                Title = "Story Time",
                Description = "Bedtime stories for little ones",
                AgeGroup = "3-5",
                IsFree = true
            }
        };

        var query = new GetAllContentBundlesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBundles);

        // Act
        var result = await GetAllContentBundlesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(b => b.Title == "Adventure Pack");
        result.Should().Contain(b => b.Title == "Story Time");
    }

    [Fact]
    public async Task GetAllContentBundles_WhenEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        var query = new GetAllContentBundlesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ContentBundle>());

        // Act
        var result = await GetAllContentBundlesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllContentBundles_LogsDebugWithCount()
    {
        // Arrange
        var bundles = new List<ContentBundle>
        {
            new ContentBundle { Id = "b1", Title = "Bundle 1" },
            new ContentBundle { Id = "b2", Title = "Bundle 2" },
            new ContentBundle { Id = "b3", Title = "Bundle 3" }
        };

        var query = new GetAllContentBundlesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundles);

        // Act
        await GetAllContentBundlesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task GetAllContentBundles_ReturnsBundlesWithAllProperties()
    {
        // Arrange
        var bundle = new ContentBundle
        {
            Id = "complete-bundle",
            Title = "Complete Bundle",
            Description = "A fully populated bundle",
            AgeGroup = "10-12",
            IsFree = false,
            ImageId = "bundle-image-1",
            ScenarioIds = new List<string> { "s1", "s2", "s3" },
            Prices = new List<BundlePrice>
            {
                new BundlePrice { Value = 9.99m, Currency = "USD" }
            }
        };

        var query = new GetAllContentBundlesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { bundle });

        // Act
        var result = await GetAllContentBundlesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        var returnedBundle = result.Single();
        returnedBundle.Id.Should().Be("complete-bundle");
        returnedBundle.Title.Should().Be("Complete Bundle");
        returnedBundle.Description.Should().Be("A fully populated bundle");
        returnedBundle.AgeGroup.Should().Be("10-12");
        returnedBundle.IsFree.Should().BeFalse();
        returnedBundle.ImageId.Should().Be("bundle-image-1");
        returnedBundle.ScenarioIds.Should().HaveCount(3);
        returnedBundle.Prices.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllContentBundles_IncludesFreeBundles()
    {
        // Arrange
        var bundles = new List<ContentBundle>
        {
            new ContentBundle { Id = "free-bundle", Title = "Free Bundle", IsFree = true },
            new ContentBundle { Id = "paid-bundle", Title = "Paid Bundle", IsFree = false }
        };

        var query = new GetAllContentBundlesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundles);

        // Act
        var result = await GetAllContentBundlesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(b => b.IsFree);
        result.Should().Contain(b => !b.IsFree);
    }

    [Fact]
    public async Task GetAllContentBundles_CallsRepositoryOnce()
    {
        // Arrange
        var query = new GetAllContentBundlesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ContentBundle>());

        // Act
        await GetAllContentBundlesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Bundle with StoryProtocol Tests

    [Fact]
    public async Task GetAllContentBundles_ReturnsBundlesWithStoryProtocolMetadata()
    {
        // Arrange
        var bundle = new ContentBundle
        {
            Id = "sp-bundle",
            Title = "Story Protocol Bundle",
            StoryProtocol = new StoryProtocolMetadata
            {
                IpAssetId = "0x123456",
                RoyaltyModuleId = "royalty-1"
            }
        };

        var query = new GetAllContentBundlesQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { bundle });

        // Act
        var result = await GetAllContentBundlesQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        var returnedBundle = result.Single();
        returnedBundle.StoryProtocol.Should().NotBeNull();
        returnedBundle.StoryProtocol!.IpAssetId.Should().Be("0x123456");
    }

    #endregion
}
