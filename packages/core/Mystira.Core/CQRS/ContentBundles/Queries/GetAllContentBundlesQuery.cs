using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.ContentBundles.Queries;

/// <summary>
/// Query to retrieve all content bundles
/// </summary>
public record GetAllContentBundlesQuery() : IQuery<IEnumerable<ContentBundle>>;
