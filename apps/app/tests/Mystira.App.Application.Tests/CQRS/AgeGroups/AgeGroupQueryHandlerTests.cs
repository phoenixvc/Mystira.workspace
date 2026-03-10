using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.AgeGroups.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.AgeGroups;

public class AgeGroupQueryHandlerTests
{
    private readonly Mock<IAgeGroupRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public AgeGroupQueryHandlerTests()
    {
        _repository = new Mock<IAgeGroupRepository>();
        _logger = new Mock<ILogger>();
    }

    #region GetAgeGroupByIdQueryHandler Tests

    [Fact]
    public async Task GetAgeGroupById_WithExistingId_ReturnsAgeGroup()
    {
        // Arrange
        var ageGroupId = "age-group-1";
        var expectedAgeGroup = new AgeGroupDefinition
        {
            Id = ageGroupId,
            Name = "Young Explorers",
            Value = "6-9",
            MinimumAge = 6,
            MaximumAge = 9,
            Description = "For children ages 6-9"
        };

        var query = new GetAgeGroupByIdQuery(ageGroupId);

        _repository.Setup(r => r.GetByIdAsync(ageGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAgeGroup);

        // Act
        var result = await GetAgeGroupByIdQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ageGroupId);
        result.Name.Should().Be("Young Explorers");
        result.MinimumAge.Should().Be(6);
        result.MaximumAge.Should().Be(9);
    }

    [Fact]
    public async Task GetAgeGroupById_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        var ageGroupId = "non-existent-id";
        var query = new GetAgeGroupByIdQuery(ageGroupId);

        _repository.Setup(r => r.GetByIdAsync(ageGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(AgeGroupDefinition));

        // Act
        var result = await GetAgeGroupByIdQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAgeGroupById_WhenNotFound_LogsWarning()
    {
        // Arrange
        var ageGroupId = "missing-id";
        var query = new GetAgeGroupByIdQuery(ageGroupId);

        _repository.Setup(r => r.GetByIdAsync(ageGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(AgeGroupDefinition));

        // Act
        await GetAgeGroupByIdQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public void GetAgeGroupByIdQuery_ImplementsCacheableQuery()
    {
        // Arrange
        var query = new GetAgeGroupByIdQuery("test-id");

        // Assert
        query.CacheKey.Should().Be("MasterData:AgeGroups:test-id");
        query.CacheDurationSeconds.Should().Be(3600); // 1 hour
    }

    #endregion

    #region GetAllAgeGroupsQueryHandler Tests

    [Fact]
    public async Task GetAllAgeGroups_ReturnsAllAgeGroups()
    {
        // Arrange
        var expectedAgeGroups = new List<AgeGroupDefinition>
        {
            new AgeGroupDefinition { Id = "1", Name = "Tiny Tots", Value = "3-5", MinimumAge = 3, MaximumAge = 5 },
            new AgeGroupDefinition { Id = "2", Name = "Young Explorers", Value = "6-9", MinimumAge = 6, MaximumAge = 9 },
            new AgeGroupDefinition { Id = "3", Name = "Pre-Teens", Value = "10-12", MinimumAge = 10, MaximumAge = 12 }
        };

        var query = new GetAllAgeGroupsQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAgeGroups);

        // Act
        var result = await GetAllAgeGroupsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(a => a.Name == "Tiny Tots");
        result.Should().Contain(a => a.Name == "Young Explorers");
        result.Should().Contain(a => a.Name == "Pre-Teens");
    }

    [Fact]
    public async Task GetAllAgeGroups_WhenEmpty_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllAgeGroupsQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgeGroupDefinition>());

        // Act
        var result = await GetAllAgeGroupsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAgeGroups_LogsInformationWithCount()
    {
        // Arrange
        var ageGroups = new List<AgeGroupDefinition>
        {
            new AgeGroupDefinition { Id = "1", Name = "Test", Value = "1-5" },
            new AgeGroupDefinition { Id = "2", Name = "Test2", Value = "6-10" }
        };

        var query = new GetAllAgeGroupsQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ageGroups);

        // Act
        await GetAllAgeGroupsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.AtLeast(2)); // Once for "Retrieving" and once for "Retrieved X"
    }

    #endregion

    #region Age Group Data Integrity Tests

    [Fact]
    public async Task GetAgeGroupById_ReturnsCompleteAgeGroupData()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var ageGroupId = "complete-age-group";
        var expectedAgeGroup = new AgeGroupDefinition
        {
            Id = ageGroupId,
            Name = "Complete Age Group",
            Value = "8-12",
            MinimumAge = 8,
            MaximumAge = 12,
            Description = "A fully populated age group",
            IsDeleted = false,
            CreatedAt = now.AddDays(-30),
            UpdatedAt = now
        };

        var query = new GetAgeGroupByIdQuery(ageGroupId);

        _repository.Setup(r => r.GetByIdAsync(ageGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAgeGroup);

        // Act
        var result = await GetAgeGroupByIdQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(ageGroupId);
        result.Name.Should().Be("Complete Age Group");
        result.Value.Should().Be("8-12");
        result.MinimumAge.Should().Be(8);
        result.MaximumAge.Should().Be(12);
        result.Description.Should().Be("A fully populated age group");
        result.IsDeleted.Should().BeFalse();
        result.CreatedAt.Should().BeCloseTo(now.AddDays(-30), TimeSpan.FromSeconds(1));
        result.UpdatedAt.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetAllAgeGroups_PreservesOrder()
    {
        // Arrange
        var ageGroups = new List<AgeGroupDefinition>
        {
            new AgeGroupDefinition { Id = "1", Name = "First", Value = "3-5", MinimumAge = 3 },
            new AgeGroupDefinition { Id = "2", Name = "Second", Value = "6-9", MinimumAge = 6 },
            new AgeGroupDefinition { Id = "3", Name = "Third", Value = "10-12", MinimumAge = 10 }
        };

        var query = new GetAllAgeGroupsQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ageGroups);

        // Act
        var result = await GetAllAgeGroupsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result[0].Name.Should().Be("First");
        result[1].Name.Should().Be("Second");
        result[2].Name.Should().Be("Third");
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task GetAgeGroupById_CallsRepositoryOnce()
    {
        // Arrange
        var ageGroupId = "test-id";
        var query = new GetAgeGroupByIdQuery(ageGroupId);

        _repository.Setup(r => r.GetByIdAsync(ageGroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AgeGroupDefinition { Id = ageGroupId });

        // Act
        await GetAgeGroupByIdQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.GetByIdAsync(ageGroupId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAgeGroups_CallsRepositoryOnce()
    {
        // Arrange
        var query = new GetAllAgeGroupsQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AgeGroupDefinition>());

        // Act
        await GetAllAgeGroupsQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        _repository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
