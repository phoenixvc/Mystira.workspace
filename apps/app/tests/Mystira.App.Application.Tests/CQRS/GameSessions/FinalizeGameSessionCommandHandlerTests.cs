using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Mystira.App.Application.CQRS.GameSessions.Commands;
using Mystira.Core.Ports.Data;
using Mystira.App.Application.Services;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Data.Repositories;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class FinalizeGameSessionCommandHandlerTests
{
    private readonly Mock<IGameSessionRepository> _sessionRepository;
    private readonly Mock<IUserProfileRepository> _profileRepository;
    private readonly Mock<IPlayerScenarioScoreRepository> _scoreRepository;
    private readonly Mock<IAxisScoringService> _scoringService;
    private readonly Mock<IBadgeAwardingService> _badgeService;
    private readonly Mock<ILogger> _logger;

    public FinalizeGameSessionCommandHandlerTests()
    {
        _sessionRepository = new Mock<IGameSessionRepository>();
        _profileRepository = new Mock<IUserProfileRepository>();
        _scoreRepository = new Mock<IPlayerScenarioScoreRepository>();
        _scoringService = new Mock<IAxisScoringService>();
        _badgeService = new Mock<IBadgeAwardingService>();
        _logger = new Mock<ILogger>();
    }

    [Fact]
    public async Task Handle_SessionNotFound_ReturnsEmptyResult()
    {
        // Arrange
        _sessionRepository.Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(GameSession));

        var command = new FinalizeGameSessionCommand("missing");

        // Act
        var result = await InvokeHandler(command);

        // Assert
        result.SessionId.Should().Be("missing");
        result.Awards.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_SingleProfile_ScoresAndAwardsBadges()
    {
        // Arrange
        var session = CreateSession(profileId: "profile-1");
        var profile = new UserProfile { Id = "profile-1", Name = "Alice" };
        var score = new PlayerScenarioScore
        {
            ProfileId = "profile-1",
            ScenarioId = session.ScenarioId,
            AxisScores = new Dictionary<string, int> { ["courage"] = 5 }
        };
        var badge = new UserBadge { BadgeName = "Brave Heart", Axis = "courage" };

        SetupSession(session);
        SetupProfile("profile-1", profile);
        _scoringService.Setup(s => s.ScoreSessionAsync(session, profile))
            .ReturnsAsync(score);
        _scoreRepository.Setup(r => r.GetByProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerScenarioScore> { score });
        _badgeService.Setup(b => b.AwardBadgesAsync(profile, It.IsAny<Dictionary<string, float>>()))
            .ReturnsAsync(new List<UserBadge> { badge });

        var command = new FinalizeGameSessionCommand(session.Id);

        // Act
        var result = await InvokeHandler(command);

        // Assert
        result.Awards.Should().HaveCount(1);
        result.Awards[0].ProfileId.Should().Be("profile-1");
        result.Awards[0].ProfileName.Should().Be("Alice");
        result.Awards[0].AlreadyPlayed.Should().BeFalse();
        result.Awards[0].NewBadges.Should().HaveCount(1);
        result.Awards[0].NewBadges[0].BadgeName.Should().Be("Brave Heart");
    }

    [Fact]
    public async Task Handle_AlreadyPlayed_MarksAlreadyPlayedTrue()
    {
        // Arrange
        var session = CreateSession(profileId: "profile-1");
        var profile = new UserProfile { Id = "profile-1", Name = "Alice" };

        SetupSession(session);
        SetupProfile("profile-1", profile);
        _scoringService.Setup(s => s.ScoreSessionAsync(session, profile))
            .ReturnsAsync(default(PlayerScenarioScore)); // Already scored
        _scoreRepository.Setup(r => r.GetByProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerScenarioScore>());
        _badgeService.Setup(b => b.AwardBadgesAsync(profile, It.IsAny<Dictionary<string, float>>()))
            .ReturnsAsync(new List<UserBadge>());

        var command = new FinalizeGameSessionCommand(session.Id);

        // Act
        var result = await InvokeHandler(command);

        // Assert
        result.Awards.Should().HaveCount(1);
        result.Awards[0].AlreadyPlayed.Should().BeTrue();
        result.Awards[0].NewBadges.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MultipleProfilesFromAssignments_ProcessesAll()
    {
        // Arrange
        var session = CreateSession(profileId: "profile-1");
        session.CharacterAssignments = new List<SessionCharacterAssignment>
        {
            new() { CharacterId = "char1", PlayerAssignment = new SessionPlayerAssignment { ProfileId = "profile-1" } },
            new() { CharacterId = "char2", PlayerAssignment = new SessionPlayerAssignment { ProfileId = "profile-2" } }
        };

        var profile1 = new UserProfile { Id = "profile-1", Name = "Alice" };
        var profile2 = new UserProfile { Id = "profile-2", Name = "Bob" };

        SetupSession(session);
        SetupProfile("profile-1", profile1);
        SetupProfile("profile-2", profile2);
        SetupDefaultScoring();

        var command = new FinalizeGameSessionCommand(session.Id);

        // Act
        var result = await InvokeHandler(command);

        // Assert
        result.Awards.Should().HaveCount(2);
        result.Awards.Select(a => a.ProfileName).Should().Contain("Alice");
        result.Awards.Select(a => a.ProfileName).Should().Contain("Bob");
    }

    [Fact]
    public async Task Handle_DuplicateProfileIds_DeduplicatesProfiles()
    {
        // Arrange - ProfileId and assignment both reference the same profile
        var session = CreateSession(profileId: "profile-1");
        session.CharacterAssignments = new List<SessionCharacterAssignment>
        {
            new() { CharacterId = "char1", PlayerAssignment = new SessionPlayerAssignment { ProfileId = "profile-1" } }
        };

        var profile = new UserProfile { Id = "profile-1", Name = "Alice" };

        SetupSession(session);
        SetupProfile("profile-1", profile);
        SetupDefaultScoring();

        var command = new FinalizeGameSessionCommand(session.Id);

        // Act
        var result = await InvokeHandler(command);

        // Assert - should only process profile once
        result.Awards.Should().HaveCount(1);
        _scoringService.Verify(s => s.ScoreSessionAsync(session, profile), Times.Once);
    }

    [Fact]
    public async Task Handle_MissingProfile_SkipsAndContinues()
    {
        // Arrange
        var session = CreateSession(profileId: "profile-1");
        session.CharacterAssignments = new List<SessionCharacterAssignment>
        {
            new() { CharacterId = "char2", PlayerAssignment = new SessionPlayerAssignment { ProfileId = "profile-missing" } }
        };

        var profile1 = new UserProfile { Id = "profile-1", Name = "Alice" };

        SetupSession(session);
        SetupProfile("profile-1", profile1);
        _profileRepository.Setup(r => r.GetByIdAsync("profile-missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(default(UserProfile));
        SetupDefaultScoring();

        var command = new FinalizeGameSessionCommand(session.Id);

        // Act
        var result = await InvokeHandler(command);

        // Assert - should only include the found profile
        result.Awards.Should().HaveCount(1);
        result.Awards[0].ProfileId.Should().Be("profile-1");
    }

    [Fact]
    public async Task Handle_CumulativeAxisScores_AggregatesAcrossScenarios()
    {
        // Arrange
        var session = CreateSession(profileId: "profile-1");
        var profile = new UserProfile { Id = "profile-1", Name = "Alice" };
        var newScore = new PlayerScenarioScore
        {
            AxisScores = new Dictionary<string, int> { ["courage"] = 3 }
        };
        var previousScore = new PlayerScenarioScore
        {
            AxisScores = new Dictionary<string, int> { ["courage"] = 2, ["wisdom"] = 1 }
        };

        SetupSession(session);
        SetupProfile("profile-1", profile);
        _scoringService.Setup(s => s.ScoreSessionAsync(session, profile))
            .ReturnsAsync(newScore);
        _scoreRepository.Setup(r => r.GetByProfileIdAsync("profile-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerScenarioScore> { newScore, previousScore });

        Dictionary<string, float>? capturedCumulative = null;
        _badgeService.Setup(b => b.AwardBadgesAsync(profile, It.IsAny<Dictionary<string, float>>()))
            .Callback<UserProfile, Dictionary<string, float>>((_, c) => capturedCumulative = c)
            .ReturnsAsync(new List<UserBadge>());

        var command = new FinalizeGameSessionCommand(session.Id);

        // Act
        await InvokeHandler(command);

        // Assert - cumulative should be sum of all scores
        capturedCumulative.Should().NotBeNull();
        capturedCumulative!["courage"].Should().Be(5.0f);
        capturedCumulative["wisdom"].Should().Be(1.0f);
    }

    [Fact]
    public async Task Handle_GuestAssignments_SkipsProfilesWithNoId()
    {
        // Arrange
        var session = CreateSession(profileId: "profile-1");
        session.CharacterAssignments = new List<SessionCharacterAssignment>
        {
            new() { CharacterId = "char1", PlayerAssignment = new SessionPlayerAssignment { ProfileId = "profile-1" } },
            new() { CharacterId = "char2", PlayerAssignment = new SessionPlayerAssignment { GuestName = "Guest", ProfileId = null } },
            new() { CharacterId = "char3", PlayerAssignment = null }
        };

        var profile = new UserProfile { Id = "profile-1", Name = "Alice" };

        SetupSession(session);
        SetupProfile("profile-1", profile);
        SetupDefaultScoring();

        var command = new FinalizeGameSessionCommand(session.Id);

        // Act
        var result = await InvokeHandler(command);

        // Assert - only the real profile is processed
        result.Awards.Should().HaveCount(1);
        result.Awards[0].ProfileId.Should().Be("profile-1");
    }

    [Fact]
    public async Task Handle_PropagatesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var session = CreateSession(profileId: "profile-1");
        var profile = new UserProfile { Id = "profile-1", Name = "Alice" };

        SetupSession(session);
        SetupProfile("profile-1", profile);
        SetupDefaultScoring();

        var command = new FinalizeGameSessionCommand(session.Id);

        // Act
        await FinalizeGameSessionCommandHandler.Handle(
            command, _sessionRepository.Object, _profileRepository.Object,
            _scoreRepository.Object, _scoringService.Object, _badgeService.Object,
            _logger.Object, cts.Token);

        // Assert - verify exact token propagation
        _sessionRepository.Verify(r => r.GetByIdAsync(session.Id, cts.Token), Times.Once);
        _profileRepository.Verify(r => r.GetByIdAsync("profile-1", cts.Token), Times.Once);
    }

    #region Helpers

    private Task<FinalizeGameSessionResult> InvokeHandler(FinalizeGameSessionCommand command)
    {
        return FinalizeGameSessionCommandHandler.Handle(
            command, _sessionRepository.Object, _profileRepository.Object,
            _scoreRepository.Object, _scoringService.Object, _badgeService.Object,
            _logger.Object, CancellationToken.None);
    }

    private void SetupSession(GameSession session)
    {
        _sessionRepository.Setup(r => r.GetByIdAsync(session.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
    }

    private void SetupProfile(string profileId, UserProfile profile)
    {
        _profileRepository.Setup(r => r.GetByIdAsync(profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(profile);
    }

    private void SetupDefaultScoring()
    {
        _scoringService.Setup(s => s.ScoreSessionAsync(It.IsAny<GameSession>(), It.IsAny<UserProfile>()))
            .ReturnsAsync(new PlayerScenarioScore { AxisScores = new Dictionary<string, int>() });
        _scoreRepository.Setup(r => r.GetByProfileIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PlayerScenarioScore>());
        _badgeService.Setup(b => b.AwardBadgesAsync(It.IsAny<UserProfile>(), It.IsAny<Dictionary<string, float>>()))
            .ReturnsAsync(new List<UserBadge>());
    }

    private static GameSession CreateSession(string profileId)
    {
        return new GameSession
        {
            Id = "session-1",
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = profileId,
            Status = SessionStatus.Completed,
            CharacterAssignments = new List<SessionCharacterAssignment>(),
            ChoiceHistory = new List<SessionChoice>(),
            EchoHistory = new List<EchoLog>(),
            CompassValues = new List<CompassTracking>()
        };
    }

    #endregion
}
