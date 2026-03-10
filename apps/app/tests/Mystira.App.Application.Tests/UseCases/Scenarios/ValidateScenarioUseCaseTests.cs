using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.Scenarios;
using Mystira.App.Domain.Models;

namespace Mystira.App.Application.Tests.UseCases.Scenarios;

public class ValidateScenarioUseCaseTests
{
    private readonly Mock<ICompassAxisRepository> _compassAxisRepository;
    private readonly Mock<IArchetypeRepository> _archetypeRepository;
    private readonly Mock<ILogger<ValidateScenarioUseCase>> _logger;
    private readonly ValidateScenarioUseCase _useCase;

    public ValidateScenarioUseCaseTests()
    {
        _compassAxisRepository = new Mock<ICompassAxisRepository>();
        _archetypeRepository = new Mock<IArchetypeRepository>();
        _logger = new Mock<ILogger<ValidateScenarioUseCase>>();
        _useCase = new ValidateScenarioUseCase(
            _logger.Object, _compassAxisRepository.Object, _archetypeRepository.Object);

        SetupValidAxesAndArchetypes();
    }

    #region Null Input Tests

    [Fact]
    public async Task ExecuteAsync_WithNullScenario_ThrowsValidationException()
    {
        var act = () => _useCase.ExecuteAsync(null!);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region Valid Scenario Tests

    [Fact]
    public async Task ExecuteAsync_WithValidScenario_DoesNotThrow()
    {
        var scenario = CreateValidScenario();

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WithLinearScenario_DoesNotThrow()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new() { Id = "scene-1", Title = "Start", NextSceneId = "scene-2", Branches = new List<Branch>(), EchoReveals = new List<EchoReveal>() },
                new() { Id = "scene-2", Title = "Middle", NextSceneId = "scene-3", Branches = new List<Branch>(), EchoReveals = new List<EchoReveal>() },
                new() { Id = "scene-3", Title = "End", Branches = new List<Branch>(), EchoReveals = new List<EchoReveal>() }
            }
        };

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region CoreAxes Validation Tests

    [Fact]
    public async Task ExecuteAsync_WithInvalidCoreAxis_ThrowsValidationException()
    {
        var scenario = CreateValidScenario();
        scenario.CoreAxes = new List<CoreAxis> { new("nonexistent_axis") };

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Invalid compass axis*nonexistent_axis*");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullCoreAxes_DoesNotThrow()
    {
        var scenario = CreateValidScenario();
        scenario.CoreAxes = null!;

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyCoreAxes_DoesNotThrow()
    {
        var scenario = CreateValidScenario();
        scenario.CoreAxes = new List<CoreAxis>();

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Archetype Validation Tests

    [Fact]
    public async Task ExecuteAsync_WithInvalidArchetype_ThrowsValidationException()
    {
        var scenario = CreateValidScenario();
        scenario.Archetypes = new List<Archetype> { new("nonexistent_archetype") };

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*Invalid archetype*nonexistent_archetype*");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullArchetypes_DoesNotThrow()
    {
        var scenario = CreateValidScenario();
        scenario.Archetypes = null!;

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().NotThrowAsync();
    }

    #endregion

    #region Scene Reference Validation Tests

    [Fact]
    public async Task ExecuteAsync_WithNoScenes_ThrowsValidationException()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>()
        };

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*at least one scene*");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullScenes_ThrowsValidationException()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = null!
        };

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidNextSceneReference_ThrowsValidationException()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1", Title = "Start",
                    NextSceneId = "nonexistent-scene",
                    Branches = new List<Branch>(),
                    EchoReveals = new List<EchoReveal>()
                }
            }
        };

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*references non-existent next scene*");
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidBranchReference_ThrowsValidationException()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1", Title = "Start",
                    Branches = new List<Branch>
                    {
                        new() { Choice = "Go", NextSceneId = "nonexistent-scene" }
                    },
                    EchoReveals = new List<EchoReveal>()
                }
            }
        };

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*branch references non-existent scene*");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyBranchNextSceneId_DoesNotThrow()
    {
        // Empty NextSceneId means story ending - this is valid
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1", Title = "Start",
                    Branches = new List<Branch>
                    {
                        new() { Choice = "End story", NextSceneId = "" }
                    },
                    EchoReveals = new List<EchoReveal>()
                }
            }
        };

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEchoRevealReference_ThrowsValidationException()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1", Title = "Start",
                    Branches = new List<Branch>(),
                    EchoReveals = new List<EchoReveal>
                    {
                        new() { TriggerSceneId = "nonexistent-scene", RevealMechanic = "test", EchoType = "moral" }
                    }
                }
            }
        };

        var act = () => _useCase.ExecuteAsync(scenario);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*echo reveal references non-existent scene*");
    }

    #endregion

    #region Character Validation Tests

    [Fact]
    public async Task ExecuteAsync_ChoiceSceneWithMissingActiveCharacter_LogsError()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Characters = new List<ScenarioCharacter>
            {
                new() { Id = "hero", Name = "Hero" }
            },
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1", Title = "Choice Scene",
                    Type = SceneType.Choice,
                    ActiveCharacter = null,
                    Branches = new List<Branch>(),
                    EchoReveals = new List<EchoReveal>()
                }
            }
        };

        await _useCase.ExecuteAsync(scenario);

        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ChoiceSceneWithInvalidActiveCharacter_LogsError()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Characters = new List<ScenarioCharacter>
            {
                new() { Id = "hero", Name = "Hero" }
            },
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1", Title = "Choice Scene",
                    Type = SceneType.Choice,
                    ActiveCharacter = "nonexistent-character",
                    Branches = new List<Branch>(),
                    EchoReveals = new List<EchoReveal>()
                }
            }
        };

        await _useCase.ExecuteAsync(scenario);

        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion

    #region Unreachable Scene Warning Tests

    [Fact]
    public async Task ExecuteAsync_WithUnreachableScenes_LogsWarning()
    {
        var scenario = new Scenario
        {
            Id = "s1",
            Scenes = new List<Scene>
            {
                new() { Id = "scene-1", Title = "Start", Branches = new List<Branch>(), EchoReveals = new List<EchoReveal>() },
                new() { Id = "scene-orphan", Title = "Orphan", Branches = new List<Branch>(), EchoReveals = new List<EchoReveal>() }
            }
        };

        await _useCase.ExecuteAsync(scenario);

        _logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    #endregion

    private void SetupValidAxesAndArchetypes()
    {
        _compassAxisRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CompassAxis>
            {
                new() { Id = "1", Name = "courage" },
                new() { Id = "2", Name = "honesty" },
                new() { Id = "3", Name = "empathy" }
            });

        _archetypeRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ArchetypeDefinition>
            {
                new() { Id = "1", Name = "Hero" },
                new() { Id = "2", Name = "Mentor" },
                new() { Id = "3", Name = "Trickster" }
            });
    }

    private static Scenario CreateValidScenario()
    {
        return new Scenario
        {
            Id = "scenario-1",
            Title = "Valid Scenario",
            CoreAxes = new List<CoreAxis> { new("courage"), new("honesty") },
            Archetypes = new List<Archetype> { new("Hero") },
            Characters = new List<ScenarioCharacter>
            {
                new() { Id = "hero", Name = "Hero Character" }
            },
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1",
                    Title = "Scene 1",
                    Type = SceneType.Choice,
                    ActiveCharacter = "hero",
                    Branches = new List<Branch>
                    {
                        new() { Choice = "Go to scene 2", NextSceneId = "scene-2" }
                    },
                    EchoReveals = new List<EchoReveal>()
                },
                new()
                {
                    Id = "scene-2",
                    Title = "Scene 2",
                    Branches = new List<Branch>(),
                    EchoReveals = new List<EchoReveal>()
                }
            }
        };
    }
}
