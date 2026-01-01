using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.EchoTypes.Queries;

/// <summary>
/// Wolverine handler for retrieving all echo types.
/// </summary>
public static class GetAllEchoTypesQueryHandler
{
    /// <summary>
    /// Handles the GetAllEchoTypesQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The echo type repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of all echo type definitions.</returns>
    public static async Task<List<EchoTypeDefinition>> Handle(
        GetAllEchoTypesQuery query,
        IEchoTypeRepository repository,
        ILogger<GetAllEchoTypesQuery> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving all echo types");
        var echoTypes = await repository.GetAllAsync();
        logger.LogInformation("Retrieved {Count} echo types", echoTypes.Count);
        return echoTypes;
    }
}
