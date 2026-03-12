using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.Badges.Queries;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.Tests.CQRS.Badges;

public class CalculateBadgeScoresQueryHandlerTests
{
    private readonly Mock<IContentBundleRepository> _bundleRepository;
    private readonly Mock<IScenarioRepository> _scenarioRepository;
    private readonly Mock<ILogger> _logger;

    public CalculateBadgeScoresQueryHandlerTests()
    {
        _bundleRepository = new Mock<IContentBundleRepository>();
        _scenarioRepository = new Mock<IScenarioRepository>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_WithValidBundle_ReturnsAxisScores()
    {
        // Arrange
        var bundleId = "bundle-123";
        var bundle = new ContentBundle
        {
            Id = bundleId,
            Title = "Test Bundle",
            ScenarioIds = new List<string> { "scenario-1" }
        };

        var scenario = new Scenario
        {
            Id = "scenario-1",
            Title = "Test Scenario",
            Scenes = new List<Scene>
            {
                new Scene
                {
                    Id = "scene-1",
                    Branches = new List<Branch>
                    {
                        new Branch
                        {
                            Choice = "Option A",
                            NextSceneId = "",
                            CompassChange = new CompassChange { AxisId = "courage", Delta = 10 }
                        }
                    }
                }
            }
        };

        var query = new CalculateBadgeScoresQuery(bundleId, new List<double> { 25, 50, 75 });

        _bundleRepository.Setup(r => r.GetByIdAsync(bundleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);
        _scenarioRepository.Setup(r => r.GetByIdAsync("scenario-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        // Act
        var result = await CalculateBadgeScoresQueryHandler.Handle(
            query,
            _bundleRepository.Object,
            _scenarioRepository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Should().Contain(r => r.AxisName == "courage");
    }

    [Fact]
    public async Task Handle_WithEmptyBundleId_ThrowsValidationException()
    {
        // Arrange
        var query = new CalculateBadgeScoresQuery("", new List<double> { 50 });

        // Act
        var act = async () => await CalculateBadgeScoresQueryHandler.Handle(
            query,
            _bundleRepository.Object,
            _scenarioRepository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Content bundle ID*");
    }

    [Fact]
    public async Task Handle_WithNullPercentiles_ThrowsValidationException()
    {
        // Arrange
        var query = new CalculateBadgeScoresQuery("bundle-123", null!);

        // Act
        var act = async () => await CalculateBadgeScoresQueryHandler.Handle(
            query,
            _bundleRepository.Object,
            _scenarioRepository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Percentiles*");
    }

    [Fact]
    public async Task Handle_WithEmptyPercentiles_ThrowsValidationException()
    {
        // Arrange
        var query = new CalculateBadgeScoresQuery("bundle-123", new List<double>());

        // Act
        var act = async () => await CalculateBadgeScoresQueryHandler.Handle(
            query,
            _bundleRepository.Object,
            _scenarioRepository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Percentiles*");
    }

    [Fact]
    public async Task Handle_WithInvalidPercentiles_ThrowsValidationException()
    {
        // Arrange
        var query = new CalculateBadgeScoresQuery("bundle-123", new List<double> { -10, 150 });

        // Act
        var act = async () => await CalculateBadgeScoresQueryHandler.Handle(
            query,
            _bundleRepository.Object,
            _scenarioRepository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*between 0 and 100*");
    }

    [Fact]
    public async Task Handle_WithNonExistingBundle_ThrowsInvalidOperationException()
    {
        // Arrange
        var query = new CalculateBadgeScoresQuery("non-existent", new List<double> { 50 });

        _bundleRepository.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(ContentBundle));

        // Act
        var act = async () => await CalculateBadgeScoresQueryHandler.Handle(
            query,
            _bundleRepository.Object,
            _scenarioRepository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*ContentBundle*not found*");
    }

    [Fact]
    public async Task Handle_WithNoScenarios_ReturnsEmptyList()
    {
        // Arrange
        var bundleId = "bundle-empty";
        var bundle = new ContentBundle
        {
            Id = bundleId,
            Title = "Empty Bundle",
            ScenarioIds = new List<string>()
        };

        var query = new CalculateBadgeScoresQuery(bundleId, new List<double> { 50 });

        _bundleRepository.Setup(r => r.GetByIdAsync(bundleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        // Act
        var result = await CalculateBadgeScoresQueryHandler.Handle(
            query,
            _bundleRepository.Object,
            _scenarioRepository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMissingScenarios_SkipsThemGracefully()
    {
        // Arrange
        var bundleId = "bundle-123";
        var bundle = new ContentBundle
        {
            Id = bundleId,
            Title = "Test Bundle",
            ScenarioIds = new List<string> { "existing-scenario", "missing-scenario" }
        };

        var existingScenario = new Scenario
        {
            Id = "existing-scenario",
            Title = "Existing",
            Scenes = new List<Scene>
            {
                new Scene
                {
                    Id = "scene-1",
                    Branches = new List<Branch>
                    {
                        new Branch
                        {
                            Choice = "Option A",
                            NextSceneId = "",
                            CompassChange = new CompassChange { AxisId = "wisdom", Delta = 5 }
                        }
                    }
                }
            }
        };

        var query = new CalculateBadgeScoresQuery(bundleId, new List<double> { 50 });

        _bundleRepository.Setup(r => r.GetByIdAsync(bundleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);
        _scenarioRepository.Setup(r => r.GetByIdAsync("existing-scenario", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingScenario);
        _scenarioRepository.Setup(r => r.GetByIdAsync("missing-scenario", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        // Act
        var result = await CalculateBadgeScoresQueryHandler.Handle(
            query,
            _bundleRepository.Object,
            _scenarioRepository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(r => r.AxisName == "wisdom");
    }

    [Fact]
    public async Task Handle_WithMultipleAxes_ReturnsScoresForEachAxis()
    {
        // Arrange
        var bundleId = "bundle-123";
        var bundle = new ContentBundle
        {
            Id = bundleId,
            ScenarioIds = new List<string> { "scenario-1" }
        };

        var scenario = new Scenario
        {
            Id = "scenario-1",
            Scenes = new List<Scene>
            {
                new Scene
                {
                    Id = "scene-1",
                    Branches = new List<Branch>
                    {
                        new Branch
                        {
                            Choice = "Option A",
                            NextSceneId = "",
                            CompassChange = new CompassChange { AxisId = "courage", Delta = 10 }
                        },
                        new Branch
                        {
                            Choice = "Option B",
                            NextSceneId = "",
                            CompassChange = new CompassChange { AxisId = "wisdom", Delta = 15 }
                        }
                    }
                }
            }
        };

        var query = new CalculateBadgeScoresQuery(bundleId, new List<double> { 50 });

        _bundleRepository.Setup(r => r.GetByIdAsync(bundleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);
        _scenarioRepository.Setup(r => r.GetByIdAsync("scenario-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        // Act
        var result = await CalculateBadgeScoresQueryHandler.Handle(
            query,
            _bundleRepository.Object,
            _scenarioRepository.Object,
            _logger.Object,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.AxisName == "courage");
        result.Should().Contain(r => r.AxisName == "wisdom");
    }
}
