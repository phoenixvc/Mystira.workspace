# Azure AI Foundry & RAG Technical Guide

This guide covers Mystira's Azure AI Foundry infrastructure and Retrieval-Augmented Generation (RAG) architecture.

## Table of Contents

- [Overview](#overview)
- [SWOT Analysis](#swot-analysis)
- [Architecture](#architecture)
- [Azure AI Foundry Setup](#azure-ai-foundry-setup)
- [Embedding Models](#embedding-models)
- [Azure AI Search](#azure-ai-search)
- [Semantic Search](#semantic-search)
- [Knowledge Graphs](#knowledge-graphs)
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

## SWOT Analysis

Strategic analysis of key architectural decisions for Mystira's AI infrastructure.

### 1. RAG vs Fine-tuning

| | RAG (Retrieval-Augmented Generation) | Fine-tuning |
|---|---|---|
| **Strengths** | Real-time knowledge updates without retraining | Better task-specific performance |
| | No training costs or GPU requirements | Faster inference (no retrieval step) |
| | Full control over source documents | Consistent tone and style |
| | Easy to audit and explain responses | Lower per-query latency |
| **Weaknesses** | Added latency from retrieval step | Expensive training ($1k-$100k+) |
| | Requires vector database infrastructure | Knowledge frozen at training time |
| | Chunking strategy affects quality | Risk of catastrophic forgetting |
| | More complex architecture | Requires ML expertise |
| **Opportunities** | Combine with fine-tuning for best results | Distill knowledge into smaller models |
| | Hybrid search (vector + keyword) | Custom domain vocabulary |
| | Multi-modal RAG (images, tables) | Reduce inference costs long-term |
| **Threats** | Retrieval failures cause hallucinations | Model drift over time |
| | Embedding model changes break indices | Training data poisoning |
| | Context window growth may reduce need | OpenAI deprecation of base models |

**Mystira Decision:** RAG for dynamic content (documents, stories), fine-tuning considered for stable domain knowledge.

---

### 2. Embedding Model Selection

| | text-embedding-3-large | text-embedding-3-small |
|---|---|---|
| **Strengths** | Highest accuracy (64.6% MTEB) | 6.5x cheaper ($0.02 vs $0.13/1M) |
| | 3072 dimensions for nuance | 5x faster processing |
| | Best for complex/technical content | Sufficient for most use cases |
| | Matryoshka support (variable dims) | Lower storage requirements |
| **Weaknesses** | Higher storage costs (3072 floats) | Lower accuracy (62.3% MTEB) |
| | Slower embedding generation | Less nuance in similar content |
| | Overkill for simple content | May miss subtle distinctions |
| **Opportunities** | Reduce dims to 1536 for cost savings | Upgrade path to large if needed |
| | Premium tier for critical queries | Batch processing for high volume |
| **Threats** | Cost overruns at scale | Accuracy issues in edge cases |
| | Dimension lock-in (reindex needed) | Competitive models may surpass |

**Mystira Decision:** Both models deployed. Large for production documents, small for high-volume/draft content.

---

### 3. Regional Deployment (South Africa North vs UK South)

| | South Africa North (Primary) | UK South (Fallback) |
|---|---|---|
| **Strengths** | Lowest latency for SA users | Full model availability |
| | Data sovereignty compliance | Standard SKU support |
| | Reduced egress costs | Anthropic Claude access |
| | Growing Azure investment | Mature infrastructure |
| **Weaknesses** | Limited model availability | 150ms+ additional latency |
| | GlobalStandard SKU only | Higher egress costs to SA |
| | No Anthropic models | GDPR considerations |
| | Smaller capacity quotas | Separate billing region |
| **Opportunities** | Microsoft expanding SA region | Multi-region failover |
| | First-mover in African AI market | EU market expansion |
| | Local partnerships | Access to latest models first |
| **Threats** | Model deprecation without replacement | Latency-sensitive features suffer |
| | Capacity constraints during growth | Currency fluctuation (ZAR/GBP) |
| | Power/connectivity issues | Brexit regulatory changes |

**Mystira Decision:** Primary workloads in SAN, specific models (gpt-5.1, Claude) in UK South with per-model override.

---

### 4. Azure AI Search SKU Tiers

| | Basic ($75/mo) | Standard ($250/mo) | Standard2 ($1000/mo) |
|---|---|---|---|
| **Strengths** | Cost-effective for dev/small prod | Semantic search support | High-volume production |
| | 2GB storage sufficient for MVP | 25GB covers most use cases | 100GB for large corpora |
| | 15 indexes for multi-tenant | 50 indexes for growth | 200 indexes enterprise |
| | Quick provisioning | 12 replicas for HA | 12 partitions for scale |
| **Weaknesses** | No semantic ranking | 4x cost of basic | 13x cost of basic |
| | Limited to 3 replicas | May be oversized for small apps | Overkill for most startups |
| | Single partition only | Complex capacity planning | Long provisioning times |
| **Opportunities** | Upgrade path when needed | Hybrid search combinations | Dedicated capacity |
| | Validate before scaling | Geographic replicas | SLA guarantees |
| **Threats** | Outgrow quickly with success | Cost creep with replicas | Underutilization waste |
| | No HA without replicas | Semantic search costs extra | Lock-in at scale |

**Mystira Decision:** Basic for dev, Standard for staging/prod (semantic search enabled).

---

### 5. LLM Provider Selection

| | Azure OpenAI (GPT-4.1) | Anthropic Claude (Sonnet 4.5) |
|---|---|---|
| **Strengths** | Native Azure integration | Superior reasoning/analysis |
| | Established ecosystem | Longer context (200k tokens) |
| | GPT-4.1 strong general purpose | Better instruction following |
| | Predictable Microsoft roadmap | Constitutional AI safety |
| | GlobalStandard in more regions | Competitive pricing |
| **Weaknesses** | Context limited vs Claude | UK South only (no SAN) |
| | Higher hallucination rate | Smaller ecosystem |
| | Azure-only deployment | Less Azure tooling integration |
| | Slower new model rollout | Newer, less battle-tested |
| **Opportunities** | GPT-5 on horizon | Claude Opus for complex tasks |
| | Multi-modal (vision, audio) | Haiku for cost-effective volume |
| | Function calling maturity | Artifacts/tool use features |
| **Threats** | OpenAI/Microsoft relationship | Anthropic funding/runway |
| | Rapid deprecation cycles | Limited regional expansion |
| | Competition from open models | API stability (newer provider) |

**Mystira Decision:** Multi-provider strategy. GPT-4.1 for general tasks, Claude Sonnet for analysis, Haiku for high-volume.

---

### 6. Vector Search Algorithm

| | HNSW (Hierarchical NSW) | IVF (Inverted File Index) | Flat/Brute Force |
|---|---|---|---|
| **Strengths** | Best recall/speed balance | Lower memory footprint | 100% recall (exact) |
| | No training required | Good for very large datasets | Simple implementation |
| | Incremental updates | Faster index building | No tuning needed |
| | Industry standard | Predictable performance | Best for small datasets |
| **Weaknesses** | Higher memory usage | Requires training step | O(n) query time |
| | Tuning parameters (M, ef) | Lower recall than HNSW | Doesn't scale |
| | Slower index builds | Updates require retraining | Impractical >100k vectors |
| **Opportunities** | Hybrid with pre-filtering | Combine with quantization | Baseline for testing |
| | Dynamic ef for quality/speed | Product quantization | Validation benchmark |
| **Threats** | Memory costs at scale | Training data distribution shift | Performance cliff |
| | Suboptimal parameters hurt recall | Cold start with new data | Cost explosion |

**Mystira Decision:** HNSW (Azure AI Search default) with M=4, efConstruction=400, efSearch=500.

---

### 7. Chunking Strategy

| | Fixed-size (500 tokens) | Semantic (paragraph/section) | Sliding Window (overlap) |
|---|---|---|---|
| **Strengths** | Predictable, simple | Preserves meaning boundaries | Context continuity |
| | Consistent embedding quality | Better retrieval relevance | Handles topic transitions |
| | Easy capacity planning | Natural document structure | Reduces boundary artifacts |
| | Reproducible results | Fewer chunks overall | Best recall for edge cases |
| **Weaknesses** | Splits mid-sentence/thought | Variable chunk sizes | 20-30% more embeddings |
| | Context loss at boundaries | Complex implementation | Higher storage/compute |
| | May separate related content | Depends on document format | Duplicate content in results |
| **Opportunities** | Hybrid with overlap | LLM-based chunking | Adaptive overlap by content |
| | Post-retrieval merging | Hierarchical chunks | Dynamic window sizing |
| **Threats** | Poor retrieval for narratives | Inconsistent quality | Cost overruns |
| | Missed context in answers | Parser failures | Deduplication complexity |

**Mystira Decision:** Semantic chunking with 100-token sliding window overlap for narrative content.

---

### Summary Decision Matrix

| Decision | Choice | Primary Driver |
|----------|--------|----------------|
| Architecture | RAG | Dynamic content, cost efficiency |
| Embedding (primary) | text-embedding-3-large | Accuracy for production |
| Embedding (volume) | text-embedding-3-small | Cost for drafts/testing |
| Primary Region | South Africa North | Latency, data sovereignty |
| Fallback Region | UK South | Model availability |
| Search Tier | Standard | Semantic search, growth |
| Primary LLM | GPT-4.1 | Integration, availability |
| Analysis LLM | Claude Sonnet | Reasoning quality |
| Volume LLM | Claude Haiku / GPT-4.1-nano | Cost efficiency |
| Vector Algorithm | HNSW | Recall/speed balance |
| Chunking | Semantic + overlap | Narrative content quality |

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

### Matryoshka Embeddings

OpenAI's `text-embedding-3` models support **Matryoshka Representation Learning (MRL)**, allowing you to truncate embeddings to smaller dimensions without recomputing them.

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Matryoshka Embeddings Concept                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Full Embedding (3072 dimensions):                                          │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ [0.12, -0.34, 0.56, ... , 0.78, -0.91, 0.23, ... , 0.45, -0.67, ...]│   │
│  │  ◀──── 256 dims ────▶ ◀──── 512 dims ────▶ ◀────── 3072 dims ─────▶│   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│           │                      │                         │                │
│           ▼                      ▼                         ▼                │
│   ┌──────────────┐     ┌──────────────────┐    ┌───────────────────────┐   │
│   │  256 dims    │     │    1024 dims     │    │     3072 dims         │   │
│   │  (smallest)  │     │   (balanced)     │    │    (full accuracy)    │   │
│   └──────────────┘     └──────────────────┘    └───────────────────────┘   │
│                                                                             │
│   Like Russian nesting dolls - smaller representations are                  │
│   contained within larger ones, no need to re-embed!                        │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### How It Works

Traditional embeddings require you to choose dimensions at embedding time. Matryoshka models embed the most important semantic information in the first dimensions, with progressively less critical information in later dimensions.

```python
from openai import AzureOpenAI

client = AzureOpenAI(...)

# Generate full 3072-dimension embedding
response = client.embeddings.create(
    model="text-embedding-3-large",
    input="Your document text here",
    dimensions=3072  # Full dimensions
)

# OR generate truncated 1024-dimension embedding (same model!)
response = client.embeddings.create(
    model="text-embedding-3-large",
    input="Your document text here",
    dimensions=1024  # Truncated - 66% storage savings
)
```

#### Dimension Trade-offs

| Dimensions | Storage (per vector) | MTEB Score | Use Case |
|------------|---------------------|------------|----------|
| 256 | 1 KB | ~60% | Prototyping, massive scale |
| 512 | 2 KB | ~62% | High-volume, cost-sensitive |
| 1024 | 4 KB | ~63% | Balanced accuracy/cost |
| 1536 | 6 KB | ~64% | Standard (matches small model) |
| 3072 | 12 KB | ~65% | Maximum accuracy |

#### Cost Implications

Using Matryoshka to reduce dimensions provides:

1. **Storage Savings**: 1024 dims = 66% less storage than 3072
2. **Faster Search**: Smaller vectors = faster similarity calculations
3. **Same Embedding Cost**: You pay the same to embed, but save on storage/compute
4. **Index Flexibility**: Can test different dimensions without re-embedding

#### Practical Strategy

```
┌─────────────────────────────────────────────────────────────────┐
│  Recommended Matryoshka Strategy                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. EMBED at full dimensions (3072) - store original vectors   │
│                                                                 │
│  2. INDEX at reduced dimensions:                                │
│     • Dev/Test: 512 dims (fast iteration)                      │
│     • Production: 1024 or 1536 dims (balanced)                 │
│     • High-precision: 3072 dims (when accuracy critical)       │
│                                                                 │
│  3. UPGRADE without re-embedding:                               │
│     • Start with 1024 dims                                      │
│     • If accuracy insufficient, expand to 1536 or 3072          │
│     • No API calls needed - just use more of stored vector      │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

#### Azure AI Search Configuration

```json
{
  "name": "documents-index",
  "fields": [
    {
      "name": "contentVector",
      "type": "Collection(Edm.Single)",
      "dimensions": 1024,
      "vectorSearchProfile": "default"
    },
    {
      "name": "contentVectorFull",
      "type": "Collection(Edm.Single)",
      "dimensions": 3072,
      "vectorSearchProfile": "high-precision"
    }
  ]
}
```

This allows querying with reduced dimensions for speed, falling back to full dimensions when precision matters.

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

## Semantic Search

Semantic search goes beyond keyword matching to understand the *meaning* and *intent* behind queries.

### How Semantic Search Differs from Traditional Search

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Traditional vs Semantic Search                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  KEYWORD SEARCH (BM25/TF-IDF)                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  Query: "How do I fix my car not starting?"                         │   │
│  │                                                                      │   │
│  │  Matches documents containing: "fix", "car", "starting"              │   │
│  │  ✗ Misses: "vehicle won't turn over" (same meaning, different words)│   │
│  │  ✗ Misses: "engine ignition problems" (related concept)             │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  SEMANTIC SEARCH (Vector Embeddings)                                        │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  Query: "How do I fix my car not starting?"                         │   │
│  │                           ↓                                          │   │
│  │              [0.12, -0.34, 0.56, ...]  (query vector)               │   │
│  │                           ↓                                          │   │
│  │  ✓ Finds: "vehicle won't turn over" (similar vector)                │   │
│  │  ✓ Finds: "troubleshooting ignition issues" (similar vector)        │   │
│  │  ✓ Finds: "battery dead symptoms" (contextually related)            │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Azure AI Search Semantic Ranking

Azure AI Search offers **semantic ranking** as an additional layer on top of vector search:

```
┌─────────────────────────────────────────────────────────────────┐
│                   Semantic Ranking Pipeline                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  1. INITIAL RETRIEVAL (Vector or Keyword)                       │
│     └─▶ Returns top 50 candidate documents                      │
│                                                                 │
│  2. SEMANTIC RERANKING (Microsoft's language models)            │
│     └─▶ Reorders based on deep semantic understanding          │
│     └─▶ Considers query intent, not just word overlap           │
│                                                                 │
│  3. SEMANTIC CAPTIONS                                           │
│     └─▶ Extracts most relevant passages                         │
│     └─▶ Highlights key phrases                                  │
│                                                                 │
│  4. SEMANTIC ANSWERS (optional)                                 │
│     └─▶ Extracts direct answers from content                    │
│     └─▶ Like a mini-QA system                                   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Terraform Configuration for Semantic Search

```hcl
module "shared_azure_search" {
  source = "../../modules/shared/azure-search"

  environment         = "prod"
  location           = "southafricanorth"
  resource_group_name = azurerm_resource_group.shared.name

  sku                 = "standard"    # Required for semantic search
  semantic_search_sku = "standard"    # Enable semantic ranking
  replica_count       = 2
  partition_count     = 1
}
```

### Hybrid Search: Best of Both Worlds

Combine keyword and vector search for optimal results:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Hybrid Search Strategy                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                              User Query                                      │
│                                  │                                          │
│                    ┌─────────────┴─────────────┐                            │
│                    ▼                           ▼                            │
│           ┌──────────────┐            ┌──────────────┐                      │
│           │   Keyword    │            │   Vector     │                      │
│           │   Search     │            │   Search     │                      │
│           │   (BM25)     │            │ (Embeddings) │                      │
│           └──────┬───────┘            └──────┬───────┘                      │
│                  │                           │                              │
│                  │     ┌───────────────┐     │                              │
│                  └────▶│  RRF Fusion   │◀────┘                              │
│                        │ (Reciprocal   │                                    │
│                        │  Rank Fusion) │                                    │
│                        └───────┬───────┘                                    │
│                                │                                            │
│                                ▼                                            │
│                      ┌──────────────────┐                                   │
│                      │ Semantic Rerank  │                                   │
│                      │   (optional)     │                                   │
│                      └────────┬─────────┘                                   │
│                               │                                             │
│                               ▼                                             │
│                        Final Results                                        │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Search Query Example (C#)

```csharp
var searchOptions = new SearchOptions
{
    // Hybrid: keyword + vector
    QueryType = SearchQueryType.Semantic,
    SemanticSearch = new SemanticSearchOptions
    {
        SemanticConfigurationName = "my-semantic-config",
        QueryCaption = new QueryCaption(QueryCaptionType.Extractive),
        QueryAnswer = new QueryAnswer(QueryAnswerType.Extractive)
    },
    VectorSearch = new VectorSearchOptions
    {
        Queries = {
            new VectorizedQuery(queryEmbedding)
            {
                KNearestNeighborsCount = 10,
                Fields = { "contentVector" }
            }
        }
    },
    Size = 10
};

var results = await searchClient.SearchAsync<Document>(
    searchText: "How do I reset my password?",  // Keyword component
    searchOptions
);

// Access semantic answers
foreach (var answer in results.SemanticSearch.Answers)
{
    Console.WriteLine($"Answer: {answer.Text}");
    Console.WriteLine($"Confidence: {answer.Score}");
}
```

### When to Use Each Search Type

| Search Type | Best For | Latency | Cost |
|-------------|----------|---------|------|
| Keyword (BM25) | Exact matches, known terms | ~10ms | Low |
| Vector | Semantic similarity, concept search | ~50ms | Medium |
| Hybrid (Keyword + Vector) | General purpose, best recall | ~60ms | Medium |
| Hybrid + Semantic Rerank | Highest quality results | ~200ms | Higher |

**Mystira Recommendation:** Use hybrid search with semantic reranking for user-facing queries, pure vector search for background/batch operations.

---

## Knowledge Graphs

Knowledge graphs represent information as interconnected entities and relationships, enabling structured reasoning beyond what vector search alone can provide.

### What is a Knowledge Graph?

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Knowledge Graph Structure                            │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ENTITIES (Nodes)                    RELATIONSHIPS (Edges)                  │
│  ┌─────────────┐                                                           │
│  │   Author    │                                                           │
│  │  "J.K.      │──────── wrote ────────▶┌─────────────┐                    │
│  │  Rowling"   │                        │    Book     │                    │
│  └─────────────┘                        │  "Harry     │                    │
│        │                                │   Potter"   │                    │
│        │                                └──────┬──────┘                    │
│   born_in                                      │                           │
│        │                          features     │    set_in                 │
│        ▼                                       ▼        │                  │
│  ┌─────────────┐                        ┌───────────┐   │                  │
│  │  Location   │                        │ Character │   │                  │
│  │   "UK"      │                        │ "Hermione"│   │                  │
│  └─────────────┘                        └───────────┘   │                  │
│                                               │         ▼                  │
│                                          attends  ┌───────────┐            │
│                                               │   │  Location │            │
│                                               ▼   │ "Hogwarts"│            │
│                                         ┌─────────┴───┐                    │
│                                         │    School   │                    │
│                                         │  "Hogwarts" │                    │
│                                         └─────────────┘                    │
│                                                                             │
│  Triple Format: (Subject) ─[Predicate]─▶ (Object)                          │
│  Example: (Harry Potter) ─[written_by]─▶ (J.K. Rowling)                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Why Knowledge Graphs + RAG?

Vector search finds semantically similar text, but can miss structured relationships:

| Query | Vector Search Result | Knowledge Graph Result |
|-------|---------------------|----------------------|
| "Who wrote Harry Potter?" | Passages mentioning the book | Direct: J.K. Rowling (author entity) |
| "Books by British authors" | Text about British literature | Traversal: Author(country=UK) → wrote → Book |
| "Characters in the same school as Harry" | Passages mentioning Hogwarts | Graph query: Harry → attends → Hogwarts ← attends ← ? |

### GraphRAG Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          GraphRAG Architecture                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                              User Query                                      │
│                                  │                                          │
│                    ┌─────────────┼─────────────┐                            │
│                    ▼             ▼             ▼                            │
│           ┌──────────────┐ ┌──────────┐ ┌──────────────┐                   │
│           │   Vector     │ │  Graph   │ │   Keyword    │                   │
│           │   Search     │ │  Query   │ │   Search     │                   │
│           │ (Embeddings) │ │ (Cypher/ │ │   (BM25)     │                   │
│           │              │ │  SPARQL) │ │              │                   │
│           └──────┬───────┘ └────┬─────┘ └──────┬───────┘                   │
│                  │              │              │                            │
│                  │   ┌──────────┴──────────┐   │                            │
│                  │   │   Entity Linking    │   │                            │
│                  │   │ (Connect text to    │   │                            │
│                  │   │  graph entities)    │   │                            │
│                  │   └──────────┬──────────┘   │                            │
│                  │              │              │                            │
│                  └──────────────┼──────────────┘                            │
│                                 ▼                                           │
│                    ┌────────────────────────┐                               │
│                    │    Context Assembly    │                               │
│                    │  • Retrieved passages  │                               │
│                    │  • Graph triples       │                               │
│                    │  • Entity properties   │                               │
│                    └───────────┬────────────┘                               │
│                                │                                            │
│                                ▼                                            │
│                    ┌────────────────────────┐                               │
│                    │         LLM            │                               │
│                    │  (Enhanced context     │                               │
│                    │   with structured      │                               │
│                    │   knowledge)           │                               │
│                    └───────────┬────────────┘                               │
│                                │                                            │
│                                ▼                                            │
│                         Final Response                                      │
│                  (Grounded in both text AND                                │
│                   structured relationships)                                 │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Building a Knowledge Graph from Documents

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Knowledge Graph Construction Pipeline                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  1. ENTITY EXTRACTION (NER)                                                 │
│     ┌─────────────────────────────────────────────────────────────────┐    │
│     │ "Microsoft announced that Satya Nadella will present the new    │    │
│     │  Azure AI features at Build 2025 in Seattle."                   │    │
│     └─────────────────────────────────────────────────────────────────┘    │
│                                    │                                        │
│                                    ▼                                        │
│     Entities: [Microsoft:ORG] [Satya Nadella:PERSON] [Azure AI:PRODUCT]    │
│               [Build 2025:EVENT] [Seattle:LOCATION]                         │
│                                                                             │
│  2. RELATION EXTRACTION                                                     │
│     • (Satya Nadella) ─[CEO_of]─▶ (Microsoft)                              │
│     • (Satya Nadella) ─[presents_at]─▶ (Build 2025)                        │
│     • (Build 2025) ─[located_in]─▶ (Seattle)                               │
│     • (Azure AI) ─[product_of]─▶ (Microsoft)                               │
│                                                                             │
│  3. ENTITY RESOLUTION (Deduplication)                                       │
│     • "Microsoft" = "Microsoft Corp" = "MSFT" → Single entity              │
│     • "Satya Nadella" = "Nadella" = "Microsoft CEO" → Single entity        │
│                                                                             │
│  4. GRAPH STORAGE                                                           │
│     • Neo4j, Azure Cosmos DB (Gremlin), or Amazon Neptune                  │
│     • Indexed for fast traversal queries                                    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### LLM-Powered Entity Extraction

```python
from openai import AzureOpenAI

def extract_entities_and_relations(text: str) -> dict:
    """Use LLM to extract knowledge graph triples from text."""

    prompt = """Extract entities and relationships from the following text.

    Return JSON format:
    {
        "entities": [
            {"name": "...", "type": "PERSON|ORG|LOCATION|PRODUCT|EVENT|CONCEPT"}
        ],
        "relations": [
            {"subject": "...", "predicate": "...", "object": "..."}
        ]
    }

    Text: {text}
    """

    response = client.chat.completions.create(
        model="gpt-4.1",
        messages=[{"role": "user", "content": prompt.format(text=text)}],
        response_format={"type": "json_object"}
    )

    return json.loads(response.choices[0].message.content)

# Example usage
text = "Mystira is a storytelling platform founded in South Africa that uses Azure AI."
result = extract_entities_and_relations(text)

# Result:
# {
#     "entities": [
#         {"name": "Mystira", "type": "ORG"},
#         {"name": "South Africa", "type": "LOCATION"},
#         {"name": "Azure AI", "type": "PRODUCT"}
#     ],
#     "relations": [
#         {"subject": "Mystira", "predicate": "is_a", "object": "storytelling platform"},
#         {"subject": "Mystira", "predicate": "founded_in", "object": "South Africa"},
#         {"subject": "Mystira", "predicate": "uses", "object": "Azure AI"}
#     ]
# }
```

### Azure Options for Knowledge Graphs

| Service | Best For | Query Language | Integration |
|---------|----------|---------------|-------------|
| **Azure Cosmos DB (Gremlin)** | Managed graph DB, global distribution | Gremlin | Native Azure |
| **Neo4j on Azure** | Full-featured graph, Cypher queries | Cypher | Marketplace VM |
| **Azure SQL Graph** | SQL Server with graph extensions | T-SQL + MATCH | Existing SQL workloads |
| **RDF Triple Store** | Semantic web, ontologies | SPARQL | Standards-based |

### Cosmos DB Gremlin Example

```csharp
// Add entities
await gremlinClient.SubmitAsync(
    "g.addV('Author').property('name', 'J.K. Rowling').property('country', 'UK')"
);
await gremlinClient.SubmitAsync(
    "g.addV('Book').property('title', 'Harry Potter').property('year', 1997)"
);

// Add relationship
await gremlinClient.SubmitAsync(
    "g.V().has('Author', 'name', 'J.K. Rowling')" +
    ".addE('wrote')" +
    ".to(g.V().has('Book', 'title', 'Harry Potter'))"
);

// Query: Find all books by British authors
var query = @"
    g.V().hasLabel('Author')
         .has('country', 'UK')
         .out('wrote')
         .hasLabel('Book')
         .values('title')
";
var results = await gremlinClient.SubmitAsync<string>(query);
```

### When to Use Knowledge Graphs

| Use Case | Vector Search | Knowledge Graph | Both (GraphRAG) |
|----------|--------------|-----------------|-----------------|
| "Find similar documents" | ✓ Best | ✗ | ✓ |
| "Who is the CEO of X?" | ✗ Indirect | ✓ Best | ✓ |
| "List all products in category Y" | ✗ | ✓ Best | ✓ |
| "Explain concept X with examples" | ✓ | ✓ | ✓ Best |
| "How are A and B related?" | ✗ | ✓ Best | ✓ |
| Multi-hop reasoning | ✗ | ✓ Best | ✓ |

### GraphRAG for Mystira: Potential Use Cases

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     Mystira Knowledge Graph Schema                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────┐         creates        ┌─────────┐        contains            │
│  │  User   │─────────────────────▶│  Story  │────────────────────┐        │
│  └────┬────┘                        └────┬────┘                   │        │
│       │                                  │                        ▼        │
│       │ follows                          │ features         ┌──────────┐   │
│       │                                  │                  │  Scene   │   │
│       ▼                                  ▼                  └──────────┘   │
│  ┌─────────┐                       ┌───────────┐                           │
│  │  User   │                       │ Character │◀───────────┐              │
│  └─────────┘                       └─────┬─────┘            │              │
│                                          │                  │ appears_in   │
│                                    related_to               │              │
│                                          │                  │              │
│                                          ▼            ┌─────┴────┐         │
│                                    ┌───────────┐      │  Scene   │         │
│                                    │ Character │      └──────────┘         │
│                                    └───────────┘                           │
│                                                                             │
│  Query Examples:                                                            │
│  • "Stories with characters similar to [X]"                                │
│  • "Users who like stories featuring [theme]"                              │
│  • "Character relationship map for [story]"                                │
│  • "Recommend stories based on graph similarity"                           │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Implementation Roadmap

| Phase | Component | Description |
|-------|-----------|-------------|
| 1 | Vector Search (Current) | Azure AI Search with embeddings |
| 2 | Semantic Ranking | Enable semantic reranking on search |
| 3 | Entity Extraction | LLM-powered NER on story content |
| 4 | Graph Storage | Cosmos DB Gremlin for entities |
| 5 | GraphRAG Integration | Combined retrieval pipeline |

**Mystira Decision:** Start with hybrid semantic search (Phase 1-2), evaluate knowledge graph needs based on query patterns and user feedback.

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
