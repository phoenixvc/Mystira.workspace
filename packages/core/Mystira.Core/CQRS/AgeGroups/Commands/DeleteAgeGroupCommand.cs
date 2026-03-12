namespace Mystira.Core.CQRS.AgeGroups.Commands;

/// <summary>
/// Command to delete an age group.
/// </summary>
public record DeleteAgeGroupCommand(string Id) : ICommand<bool>;
