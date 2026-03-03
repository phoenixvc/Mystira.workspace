using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Specifications;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.ContentBundles.Queries;

/// <summary>
/// Wolverine handler for GetContentBundlesByAgeGroupQuery
/// Retrieves content bundles filtered by age group
/// </summary>
public static class GetContentBundlesByAgeGroupQueryHandler
{
    /// <summary>
    /// Handles the GetContentBundlesByAgeGroupQuery.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="repository">The content bundle repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of content bundles filtered by the specified age group.</returns>
    public static async Task<IEnumerable<ContentBundle>> Handle(
        GetContentBundlesByAgeGroupQuery request,
        IContentBundleRepository repository,
        ILogger<GetContentBundlesByAgeGroupQuery> logger,
        CancellationToken ct)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.AgeGroup))
        {
            logger.LogWarning("Age group cannot be null or empty");
            throw new ArgumentException("Age group cannot be null or empty", nameof(request.AgeGroup));
        }

        // Create specification for reusable query logic
        var spec = new ContentBundlesByAgeGroupSpec(request.AgeGroup);

        // Execute query using repository
        var bundles = await repository.ListAsync(spec);

        logger.LogDebug("Retrieved {Count} bundles for age group {AgeGroup}",
            bundles.Count(), request.AgeGroup);

        return bundles;
    }
}
