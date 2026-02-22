using FluentAssertions;
using Mystira.App.Application.CQRS.GameSessions.Commands;
using Mystira.Contracts.App.Models;
using Mystira.Contracts.App.Requests.GameSessions;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class StartGameSessionRequestValidatorTests
{
    [Fact]
    public void Validate_WithNullRequest_ThrowsArgumentNullException()
    {
        var act = () => StartGameSessionRequestValidator.Validate(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("request");
    }

    [Fact]
    public void Validate_WithEmptyScenarioId_ThrowsArgumentException()
    {
        var request = new StartGameSessionRequest
        {
            ScenarioId = string.Empty,
            AccountId = "account-1",
            ProfileId = "profile-1",
            PlayerNames = new List<string> { "Player1" }
        };

        var act = () => StartGameSessionRequestValidator.Validate(request);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*ScenarioId*");
    }

    [Fact]
    public void Validate_WithEmptyAccountId_ThrowsArgumentException()
    {
        var request = new StartGameSessionRequest
        {
            ScenarioId = "scenario-1",
            AccountId = string.Empty,
            ProfileId = "profile-1",
            PlayerNames = new List<string> { "Player1" }
        };

        var act = () => StartGameSessionRequestValidator.Validate(request);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*AccountId*");
    }

    [Fact]
    public void Validate_WithEmptyProfileId_ThrowsArgumentException()
    {
        var request = new StartGameSessionRequest
        {
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = string.Empty,
            PlayerNames = new List<string> { "Player1" }
        };

        var act = () => StartGameSessionRequestValidator.Validate(request);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*ProfileId*");
    }

    [Fact]
    public void Validate_WithNoPlayersAndNoAssignments_ThrowsArgumentException()
    {
        var request = new StartGameSessionRequest
        {
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = "profile-1",
            PlayerNames = new List<string>(),
            CharacterAssignments = new List<CharacterAssignmentDto>()
        };

        var act = () => StartGameSessionRequestValidator.Validate(request);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*player or character assignment*");
    }

    [Fact]
    public void Validate_WithPlayerNames_DoesNotThrow()
    {
        var request = new StartGameSessionRequest
        {
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = "profile-1",
            PlayerNames = new List<string> { "Player1" }
        };

        var act = () => StartGameSessionRequestValidator.Validate(request);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithCharacterAssignments_DoesNotThrow()
    {
        var request = new StartGameSessionRequest
        {
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = "profile-1",
            CharacterAssignments = new List<CharacterAssignmentDto>
            {
                new CharacterAssignmentDto { CharacterId = "char-1", CharacterName = "Hero" }
            }
        };

        var act = () => StartGameSessionRequestValidator.Validate(request);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithNullPlayerNamesAndNullAssignments_ThrowsArgumentException()
    {
        var request = new StartGameSessionRequest
        {
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = "profile-1",
            PlayerNames = null,
            CharacterAssignments = null
        };

        var act = () => StartGameSessionRequestValidator.Validate(request);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*player or character assignment*");
    }

    [Theory]
    [InlineData(6, "6-9")]   // Exact match: scenario min == target min
    [InlineData(3, "6-9")]   // Scenario allows younger: scenario min < target min
    [InlineData(1, "10-12")] // Scenario allows much younger
    [InlineData(9, "10-12")] // Scenario max within target range
    public void ValidateAgeGroup_WhenScenarioMinIsLessOrEqual_DoesNotThrow(int scenarioMin, string targetAgeGroup)
    {
        var act = () => StartGameSessionRequestValidator.ValidateAgeGroup(scenarioMin, targetAgeGroup);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(12, "6-9")]   // Scenario too mature for target
    [InlineData(10, "6-9")]   // Scenario min exceeds target min
    [InlineData(18, "10-12")] // Teens scenario for preteens
    public void ValidateAgeGroup_WhenScenarioMinExceedsTargetMin_ThrowsArgumentException(int scenarioMin, string targetAgeGroup)
    {
        var act = () => StartGameSessionRequestValidator.ValidateAgeGroup(scenarioMin, targetAgeGroup);

        act.Should().Throw<ArgumentException>()
            .WithMessage($"*{scenarioMin}*{targetAgeGroup}*");
    }

    [Fact]
    public void ValidateAgeGroup_WithInvalidAgeGroup_DefaultsTo6_9()
    {
        // Invalid age group string defaults to AgeGroup(6, 9), so scenario min 6 should pass
        var act = () => StartGameSessionRequestValidator.ValidateAgeGroup(6, "invalid");

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateAgeGroup_WithInvalidAgeGroup_RejectsHigherMin()
    {
        // Invalid age group defaults to AgeGroup(6, 9), so scenario min 10 should fail
        var act = () => StartGameSessionRequestValidator.ValidateAgeGroup(10, "invalid");

        act.Should().Throw<ArgumentException>();
    }
}
