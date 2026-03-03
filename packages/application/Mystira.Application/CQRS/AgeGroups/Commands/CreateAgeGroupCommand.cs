using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.AgeGroups.Commands;

/// <summary>
/// Command to create a new age group.
/// </summary>
/// <param name="Name">The name of the age group.</param>
/// <param name="Value">The value representing the age group.</param>
/// <param name="MinimumAge">The minimum age for this age group.</param>
/// <param name="MaximumAge">The maximum age for this age group.</param>
/// <param name="Description">The description of the age group.</param>
public record CreateAgeGroupCommand(string Name, string Value, int MinimumAge, int MaximumAge, string Description) : ICommand<AgeGroupDefinition>;
