using Microsoft.Extensions.Logging;
using Mystira.Application.Ports.Data;
using Mystira.Domain.Models;

namespace Mystira.Application.CQRS.EchoTypes.Queries;

/// <summary>
/// Wolverine handler for retrieving an echo type by ID.
/// </summary>
public static class GetEchoTypeByIdQueryHandler
{
    /// <summary>
    /// Handles the GetEchoTypeByIdQuery.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="repository">The echo type repository.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The echo type definition if found; otherwise, null.</returns>
    public static async Task<EchoTypeDefinition?> Handle(
        GetEchoTypeByIdQuery query,
        IEchoTypeRepository repository,
        ILogger<GetEchoTypeByIdQuery> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Retrieving echo type with id: {Id}", query.Id);
        var echoType = await repository.GetByIdAsync(query.Id);

        if (echoType == null)
        {
            logger.LogWarning("Echo type with id {Id} not found", query.Id);
        }

        return echoType;
    }
}
