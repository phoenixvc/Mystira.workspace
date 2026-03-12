using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.EchoTypes.Commands;

/// <summary>
/// Command to create a new echo type.
/// </summary>
public record CreateEchoTypeCommand(string Name, string Description, string Category = "") : ICommand<EchoTypeDefinition>;

/// <summary>
/// Allowed echo type categories.
/// </summary>
public static class EchoTypeCategories
{
    public static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "moral", "emotional", "behavioral", "social", "cognitive", "meta"
    };

    public static bool IsValid(string? category) =>
        !string.IsNullOrWhiteSpace(category) && Allowed.Contains(category);
}
