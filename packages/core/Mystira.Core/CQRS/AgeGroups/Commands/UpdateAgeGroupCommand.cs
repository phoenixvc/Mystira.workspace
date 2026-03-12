using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.AgeGroups.Commands;

/// <summary>
/// Command to update an existing age group.
/// </summary>
public record UpdateAgeGroupCommand(string Id, string Name, string Value, int MinimumAge, int MaximumAge, string Description) : ICommand<AgeGroupDefinition?>;
