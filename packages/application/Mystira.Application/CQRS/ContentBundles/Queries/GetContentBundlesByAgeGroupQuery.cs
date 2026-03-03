using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.ContentBundles.Queries;

/// <summary>
/// Query to retrieve content bundles by age group
/// </summary>
/// <param name="AgeGroup">The age group to filter content bundles by.</param>
public record GetContentBundlesByAgeGroupQuery(string AgeGroup) : IQuery<IEnumerable<ContentBundle>>;
