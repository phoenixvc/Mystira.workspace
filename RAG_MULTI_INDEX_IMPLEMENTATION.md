# RAG Multi-Index Age Group Support - Implementation Summary

## Overview

This implementation adds support for multiple RAG instruction indexes based on age groups. The system now routes instruction searches to age-specific indexes, allowing for tailored content and guidelines for different age groups.

## Supported Age Groups

- **1-2**: Toddler stories
- **3-5**: Preschool stories  
- **6-9**: School-age stories
- **10-12**: Preteen stories
- **13-18**: Teen stories

Each age group maps to a dedicated Azure AI Search index.

## Architecture Changes

### 1. Configuration Layer (`InstructionSearchSettings`)

**File**: `src/Mystira.StoryGenerator.Contracts/Configuration/InstructionSearchSettings.cs`

- Added `AgeGroupIndexMapping` property (Dictionary<string, string>) to map age groups to index names
- Added `ResolveIndexName(string? ageGroup)` method to determine which index to use
- Updated `IsConfigured` property to support both direct index name and age group mapping

### 2. Domain Models

**File**: `src/Mystira.StoryGenerator.Domain/Services/IInstructionBlockService.cs`

- Added optional `AgeGroup` property to `InstructionSearchContext` class
- Allows passing age group information through instruction search requests

**File**: `src/Mystira.StoryGenerator.Contracts/Chat/StorySnapshot.cs`

- Added computed `AgeGroup` property that extracts `age_group` from story JSON content
- Enables easy access to age group information from story contexts

### 3. Service Implementation

**File**: `src/Mystira.StoryGenerator.Llm/Services/Instructions/InstructionBlockService.cs`

Major refactoring:
- Changed from single `SearchClient` to `Dictionary<string, SearchClient>` to support multiple indexes
- Added `InitializeSearchClients()` method to initialize all configured indexes at startup
- Added `GetSearchClient(string indexName)` method to retrieve appropriate client
- Updated `BuildInstructionBlockAsync()` to resolve index by age group
- Updated `ExecuteVectorSearchAsync()` and `FetchMandatoryChunksAsync()` to accept `SearchClient` parameter

### 4. Command Handlers

Updated all chat and story command handlers to extract and pass age group:

**Files Updated**:
- `Handlers/Chat/GuidelinesCommandHandler.cs`
- `Handlers/Chat/RequirementsCommandHandler.cs`
- `Handlers/Chat/SafetyPolicyCommandHandler.cs`
- `Handlers/Chat/FreeTextCommandHandler.cs`
- `Handlers/Chat/SchemaDocsCommandHandler.cs`
- `Handlers/Stories/GenerateStoryCommandHandler.cs`
- `Handlers/Stories/RefineStoryCommandHandler.cs`

**Changes**:
- Extract age group from current story context
- Pass age group to `InstructionSearchContext`
- Include helper methods `ExtractAgeGroupFromContext()` and `ExtractAgeGroupFromJson()` (duplicated per handler for encapsulation)

### 5. RAG Indexer Updates

**Files Updated**:
- `src/Mystira.StoryGenerator.RagIndexer/Configuration/RagIndexerSettings.cs`
- `src/Mystira.StoryGenerator.RagIndexer/Models/RagIndexRequest.cs`
- `src/Mystira.StoryGenerator.RagIndexer/Interfaces/IAzureAISearchService.cs`
- `src/Mystira.StoryGenerator.RagIndexer/Services/AzureAISearchService.cs`
- `src/Mystira.StoryGenerator.RagIndexer/Services/RagIndexingService.cs`

**Changes**:
- Added `AgeGroup` property to `RagIndexRequest` to specify target index during indexing
- Added `AgeGroupIndexMapping` to indexer settings
- Refactored `AzureAISearchService` to support multiple indexes
- Updated interface methods to accept optional `ageGroup` parameter
- All indexing operations now resolve correct index by age group

## Configuration

### API Configuration

**File**: `src/Mystira.StoryGenerator.Api/appsettings.json`

```json
"InstructionSearch": {
  "AgeGroupIndexMapping": {
    "1-2": "mystira-instructions-1-2",
    "3-5": "mystira-instructions-3-5",
    "6-9": "mystira-instructions-6-9",
    "10-12": "mystira-instructions-10-12",
    "13-18": "mystira-instructions-13-18"
  }
}
```

### RAG Indexer Configuration

**File**: `src/Mystira.StoryGenerator.RagIndexer/appsettings.json`

Same age group mapping configuration as API.

## Usage

### Indexing Instructions for an Age Group

```bash
cd src/Mystira.StoryGenerator.RagIndexer

# Index toddler instructions (1-2)
dotnet run -- ./data/sample_instructions_1-2.json

# Index preschool instructions (3-5)
dotnet run -- ./data/sample_instructions_3-5.json

# Index school-age instructions (6-9)
dotnet run -- ./data/sample_instructions_6-9.json

# Index preteen instructions (10-12)
dotnet run -- ./data/sample_instructions_10-12.json

# Index teen instructions (13-18)
dotnet run -- ./data/sample_instructions_13-18.json
```

### JSON Input Format

```json
{
  "dataset": "mystira_school_age_story_template",
  "version": "1.0",
  "ageGroup": "6-9",
  "chunks": [
    {
      "chunk_id": "unique_id",
      "section": "1.1",
      "title": "Section Title",
      "content": "Instruction content...",
      "category": "story_generation",
      "subcategory": "core_story_rules",
      "instructionType": "requirements",
      "priority": "high",
      "isMandatory": true,
      "tags": ["tag1", "tag2"],
      "keywords": ["keyword1", "keyword2"]
    }
  ]
}
```

## Data Files

Sample instruction data files for each age group:

- `src/Mystira.StoryGenerator.RagIndexer/data/sample_instructions_1-2.json` - Toddler
- `src/Mystira.StoryGenerator.RagIndexer/data/sample_instructions_3-5.json` - Preschool
- `src/Mystira.StoryGenerator.RagIndexer/data/sample_instructions_6-9.json` - School-age (existing)
- `src/Mystira.StoryGenerator.RagIndexer/data/sample_instructions_10-12.json` - Preteen
- `src/Mystira.StoryGenerator.RagIndexer/data/sample_instructions_13-18.json` - Teen

## Flow

1. **Story Generation/Refinement Request**
   - Command handler receives request with story context
   - Extracts `age_group` from story JSON

2. **Instruction Search**
   - Passes age group to `InstructionSearchContext`
   - Service resolves appropriate index name using `ResolveIndexName()`

3. **Azure AI Search**
   - Retrieves correct `SearchClient` for the age group's index
   - Executes vector search and mandatory chunk lookup
   - Returns filtered results for that age group

4. **RAG Integration**
   - Results are used to build instruction block for LLM
   - Age-appropriate guidelines and requirements are included in prompts

## Backward Compatibility

- Default index specified in `IndexName` is used if age group is not provided
- Existing code that doesn't pass age group continues to work
- System gracefully handles missing or invalid age groups

## Error Handling

- If age group is invalid, system falls back to default index
- Missing age groups in mapping use default index
- Null age groups default to the configured `IndexName`

## Testing Considerations

- Test each age group index independently
- Verify correct results for age-specific queries
- Test fallback behavior with missing age groups
- Verify instruction quality for each age group
