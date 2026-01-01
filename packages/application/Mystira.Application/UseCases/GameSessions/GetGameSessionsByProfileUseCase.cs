using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.GameSessions;

/// <summary>
/// Use case for retrieving game sessions by profile ID
/// </summary>
public class GetGameSessionsByProfileUseCase
{
    private readonly IGameSessionRepository _repository;
    private readonly ILogger<GetGameSessionsByProfileUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGameSessionsByProfileUseCase"/> class.
    /// </summary>
    /// <param name="repository">The game session repository.</param>
    /// <param name="logger">The logger instance.</param>
    public GetGameSessionsByProfileUseCase(
        IGameSessionRepository repository,
        ILogger<GetGameSessionsByProfileUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all game sessions for the specified profile.
    /// </summary>
    /// <param name="profileId">The profile identifier.</param>
    /// <returns>A list of game sessions for the profile.</returns>
    public async Task<List<GameSession>> ExecuteAsync(string profileId)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            throw new ArgumentException("Profile ID cannot be null or empty", nameof(profileId));
        }

        var sessions = await _repository.GetByProfileIdAsync(profileId);
        var sessionList = sessions.ToList();

        _logger.LogInformation("Retrieved {Count} game sessions for profile {ProfileId}", sessionList.Count, profileId);

        return sessionList;
    }
}

