using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;
using System.Threading;

namespace Mystira.App.Application.UseCases.Badges;

/// <summary>
/// Use case for retrieving a badge by ID
/// </summary>
public class GetBadgeUseCase
{
    private readonly IUserBadgeRepository _repository;
    private readonly ILogger<GetBadgeUseCase> _logger;

    public GetBadgeUseCase(
        IUserBadgeRepository repository,
        ILogger<GetBadgeUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserBadge?> ExecuteAsync(string badgeId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(badgeId))
        {
            throw new ValidationException("badgeId", "badgeId is required");
        }

        var badge = await _repository.GetByIdAsync(badgeId, ct);

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

