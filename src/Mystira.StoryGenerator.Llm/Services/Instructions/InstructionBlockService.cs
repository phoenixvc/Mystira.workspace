using System.ClientModel;
using System.Text;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Services;
using OpenAI.Embeddings;

namespace Mystira.StoryGenerator.Llm.Services.Instructions;

public sealed class InstructionBlockService : IInstructionBlockService
{
    private readonly InstructionSearchSettings _searchSettings;
    private readonly SearchClient? _searchClient;
    private readonly EmbeddingClient? _embeddingClient;
    private readonly ILogger<InstructionBlockService> _logger;

    public InstructionBlockService(
        IOptions<InstructionSearchSettings> searchOptions,
        IOptions<AiSettings> aiOptions,
        ILogger<InstructionBlockService> logger)
    {
        _searchSettings = searchOptions.Value;
        _logger = logger;

        if (_searchSettings.IsConfigured)
        {
            try
            {
                _searchClient = new SearchClient(
                    new Uri(_searchSettings.Endpoint!),
                    _searchSettings.IndexName!,
                    new AzureKeyCredential(_searchSettings.ApiKey!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure AI Search client");
            }
        }

        var aiSettings = aiOptions.Value;
        if (!string.IsNullOrWhiteSpace(aiSettings.AzureOpenAI.EmbeddingDeploymentName))
        {
            try
            {
                var client = new AzureOpenAIClient(
                    new Uri(aiSettings.AzureOpenAI.Endpoint),
                    new ApiKeyCredential(aiSettings.AzureOpenAI.ApiKey));
                _embeddingClient = client.GetEmbeddingClient(aiSettings.AzureOpenAI.EmbeddingDeploymentName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure OpenAI embedding client");
            }
        }
    }

    // Domain adapter not implemented here; adapter class handles mapping between domain and API contexts.

    public async Task<string?> BuildInstructionBlockAsync(InstructionSearchContext context, CancellationToken cancellationToken = default)
    {
        if (context is null || string.IsNullOrWhiteSpace(context.QueryText))
        {
            return null;
        }

        if (_searchClient is null || _embeddingClient is null)
        {
            _logger.LogDebug("Instruction search is disabled because search or embedding clients are not configured.");
            return null;
        }

        var queryVector = await GenerateEmbeddingAsync(context.QueryText, cancellationToken);
        if (queryVector is null)
        {
            return null;
        }

        var vectorChunks = await ExecuteVectorSearchAsync(queryVector, context, cancellationToken);
        var mandatoryChunks = await FetchMandatoryChunksAsync(context, cancellationToken);
        var mergedChunks = MergeChunks(vectorChunks, mandatoryChunks);

        if (mergedChunks.Count == 0)
        {
            return null;
        }

        return BuildInstructionBlock(mergedChunks);
    }

    private async Task<float[]?> GenerateEmbeddingAsync(string query, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _embeddingClient!.GenerateEmbeddingAsync(query, new EmbeddingGenerationOptions(), cancellationToken);
            var embedding = response.Value?.ToFloats().ToArray();
            if (embedding is null || embedding.Length == 0)
            {
                _logger.LogWarning("Received empty embedding result for instruction block query");
                return null;
            }

            return embedding;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate embedding for instruction block query");
            return null;
        }
    }

    private async Task<List<InstructionChunk>> ExecuteVectorSearchAsync(float[] queryVector, InstructionSearchContext context, CancellationToken cancellationToken)
    {
        var results = new List<InstructionChunk>();
        if (_searchClient is null)
        {
            return results;
        }

        try
        {
            var top = context.TopK.GetValueOrDefault(_searchSettings.DefaultTopK);
            var vectorQuery = new VectorizedQuery(queryVector.AsMemory())
            {
                KNearestNeighborsCount = top,
                Fields = { _searchSettings.EmbeddingFieldName }
            };

            var options = new SearchOptions
            {
                Size = top,
                VectorSearch = new VectorSearchOptions { Queries = { vectorQuery } }
            };

            var filter = BuildFilterClause(context, mandatoryOnly: false);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                options.Filter = filter;
            }

            ApplySelect(options);

            var response = await _searchClient.SearchAsync<SearchDocument>("*", options, cancellationToken);
            await foreach (var result in response.Value.GetResultsAsync())
            {
                var chunk = MapToInstructionChunk(result.Document);
                chunk.Score = result.Score;
                results.Add(chunk);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Vector search for instruction block failed");
        }

        return results;
    }

    private async Task<List<InstructionChunk>> FetchMandatoryChunksAsync(InstructionSearchContext context, CancellationToken cancellationToken)
    {
        var results = new List<InstructionChunk>();
        if (_searchClient is null)
        {
            return results;
        }

        try
        {
            var options = new SearchOptions
            {
                Size = Math.Max(1, _searchSettings.MandatoryChunkLimit)
            };

            var filter = BuildFilterClause(context, mandatoryOnly: true);
            if (!string.IsNullOrWhiteSpace(filter))
            {
                options.Filter = filter;
            }

            ApplySelect(options);

            var response = await _searchClient.SearchAsync<SearchDocument>("*", options, cancellationToken);
            await foreach (var result in response.Value.GetResultsAsync())
            {
                var chunk = MapToInstructionChunk(result.Document);
                chunk.IsMandatory = true;
                results.Add(chunk);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Mandatory instruction chunk lookup failed");
        }

        return results;
    }

    private string? BuildFilterClause(InstructionSearchContext context, bool mandatoryOnly)
    {
        var filters = new List<string>();

        if (context?.Categories is { Length: > 0 })
        {
            var clause = BuildFieldFilter(_searchSettings.CategoryFieldName, context.Categories, _searchSettings.IsCategoryFieldCollection);
            if (!string.IsNullOrWhiteSpace(clause))
            {
                filters.Add(clause);
            }
        }

        if (context.InstructionTypes?.Length > 0)
        {
            var clause = BuildFieldFilter(_searchSettings.InstructionTypeFieldName, context.InstructionTypes, _searchSettings.IsInstructionTypeFieldCollection);
            if (!string.IsNullOrWhiteSpace(clause))
            {
                filters.Add(clause);
            }
        }

        if (mandatoryOnly)
        {
            filters.Add($"{_searchSettings.MandatoryFieldName} eq true");
        }

        if (filters.Count == 0)
        {
            return mandatoryOnly ? $"{_searchSettings.MandatoryFieldName} eq true" : null;
        }

        return string.Join(" and ", filters);
    }

    private static string BuildFieldFilter(string fieldName, IReadOnlyCollection<string> values, bool isCollection)
    {
        if (string.IsNullOrWhiteSpace(fieldName) || values.Count == 0)
        {
            return string.Empty;
        }

        var sanitized = values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => $"'{v.Replace("'", "''", StringComparison.Ordinal)}'")
            .ToList();

        if (sanitized.Count == 0)
        {
            return string.Empty;
        }

        if (sanitized.Count == 1)
        {
            var value = sanitized[0];
            return isCollection
                ? $"{fieldName}/any(item: item eq {value})"
                : $"{fieldName} eq {value}";
        }

        var joined = sanitized
            .Select(value => isCollection ? $"{fieldName}/any(item: item eq {value})" : $"{fieldName} eq {value}")
            .ToList();

        return $"({string.Join(" or ", joined)})";
    }

    private InstructionChunk MapToInstructionChunk(SearchDocument document)
    {
        var chunk = new InstructionChunk
        {
            Id = ReadString(document, _searchSettings.IdFieldName),
            Content = ReadString(document, _searchSettings.ContentFieldName),
            Title = ReadString(document, _searchSettings.TitleFieldName),
            Category = ReadString(document, _searchSettings.CategoryFieldName),
            Subcategory = ReadString(document, _searchSettings.SubcategoryFieldName),
            InstructionType = ReadString(document, _searchSettings.InstructionTypeFieldName),
            IsMandatory = ReadBool(document, _searchSettings.MandatoryFieldName),
            Priority = ReadNullableInt(document, _searchSettings.PriorityFieldName),
            Tags = ReadStringList(document, _searchSettings.TagsFieldName),
            Source = ReadString(document, _searchSettings.SourceFieldName),
            Version = ReadString(document, _searchSettings.VersionFieldName),
            CreatedAt = ReadDateTimeOffset(document, _searchSettings.CreatedAtFieldName),
            UpdatedAt = ReadDateTimeOffset(document, _searchSettings.UpdatedAtFieldName),
            Section = ReadString(document, _searchSettings.SectionFieldName),
            Dataset = ReadString(document, _searchSettings.DatasetFieldName),
            Keywords = ReadStringList(document, _searchSettings.KeywordsFieldName)
        };

        return chunk;
    }

    private static string ReadString(SearchDocument document, string? fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return string.Empty;
        }

        return document.TryGetValue(fieldName, out var value) && value is string text
            ? text
            : string.Empty;
    }

    private static bool ReadBool(SearchDocument document, string? fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return false;
        }

        if (!document.TryGetValue(fieldName, out var value))
        {
            return false;
        }

        return value switch
        {
            bool b => b,
            string s when bool.TryParse(s, out var parsed) => parsed,
            int i => i != 0,
            long l => l != 0,
            _ => false
        };
    }

    private static int? ReadNullableInt(SearchDocument document, string? fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        if (!document.TryGetValue(fieldName, out var value))
        {
            return null;
        }

        return value switch
        {
            int i => i,
            long l => (int)l,
            string s when int.TryParse(s, out var parsed) => parsed,
            _ => null
        };
    }

    private static IReadOnlyList<string> ReadStringList(SearchDocument document, string? fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return Array.Empty<string>();
        }

        if (!document.TryGetValue(fieldName, out var value))
        {
            return Array.Empty<string>();
        }

        if (value is IEnumerable<string> stringEnumerable)
        {
            return stringEnumerable
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!)
                .ToArray();
        }

        if (value is IEnumerable<object> objectEnumerable)
        {
            return objectEnumerable
                .Select(item => item?.ToString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!)
                .ToArray();
        }

        return Array.Empty<string>();
    }

    private static DateTimeOffset? ReadDateTimeOffset(SearchDocument document, string? fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return null;
        }

        if (!document.TryGetValue(fieldName, out var value) || value is null)
        {
            return null;
        }

        switch (value)
        {
            case DateTimeOffset dto:
                return dto;
            case DateTime dt:
                return new DateTimeOffset(dt);
            case string s when !string.IsNullOrWhiteSpace(s):
                if (DateTimeOffset.TryParse(s, out var parsedDto))
                {
                    return parsedDto;
                }
                if (DateTime.TryParse(s, out var parsedDt))
                {
                    return new DateTimeOffset(parsedDt);
                }
                return null;
            default:
                return null;
        }
    }

    private void ApplySelect(SearchOptions options)
    {
        var fields = new[]
        {
            _searchSettings.IdFieldName,
            _searchSettings.ContentFieldName,
            _searchSettings.TitleFieldName,
            _searchSettings.CategoryFieldName,
            _searchSettings.SubcategoryFieldName,
            _searchSettings.InstructionTypeFieldName,
            _searchSettings.MandatoryFieldName,
            _searchSettings.PriorityFieldName,
            _searchSettings.SourceFieldName,
            _searchSettings.VersionFieldName,
            _searchSettings.CreatedAtFieldName,
            _searchSettings.UpdatedAtFieldName,
            _searchSettings.SectionFieldName,
            _searchSettings.DatasetFieldName,
            _searchSettings.KeywordsFieldName,
            _searchSettings.TagsFieldName
        };

        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field) || options.Select.Contains(field))
            {
                continue;
            }

            options.Select.Add(field);
        }
    }

    private static List<InstructionChunk> MergeChunks(List<InstructionChunk> vectorChunks, List<InstructionChunk> mandatoryChunks)
    {
        var merged = new List<InstructionChunk>(vectorChunks);

        foreach (var mandatory in mandatoryChunks)
        {
            var match = merged.FirstOrDefault(chunk =>
                !string.IsNullOrWhiteSpace(chunk.Id) &&
                !string.IsNullOrWhiteSpace(mandatory.Id) &&
                string.Equals(chunk.Id, mandatory.Id, StringComparison.OrdinalIgnoreCase));

            if (match is not null)
            {
                match.IsMandatory = true;
                match.Order ??= mandatory.Order;
                continue;
            }

            merged.Add(mandatory);
        }

        return merged
            .Where(chunk => !string.IsNullOrWhiteSpace(chunk.Content))
            .OrderByDescending(chunk => chunk.IsMandatory)
            .ThenBy(chunk => chunk.Order ?? int.MaxValue)
            .ThenByDescending(chunk => chunk.Score ?? 0)
            .ToList();
    }

    private static string BuildInstructionBlock(IReadOnlyList<InstructionChunk> chunks)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Mystira Instruction Block");
        sb.AppendLine("Follow every item below when generating your response:");

        for (var i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var flags = new List<string>();
            if (chunk.IsMandatory)
            {
                flags.Add("MANDATORY");
            }

            if (!string.IsNullOrWhiteSpace(chunk.Category))
            {
                flags.Add(chunk.Category!);
            }

            if (!string.IsNullOrWhiteSpace(chunk.InstructionType))
            {
                flags.Add(chunk.InstructionType!);
            }

            var label = flags.Count > 0 ? $" [{string.Join(" | ", flags)}]" : string.Empty;
            sb.AppendLine($"{i + 1}. {chunk.Content.Trim()}{label}");
        }

        return sb.ToString().TrimEnd();
    }
}
