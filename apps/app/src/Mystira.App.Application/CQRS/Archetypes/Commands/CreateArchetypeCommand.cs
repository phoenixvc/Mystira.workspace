using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Application.CQRS.Archetypes.Commands;

/// <summary>
/// Command to create a new archetype.
/// </summary>
public record CreateArchetypeCommand(string Name, string Description) : ICommand<ArchetypeDefinition>;
