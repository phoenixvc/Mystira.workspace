using Mystira.Shared.CQRS;
using Mystira.Application.Interfaces;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.BadgeConfigurations.Queries;

/// <summary>
/// Query to retrieve a badge configuration by ID.
/// </summary>
/// <param name="Id">The unique identifier of the badge configuration.</param>
public record GetBadgeConfigurationQuery(string Id)
    : IQuery<BadgeConfiguration?>, ICacheableQuery
{
    /// <summary>
    /// Gets the cache key for storing this query result.
    /// </summary>
    public string CacheKey => $"BadgeConfigurations:Id:{Id}";
}

/// <summary>
/// Handler for processing GetBadgeConfigurationQuery requests.
/// </summary>
public sealed class GetBadgeConfigurationQueryHandler
{
    private readonly IRepository<BadgeConfiguration> _repository;

    /// <summary>
    /// Initializes a new instance of the GetBadgeConfigurationQueryHandler class.
    /// </summary>
    /// <param name="repository">The repository for accessing badge configuration data.</param>
    public GetBadgeConfigurationQueryHandler(IRepository<BadgeConfiguration> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Handles the query to retrieve a badge configuration by ID.
    /// </summary>
    /// <param name="request">The query request containing the badge configuration ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The badge configuration if found; otherwise, null.</returns>
    public async Task<BadgeConfiguration?> Handle(GetBadgeConfigurationQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id);
        if (entity == null) return null;
        return Clone(entity);
    }

    private static BadgeConfiguration Clone(BadgeConfiguration s) => new()
    {
        Id = s.Id,
        Name = s.Name,
        Message = s.Message,
        Axis = s.Axis,
        Threshold = s.Threshold,
        ImageId = s.ImageId,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
