using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.GameSessions.Commands;
using Mystira.Application.Ports.Data;
using Mystira.App.Application.UseCases.GameSessions;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Contracts.App.Models;
using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class StartGameSessionCommandHandlerTests
{
    private readonly Mock<IGameSessionRepository> _sessionRepository;
    private readonly Mock<IScenarioRepository> _scenarioRepository;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly Mock<ILogger<CreateGameSessionUseCase>> _useCaseLogger;
    private readonly Mock<ILogger> _handlerLogger;
    private readonly CreateGameSessionUseCase _useCase;

    public StartGameSessionCommandHandlerTests()
    {
        _sessionRepository = new Mock<IGameSessionRepository>();
        _scenarioRepository = new Mock<IScenarioRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _useCaseLogger = new Mock<ILogger<CreateGameSessionUseCase>>();
        _handlerLogger = new Mock<ILogger>();
        _useCase = new CreateGameSessionUseCase(
            _sessionRepository.Object,
            _scenarioRepository.Object,
            _unitOfWork.Object,
            _useCaseLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_CreatesNewGameSession()
    {
        // Arrange
        var request = CreateValidRequest();
        var scenario = CreateTestScenario();

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());
        _scenarioRepository.Setup(r => r.GetByIdAsync(request.ScenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command,
            _useCase,
            _handlerLogger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ScenarioId.Should().Be(request.ScenarioId);
        result.AccountId.Should().Be(request.AccountId);
        result.ProfileId.Should().Be(request.ProfileId);
        result.Status.Should().Be(SessionStatus.InProgress);
        result.PlayerNames.Should().Contain("Player1");
        result.CompassValues.Should().NotBeEmpty();

        _sessionRepository.Verify(r => r.AddAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingActiveSession_ReusesExistingSession()
    {
        // Arrange
        var request = CreateValidRequest();
        var existingSession = new GameSession
        {
            Id = "existing-session-id",
            ScenarioId = request.ScenarioId,
            AccountId = request.AccountId,
            ProfileId = request.ProfileId,
            Status = SessionStatus.InProgress,
            StartTime = DateTime.UtcNow.AddMinutes(-10),
            PlayerNames = new List<string> { "ExistingPlayer" },
            ChoiceHistory = new List<SessionChoice>(),
            CompassValues = new List<CompassTracking>()
        };

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { existingSession });
        _scenarioRepository.Setup(r => r.GetByIdAsync(request.ScenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestScenario());

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command,
            _useCase,
            _handlerLogger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingSession.Id);
        _sessionRepository.Verify(r => r.AddAsync(It.IsAny<GameSession>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMissingScenarioId_ReturnsNull()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ScenarioId = "";
        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command,
            _useCase,
            _handlerLogger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithMissingAccountId_ReturnsNull()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AccountId = "";
        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command,
            _useCase,
            _handlerLogger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithMissingProfileId_ReturnsNull()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ProfileId = "";
        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command,
            _useCase,
            _handlerLogger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNoPlayersOrAssignments_ReturnsNull()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PlayerNames = new List<string>();
        request.CharacterAssignments = new List<CharacterAssignmentDto>();
        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command,
            _useCase,
            _handlerLogger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithScenarioAgeRestriction_ReturnsNull()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TargetAgeGroup = "6-9";
        var scenario = CreateTestScenario();
        scenario.MinimumAge = 12; // Higher than target age group

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());
        _scenarioRepository.Setup(r => r.GetByIdAsync(request.ScenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command,
            _useCase,
            _handlerLogger.Object,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithCharacterAssignments_SetsPlayerNamesFromAssignments()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PlayerNames = new List<string>(); // Empty
        request.CharacterAssignments = new List<CharacterAssignmentDto>
        {
            new CharacterAssignmentDto
            {
                CharacterId = "char1",
                CharacterName = "Hero",
                PlayerAssignment = new PlayerAssignmentDto
                {
                    Type = "profile",
                    ProfileName = "TestPlayer"
                }
            }
        };

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());
        _scenarioRepository.Setup(r => r.GetByIdAsync(request.ScenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestScenario());

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command,
            _useCase,
            _handlerLogger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PlayerNames.Should().Contain("TestPlayer");
        result.CharacterAssignments.Should().HaveCount(1);
        result.CharacterAssignments.First().CharacterName.Should().Be("Hero");
    }

    [Fact]
    public async Task Handle_WithScenarioAxes_InitializesCompassTracking()
    {
        // Arrange
        var request = CreateValidRequest();
        var scenario = CreateTestScenario();
        scenario.CoreAxes = new List<string>
        {
            "courage",
            "wisdom"
        };

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());
        _scenarioRepository.Setup(r => r.GetByIdAsync(request.ScenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command,
            _useCase,
            _handlerLogger.Object,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CompassValues.Should().Contain(cv => cv.Axis == "courage");
        result.CompassValues.Should().Contain(cv => cv.Axis == "wisdom");
        result.CompassValues.First(cv => cv.Axis == "courage").CurrentValue.Should().Be(0.0);
    }

    #region Duplicate Session Cleanup Tests

    [Fact]
    public async Task Handle_WithMultipleDuplicateSessions_DeletesEmptyDuplicates()
    {
        // Arrange
        var request = CreateValidRequest();
        var primary = CreateExistingSession(request, minutesAgo: 5);
        var emptyDuplicate1 = CreateExistingSession(request, minutesAgo: 10, id: "empty-dup-1");
        var emptyDuplicate2 = CreateExistingSession(request, minutesAgo: 15, id: "empty-dup-2");

        // Empty sessions: no choices, no echoes, no achievements, no CurrentSceneId
        emptyDuplicate1.CurrentSceneId = string.Empty;
        emptyDuplicate2.CurrentSceneId = string.Empty;

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { primary, emptyDuplicate1, emptyDuplicate2 });

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command, _useCase, _handlerLogger.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(primary.Id);
        _sessionRepository.Verify(r => r.DeleteAsync("empty-dup-1", It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepository.Verify(r => r.DeleteAsync("empty-dup-2", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_WithMultipleDuplicateSessions_AbandonsNonEmptyDuplicates()
    {
        // Arrange
        var request = CreateValidRequest();
        var primary = CreateExistingSession(request, minutesAgo: 5);
        var nonEmptyDuplicate = CreateExistingSession(request, minutesAgo: 10, id: "nonempty-dup");
        nonEmptyDuplicate.ChoiceHistory = new List<SessionChoice>
        {
            new SessionChoice { SceneId = "scene1", ChoiceText = "Be brave" }
        };

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { primary, nonEmptyDuplicate });

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command, _useCase, _handlerLogger.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(primary.Id);
        nonEmptyDuplicate.Status.Should().Be(SessionStatus.Abandoned);
        nonEmptyDuplicate.EndTime.Should().NotBeNull();
        _sessionRepository.Verify(r => r.UpdateAsync(nonEmptyDuplicate, It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepository.Verify(r => r.DeleteAsync("nonempty-dup", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMixedDuplicates_DeletesEmptyAndAbandonsNonEmpty()
    {
        // Arrange
        var request = CreateValidRequest();
        var primary = CreateExistingSession(request, minutesAgo: 5);
        var emptyDuplicate = CreateExistingSession(request, minutesAgo: 10, id: "empty-dup");
        emptyDuplicate.CurrentSceneId = string.Empty;

        var nonEmptyDuplicate = CreateExistingSession(request, minutesAgo: 15, id: "nonempty-dup");
        nonEmptyDuplicate.CurrentSceneId = "scene1"; // Has a current scene => non-empty

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { primary, emptyDuplicate, nonEmptyDuplicate });

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command, _useCase, _handlerLogger.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(primary.Id);
        _sessionRepository.Verify(r => r.DeleteAsync("empty-dup", It.IsAny<CancellationToken>()), Times.Once);
        nonEmptyDuplicate.Status.Should().Be(SessionStatus.Abandoned);
        _sessionRepository.Verify(r => r.UpdateAsync(nonEmptyDuplicate, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Session Hydration Tests

    [Fact]
    public async Task Handle_ExistingSession_HydratesCharacterAssignmentsFromRequest()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CharacterAssignments = new List<CharacterAssignmentDto>
        {
            new CharacterAssignmentDto
            {
                CharacterId = "char1",
                CharacterName = "Hero",
                PlayerAssignment = new PlayerAssignmentDto
                {
                    Type = "Profile",
                    ProfileName = "TestPlayer"
                }
            }
        };

        var existingSession = CreateExistingSession(request, minutesAgo: 5);
        existingSession.CharacterAssignments = new List<SessionCharacterAssignment>(); // Empty

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { existingSession });

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command, _useCase, _handlerLogger.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CharacterAssignments.Should().HaveCount(1);
        result.CharacterAssignments.First().CharacterId.Should().Be("char1");
        result.CharacterAssignments.First().CharacterName.Should().Be("Hero");
        _sessionRepository.Verify(r => r.UpdateAsync(existingSession, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingSession_HydratesPlayerNamesFromRequest()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PlayerNames = new List<string> { "Alice", "Bob" };

        var existingSession = CreateExistingSession(request, minutesAgo: 5);
        existingSession.PlayerNames = new List<string>(); // Empty

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { existingSession });

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command, _useCase, _handlerLogger.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.PlayerNames.Should().BeEquivalentTo(new[] { "Alice", "Bob" });
        _sessionRepository.Verify(r => r.UpdateAsync(existingSession, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingSession_DerivesPlayerNamesFromCharacterAssignments()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PlayerNames = null;
        request.CharacterAssignments = new List<CharacterAssignmentDto>
        {
            new CharacterAssignmentDto
            {
                CharacterId = "char1",
                CharacterName = "Hero",
                PlayerAssignment = new PlayerAssignmentDto
                {
                    Type = "Profile",
                    ProfileName = "Alice"
                }
            },
            new CharacterAssignmentDto
            {
                CharacterId = "char2",
                CharacterName = "Sidekick",
                PlayerAssignment = new PlayerAssignmentDto
                {
                    Type = "Guest",
                    GuestName = "Bob"
                }
            }
        };

        var existingSession = CreateExistingSession(request, minutesAgo: 5);
        existingSession.PlayerNames = new List<string>(); // Empty
        existingSession.CharacterAssignments = new List<SessionCharacterAssignment>(); // Empty, will be hydrated

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { existingSession });

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command, _useCase, _handlerLogger.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CharacterAssignments.Should().HaveCount(2);
        result.PlayerNames.Should().Contain("Alice");
        result.PlayerNames.Should().Contain("Bob");
    }

    [Fact]
    public async Task Handle_ExistingSession_HydratesCurrentSceneIdFromScenario()
    {
        // Arrange
        var request = CreateValidRequest();
        var scenario = CreateTestScenario();

        var existingSession = CreateExistingSession(request, minutesAgo: 5);
        existingSession.CurrentSceneId = string.Empty; // Missing

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { existingSession });
        _scenarioRepository.Setup(r => r.GetByIdAsync(request.ScenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command, _useCase, _handlerLogger.Object, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CurrentSceneId.Should().Be("scene1");
        result.SceneCount.Should().Be(1);
        _sessionRepository.Verify(r => r.UpdateAsync(existingSession, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region CancellationToken Propagation Tests

    [Fact]
    public async Task Handle_PropagatesCancellationToken_ToNewSessionRepositoryCalls()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var request = CreateValidRequest();
        var scenario = CreateTestScenario();

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());
        _scenarioRepository.Setup(r => r.GetByIdAsync(request.ScenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario);

        var command = new StartGameSessionCommand(request);

        // Act
        await StartGameSessionCommandHandler.Handle(
            command, _useCase, _handlerLogger.Object, cts.Token);

        // Assert - verify the exact token was passed
        _sessionRepository.Verify(r => r.AddAsync(It.IsAny<GameSession>(), cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_PropagatesCancellationToken_ToDuplicateCleanupCalls()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var request = CreateValidRequest();

        var primary = CreateExistingSession(request, minutesAgo: 5);
        var emptyDuplicate = CreateExistingSession(request, minutesAgo: 10, id: "empty-dup");
        emptyDuplicate.CurrentSceneId = string.Empty;

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession> { primary, emptyDuplicate });

        var command = new StartGameSessionCommand(request);

        // Act
        await StartGameSessionCommandHandler.Handle(
            command, _useCase, _handlerLogger.Object, cts.Token);

        // Assert - verify the exact token was passed to cleanup calls
        _sessionRepository.Verify(r => r.DeleteAsync("empty-dup", cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.AtLeastOnce);
    }

    #endregion

    #region Missing Scenario Tests

    [Fact]
    public async Task Handle_WithMissingScenario_ReturnsNull()
    {
        // Arrange
        var request = CreateValidRequest();

        _sessionRepository.Setup(r => r.GetActiveSessionsByScenarioAndAccountAsync(request.ScenarioId, request.AccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<GameSession>());
        _scenarioRepository.Setup(r => r.GetByIdAsync(request.ScenarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(Scenario));

        var command = new StartGameSessionCommand(request);

        // Act
        var result = await StartGameSessionCommandHandler.Handle(
            command, _useCase, _handlerLogger.Object, CancellationToken.None);

        // Assert - UseCase returns Failure when scenario not found, handler returns null
        result.Should().BeNull();
    }

    #endregion

    #region Helpers

    private static StartGameSessionRequest CreateValidRequest()
    {
        return new StartGameSessionRequest
        {
            ScenarioId = "test-scenario-id",
            AccountId = "test-account-id",
            ProfileId = "test-profile-id",
            PlayerNames = new List<string> { "Player1" },
            TargetAgeGroup = "6-9"
        };
    }

    private static Scenario CreateTestScenario()
    {
        return new Scenario
        {
            Id = "test-scenario-id",
            Title = "Test Scenario",
            MinimumAge = 6,
            CoreAxes = new List<string> { "courage" },
            Scenes = new List<Scene>
            {
                new Scene { Id = "scene1", Title = "Opening Scene" }
            }
        };
    }

    private static GameSession CreateExistingSession(StartGameSessionRequest request, int minutesAgo, string? id = null)
    {
        return new GameSession
        {
            Id = id ?? $"session-{minutesAgo}min-ago",
            ScenarioId = request.ScenarioId,
            AccountId = request.AccountId,
            ProfileId = request.ProfileId,
            Status = SessionStatus.InProgress,
            StartTime = DateTime.UtcNow.AddMinutes(-minutesAgo),
            PlayerNames = new List<string> { "Player1" },
            CurrentSceneId = "scene1",
            ChoiceHistory = new List<SessionChoice>(),
            EchoHistory = new List<EchoLog>(),
            Achievements = new List<SessionAchievement>(),
            CompassValues = new List<CompassTracking>()
        };
    }

    #endregion
}
