namespace Mystira.Core.CQRS.AgeGroups.Queries;

/// <summary>
/// Query to validate if an age group value exists.
/// </summary>
public record ValidateAgeGroupQuery(string Value) : IQuery<bool>;
