using Mystira.StoryGenerator.RagIndexer.Models;

namespace Mystira.StoryGenerator.RagIndexer.Interfaces;

public interface IRagIndexingService
{
    Task IndexDatasetAsync(RagIndexRequest request);
}