using Mystira.Contracts.App.Requests.GameSessions;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;

namespace Mystira.App.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Validates StartGameSessionRequest before processing.
/// Extracted from StartGameSessionCommandHandler to reduce complexity.
/// </summary>
public static class StartGameSessionRequestValidator
{
    /// <summary>
    /// Validates the request and throws ValidationException if invalid.
    /// </summary>
    public static void Validate(StartGameSessionRequest request)
    {
        if (request == null)
        {
            throw new ValidationException("request", "request is required");
        }

        if (string.IsNullOrEmpty(request.ScenarioId))
        {
            throw new ValidationException("scenarioId", "ScenarioId is required");
        }

        if (string.IsNullOrEmpty(request.AccountId))
        {
            throw new ValidationException("accountId", "AccountId is required");
        }

        if (string.IsNullOrEmpty(request.ProfileId))
        {
            throw new ValidationException("profileId", "ProfileId is required");
        }

        if ((request.PlayerNames == null || !request.PlayerNames.Any())
            && (request.CharacterAssignments == null || !request.CharacterAssignments.Any()))
        {
            throw new ValidationException("input", "At least one player or character assignment is required");
        }
    }

    /// <summary>
    /// Validates that the scenario age group is compatible with the target age group.
    /// </summary>
    public static void ValidateAgeGroup(int scenarioMinimumAge, string targetAgeGroup)
    {
        var targetAge = AgeGroup.Parse(targetAgeGroup) ?? AgeGroup.MiddleChildhood;

        if (scenarioMinimumAge > targetAge.MinAge)
        {
            throw new ValidationException("input",
                $"Scenario minimum age ({scenarioMinimumAge}) exceeds target age group ({targetAgeGroup})");
        }
    }
}
