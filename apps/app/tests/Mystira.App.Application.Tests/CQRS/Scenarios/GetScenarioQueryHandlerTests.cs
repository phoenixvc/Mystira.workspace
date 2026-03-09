using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Scenarios.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.CQRS.Scenarios;

public class GetScenarioQueryHandlerTests
{
    private readonly Mock<IScenarioRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetScenarioQueryHandlerTests()
    {
        _repository = new Mock<IScenarioRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithExistingScenarioId_ReturnsScenario()
    {
        // Arrange
        var scenarioId = "scenario-123";
        var expectedScenario = new Scenario
        {
            Id = scenarioId,
            Title = "The Dragon's Quest",
            Description = "An epic adventure",
            AgeGroup = "6-9"
        };

        var query = new GetScenarioQuery(scenarioId);

        _repository.Setup(r => r.GetByIdAsync(scenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedScenario);

        // Act
        var result = await GetScenarioQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(scenarioId);
        result.Title.Should().Be("The Dragon's Quest");
        result.AgeGroup.Should().Be("6-9");

        _repository.Verify(r => r.GetByIdAsync(scenarioId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingScenarioId_ReturnsNull()
    {
        // Arrange
        var scenarioId = "non-existent-scenario";
        var query = new GetScenarioQuery(scenarioId);

        _repository.Setup(r => r.GetByIdAsync(scenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        // Act
        var result = await GetScenarioQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _repository.Verify(r => r.GetByIdAsync(scenarioId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_WithInvalidScenarioId_ThrowsArgumentException(string? scenarioId)
    {
        // Arrange
        var query = new GetScenarioQuery(scenarioId!);

        // Act
        var act = () => GetScenarioQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cannot be null or empty*");
    }

    [Fact]
    public async Task Handle_WhenScenarioNotFound_LogsWarning()
    {
        // Arrange
        var scenarioId = "missing-scenario";
        var query = new GetScenarioQuery(scenarioId);

        _repository.Setup(r => r.GetByIdAsync(scenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        // Act
        await GetScenarioQueryHandler.Handle(
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
    public async Task Handle_WhenScenarioFound_LogsDebug()
    {
        // Arrange
        var scenarioId = "found-scenario";
        var query = new GetScenarioQuery(scenarioId);

        _repository.Setup(r => r.GetByIdAsync(scenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Scenario { Id = scenarioId, Title = "Found" });

        // Act
        await GetScenarioQueryHandler.Handle(
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
    public async Task Handle_ReturnsScenarioWithScenes()
    {
        // Arrange
        var scenarioId = "scenario-with-scenes";
        var expectedScenario = new Scenario
        {
            Id = scenarioId,
            Title = "Multi-Scene Adventure",
            Scenes = new List<Scene>
            {
                new Scene { Id = "scene-1", Title = "Introduction" },
                new Scene { Id = "scene-2", Title = "Challenge" },
                new Scene { Id = "scene-3", Title = "Resolution" }
            }
        };

        var query = new GetScenarioQuery(scenarioId);

        _repository.Setup(r => r.GetByIdAsync(scenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedScenario);

        // Act
        var result = await GetScenarioQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Scenes.Should().HaveCount(3);
        result.Scenes.Should().Contain(s => s.Title == "Introduction");
    }

    [Fact]
    public async Task Handle_DoesNotModifyRepository()
    {
        // Arrange
        var scenarioId = "readonly-scenario";
        var query = new GetScenarioQuery(scenarioId);

        _repository.Setup(r => r.GetByIdAsync(scenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Scenario { Id = scenarioId, Title = "Test" });

        // Act
        await GetScenarioQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert - verify no write operations were called
        _repository.Verify(r => r.AddAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Scenario>(), It.IsAny<CancellationToken>()), Times.Never);
        _repository.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void GetScenarioQuery_ImplementsCacheableQuery()
    {
        // Arrange
        var query = new GetScenarioQuery("test-id");

        // Assert
        query.CacheKey.Should().Be("Scenario:test-id");
        query.CacheDurationSeconds.Should().Be(300); // 5 minutes
    }
}
