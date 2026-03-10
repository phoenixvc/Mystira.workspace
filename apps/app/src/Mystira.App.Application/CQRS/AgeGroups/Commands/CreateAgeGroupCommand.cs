using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.AgeGroups.Commands;

/// <summary>
/// Command to create a new age group.
/// </summary>
public record CreateAgeGroupCommand(string Name, string Value, int MinimumAge, int MaximumAge, string Description) : ICommand<AgeGroupDefinition>;
