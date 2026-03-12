using Mystira.Shared.Exceptions;
using FluentAssertions;
using Mystira.App.Application.CQRS.GameSessions.Commands;
using Mystira.Contracts.App.Models;
using Mystira.Contracts.App.Requests.GameSessions;

namespace Mystira.App.Application.Tests.CQRS.GameSessions;

public class StartGameSessionRequestValidatorTests
{
    [Fact]
    public void Validate_WithNullRequest_ThrowsValidationException()
    {
        var act = () => StartGameSessionRequestValidator.Validate(null!);

        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void Validate_WithEmptyScenarioId_ThrowsValidationException()
    {
        var request = new StartGameSessionRequest
        {
            ScenarioId = string.Empty,
            AccountId = "account-1",
            ProfileId = "profile-1",
            PlayerNames = new List<string> { "Player1" }
        };

        var act = () => StartGameSessionRequestValidator.Validate(request);

        act.Should().Throw<ValidationException>()
            .WithMessage("*ScenarioId*");
    }

    [Fact]
    public void Validate_WithEmptyAccountId_ThrowsValidationException()
    {
        var request = new StartGameSessionRequest
        {
            ScenarioId = "scenario-1",
            AccountId = string.Empty,
            ProfileId = "profile-1",
            PlayerNames = new List<string> { "Player1" }
        };

        var act = () => StartGameSessionRequestValidator.Validate(request);

        act.Should().Throw<ValidationException>()
            .WithMessage("*AccountId*");
    }

    [Fact]
    public void Validate_WithEmptyProfileId_ThrowsValidationException()
    {
        var request = new StartGameSessionRequest
        {
            ScenarioId = "scenario-1",
            AccountId = "account-1",
            ProfileId = string.Empty,
            PlayerNames = new List<string> { "Player1" }
        };

        var act = () => StartGameSessionRequestValidator.Validate(request);

        act.Should().Throw<ValidationException>()
            .WithMessage("*ProfileId*");
    }

    [Fact]
    public void Validate_WithNoPlayersAndNoAssignments_ThrowsValidationException()
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

        act.Should().Throw<ValidationException>()
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
    public void Validate_WithNullPlayerNamesAndNullAssignments_ThrowsValidationException()
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

        act.Should().Throw<ValidationException>()
            .WithMessage("*player or character assignment*");
    }

    [Theory]
    [InlineData(7, "middle_childhood")]   // Exact match: scenario min == target min
    [InlineData(3, "middle_childhood")]   // Scenario allows younger: scenario min < target min
    [InlineData(1, "preteen")] // Scenario allows much younger
    [InlineData(9, "preteen")] // Scenario max within target range
    public void ValidateAgeGroup_WhenScenarioMinIsLessOrEqual_DoesNotThrow(int scenarioMin, string targetAgeGroup)
    {
        var act = () => StartGameSessionRequestValidator.ValidateAgeGroup(scenarioMin, targetAgeGroup);

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(12, "middle_childhood")]   // Scenario too mature for target
    [InlineData(10, "middle_childhood")]   // Scenario min exceeds target min
    [InlineData(18, "preteen")] // Teens scenario for preteens
    public void ValidateAgeGroup_WhenScenarioMinExceedsTargetMin_ThrowsValidationException(int scenarioMin, string targetAgeGroup)
    {
        var act = () => StartGameSessionRequestValidator.ValidateAgeGroup(scenarioMin, targetAgeGroup);

        act.Should().Throw<ValidationException>()
            .WithMessage($"*{scenarioMin}*{targetAgeGroup}*");
    }

    [Fact]
    public void ValidateAgeGroup_WithInvalidAgeGroup_DefaultsToMiddleChildhood()
    {
        // Invalid age group string defaults to MiddleChildhood (min=7), so scenario min 7 should pass
        var act = () => StartGameSessionRequestValidator.ValidateAgeGroup(7, "invalid");

        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateAgeGroup_WithInvalidAgeGroup_RejectsHigherMin()
    {
        // Invalid age group defaults to MiddleChildhood (min=7), so scenario min 10 should fail (10 > 7)
        var act = () => StartGameSessionRequestValidator.ValidateAgeGroup(10, "invalid");

        act.Should().Throw<ValidationException>();
    }
}
