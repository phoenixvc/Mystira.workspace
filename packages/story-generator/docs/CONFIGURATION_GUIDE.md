# Foundry Agent Configuration Guide

## Overview

This guide explains the clean, nested configuration structure for the Mystira Story Generator, separating FileSearch and AISearch settings for clarity.

---

## New Configuration Structure (Recommended)

### Clean Nested Format

```json
{
  "FoundryAgent": {
    "WriterAgentId": "asst_writer_abc123",
    "JudgeAgentId": "asst_judge_def456",
    "RefinerAgentId": "asst_refiner_ghi789",
    "RubricSummaryAgentId": "asst_summary_jkl012",
    "Endpoint": "https://your-foundry-project.azure.com",
    "ApiKey": "your-foundry-api-key",
    "ProjectId": "your-project-id",
    "MaxIterations": 5,
    "RunTimeout": "00:05:00",
    "KnowledgeMode": "FileSearch",

    "FileSearch": {
      "DefaultVectorStoreId": "vs_mystira_default",
      "VectorStoresByAgeGroup": {
        "1-2": "vs_toddler_abc123",
        "3-5": "vs_preschool_def456",
        "6-9": "vs_elementary_ghi789",
        "10-12": "vs_preteen_jkl012"
      },
      "MaxFiles": 20,
      "MaxTokens": 4000
    },

    "AISearch": {
      "Endpoint": "https://your-search-service.search.windows.net",
      "ApiKey": "your-search-api-key",
      "IndexName": "mystira-instructions",
      "AgeGroupFieldName": "age_group",
      "TopK": 5
    }
  }
}
```

### Benefits of Nested Structure

✅ **Clear separation** - FileSearch and AISearch settings in distinct sections
✅ **Self-documenting** - Easy to see which settings apply to which mode
✅ **IDE support** - Better IntelliSense and autocomplete
✅ **Validation** - Easier to validate mode-specific configuration
✅ **Extensibility** - Add mode-specific settings without cluttering root

---

## Configuration Sections Explained

### 1. **Root FoundryAgent Settings** (Apply to All Modes)

```json
{
  "FoundryAgent": {
    "WriterAgentId": "asst_writer_abc123",        // Required: Writer agent ID
    "JudgeAgentId": "asst_judge_def456",          // Required: Judge agent ID
    "RefinerAgentId": "asst_refiner_ghi789",      // Optional: Refiner agent ID
    "RubricSummaryAgentId": "asst_summary_jkl012",// Optional: Summary agent ID
    "Endpoint": "https://...",                     // Required: Azure AI Foundry endpoint
    "ApiKey": "your-api-key",                      // Required: Foundry API key
    "ProjectId": "your-project-id",                // Required: Foundry project ID
    "MaxIterations": 5,                            // Default: 5 refinement loops
    "RunTimeout": "00:05:00",                      // Default: 5 minutes per agent run
    "KnowledgeMode": "FileSearch"                  // Required: "FileSearch" or "AISearch"
  }
}
```

---

### 2. **FileSearch Configuration** (Used when KnowledgeMode = "FileSearch")

```json
{
  "FileSearch": {
    "DefaultVectorStoreId": "vs_mystira_default",  // Fallback vector store ID
    "VectorStoresByAgeGroup": {                    // Age-specific vector stores
      "1-2": "vs_toddler_abc123",
      "3-5": "vs_preschool_def456",
      "6-9": "vs_elementary_ghi789",
      "10-12": "vs_preteen_jkl012"
    },
    "MaxFiles": 20,                                // Optional: Max files per search
    "MaxTokens": 4000                              // Optional: Max tokens for results
  }
}
```

**Fields:**
- **`DefaultVectorStoreId`** - Used when age group not in `VectorStoresByAgeGroup` or for general knowledge
- **`VectorStoresByAgeGroup`** - Dictionary mapping age groups to vector store IDs (enables age-specific content)
- **`MaxFiles`** - Limits number of files retrieved per search (performance tuning)
- **`MaxTokens`** - Limits token usage for search results (cost control)

**Behavior:**
1. When user requests story for age `"6-9"`, system looks up `VectorStoresByAgeGroup["6-9"]`
2. If found, uses `"vs_elementary_ghi789"`
3. If not found, uses `DefaultVectorStoreId` as fallback
4. Creates thread with that specific vector store attached
5. Agent can only search documents in that vector store

---

### 3. **AISearch Configuration** (Used when KnowledgeMode = "AISearch")

```json
{
  "AISearch": {
    "Endpoint": "https://your-search.search.windows.net",  // Azure AI Search endpoint
    "ApiKey": "your-search-api-key",                        // Search service API key
    "IndexName": "mystira-instructions",                    // Index containing guidelines
    "AgeGroupFieldName": "age_group",                       // Metadata field for age filtering
    "ContentFieldName": "content",                          // Optional: Override content field
    "TopK": 5                                               // Number of results to retrieve
  }
}
```

**Fields:**
- **`Endpoint`** - Your Azure AI Search service URL
- **`ApiKey`** - Search service admin or query API key
- **`IndexName`** - Name of index containing story guidelines (e.g., "mystira-instructions")
- **`AgeGroupFieldName`** - Field name for age group metadata (used in filters like `age_group eq '6-9'`)
- **`ContentFieldName`** - Override default content field name (optional)
- **`TopK`** - Number of documents to retrieve per search (default: 5)

**Behavior:**
1. When user requests story for age `"6-9"`, agent receives prompt:
   ```
   "Always include filter 'age_group eq 6-9' in your searches"
   ```
2. Agent calls `azure_ai_search` tool with filter
3. Only documents with `age_group: "6-9"` are returned
4. Agent uses those documents to write age-appropriate story

---

## Usage Examples

### Example 1: FileSearch with Age-Specific Vector Stores

**Use case:** You want complete content isolation per age group

```json
{
  "FoundryAgent": {
    "KnowledgeMode": "FileSearch",
    "FileSearch": {
      "DefaultVectorStoreId": "vs_general",
      "VectorStoresByAgeGroup": {
        "1-2": "vs_toddler",
        "6-9": "vs_elementary"
      }
    }
  }
}
```

**Request:**
```http
POST /api/story-agent/sessions/start
{
  "storyPrompt": "A brave knight",
  "ageGroup": "6-9"
}
```

**What happens:**
1. System looks up `VectorStoresByAgeGroup["6-9"]` → `"vs_elementary"`
2. Creates thread with `vs_elementary` attached
3. Agent can ONLY search elementary-age documents
4. **Isolation**: Agent cannot access toddler or other age group content

---

### Example 2: FileSearch with Single Vector Store (No Age Separation)

**Use case:** All age groups share same knowledge base

```json
{
  "FoundryAgent": {
    "KnowledgeMode": "FileSearch",
    "FileSearch": {
      "DefaultVectorStoreId": "vs_all_ages"
    }
  }
}
```

**Behavior:**
- All story requests use `vs_all_ages` regardless of age group
- Age-appropriateness controlled via prompts, not vector store isolation

---

### Example 3: AISearch with Metadata Filtering

**Use case:** Single index with age group metadata, filter at query time

```json
{
  "FoundryAgent": {
    "KnowledgeMode": "AISearch",
    "AISearch": {
      "Endpoint": "https://mystira-search.search.windows.net",
      "ApiKey": "abc123",
      "IndexName": "guidelines",
      "AgeGroupFieldName": "age_group",
      "TopK": 5
    }
  }
}
```

**Index structure:**
```json
[
  {
    "id": "doc1",
    "content": "Use 2-4 word sentences...",
    "age_group": "1-2"
  },
  {
    "id": "doc2",
    "content": "Use compound sentences...",
    "age_group": "6-9"
  }
]
```

**Request for age 6-9:**
1. Agent searches with filter: `age_group eq '6-9'`
2. Only doc2 is returned
3. Agent uses 6-9 appropriate guidelines

---

## Switching Between Modes

### FileSearch → AISearch

**Before (FileSearch):**
```json
{
  "KnowledgeMode": "FileSearch",
  "FileSearch": {
    "VectorStoresByAgeGroup": { "6-9": "vs_elementary" }
  }
}
```

**After (AISearch):**
```json
{
  "KnowledgeMode": "AISearch",
  "AISearch": {
    "Endpoint": "https://...",
    "ApiKey": "...",
    "IndexName": "guidelines"
  }
}
```

**Migration steps:**
1. Create AI Search index
2. Upload documents from vector store to index (add `age_group` metadata)
3. Update `KnowledgeMode` to `"AISearch"`
4. Test with different age groups

---

## Backward Compatibility (Legacy Flat Config)

### Old Configuration (Still Supported)

```json
{
  "FoundryAgent": {
    "KnowledgeMode": "FileSearch",
    "VectorStoreName": "vs_mystira_default",        // DEPRECATED
    "SearchIndexName": "mystira-instructions",      // DEPRECATED
    "VectorStoresByAgeGroup": {                     // DEPRECATED
      "6-9": "vs_elementary"
    }
  }
}
```

**What happens:**
- System detects legacy flat config
- Maps `VectorStoreName` → `FileSearch.DefaultVectorStoreId`
- Maps `SearchIndexName` → `AISearch.IndexName`
- Maps `VectorStoresByAgeGroup` → `FileSearch.VectorStoresByAgeGroup`
- Logs warning: `"Using deprecated configuration format. Migrate to nested FileSearch/AISearch sections."`

### Migration Path (No Breaking Changes)

1. **Phase 1: Keep old config** - System continues working
2. **Phase 2: Add new nested config** - Both coexist, new takes precedence
3. **Phase 3: Remove old config** - Clean config, no deprecated fields

**Example migration:**
```json
{
  "FoundryAgent": {
    "KnowledgeMode": "FileSearch",

    // NEW (takes precedence)
    "FileSearch": {
      "DefaultVectorStoreId": "vs_default",
      "VectorStoresByAgeGroup": { "6-9": "vs_elementary" }
    },

    // OLD (ignored if new exists, can be removed)
    "VectorStoreName": "vs_old",
    "VectorStoresByAgeGroup": { "6-9": "vs_old_elementary" }
  }
}
```

---

## Validation & Troubleshooting

### Common Issues

#### Issue 1: KnowledgeMode = "FileSearch" but no FileSearch section

**Error:**
```
InvalidOperationException: No vector store configured.
Set either FoundryAgent:FileSearch:DefaultVectorStoreId or FoundryAgent:VectorStoreName
```

**Solution:**
Add FileSearch section:
```json
{
  "FileSearch": {
    "DefaultVectorStoreId": "vs_mystira_default"
  }
}
```

---

#### Issue 2: KnowledgeMode = "AISearch" but no AISearch section

**Error:**
```
InvalidOperationException: AISearch endpoint is required.
Set FoundryAgent:AISearch:Endpoint
```

**Solution:**
Add AISearch section:
```json
{
  "AISearch": {
    "Endpoint": "https://...",
    "ApiKey": "...",
    "IndexName": "guidelines"
  }
}
```

---

#### Issue 3: Age group not found in VectorStoresByAgeGroup

**Log warning:**
```
[WARN] No vector store configured for age group '13-15', using default
```

**Behavior:**
- System falls back to `DefaultVectorStoreId`
- Story generation continues

**Solution (if intentional):**
- This is expected for age groups without dedicated stores
- Default vector store should contain general guidelines

**Solution (if unintentional):**
Add missing age group:
```json
{
  "FileSearch": {
    "VectorStoresByAgeGroup": {
      "6-9": "vs_elementary",
      "13-15": "vs_teen"  // Add this
    }
  }
}
```

---

## Best Practices

### 1. **Use Nested Config for New Deployments**

✅ **Do:**
```json
{
  "KnowledgeMode": "FileSearch",
  "FileSearch": { ... }
}
```

❌ **Don't:**
```json
{
  "KnowledgeMode": "FileSearch",
  "VectorStoreName": "..."  // Legacy flat config
}
```

---

### 2. **Keep Unused Mode Config for Easy Switching**

```json
{
  "KnowledgeMode": "FileSearch",  // Currently using FileSearch
  "FileSearch": {
    "DefaultVectorStoreId": "vs_default"
  },
  "AISearch": {
    "Endpoint": "...",  // Pre-configured for easy switch
    "ApiKey": "...",
    "IndexName": "guidelines"
  }
}
```

**Benefit:** Switch modes by changing only `KnowledgeMode` value

---

### 3. **Use Environment-Specific Values**

```json
{
  "FoundryAgent": {
    "FileSearch": {
      "DefaultVectorStoreId": "vs_prod_default"  // Production
    }
  }
}
```

**Development:**
```json
{
  "FoundryAgent": {
    "FileSearch": {
      "DefaultVectorStoreId": "vs_dev_default"  // Development
    }
  }
}
```

---

### 4. **Validate Configuration on Startup**

Add to `Program.cs`:
```csharp
var foundryConfig = builder.Configuration
    .GetSection("FoundryAgent")
    .Get<FoundryAgentConfig>();

if (foundryConfig.KnowledgeMode == "FileSearch")
{
    if (foundryConfig.FileSearch?.DefaultVectorStoreId == null
        && foundryConfig.FileSearch?.VectorStoresByAgeGroup == null)
    {
        throw new InvalidOperationException(
            "FileSearch mode requires DefaultVectorStoreId or VectorStoresByAgeGroup");
    }
}
```

---

## Configuration Schema Reference

### Complete FileSearchConfig

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `DefaultVectorStoreId` | string | No* | - | Fallback vector store ID |
| `VectorStoresByAgeGroup` | Dictionary<string, string> | No* | - | Age-specific vector stores |
| `MaxFiles` | int? | No | - | Max files per search |
| `MaxTokens` | int? | No | - | Max tokens for results |

*At least one of `DefaultVectorStoreId` or `VectorStoresByAgeGroup` required

---

### Complete AISearchConfig

| Property | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `Endpoint` | string | Yes | - | Azure AI Search endpoint |
| `ApiKey` | string | Yes | - | Search service API key |
| `IndexName` | string | Yes | "mystira-instructions" | Index name |
| `AgeGroupFieldName` | string | No | "age_group" | Age metadata field |
| `ContentFieldName` | string | No | - | Content field override |
| `TopK` | int | No | 5 | Results per search |

---

## Summary

### Old Config (Deprecated but Supported)
```json
{
  "KnowledgeMode": "FileSearch",
  "VectorStoreName": "vs_default",
  "SearchIndexName": "guidelines"
}
```

### New Config (Recommended)
```json
{
  "KnowledgeMode": "FileSearch",
  "FileSearch": {
    "DefaultVectorStoreId": "vs_default",
    "VectorStoresByAgeGroup": { "6-9": "vs_elementary" }
  },
  "AISearch": {
    "Endpoint": "...",
    "IndexName": "guidelines"
  }
}
```

### Key Improvements
✅ Clear separation of FileSearch vs AISearch settings
✅ Self-documenting structure
✅ Easy mode switching
✅ Better IDE support
✅ Backward compatible
✅ Extensible for future enhancements
