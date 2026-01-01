using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.AgeGroups.Commands;

/// <summary>
/// Command to update an existing age group.
/// </summary>
/// <param name="Id">The unique identifier of the age group to update.</param>
/// <param name="Name">The new name of the age group.</param>
/// <param name="Value">The new value representing the age group.</param>
/// <param name="MinimumAge">The new minimum age for this age group.</param>
/// <param name="MaximumAge">The new maximum age for this age group.</param>
/// <param name="Description">The new description of the age group.</param>
public record UpdateAgeGroupCommand(string Id, string Name, string Value, int MinimumAge, int MaximumAge, string Description) : ICommand<AgeGroupDefinition?>;
