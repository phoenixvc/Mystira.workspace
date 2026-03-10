using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.UseCases;

public class CreateGameSessionUseCaseTests
{
    private readonly Mock<IGameSessionRepository> _sessionRepository;
    private readonly Mock<IScenarioRepository> _scenarioRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<CreateGameSessionUseCase>> _logger;
    private readonly CreateGameSessionUseCase _useCase;

    public CreateGameSessionUseCaseTests()
    {
        _sessionRepository = new Mock<IGameSessionRepository>();
        _scenarioRepository = new Mock<IScenarioRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<CreateGameSessionUseCase>>();

        _useCase = new CreateGameSessionUseCase(
            _sessionRepository.Object,
            _scenarioRepository.Object,
            _unitOfWork.Object,
            _logger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_CreatesNewSession()
    {
        var scenario = CreateTestScenario();
        var request = new StartGameSessionRequest
        {
            ScenarioId = scenario.Id,
            AccountId = "account-1",
            ProfileId = "profile-1",
            TargetAgeGroup = "6-9",
            PlayerNames = new List<string> { "Player1" }
        };

        _scenarioRepository.Setup(r => r.GetByIdAsync(scenario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);
        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(
                scenario.Id, "account-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<GameSession>());

        var result = await _useCase.ExecuteAsync(request);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data!.ScenarioId.Should().Be(scenario.Id);
        result.Data.AccountId.Should().Be("account-1");
        result.Data.ProfileId.Should().Be("profile-1");
        result.Data.Status.Should().Be(SessionStatus.InProgress);
        result.Data.CurrentSceneId.Should().Be("scene1");
        result.Data.StartTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Data.Id.Should().NotBeNullOrEmpty();

        _sessionRepository.Verify(r => r.AddAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_InitializesCompassTrackingFromScenarioAxes()
    {
        var scenario = CreateTestScenario();
        var request = new StartGameSessionRequest
        {
            ScenarioId = scenario.Id,
            AccountId = "account-1",
            ProfileId = "profile-1",
            TargetAgeGroup = "6-9",
            PlayerNames = new List<string> { "Player1" }
        };

        _scenarioRepository.Setup(r => r.GetByIdAsync(scenario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);
        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(
                scenario.Id, "account-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<GameSession>());

        var result = await _useCase.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.CompassValues.Should().Contain(cv => cv.Axis == "courage");
        result.Data.CompassValues.Should().Contain(cv => cv.Axis == "honesty");
        result.Data.CompassValues.First(cv => cv.Axis == "courage").CurrentValue.Should().Be(0.0);
        result.Data.CompassValues.First(cv => cv.Axis == "honesty").CurrentValue.Should().Be(0.0);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentScenario_ReturnsFailure()
    {
        var request = new StartGameSessionRequest
        {
            ScenarioId = "missing-scenario",
            AccountId = "account-1",
            ProfileId = "profile-1",
            TargetAgeGroup = "6-9",
            PlayerNames = new List<string> { "Player1" }
        };

        _scenarioRepository.Setup(r => r.GetByIdAsync("missing-scenario", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));
        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(
                "missing-scenario", "account-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<GameSession>());

        var result = await _useCase.ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task ExecuteAsync_WithIncompatibleAgeGroup_ReturnsFailure()
    {
        var scenario = CreateTestScenario();
        scenario.MinimumAge = 13; // Requires 13+

        var request = new StartGameSessionRequest
        {
            ScenarioId = scenario.Id,
            AccountId = "account-1",
            ProfileId = "profile-1",
            TargetAgeGroup = "6-9", // Too young
            PlayerNames = new List<string> { "Player1" }
        };

        _scenarioRepository.Setup(r => r.GetByIdAsync(scenario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);
        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(
                scenario.Id, "account-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<GameSession>());

        var result = await _useCase.ExecuteAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("minimum age");
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingInProgressSession_ReusesIt()
    {
        var scenario = CreateTestScenario();
        var existingSession = new GameSession
        {
            Id = "old-session",
            ScenarioId = scenario.Id,
            AccountId = "account-1",
            ProfileId = "profile-1",
            Status = SessionStatus.InProgress,
            StartTime = DateTime.UtcNow.AddHours(-1),
            CurrentSceneId = "scene1"
        };

        var request = new StartGameSessionRequest
        {
            ScenarioId = scenario.Id,
            AccountId = "account-1",
            ProfileId = "profile-1",
            TargetAgeGroup = "6-9",
            PlayerNames = new List<string> { "Player1" }
        };

        _scenarioRepository.Setup(r => r.GetByIdAsync(scenario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);
        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(
                scenario.Id, "account-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { existingSession });

        var result = await _useCase.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be("old-session");
        result.Data.Status.Should().Be(SessionStatus.InProgress);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingPausedSession_ReusesIt()
    {
        var scenario = CreateTestScenario();
        var pausedSession = new GameSession
        {
            Id = "paused-session",
            ScenarioId = scenario.Id,
            AccountId = "account-1",
            ProfileId = "profile-1",
            Status = SessionStatus.Paused,
            StartTime = DateTime.UtcNow.AddHours(-2),
            CurrentSceneId = "scene1"
        };

        var request = new StartGameSessionRequest
        {
            ScenarioId = scenario.Id,
            AccountId = "account-1",
            ProfileId = "profile-1",
            TargetAgeGroup = "6-9",
            PlayerNames = new List<string> { "Player1" }
        };

        _scenarioRepository.Setup(r => r.GetByIdAsync(scenario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);
        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(
                scenario.Id, "account-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { pausedSession });

        var result = await _useCase.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be("paused-session");
    }

    [Fact]
    public async Task ExecuteAsync_SetsSceneCountFromScenario()
    {
        var scenario = CreateTestScenario();
        var request = new StartGameSessionRequest
        {
            ScenarioId = scenario.Id,
            AccountId = "account-1",
            ProfileId = "profile-1",
            TargetAgeGroup = "6-9",
            PlayerNames = new List<string> { "Player1" }
        };

        _scenarioRepository.Setup(r => r.GetByIdAsync(scenario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);
        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(
                scenario.Id, "account-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<GameSession>());

        var result = await _useCase.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.SceneCount.Should().Be(scenario.Scenes.Count);
    }

    [Fact]
    public async Task ExecuteAsync_WithPlayerNames_SetsPlayerNames()
    {
        var scenario = CreateTestScenario();
        var request = new StartGameSessionRequest
        {
            ScenarioId = scenario.Id,
            AccountId = "account-1",
            ProfileId = "profile-1",
            TargetAgeGroup = "6-9",
            PlayerNames = new List<string> { "Alice", "Bob" }
        };

        _scenarioRepository.Setup(r => r.GetByIdAsync(scenario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);
        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(
                scenario.Id, "account-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<GameSession>());

        var result = await _useCase.ExecuteAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.PlayerNames.Should().BeEquivalentTo(new[] { "Alice", "Bob" });
    }

    private static Scenario CreateTestScenario()
    {
        return new Scenario
        {
            Id = "test-scenario",
            Title = "Test Scenario",
            MinimumAge = 1,
            CoreAxes = new List<string>
            {
                "courage",
                "honesty"
            },
            Scenes = new List<Scene>
            {
                new() { Id = "scene1", Title = "Scene 1" },
                new() { Id = "scene2", Title = "Scene 2" }
            }
        };
    }
}
