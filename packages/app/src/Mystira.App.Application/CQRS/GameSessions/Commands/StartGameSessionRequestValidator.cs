using Mystira.Contracts.App.Requests.GameSessions;

namespace Mystira.App.Application.CQRS.GameSessions.Commands;

/// <summary>
/// Validates StartGameSessionRequest before processing.
/// Extracted from StartGameSessionCommandHandler to reduce complexity.
/// </summary>
public static class StartGameSessionRequestValidator
{
    /// <summary>
    /// Validates the request and throws ArgumentException if invalid.
    /// </summary>
    public static void Validate(StartGameSessionRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrEmpty(request.ScenarioId))
        {
            throw new ArgumentException("ScenarioId is required", nameof(request));
        }

        if (string.IsNullOrEmpty(request.AccountId))
        {
            throw new ArgumentException("AccountId is required", nameof(request));
        }

        if (string.IsNullOrEmpty(request.ProfileId))
        {
            throw new ArgumentException("ProfileId is required", nameof(request));
        }

        if ((request.PlayerNames == null || !request.PlayerNames.Any())
            && (request.CharacterAssignments == null || !request.CharacterAssignments.Any()))
        {
            throw new ArgumentException("At least one player or character assignment is required", nameof(request));
        }
    }

    /// <summary>
    /// Validates that the scenario age group is compatible with the target age group.
    /// </summary>
    public static void ValidateAgeGroup(int scenarioMinimumAge, string targetAgeGroup)
    {
        var targetAge = Domain.Models.AgeGroup.Parse(targetAgeGroup) ?? Domain.Models.AgeGroup.Default;

        if (scenarioMinimumAge > targetAge.MinimumAge)
        {
            throw new ArgumentException(
                $"Scenario minimum age ({scenarioMinimumAge}) exceeds target age group ({targetAgeGroup})");
        }
    }
}
