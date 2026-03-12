using Mystira.Domain.Models;

namespace Mystira.Core.CQRS.ContentBundles.Queries;

/// <summary>
/// Query to retrieve all content bundles
/// </summary>
public record GetAllContentBundlesQuery() : IQuery<IEnumerable<ContentBundle>>;
