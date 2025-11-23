using Mystira.StoryGenerator.Contracts.Intent;

namespace Mystira.StoryGenerator.Domain.Services;

public interface IIntentClassificationService
{
    Task<IntentClassification?> ClassifyIntentAsync(string userQuery, CancellationToken cancellationToken = default);
}
