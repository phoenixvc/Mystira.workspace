using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.UserProfiles;

/// <summary>
/// Use case for retrieving a user profile by ID
/// </summary>
public class GetUserProfileUseCase
{
    private readonly IUserProfileRepository _repository;
    private readonly ILogger<GetUserProfileUseCase> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUserProfileUseCase"/> class.
    /// </summary>
    /// <param name="repository">The user profile repository.</param>
    /// <param name="logger">The logger instance.</param>
    public GetUserProfileUseCase(
        IUserProfileRepository repository,
        ILogger<GetUserProfileUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a user profile by its unique identifier.
    /// </summary>
    /// <param name="id">The user profile identifier.</param>
    /// <returns>The user profile if found; otherwise, null.</returns>
    public async Task<UserProfile?> ExecuteAsync(string id)
    {
        var profile = await _repository.GetByIdAsync(id);
        if (profile == null)
        {
            _logger.LogDebug("Profile not found: {ProfileId}", id);
        }
        return profile;
    }
}

