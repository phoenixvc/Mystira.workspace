using Microsoft.Extensions.Logging;
using Mystira.Core.Ports.Data;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.Core.CQRS.EchoTypes.Queries;

/// <summary>
/// Wolverine handler for retrieving all echo types.
/// </summary>
public static class GetAllEchoTypesQueryHandler
{
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
