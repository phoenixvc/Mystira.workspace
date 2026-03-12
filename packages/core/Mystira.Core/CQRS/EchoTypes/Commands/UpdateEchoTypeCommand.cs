using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.EchoTypes.Commands;

/// <summary>
/// Command to update an existing echo type.
/// </summary>
public record UpdateEchoTypeCommand(string Id, string Name, string Description, string Category = "") : ICommand<EchoTypeDefinition?>;
