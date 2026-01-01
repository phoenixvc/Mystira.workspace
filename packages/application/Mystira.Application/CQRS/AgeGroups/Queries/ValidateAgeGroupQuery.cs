namespace Mystira.Application.CQRS.AgeGroups.Queries;

/// <summary>
/// Query to validate if an age group value exists.
/// </summary>
/// <param name="Value">The value of the age group to validate.</param>
public record ValidateAgeGroupQuery(string Value) : IQuery<bool>;
