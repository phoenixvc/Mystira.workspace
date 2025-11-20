# Mystira RAG Indexer

This console application implements the indexing phase for Retrieval Augmented Generation (RAG) for the Mystira Story Generator. It processes instruction chunks, generates embeddings using Azure OpenAI, and stores them in Azure AI Search for later retrieval.

## Features

- Reads JSON input containing instruction chunks
- Generates embeddings for each chunk using Azure OpenAI
- Stores chunks with embeddings in Azure AI Search
- Supports dataset versioning (deletes old versions before indexing)
- Configurable through appsettings.json

## Configuration

Update `appsettings.json` with your Azure service details:

```json
{
  "RagIndexer": {
    "AzureAISearch": {
      "Endpoint": "https://your-search-service.search.windows.net",
      "IndexName": "mystira-instructions",
      "ApiKey": "your-search-api-key"
    },
    "AzureOpenAIEmbedding": {
      "Endpoint": "https://your-openai-resource.openai.azure.com/",
      "ApiKey": "your-openai-api-key",
      "DeploymentName": "text-embedding-ada-002"
    }
  }
}
```

## Usage

```bash
dotnet run -- <json-file-path>
```

Example:
```bash
dotnet run -- ./data/sample-instructions.json
```

## JSON Input Format

The input JSON should follow this structure:

```json
{
  "dataset": "dataset_name",
  "version": "1.0",
  "chunks": [
    {
      "chunk_id": "unique_id",
      "section": "1.1",
      "title": "Section Title",
      "content": "The instruction text to embed",
      "keywords": ["keyword1", "keyword2"]
    }
  ]
}
```

## Azure AI Search Index Schema

The indexer creates an index with the following fields:

- `chunk_id` (key): Unique identifier for each chunk
- `content`: Full text content (searchable)
- `title`: Section title (searchable, filterable)
- `section`: Section number (searchable, filterable)
- `dataset`: Dataset name (filterable, facetable)
- `version`: Dataset version (filterable)
- `keywords`: List of keywords (filterable, facetable)
- `embedding`: Vector embedding (1536 dimensions, searchable)