using Mystira.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

using Mystira.Shared.Locking;

namespace Mystira.App.Application.Tests.UseCases;

public class MakeChoiceUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<IScenarioRepository> _scenarioRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<IDistributedLockService> _lockService;
    private readonly Mock<ILogger<MakeChoiceUseCase>> _logger;
    private readonly MakeChoiceUseCase _useCase;

    public MakeChoiceUseCaseTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _scenarioRepository = new Mock<IScenarioRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _lockService = new Mock<IDistributedLockService>();
        _logger = new Mock<ILogger<MakeChoiceUseCase>>();
        _useCase = new MakeChoiceUseCase(
            _repository.Object, _scenarioRepository.Object,
            _unitOfWork.Object, _lockService.Object, _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidChoice_RecordsChoiceAndProgresses()
    {
        var session = CreateTestSession();
        var scenario = CreateTestScenario();
        SetupRepositories(session, scenario);

        var request = new MakeChoiceRequest
        {
            SessionId = "session-1",
            SceneId = "scene-1",
            ChoiceText = "Be brave",
            NextSceneId = "scene-2"
        };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result!.ChoiceHistory.Should().HaveCount(1);
        result.ChoiceHistory[0].ChoiceText.Should().Be("Be brave");
        result.ChoiceHistory[0].NextScene.Should().Be("scene-2");
        result.CurrentSceneId.Should().Be("scene-2");
        _repository.Verify(r => r.UpdateAsync(session, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithCompassChange_TracksCompassDelta()
    {
        var session = CreateTestSession();
        session.CompassValues.Add(new CompassTracking
        {
            Axis = "courage",
            CurrentValue = 0,
            StartingValue = 0,
            History = new List<CompassChangeRecord>(),
            LastUpdated = DateTime.UtcNow
        });
        var scenario = CreateTestScenarioWithCompassChange();
        SetupRepositories(session, scenario);

        var request = new MakeChoiceRequest
        {
            SessionId = "session-1",
            SceneId = "scene-1",
            ChoiceText = "Be brave",
            NextSceneId = "scene-2"
        };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result!.ChoiceHistory[0].CompassAxis.Should().Be("courage");
        result.ChoiceHistory[0].CompassDelta.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithEchoLog_RecordsEcho()
    {
        var session = CreateTestSession();
        var scenario = CreateTestScenarioWithEcho();
        SetupRepositories(session, scenario);

        var request = new MakeChoiceRequest
        {
            SessionId = "session-1",
            SceneId = "scene-1",
            ChoiceText = "Be brave",
            NextSceneId = "scene-2"
        };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result!.EchoHistory.Should().HaveCount(1);
        result.EchoHistory[0].EchoTypeId.Should().Be("moral");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentSession_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var request = new MakeChoiceRequest { SessionId = "missing", SceneId = "s1", ChoiceText = "c" };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WithCompletedSession_ThrowsInvalidOperationException()
    {
        var session = CreateTestSession();
        session.Status = SessionStatus.Completed;
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var request = new MakeChoiceRequest
        {
            SessionId = "session-1",
            SceneId = "scene-1",
            ChoiceText = "Be brave",
            NextSceneId = "scene-2"
        };

        var act = () => _useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingScenario_ThrowsInvalidOperationException()
    {
        var session = CreateTestSession();
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _scenarioRepository.Setup(r => r.GetByIdAsync("scenario-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        var request = new MakeChoiceRequest
        {
            SessionId = "session-1",
            SceneId = "scene-1",
            ChoiceText = "Be brave",
            NextSceneId = "scene-2"
        };

        var act = () => _useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidSceneId_ThrowsValidationException()
    {
        var session = CreateTestSession();
        var scenario = CreateTestScenario();
        SetupRepositories(session, scenario);

        var request = new MakeChoiceRequest
        {
            SessionId = "session-1",
            SceneId = "nonexistent-scene",
            ChoiceText = "Be brave",
            NextSceneId = "scene-2"
        };

        var act = () => _useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidChoiceText_ThrowsValidationException()
    {
        var session = CreateTestSession();
        var scenario = CreateTestScenario();
        SetupRepositories(session, scenario);

        var request = new MakeChoiceRequest
        {
            SessionId = "session-1",
            SceneId = "scene-1",
            ChoiceText = "Invalid choice text",
            NextSceneId = "scene-2"
        };

        var act = () => _useCase.ExecuteAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenReachingFinalScene_CompletesSession()
    {
        var session = CreateTestSession();
        var scenario = CreateTestScenario();
        SetupRepositories(session, scenario);

        // scene-2 has no branches and no NextSceneId => final scene
        var request = new MakeChoiceRequest
        {
            SessionId = "session-1",
            SceneId = "scene-1",
            ChoiceText = "Be brave",
            NextSceneId = "scene-2"
        };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result!.Status.Should().Be(SessionStatus.Completed);
        result.EndTime.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_ResolvesPlayerFromActiveCharacterAssignment()
    {
        var session = CreateTestSession();
        session.CharacterAssignments = new List<SessionCharacterAssignment>
        {
            new()
            {
                CharacterId = "hero",
                PlayerAssignment = new SessionPlayerAssignment
                {
                    ProfileId = "profile-assigned",
                    ProfileName = "Assigned Player"
                }
            }
        };

        var scenario = CreateTestScenarioWithActiveCharacter();
        SetupRepositories(session, scenario);

        var request = new MakeChoiceRequest
        {
            SessionId = "session-1",
            SceneId = "scene-1",
            ChoiceText = "Be brave",
            NextSceneId = "scene-2"
        };

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result!.ChoiceHistory[0].PlayerId.Should().Be("profile-assigned");
    }

    private void SetupRepositories(GameSession session, Scenario scenario)
    {
        _repository.Setup(r => r.GetByIdAsync("session-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _scenarioRepository.Setup(r => r.GetByIdAsync("scenario-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);
    }

    private static GameSession CreateTestSession()
    {
        return new GameSession
        {
            Id = "session-1",
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = "profile-1",
            Status = SessionStatus.InProgress,
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            CurrentSceneId = "scene-1",
            PlayerNames = new List<string> { "Player1" },
            ChoiceHistory = new List<SessionChoice>(),
            EchoHistory = new List<EchoLog>(),
            Achievements = new List<SessionAchievement>(),
            CompassValues = new List<CompassTracking>(),
            CharacterAssignments = new List<SessionCharacterAssignment>()
        };
    }

    private static Scenario CreateTestScenario()
    {
        return new Scenario
        {
            Id = "scenario-1",
            Title = "Test Scenario",
            CoreAxes = new List<string> { "courage" },
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1",
                    Title = "Scene 1",
                    Type = SceneType.Choice,
                    Branches = new List<Branch>
                    {
                        new() { Choice = "Be brave", NextSceneId = "scene-2" }
                    },
                    EchoReveals = new List<EchoReveal>()
                },
                new()
                {
                    Id = "scene-2",
                    Title = "Scene 2 (Final)",
                    Branches = new List<Branch>(),
                    EchoReveals = new List<EchoReveal>()
                }
            }
        };
    }

    private static Scenario CreateTestScenarioWithCompassChange()
    {
        return new Scenario
        {
            Id = "scenario-1",
            Title = "Test",
            CoreAxes = new List<string> { "courage" },
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1",
                    Title = "Scene 1",
                    Type = SceneType.Choice,
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "Be brave",
                            NextSceneId = "scene-2",
                            CompassChange = new CompassChange { AxisId = "courage", Delta = 2 }
                        }
                    },
                    EchoReveals = new List<EchoReveal>()
                },
                new() { Id = "scene-2", Title = "Scene 2", Branches = new List<Branch>(), EchoReveals = new List<EchoReveal>() }
            }
        };
    }

    private static Scenario CreateTestScenarioWithEcho()
    {
        return new Scenario
        {
            Id = "scenario-1",
            Title = "Test",
            CoreAxes = new List<string> { "courage" },
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = "scene-1",
                    Title = "Scene 1",
                    Type = SceneType.Choice,
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = "Be brave",
                            NextSceneId = "scene-2",
                            EchoLog = new EchoLog
                            {
                                EchoTypeId = "moral",
                                Description = "Showed courage",
                                Strength = 1.0
                            }
                        }
                    },
                    EchoReveals = new List<EchoReveal>()
                },
                new() { Id = "scene-2", Title = "Scene 2", Branches = new List<Branch>(), EchoReveals = new List<EchoReveal>() }
            }
        };
    }

    private static Scenario CreateTestScenarioWithActiveCharacter()
    {
        return new Scenario
        {
            Id = "scenario-1",
            Title = "Test",
            CoreAxes = new List<string> { "courage" },
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
                        new() { Choice = "Be brave", NextSceneId = "scene-2" }
                    },
                    EchoReveals = new List<EchoReveal>()
                },
                new() { Id = "scene-2", Title = "Scene 2", Branches = new List<Branch>(), EchoReveals = new List<EchoReveal>() }
            }
        };
    }
}
