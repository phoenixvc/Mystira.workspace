using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.ContentBundles.Queries;

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
        // Get all content bundles
        var bundles = await repository.GetAllAsync(ct);

        logger.LogDebug("Retrieved {Count} content bundles", bundles.Count());

        return bundles;
    }
}
