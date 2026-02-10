using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.GameSessions.Commands;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.App.Domain.Models;
using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class MakeChoiceCommandHandlerTests
{
    private readonly Mock<IGameSessionRepository> _repository;
    private readonly Mock<IScenarioRepository> _scenarioRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<MakeChoiceUseCase>> _logger;

    public MakeChoiceCommandHandlerTests()
    {
        _repository = new Mock<IGameSessionRepository>();
        _scenarioRepository = new Mock<IScenarioRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<MakeChoiceUseCase>>();
    }

    [Fact]
    public async Task Handle_DelegatesToUseCase_ReturnsSession()
    {
        // Arrange - set up UseCase dependencies for a valid choice
        var session = CreateActiveSession();
        var scenario = CreateScenarioWithChoiceBranch(session.ScenarioId, "scene1", "Go left", "scene2");

        _repository.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _scenarioRepository.Setup(r => r.GetByIdAsync(session.ScenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var useCase = new MakeChoiceUseCase(
            _repository.Object, _scenarioRepository.Object, _unitOfWork.Object, _logger.Object);

        var request = new MakeChoiceRequest
        {
            SessionId = session.Id,
            SceneId = "scene1",
            ChoiceText = "Go left",
            NextSceneId = "scene2"
        };

        // Act
        var result = await MakeChoiceCommandHandler.Handle(
            new MakeChoiceCommand(request), useCase, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ChoiceHistory.Should().HaveCount(1);
        result.ChoiceHistory[0].ChoiceText.Should().Be("Go left");
        result.CurrentSceneId.Should().Be("scene2");
    }

    [Fact]
    public async Task Handle_WithNonExistentSession_ReturnsNull()
    {
        _repository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var useCase = new MakeChoiceUseCase(
            _repository.Object, _scenarioRepository.Object, _unitOfWork.Object, _logger.Object);

        var request = new MakeChoiceRequest
        {
            SessionId = "missing",
            SceneId = "scene1",
            ChoiceText = "Go left",
            NextSceneId = "scene2"
        };

        var result = await MakeChoiceCommandHandler.Handle(
            new MakeChoiceCommand(request), useCase, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithCompletedSession_ThrowsInvalidOperationException()
    {
        var session = CreateActiveSession();
        session.Status = SessionStatus.Completed;

        _repository.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        var useCase = new MakeChoiceUseCase(
            _repository.Object, _scenarioRepository.Object, _unitOfWork.Object, _logger.Object);

        var request = new MakeChoiceRequest
        {
            SessionId = session.Id,
            SceneId = "scene1",
            ChoiceText = "Go left",
            NextSceneId = "scene2"
        };

        var act = () => MakeChoiceCommandHandler.Handle(
            new MakeChoiceCommand(request), useCase, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ValidatesScenarioExists()
    {
        var session = CreateActiveSession();
        _repository.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _scenarioRepository.Setup(r => r.GetByIdAsync(session.ScenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        var useCase = new MakeChoiceUseCase(
            _repository.Object, _scenarioRepository.Object, _unitOfWork.Object, _logger.Object);

        var request = new MakeChoiceRequest
        {
            SessionId = session.Id,
            SceneId = "scene1",
            ChoiceText = "Go left",
            NextSceneId = "scene2"
        };

        var act = () => MakeChoiceCommandHandler.Handle(
            new MakeChoiceCommand(request), useCase, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Scenario not found*");
    }

    [Fact]
    public async Task Handle_ValidatesSceneExistsInScenario()
    {
        var session = CreateActiveSession();
        var scenario = new Scenario
        {
            Id = session.ScenarioId,
            Title = "Test",
            Scenes = new List<Scene>() // empty - no scenes
        };

        _repository.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _scenarioRepository.Setup(r => r.GetByIdAsync(session.ScenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var useCase = new MakeChoiceUseCase(
            _repository.Object, _scenarioRepository.Object, _unitOfWork.Object, _logger.Object);

        var request = new MakeChoiceRequest
        {
            SessionId = session.Id,
            SceneId = "missing-scene",
            ChoiceText = "Go left",
            NextSceneId = "scene2"
        };

        var act = () => MakeChoiceCommandHandler.Handle(
            new MakeChoiceCommand(request), useCase, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Scene not found*");
    }

    private static GameSession CreateActiveSession()
    {
        return new GameSession
        {
            Id = Guid.NewGuid().ToString("N"),
            ScenarioId = "test-scenario",
            AccountId = "test-account",
            ProfileId = "test-profile",
            Status = SessionStatus.InProgress,
            StartTime = DateTime.UtcNow,
            CurrentSceneId = "scene1",
            ChoiceHistory = new List<SessionChoice>(),
            EchoHistory = new List<EchoLog>(),
            Achievements = new List<SessionAchievement>(),
            CompassValues = new Dictionary<string, CompassTracking>(),
            CharacterAssignments = new List<SessionCharacterAssignment>()
        };
    }

    private static Scenario CreateScenarioWithChoiceBranch(
        string scenarioId, string sceneId, string choiceText, string nextSceneId)
    {
        return new Scenario
        {
            Id = scenarioId,
            Title = "Test Scenario",
            CoreAxes = new List<CoreAxis>(),
            Scenes = new List<Scene>
            {
                new()
                {
                    Id = sceneId,
                    Title = "Scene 1",
                    Type = SceneType.Choice,
                    Branches = new List<Branch>
                    {
                        new()
                        {
                            Choice = choiceText,
                            NextSceneId = nextSceneId
                        }
                    }
                },
                new()
                {
                    Id = nextSceneId,
                    Title = "Scene 2",
                    Branches = new List<Branch>
                    {
                        new() { Choice = "Continue", NextSceneId = "scene3" }
                    }
                }
            }
        };
    }
}
