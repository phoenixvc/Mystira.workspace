# Azure AI Foundry & RAG Technical Guide

This guide covers Mystira's Azure AI Foundry infrastructure and Retrieval-Augmented Generation (RAG) architecture.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Azure AI Foundry Setup](#azure-ai-foundry-setup)
- [Embedding Models](#embedding-models)
- [Azure AI Search](#azure-ai-search)
- [RAG Pipeline](#rag-pipeline)
- [Token Economics](#token-economics)
- [Regional Availability](#regional-availability)
- [Cost Optimization](#cost-optimization)

---

## Overview

Mystira uses Azure AI Foundry (formerly Azure OpenAI) with Azure AI Search to implement a RAG architecture for intelligent document retrieval and generation.

### Key Components

| Component | Purpose | Azure Service |
|-----------|---------|---------------|
| AI Foundry | LLM inference & embeddings | `Microsoft.CognitiveServices/accounts` (kind: AIServices) |
| AI Search | Vector storage & retrieval | `Microsoft.Search/searchServices` |
| AI Project | Workload isolation | AzAPI resource |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              RAG Architecture                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────┐    ┌─────────────────┐    ┌─────────────────┐                │
│  │  Query   │───▶│ Embedding Model │───▶│  Query Vector   │                │
│  │ (text)   │    │ (text-embed-3)  │    │  (1536 dims)    │                │
│  └──────────┘    └─────────────────┘    └────────┬────────┘                │
│                                                   │                         │
│                                                   ▼                         │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                        Azure AI Search                               │   │
│  │  ┌─────────────────────────────────────────────────────────────┐    │   │
│  │  │                     Vector Index                             │    │   │
│  │  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐           │    │   │
│  │  │  │ Chunk 1 │ │ Chunk 2 │ │ Chunk 3 │ │ Chunk N │  ...      │    │   │
│  │  │  │ [vec]   │ │ [vec]   │ │ [vec]   │ │ [vec]   │           │    │   │
│  │  │  └─────────┘ └─────────┘ └─────────┘ └─────────┘           │    │   │
│  │  └─────────────────────────────────────────────────────────────┘    │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                   │                         │
│                                         Top-K Results                       │
│                                                   ▼                         │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                         LLM (GPT-4.1 / Claude)                       │   │
│  │                                                                      │   │
│  │   System: You are a helpful assistant.                               │   │
│  │   Context: [Retrieved chunks from vector search]                     │   │
│  │   User: [Original query]                                             │   │
│  │                                                                      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                   │                         │
│                                                   ▼                         │
│                                           ┌──────────────┐                  │
│                                           │   Response   │                  │
│                                           └──────────────┘                  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Azure AI Foundry Setup

### Terraform Module

The `modules/shared/azure-ai` module provisions:

```hcl
module "shared_azure_ai" {
  source = "../../modules/shared/azure-ai"

  environment         = "dev"
  location           = "southafricanorth"
  region_code        = "san"
  resource_group_name = azurerm_resource_group.shared.name

  model_deployments = {
    # Embedding models for RAG
    "text-embedding-3-large" = {
      model_name    = "text-embedding-3-large"
      model_version = "1"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 120
    }

    # LLM for generation
    "gpt-4.1" = {
      model_name    = "gpt-4.1"
      model_version = "2025-04-14"
      model_format  = "OpenAI"
      sku_name      = "GlobalStandard"
      capacity      = 10
    }
  }
}
```

### Resource Naming

Resources follow the pattern: `mys-shared-ai-{region_code}`

| Environment | Resource Name | Region |
|-------------|---------------|--------|
| dev | mys-shared-ai-san | South Africa North |
| staging | mys-shared-ai-san | South Africa North |
| prod | mys-shared-ai-san | South Africa North |

---

## Embedding Models

### Available Models

| Model | Dimensions | Max Tokens | Use Case |
|-------|------------|------------|----------|
| `text-embedding-3-large` | 3072 | 8191 | High accuracy, production |
| `text-embedding-3-small` | 1536 | 8191 | Cost-effective, high volume |

### Embedding Configuration

```hcl
"text-embedding-3-large" = {
  model_name    = "text-embedding-3-large"
  model_version = "1"
  model_format  = "OpenAI"
  sku_name      = "GlobalStandard"  # Required for SAN region
  capacity      = 120               # Tokens per minute (K)
}
```

### Capacity Planning

| Environment | Capacity (TPM) | Est. Embeddings/min | Use Case |
|-------------|----------------|---------------------|----------|
| dev | 120K | ~15,000 | Development/testing |
| staging | 120K | ~15,000 | Integration testing |
| prod | 240K | ~30,000 | Production workloads |

*Assuming average chunk size of 500 tokens*

---

## Azure AI Search

### Terraform Module

```hcl
module "shared_azure_search" {
  source = "../../modules/shared/azure-search"

  environment         = "dev"
  location           = "southafricanorth"
  resource_group_name = azurerm_resource_group.shared.name

  sku                 = "basic"      # basic, standard, standard2, standard3
  replica_count       = 1
  partition_count     = 1
  semantic_search_sku = "disabled"   # free, standard (requires standard+ tier)
}
```

### SKU Comparison

| SKU | Indexes | Storage | Replicas | Partitions | Cost/mo (est) |
|-----|---------|---------|----------|------------|---------------|
| free | 3 | 50 MB | 1 | 1 | $0 |
| basic | 15 | 2 GB | 3 | 1 | ~$75 |
| standard | 50 | 25 GB | 12 | 12 | ~$250 |
| standard2 | 200 | 100 GB | 12 | 12 | ~$1,000 |
| standard3 | 200 | 200 GB | 12 | 12 | ~$2,000 |

### Vector Search Configuration

```json
{
  "name": "mystira-documents",
  "fields": [
    { "name": "id", "type": "Edm.String", "key": true },
    { "name": "content", "type": "Edm.String", "searchable": true },
    { "name": "contentVector", "type": "Collection(Edm.Single)",
      "dimensions": 1536, "vectorSearchProfile": "default" },
    { "name": "metadata", "type": "Edm.String", "filterable": true }
  ],
  "vectorSearch": {
    "algorithms": [{
      "name": "hnsw",
      "kind": "hnsw",
      "hnswParameters": { "m": 4, "efConstruction": 400, "efSearch": 500 }
    }],
    "profiles": [{
      "name": "default",
      "algorithmConfigurationName": "hnsw"
    }]
  }
}
```

---

## RAG Pipeline

### Document Ingestion

```
┌──────────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│   Document   │───▶│   Chunking   │───▶│  Embedding   │───▶│   Indexing   │
│   Upload     │    │  (500 tok)   │    │  API Call    │    │  AI Search   │
└──────────────┘    └──────────────┘    └──────────────┘    └──────────────┘
```

### Chunking Strategy

| Parameter | Recommended Value | Notes |
|-----------|-------------------|-------|
| Chunk size | 500-1000 tokens | Balance context vs precision |
| Overlap | 50-100 tokens | Preserve context at boundaries |
| Separator | Paragraphs/sentences | Natural language breaks |

### Query Flow

```csharp
// 1. Embed the query
var queryEmbedding = await embeddingClient.EmbedAsync(userQuery);

// 2. Vector search in AI Search
var searchResults = await searchClient.SearchAsync<Document>(
    searchText: null,
    new SearchOptions {
        VectorSearch = new() {
            Queries = { new VectorizedQuery(queryEmbedding) {
                KNearestNeighborsCount = 5,
                Fields = { "contentVector" }
            }}
        }
    });

// 3. Build context from results
var context = string.Join("\n\n", searchResults.Select(r => r.Content));

// 4. Generate response with LLM
var response = await chatClient.CompleteAsync(new[] {
    new SystemMessage($"Use this context to answer: {context}"),
    new UserMessage(userQuery)
});
```

---

## Token Economics

### Cost Comparison: With vs Without RAG

#### Without RAG (Context Stuffing)

```
Every Query = System Prompt + Full Document Context + User Query
            = 500 + 50,000 + 100 = 50,600 tokens

Cost per query (GPT-4): ~$1.50
```

#### With RAG

```
Indexing (one-time):
  - Embed 50,000 tokens = $0.001

Per Query:
  - Embed query: 100 tokens = $0.000002
  - Vector search: Free (compute only)
  - LLM call: 500 + 3,000 + 100 = 3,600 tokens

Cost per query (GPT-4): ~$0.11
```

### Savings Summary

| Knowledge Base | Without RAG | With RAG | Savings |
|----------------|-------------|----------|---------|
| 50K tokens | $1.50/query | $0.11/query | **13x** |
| 500K tokens | $15.00/query | $0.15/query | **100x** |
| 5M tokens | N/A (exceeds context) | $0.20/query | **Infinite** |

### Embedding Costs (Azure OpenAI)

| Model | Price per 1M tokens |
|-------|---------------------|
| text-embedding-3-small | $0.02 |
| text-embedding-3-large | $0.13 |

---

## Regional Availability

### South Africa North (Primary)

| Model | SKU | Available |
|-------|-----|-----------|
| gpt-4o | GlobalStandard | Yes |
| gpt-4o-mini | GlobalStandard | Yes |
| gpt-4.1 | GlobalStandard | Yes |
| gpt-4.1-nano | GlobalStandard | Yes |
| gpt-5-nano | GlobalStandard | Yes |
| gpt-5.1 | GlobalStandard | **No** |
| text-embedding-3-large | GlobalStandard | Yes |
| text-embedding-3-small | GlobalStandard | Yes |

**Note:** South Africa North only supports `GlobalStandard` SKU. `Standard` deployment is not available.

### UK South (Fallback)

Used for models not available in SAN:

| Model | SKU | Notes |
|-------|-----|-------|
| gpt-5.1 | GlobalStandard | Fallback region |
| claude-haiku-4-5 | Standard | Anthropic catalog |
| claude-sonnet-4-5 | Standard | Anthropic catalog |
| claude-opus-4-5 | Standard | Anthropic catalog |

### Per-Model Region Override

```hcl
"gpt-5.1" = {
  model_name    = "gpt-5.1"
  model_version = "2025-04-14"
  model_format  = "OpenAI"
  sku_name      = "GlobalStandard"
  capacity      = 10
  location      = "uksouth"  # Override: not available in SAN
}
```

---

## Cost Optimization

### Recommendations

1. **Use smaller embeddings for high-volume scenarios**
   - `text-embedding-3-small` is 6.5x cheaper than `large`
   - Accuracy difference is marginal for most use cases

2. **Optimize chunk sizes**
   - Smaller chunks = more precision but more API calls
   - Larger chunks = fewer calls but less precision
   - Sweet spot: 500-800 tokens

3. **Cache embeddings**
   - Store embeddings in AI Search, never re-compute
   - Use content hashing to detect changes

4. **Use appropriate LLM tiers**
   - `gpt-4.1-nano` for simple queries (10x cheaper than gpt-4.1)
   - `gpt-4.1` for complex reasoning
   - `claude-haiku` for high-volume, simple tasks

5. **Batch embedding requests**
   - Embed multiple chunks in single API call
   - Reduces latency and overhead

### Cost Estimation Formula

```
Monthly Cost = Indexing Cost + Query Cost

Indexing Cost = (Total Documents × Avg Tokens per Doc) / 1M × $0.02

Query Cost = Queries per Month × (
  Embedding Cost +
  Search Cost +
  LLM Cost
)

Where:
  Embedding Cost ≈ $0.000002 per query
  Search Cost ≈ SKU monthly cost / queries
  LLM Cost = (Context Tokens + Response Tokens) × Token Price
```

---

## Terraform Outputs

### Azure AI Foundry

```hcl
output "ai_foundry_endpoint" {
  value = module.shared_azure_ai.endpoint
}

output "ai_foundry_key" {
  value     = module.shared_azure_ai.primary_access_key
  sensitive = true
}

output "ai_foundry_deployments" {
  value = module.shared_azure_ai.deployments
}
```

### Azure AI Search

```hcl
output "search_endpoint" {
  value = module.shared_azure_search.endpoint
}

output "search_admin_key" {
  value     = module.shared_azure_search.primary_admin_key
  sensitive = true
}
```

---

## References

- [Azure AI Foundry Documentation](https://learn.microsoft.com/en-us/azure/ai-services/)
- [Azure AI Search Vector Search](https://learn.microsoft.com/en-us/azure/search/vector-search-overview)
- [OpenAI Embeddings Guide](https://platform.openai.com/docs/guides/embeddings)
- [RAG Pattern Best Practices](https://learn.microsoft.com/en-us/azure/architecture/ai-ml/guide/rag-solution-design)
