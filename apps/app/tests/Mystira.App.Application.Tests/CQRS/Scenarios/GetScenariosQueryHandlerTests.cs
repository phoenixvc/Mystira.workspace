using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Scenarios.Queries;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Scenarios;

public class GetScenariosQueryHandlerTests
{
    private readonly Mock<IScenarioRepository> _repository;
    private readonly Mock<ILogger> _logger;

    public GetScenariosQueryHandlerTests()
    {
        _repository = new Mock<IScenarioRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WhenCalled_ReturnsAllScenarios()
    {
        // Arrange
        var expectedScenarios = new List<Scenario>
        {
            new Scenario { Id = "scenario-1", Title = "Adventure One" },
            new Scenario { Id = "scenario-2", Title = "Adventure Two" },
            new Scenario { Id = "scenario-3", Title = "Adventure Three" }
        };

        var query = new GetScenariosQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedScenarios);

        // Act
        var result = await GetScenariosQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().Contain(s => s.Title == "Adventure One");
        result.Should().Contain(s => s.Title == "Adventure Two");
        result.Should().Contain(s => s.Title == "Adventure Three");

        _repository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoScenarios_ReturnsEmptyCollection()
    {
        // Arrange
        var query = new GetScenariosQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Scenario>());

        // Act
        var result = await GetScenariosQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCalled_LogsDebugWithCount()
    {
        // Arrange
        var scenarios = new List<Scenario>
        {
            new Scenario { Id = "s1", Title = "Test 1" },
            new Scenario { Id = "s2", Title = "Test 2" }
        };

        var query = new GetScenariosQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenarios);

        // Act
        await GetScenariosQueryHandler.Handle(
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
    public async Task Handle_WhenScenariosExist_ReturnsScenariosWithAllProperties()
    {
        // Arrange
        var expectedScenarios = new List<Scenario>
        {
            new Scenario
            {
                Id = "full-scenario",
                Title = "Complete Adventure",
                Description = "A fully detailed scenario",
                AgeGroupId = "10-12",
                Scenes = new List<Scene>
                {
                    new Scene { Id = "scene-1", Title = "Opening" }
                },
                Characters = new List<ScenarioCharacter>
                {
                    new ScenarioCharacter { Id = "char-1", Name = "Hero" }
                }
            }
        };

        var query = new GetScenariosQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedScenarios);

        // Act
        var result = await GetScenariosQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        var scenario = result.First();
        scenario.Title.Should().Be("Complete Adventure");
        scenario.Description.Should().Be("A fully detailed scenario");
        scenario.AgeGroupId.Should().Be("10-12");
        scenario.Scenes.Should().HaveCount(1);
        scenario.Characters.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenCalled_DoesNotModifyRepository()
    {
        // Arrange
        var query = new GetScenariosQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Scenario>());

        // Act
        await GetScenariosQueryHandler.Handle(
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
    public async Task Handle_WithLargeDataset_ReturnsAllItems()
    {
        // Arrange
        var largeDataset = Enumerable.Range(1, 100)
            .Select(i => new Scenario { Id = $"scenario-{i}", Title = $"Adventure {i}" })
            .ToList();

        var query = new GetScenariosQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(largeDataset);

        // Act
        var result = await GetScenariosQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(100);
    }

    [Fact]
    public async Task Handle_WhenCalled_PreservesScenarioOrder()
    {
        // Arrange
        var orderedScenarios = new List<Scenario>
        {
            new Scenario { Id = "first", Title = "First" },
            new Scenario { Id = "second", Title = "Second" },
            new Scenario { Id = "third", Title = "Third" }
        };

        var query = new GetScenariosQuery();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderedScenarios);

        // Act
        var result = await GetScenariosQueryHandler.Handle(
            query,
            _repository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        var resultList = result.ToList();
        resultList[0].Id.Should().Be("first");
        resultList[1].Id.Should().Be("second");
        resultList[2].Id.Should().Be("third");
    }
}
