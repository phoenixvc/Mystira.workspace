using Mystira.StoryGenerator.Contracts.Intent;

namespace Mystira.StoryGenerator.Api.Services.Intent;

public interface IIntentRouterService
{
    Task<IntentClassification?> ClassifyIntentAsync(string userQuery, CancellationToken cancellationToken = default);
}
