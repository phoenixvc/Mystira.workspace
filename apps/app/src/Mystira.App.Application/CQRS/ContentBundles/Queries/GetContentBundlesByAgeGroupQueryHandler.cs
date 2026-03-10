using Microsoft.Extensions.Logging;
using Mystira.App.Application.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;
using Mystira.Shared.Exceptions;

namespace Mystira.App.Application.CQRS.ContentBundles.Queries;

/// <summary>
/// Wolverine handler for GetContentBundlesByAgeGroupQuery
/// Retrieves content bundles filtered by age group
/// </summary>
public static class GetContentBundlesByAgeGroupQueryHandler
{
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
            throw new ValidationException("ageGroup", "Age group cannot be null or empty");
        }

        // Execute query using repository
        var bundles = await repository.GetByAgeGroupAsync(request.AgeGroup, ct);

        logger.LogDebug("Retrieved {Count} bundles for age group {AgeGroup}",
            bundles.Count(), request.AgeGroup);

        return bundles;
    }
}
