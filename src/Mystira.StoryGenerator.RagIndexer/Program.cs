using Microsoft.Extensions.Configuration;
using Mystira.StoryGenerator.RagIndexer.Configuration;
using Mystira.StoryGenerator.RagIndexer.Models;
using Mystira.StoryGenerator.RagIndexer.Services;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var settings = configuration.GetSection(RagIndexerSettings.SectionName).Get<RagIndexerSettings>()
    ?? throw new InvalidOperationException($"Configuration section '{RagIndexerSettings.SectionName}' not found or invalid.");

if (args.Length < 1)
{
    Console.WriteLine("Usage: Mystira.StoryGenerator.RagIndexer <json-file-path>");
    Console.WriteLine("Example: Mystira.StoryGenerator.RagIndexer ./data/instructions.json");
    return 1;
}

var jsonFilePath = args[0];

if (!File.Exists(jsonFilePath))
{
    Console.WriteLine($"Error: File not found: {jsonFilePath}");
    return 1;
}

try
{
    // Read and parse the JSON file
    var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
    var indexRequest = System.Text.Json.JsonSerializer.Deserialize<RagIndexRequest>(jsonContent, new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (indexRequest == null)
    {
        Console.WriteLine("Error: Failed to parse JSON file");
        return 1;
    }

    // Initialize services
    var searchService = new AzureAISearchService(settings.AzureAISearch);
    var embeddingService = new AzureOpenAIEmbeddingService(settings.AzureOpenAIEmbedding);
    var indexingService = new RagIndexingService(searchService, embeddingService);

    // Perform indexing
    await indexingService.IndexDatasetAsync(indexRequest);

    Console.WriteLine("Indexing completed successfully!");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    return 1;
}
