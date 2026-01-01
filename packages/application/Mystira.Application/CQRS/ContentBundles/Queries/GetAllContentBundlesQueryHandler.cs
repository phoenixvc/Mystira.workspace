using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Application.Specifications;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.ContentBundles.Queries;

/// <summary>
/// Wolverine handler for GetAllContentBundlesQuery
/// Retrieves all active content bundles
/// </summary>
public static class GetAllContentBundlesQueryHandler
{
    /// <summary>
    /// Handles the GetAllContentBundlesQuery.
    /// </summary>
    /// <param name="request">The query to handle.</param>
    /// <param name="repository">The content bundle repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of all active content bundles.</returns>
    public static async Task<IEnumerable<ContentBundle>> Handle(
        GetAllContentBundlesQuery request,
        IContentBundleRepository repository,
        ILogger<GetAllContentBundlesQuery> logger,
        CancellationToken ct)
    {
        // Use specification for consistent filtering
        var spec = new ActiveContentBundlesSpec();

        // Execute query
        var bundles = await repository.ListAsync(spec);

        logger.LogDebug("Retrieved {Count} content bundles", bundles.Count());

        return bundles;
    }
}
