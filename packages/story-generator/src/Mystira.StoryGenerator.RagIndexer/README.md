# Mystira RAG Indexer

This console application implements the indexing phase for Retrieval Augmented Generation (RAG) for the Mystira Story Generator. It processes instruction chunks, generates embeddings using Azure OpenAI, and stores them in Azure AI Search for later retrieval.

## Features

- Reads JSON input containing instruction chunks
- Generates embeddings for each chunk using Azure OpenAI
- Stores chunks with embeddings in Azure AI Search
- Supports dataset versioning (deletes old versions before indexing)
- Configurable through appsettings.json
- Built with SOLID principles and DRY practices
- Comprehensive error handling and retry policies
- Dependency injection and service factory pattern
- Structured logging and monitoring

## Configuration

Update `appsettings.json` with your Azure service details:

```json
{
  "RagIndexer": {
    "AzureAISearch": {
      "Endpoint": "https://your-search-service.search.windows.net",
      "IndexName": "mystira-instructions",
      "ApiKey": "your-search-api-key",
      "AgeGroupIndexMapping": {
        "1-2": "mystira-instructions-1-2",
        "3-5": "mystira-instructions-3-5",
        "6-9": "mystira-instructions-6-9",
        "10-12": "mystira-instructions-10-12",
        "13-18": "mystira-instructions-13-18"
      }
    },
    "AzureOpenAIEmbedding": {
      "Endpoint": "https://your-openai-resource.openai.azure.com/",
      "ApiKey": "your-openai-api-key",
      "DeploymentName": "text-embedding-ada-002"
    }
  }
}
```

### Age Group Support

The indexer now supports indexing instructions for multiple age groups. Each age group has a dedicated index:
- `1-2`: Toddler stories
- `3-5`: Preschool stories
- `6-9`: School-age stories
- `10-12`: Preteen stories
- `13-18`: Teen stories

## Usage

```bash
dotnet run -- <json-file-path>
```

Examples:
```bash
# Index instructions for school-age (6-9)
dotnet run -- ./data/sample_instructions_6-9.json

# Index instructions for preschool (3-5)
dotnet run -- ./data/sample_instructions_3-5.json

# Index instructions for preteen (10-12)
dotnet run -- ./data/sample_instructions_10-12.json
```

## JSON Input Format

The input JSON should follow this structure:

```json
{
  "dataset": "dataset_name",
  "version": "1.0",
  "ageGroup": "6-9",
  "chunks": [
    {
      "chunk_id": "unique_id",
      "section": "1.1",
      "title": "Section Title",
      "content": "The instruction text to embed",
      "category": "story_generation | validation | autofix | summarization | config | safety | meta",
      "subcategory": "core_story_rules | tone_and_humour | scene_types_and_rolls | character_and_archetypes | narrative_causality | compass_scoring | focus_vs_microchoices | cumulative_consequences | developmental_goals | educational_goals | gameplay_and_tone | smart_roster | developmental_link | field_enums",
      "instructionType": "requirements | guidelines | examples | validation | schema_docs",
      "priority": "high | normal | low",
      "isMandatory": true | false,
      "examples": "Example usage or context",
      "tags": ["tag1", "tag2"],
      "source": "mystira_instruction_schema",
      "version": "1.0",
      "createdAt": "2025-11-19T00:00:00Z",
      "updatedAt": "2025-11-19T00:00:00Z",
      "keywords": ["keyword1", "keyword2"]
    }
  ]
}
```

## Azure AI Search Index Schema

The indexer creates an index with the following fields:

### Primary Key
- `id` (key): Unique identifier for each chunk

### Content
- `content`: Full text content (searchable) with standard.lucene analyzer

### Instruction Categorization
- `category`: High-level pipeline step (filterable, facetable)
- `subcategory`: Topic/area of the rule (filterable, facetable)
- `instructionType`: Type of instruction (filterable, facetable)
- `priority`: Importance level (filterable, facetable)
- `isMandatory`: Whether instruction is required (filterable)
- `examples`: Example usage or context (searchable)
- `tags`: List of descriptive tags (filterable, facetable)

### Context and Relationships
- `title`: Section title (searchable, filterable)
- `section`: Section number (searchable, filterable)
- `keywords`: List of keywords (filterable, facetable)

### Metadata
- `source`: Source of instruction (filterable)
- `version`: Instruction version (filterable)
- `createdAt`: Creation timestamp (filterable, sortable)
- `updatedAt`: Last update timestamp (filterable, sortable)

### Legacy Fields (for backward compatibility)
- `chunk_id`: Legacy chunk identifier (filterable)
- `dataset`: Dataset name (filterable, facetable)
- `version`: Dataset version (filterable)

### Vector Search
- `embedding`: Vector embedding (1536 dimensions, searchable) with vector search profile