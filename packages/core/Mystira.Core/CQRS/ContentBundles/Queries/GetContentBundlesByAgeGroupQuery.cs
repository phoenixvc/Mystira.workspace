using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.ContentBundles.Queries;

/// <summary>
/// Query to retrieve content bundles by age group
/// </summary>
public record GetContentBundlesByAgeGroupQuery(string AgeGroup) : IQuery<IEnumerable<ContentBundle>>;
