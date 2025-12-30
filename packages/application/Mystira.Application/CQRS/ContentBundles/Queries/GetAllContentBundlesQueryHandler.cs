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
