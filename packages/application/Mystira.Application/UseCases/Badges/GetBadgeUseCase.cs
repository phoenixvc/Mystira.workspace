using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.UseCases.Badges;

/// <summary>
/// Use case for retrieving a badge by ID
/// </summary>
public class GetBadgeUseCase
{
    private readonly IUserBadgeRepository _repository;
    private readonly ILogger<GetBadgeUseCase> _logger;

    /// <summary>Initializes a new instance of the <see cref="GetBadgeUseCase"/> class.</summary>
    /// <param name="repository">The user badge repository.</param>
    /// <param name="logger">The logger.</param>
    public GetBadgeUseCase(
        IUserBadgeRepository repository,
        ILogger<GetBadgeUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>Retrieves a badge by its identifier.</summary>
    /// <param name="badgeId">The badge identifier.</param>
    /// <returns>The badge if found; otherwise, null.</returns>
    public async Task<UserBadge?> ExecuteAsync(string badgeId)
    {
        if (string.IsNullOrWhiteSpace(badgeId))
        {
            throw new ArgumentException("Badge ID cannot be null or empty", nameof(badgeId));
        }

        var badge = await _repository.GetByIdAsync(badgeId);

        if (badge == null)
        {
            _logger.LogWarning("Badge not found: {BadgeId}", badgeId);
        }
        else
        {
            _logger.LogDebug("Retrieved badge: {BadgeId}", badgeId);
        }

        return badge;
    }
}

