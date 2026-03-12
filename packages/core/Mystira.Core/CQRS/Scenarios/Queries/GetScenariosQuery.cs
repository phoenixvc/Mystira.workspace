using Mystira.Domain.Models;

namespace Mystira.Core.CQRS.Scenarios.Queries;

/// <summary>
/// Query to retrieve all scenarios (read operation)
/// Can be extended with filters, pagination, etc.
/// </summary>
public record GetScenariosQuery : IQuery<IEnumerable<Scenario>>;
