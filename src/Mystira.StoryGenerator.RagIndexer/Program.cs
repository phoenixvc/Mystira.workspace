using Microsoft.Extensions.Configuration;
using Mystira.StoryGenerator.RagIndexer.Configuration;
using Mystira.StoryGenerator.RagIndexer.Interfaces;
using Mystira.StoryGenerator.RagIndexer.Models;
using Mystira.StoryGenerator.RagIndexer.Services;

namespace Mystira.StoryGenerator.RagIndexer;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            // Build configuration
            var configuration = BuildConfiguration();
            var settings = ValidateConfiguration(configuration);

            // Validate command line arguments
            var jsonFilePath = ValidateArguments(args);
            
            // Load and validate request data
            var indexRequest = await LoadIndexRequestAsync(jsonFilePath);

            // Initialize services using factory
            var services = InitializeServices(settings);

            // Execute indexing
            await services.indexingService.IndexDatasetAsync(indexRequest);

            Console.WriteLine("Indexing completed successfully!");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return 1;
        }
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static RagIndexerSettings ValidateConfiguration(IConfiguration configuration)
    {
        var settings = configuration.GetSection(RagIndexerSettings.SectionName).Get<RagIndexerSettings>()
            ?? throw new InvalidOperationException($"Configuration section '{RagIndexerSettings.SectionName}' not found or invalid.");

        ValidateSettings(settings);
        return settings;
    }

    private static void ValidateSettings(RagIndexerSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.AzureAISearch.Endpoint))
            throw new InvalidOperationException("Azure AI Search endpoint is required.");
        
        if (string.IsNullOrWhiteSpace(settings.AzureAISearch.ApiKey))
            throw new InvalidOperationException("Azure AI Search API key is required.");
        
        if (string.IsNullOrWhiteSpace(settings.AzureOpenAIEmbedding.Endpoint))
            throw new InvalidOperationException("Azure OpenAI endpoint is required.");
        
        if (string.IsNullOrWhiteSpace(settings.AzureOpenAIEmbedding.ApiKey))
            throw new InvalidOperationException("Azure OpenAI API key is required.");
    }

    private static string ValidateArguments(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: Mystira.StoryGenerator.RagIndexer <json-file-path>");
            Console.WriteLine("Example: Mystira.StoryGenerator.RagIndexer ./data/instructions.json");
            Environment.Exit(1);
        }

        var jsonFilePath = args[0];
        if (!File.Exists(jsonFilePath))
        {
            Console.WriteLine($"Error: File not found: {jsonFilePath}");
            Environment.Exit(1);
        }

        return jsonFilePath;
    }

    private static async Task<RagIndexRequest> LoadIndexRequestAsync(string jsonFilePath)
    {
        var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
        var indexRequest = System.Text.Json.JsonSerializer.Deserialize<RagIndexRequest>(jsonContent, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (indexRequest == null)
        {
            throw new InvalidOperationException("Failed to parse JSON file");
        }

        if (string.IsNullOrWhiteSpace(indexRequest.Dataset))
        {
            throw new InvalidOperationException("Dataset name is required in JSON file");
        }

        if (indexRequest.Chunks == null || !indexRequest.Chunks.Any())
        {
            throw new InvalidOperationException("At least one chunk is required in JSON file");
        }

        return indexRequest;
    }

    private static (IAzureAISearchService searchService, IAzureOpenAIEmbeddingService embeddingService, IRagIndexingService indexingService) InitializeServices(RagIndexerSettings settings)
    {
        // Initialize core services
        var logger = new ConsoleLoggerService();
        var retryPolicy = new RetryPolicyService(logger);
        var serviceFactory = new ServiceFactory(logger, retryPolicy);

        // Create service instances
        var searchService = serviceFactory.CreateAzureAISearchService(settings.AzureAISearch);
        var embeddingService = serviceFactory.CreateAzureOpenAIEmbeddingService(settings.AzureOpenAIEmbedding);
        var indexingService = serviceFactory.CreateRagIndexingService(searchService, embeddingService);

        return (searchService, embeddingService, indexingService);
    }
}