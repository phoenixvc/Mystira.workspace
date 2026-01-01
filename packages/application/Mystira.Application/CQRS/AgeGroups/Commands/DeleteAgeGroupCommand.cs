namespace Mystira.Application.CQRS.AgeGroups.Commands;

/// <summary>
/// Command to delete an age group.
/// </summary>
/// <param name="Id">The unique identifier of the age group to delete.</param>
public record DeleteAgeGroupCommand(string Id) : ICommand<bool>;
