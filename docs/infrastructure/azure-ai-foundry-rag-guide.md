# Azure AI Foundry & RAG Technical Guide

This guide covers Mystira's Azure AI Foundry infrastructure and Retrieval-Augmented Generation (RAG) architecture.

## Table of Contents

- [Overview](#overview)
- [SWOT Analysis](#swot-analysis)
- [Architecture](#architecture)
- [Azure AI Foundry Setup](#azure-ai-foundry-setup)
- [Model Selection Guide](#model-selection-guide)
- [Deploying Claude Models](#deploying-claude-models)
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

## Model Selection Guide

Choose the right model for your use case based on capability, cost, and latency requirements.

### Model Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Model Capability Spectrum                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  COST ←──────────────────────────────────────────────────────────→ QUALITY  │
│                                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │ GPT-4o   │  │ GPT-4.1  │  │ GPT-4o   │  │ Claude   │  │ Claude   │     │
│  │  mini    │  │  nano    │  │          │  │ Sonnet   │  │  Opus    │     │
│  │ $0.15/1M │  │ $0.10/1M │  │ $2.50/1M │  │ $3.00/1M │  │$15.00/1M │     │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │
│       │              │             │             │             │           │
│       ▼              ▼             ▼             ▼             ▼           │
│   High-volume    Embeddings    General      Analysis     Complex          │
│   Simple tasks   + Simple      Purpose      Reasoning    Multi-step       │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Use Case Matrix

| Use Case | Recommended Model | Why |
|----------|-------------------|-----|
| **Chat/Conversational** | gpt-4o-mini | Fast, cheap, good enough for most conversations |
| **Content Generation** | gpt-4o | Better creativity and coherence |
| **Code Generation** | gpt-5.1-codex / Claude Sonnet | Specialized for code understanding |
| **Code Review/Analysis** | Claude Sonnet | Superior reasoning about code structure |
| **Summarization** | gpt-4o-mini | Cost-effective for high-volume |
| **Complex Analysis** | Claude Opus | Best reasoning, handles nuance |
| **RAG Retrieval** | gpt-4o-mini | Fast context processing |
| **Data Extraction** | gpt-4.1 | Good structured output |
| **Translation** | gpt-4o | Strong multilingual support |
| **Classification** | gpt-4.1-nano | Fastest for simple decisions |
| **Creative Writing** | Claude Sonnet | Better narrative flow |
| **Technical Docs** | Claude Sonnet | Precise, well-structured |
| **Embeddings** | text-embedding-3-large | Best accuracy for RAG |
| **High-volume Embeddings** | text-embedding-3-small | 6x cheaper, 95% accuracy |

### Model Tiers

#### Tier 1: High-Volume / Cost-Optimized

| Model | Input Cost | Output Cost | Best For |
|-------|------------|-------------|----------|
| gpt-4o-mini | $0.15/1M | $0.60/1M | Chat, summarization, classification |
| gpt-4.1-nano | $0.10/1M | $0.40/1M | Simple tasks, routing, embeddings assist |
| Claude Haiku | $0.25/1M | $1.25/1M | Fast analysis, high-volume processing |

#### Tier 2: General Purpose

| Model | Input Cost | Output Cost | Best For |
|-------|------------|-------------|----------|
| gpt-4o | $2.50/1M | $10.00/1M | General tasks, content creation |
| gpt-4.1 | $2.00/1M | $8.00/1M | Structured output, data extraction |
| gpt-5-nano | $1.00/1M | $4.00/1M | Advanced reasoning, cheaper than 4o |

#### Tier 3: Premium / Analysis

| Model | Input Cost | Output Cost | Best For |
|-------|------------|-------------|----------|
| gpt-5.1 | $5.00/1M | $15.00/1M | Complex multi-step tasks |
| gpt-5.1-codex | $5.00/1M | $15.00/1M | Code generation and review |
| Claude Sonnet | $3.00/1M | $15.00/1M | Analysis, reasoning, code review |

#### Tier 4: Maximum Capability

| Model | Input Cost | Output Cost | Best For |
|-------|------------|-------------|----------|
| Claude Opus | $15.00/1M | $75.00/1M | Most complex tasks, research, deep analysis |

### Decision Flowchart

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Model Selection Flowchart                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│                            Start                                            │
│                              │                                              │
│                              ▼                                              │
│                    ┌─────────────────┐                                      │
│                    │ Is it code-     │                                      │
│                    │ related?        │                                      │
│                    └────────┬────────┘                                      │
│                      Yes    │    No                                         │
│              ┌──────────────┴──────────────┐                                │
│              ▼                             ▼                                │
│    ┌─────────────────┐           ┌─────────────────┐                       │
│    │ Complex review? │           │ High volume?    │                       │
│    └────────┬────────┘           │ (>1000/day)     │                       │
│       Yes   │   No               └────────┬────────┘                       │
│        │    │                       Yes   │   No                           │
│        ▼    ▼                        │    │                                │
│   Claude   gpt-5.1                   ▼    ▼                                │
│   Sonnet   -codex              ┌─────────────────┐                         │
│                                │ Needs complex   │                         │
│              ┌─────────────────│ reasoning?      │                         │
│              │                 └────────┬────────┘                         │
│              ▼                    Yes   │   No                             │
│         gpt-4o-mini                │    │                                  │
│         or Haiku                   ▼    ▼                                  │
│                              ┌─────────────────┐                           │
│                              │ Creative or     │──Yes──▶ Claude Sonnet     │
│                              │ analytical?     │         or gpt-4o         │
│                              └────────┬────────┘                           │
│                                       │ No                                 │
│                                       ▼                                    │
│                              ┌─────────────────┐                           │
│                              │ Mission         │──Yes──▶ Claude Opus       │
│                              │ critical?       │                           │
│                              └────────┬────────┘                           │
│                                       │ No                                 │
│                                       ▼                                    │
│                                  gpt-4o-mini                               │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Mystira Model Deployment

*Last updated: December 2025 - See [ADR-0020](../adr/ADR-0020-ai-model-selection-strategy.md) for full details*

| Model | Region | SKU | Use Case in Mystira |
|-------|--------|-----|---------------------|
| gpt-4o | SAN | GlobalStandard | General content generation |
| gpt-4o-mini | SAN | GlobalStandard | Chat, high-volume tasks |
| gpt-4.1 | SAN | GlobalStandard | Structured data extraction (1M ctx) |
| gpt-4.1-nano | SAN | GlobalStandard | Classification, routing (1M ctx) |
| gpt-5-nano | SAN | GlobalStandard | Advanced reasoning (cost-effective) |
| gpt-5.1 | SAN | GlobalStandard | Complex analysis |
| gpt-5.1-codex | SAN | GlobalStandard | Code generation/review |
| gpt-5.2 | SAN | GlobalStandard | Latest model (400K ctx) |
| o3 | SAN | GlobalStandard | Advanced chain-of-thought |
| o3-mini | SAN | GlobalStandard | Chain-of-thought analysis |
| o4-mini | SAN | GlobalStandard | Fast reasoning |
| text-embedding-3-large | SAN | GlobalStandard | Production RAG embeddings |
| text-embedding-3-small | SAN | GlobalStandard | Draft/test embeddings |
| dall-e-3 | SAN | Standard | Story illustrations |
| gpt-image-1 | SAN | Standard | Advanced image generation |
| whisper | SAN | Standard | Speech-to-text |
| tts / tts-hd | SAN | Standard | Text-to-speech / HD |
| claude-haiku-4-5 | UK South | Serverless | High-volume analysis ($1/$5) |
| claude-sonnet-4-5 | UK South | Serverless | Deep analysis (1M ctx) |
| claude-opus-4-5 | UK South | Serverless | Complex research tasks |
| cohere-rerank-v3 | UK South | Serverless | RAG reranking |
| deepseek-v3.1 | UK South | Serverless | Budget reasoning |
| deepseek-r1 | UK South | Serverless | Chain-of-thought |
| grok-3 | UK South | Serverless | Alternative reasoning |
| llama-4-maverick | UK South | Serverless | Latest open-source |

---

## Deploying Claude Models

Claude models (Anthropic) are available through the Azure AI Model Catalog but **cannot be deployed via Terraform**. They can be deployed via:

1. **Azure CLI Script** (recommended for automation)
2. **Azure AI Foundry Portal** (for manual/first-time setup)

### Why Claude Can't Be Deployed via Terraform

1. **Marketplace Agreement**: First deployment requires accepting marketplace terms
2. **Billing Setup**: Separate pay-as-you-go billing configuration
3. **Regional Constraints**: Only available in specific regions (UK South, East US 2)
4. **Quota Management**: Separate quota system from OpenAI models

---

### Option 1: Deploy via Azure CLI (Recommended)

Use the provided deployment script for automated Claude model deployment:

```bash
# Deploy all Claude models to dev environment
./infra/scripts/deploy-claude-models.sh dev

# Deploy to production with custom region
AZURE_LOCATION=uksouth ./infra/scripts/deploy-claude-models.sh prod

# Deploy with custom resource names
AZURE_RESOURCE_GROUP=my-rg \
AZURE_AI_SERVICES_NAME=my-ai-services \
./infra/scripts/deploy-claude-models.sh staging
```

#### Script Features

- **Idempotent**: Safe to run multiple times
- **Pre-flight checks**: Validates prerequisites before deployment
- **Multiple deployment methods**: Falls back to alternative APIs if needed
- **Verification**: Lists deployments and provides usage examples

#### Manual CLI Deployment

If you prefer manual control, use `az ml serverless-endpoint create`:

```bash
# Install Azure ML extension
az extension add -n ml --yes

# Set variables
RESOURCE_GROUP="mys-dev-core-rg-san"
AI_SERVICES_NAME="mys-shared-ai-san"
LOCATION="uksouth"

# Deploy Claude Sonnet
az ml serverless-endpoint create \
  --name "claude-sonnet-4-5" \
  --model-id "azureml://registries/azure-openai/models/Anthropic-claude-sonnet-4-5" \
  --resource-group "$RESOURCE_GROUP" \
  --workspace-name "$AI_SERVICES_NAME"

# Deploy Claude Haiku
az ml serverless-endpoint create \
  --name "claude-haiku-4-5" \
  --model-id "azureml://registries/azure-openai/models/Anthropic-claude-3-5-haiku" \
  --resource-group "$RESOURCE_GROUP" \
  --workspace-name "$AI_SERVICES_NAME"

# Deploy Claude Opus
az ml serverless-endpoint create \
  --name "claude-opus-4-5" \
  --model-id "azureml://registries/azure-openai/models/Anthropic-claude-opus-4-5" \
  --resource-group "$RESOURCE_GROUP" \
  --workspace-name "$AI_SERVICES_NAME"
```

#### Alternative: Using az rest (Cognitive Services API)

```bash
# Get subscription ID
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

# Deploy via Cognitive Services deployment API
az rest --method PUT \
  --url "https://management.azure.com/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}/providers/Microsoft.CognitiveServices/accounts/${AI_SERVICES_NAME}/deployments/claude-sonnet-4-5?api-version=2024-10-01" \
  --body '{
    "sku": {
      "name": "Standard",
      "capacity": 1
    },
    "properties": {
      "model": {
        "format": "Anthropic",
        "name": "claude-sonnet-4-5",
        "version": "latest"
      }
    }
  }'
```

---

### Option 2: Deploy via Azure Portal

#### Step 1: Navigate to Azure AI Foundry

```
https://ai.azure.com
```

Or via Azure Portal:
```
Azure Portal → Azure AI services → Your AI Services account → Model catalog
```

#### Step 2: Find Claude Models

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  Azure AI Model Catalog                                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Search: [anthropic claude                    ] [🔍]                        │
│                                                                             │
│  Filter by:  □ OpenAI  ☑ Anthropic  □ Meta  □ Mistral  □ Cohere           │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  claude-opus-4-5                                                     │   │
│  │  Anthropic's most capable model for complex tasks                    │   │
│  │  Context: 200K tokens | Output: 4K tokens                           │   │
│  │  [Deploy]                                                            │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  claude-sonnet-4-5                                                   │   │
│  │  Balanced performance and cost for most tasks                        │   │
│  │  Context: 200K tokens | Output: 4K tokens                           │   │
│  │  [Deploy]                                                            │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │  claude-haiku-4-5                                                    │   │
│  │  Fast and cost-effective for high-volume tasks                       │   │
│  │  Context: 200K tokens | Output: 4K tokens                           │   │
│  │  [Deploy]                                                            │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### Step 3: Configure Deployment

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  Deploy claude-sonnet-4-5                                                    │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Deployment name:    [claude-sonnet-4-5          ]                         │
│                                                                             │
│  Azure AI resource:  [mys-shared-ai-san          ] ▼                       │
│                                                                             │
│  Region:             [UK South                   ] ▼                       │
│                      ⚠️ Model not available in South Africa North           │
│                                                                             │
│  Pricing tier:       ○ Standard (Pay-as-you-go)                            │
│                      ● Provisioned (Reserved capacity)                      │
│                                                                             │
│  ☑ I accept the Anthropic terms of service                                 │
│                                                                             │
│                                      [Cancel]  [Deploy]                     │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### Step 4: Accept Marketplace Terms (First Time Only)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│  Anthropic Claude Terms of Service                                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  By deploying Claude models, you agree to:                                  │
│                                                                             │
│  • Anthropic's Acceptable Use Policy                                        │
│  • Azure Marketplace Terms                                                  │
│  • Pay-as-you-go pricing (separate from Azure OpenAI)                      │
│                                                                             │
│  Pricing:                                                                   │
│  ┌─────────────────────────────────────────────────────────┐               │
│  │ Model           │ Input (per 1M) │ Output (per 1M)     │               │
│  ├─────────────────┼────────────────┼─────────────────────┤               │
│  │ Claude Haiku    │ $0.25          │ $1.25               │               │
│  │ Claude Sonnet   │ $3.00          │ $15.00              │               │
│  │ Claude Opus     │ $15.00         │ $75.00              │               │
│  └─────────────────────────────────────────────────────────┘               │
│                                                                             │
│  ☑ I have read and accept the terms                                        │
│                                                                             │
│                                              [Accept and Continue]          │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### Step 5: Verify Deployment

After deployment, verify via Azure CLI:

```bash
# List all deployments
az cognitiveservices account deployment list \
  --name mys-shared-ai-san \
  --resource-group mys-dev-core-rg-san \
  --output table

# Test Claude endpoint
curl -X POST "https://mys-shared-ai-san.cognitiveservices.azure.com/openai/deployments/claude-sonnet-4-5/chat/completions?api-version=2024-10-01" \
  -H "Content-Type: application/json" \
  -H "api-key: $AZURE_AI_KEY" \
  -d '{
    "messages": [{"role": "user", "content": "Hello Claude!"}],
    "max_tokens": 100
  }'
```

### Using Claude in Code

```csharp
// C# - Using Azure.AI.OpenAI client (works with Claude too)
var client = new AzureOpenAIClient(
    new Uri("https://mys-shared-ai-san.cognitiveservices.azure.com"),
    new AzureKeyCredential(apiKey)
);

var chatClient = client.GetChatClient("claude-sonnet-4-5");

var response = await chatClient.CompleteChatAsync(new[]
{
    new UserChatMessage("Analyze this code for potential issues...")
});
```

```python
# Python - Using openai client with Azure
from openai import AzureOpenAI

client = AzureOpenAI(
    azure_endpoint="https://mys-shared-ai-san.cognitiveservices.azure.com",
    api_key=os.getenv("AZURE_AI_KEY"),
    api_version="2024-10-01"
)

response = client.chat.completions.create(
    model="claude-sonnet-4-5",  # deployment name
    messages=[
        {"role": "user", "content": "Analyze this code for potential issues..."}
    ],
    max_tokens=1000
)
```

### Claude-Specific Features (Claude 4.5)

| Feature | Claude Advantage |
|---------|-----------------|
| **Context Window** | 1M tokens (Sonnet 4.5) vs 400K for GPT-5.2 |
| **Hybrid Reasoning** | Auto/Fast/Thinking modes for flexible reasoning |
| **Constitutional AI** | Built-in safety guardrails |
| **Artifacts** | Can generate interactive components |
| **XML Handling** | Excellent at structured XML output |
| **Long-form Analysis** | Superior at maintaining coherence |
| **SWE-bench** | 77.2% (Sonnet 4.5), 73.3% (Haiku 4.5) |

**Note**: All Claude 4.5 models support hybrid reasoning. Claude 3 Opus is deprecated (Jun 2025) and retiring Jan 2026.

### When to Route to Claude vs GPT vs Others

```python
def select_model(task_type: str, complexity: str, volume: str, context_size: int = 0) -> str:
    """Route requests to optimal model based on task characteristics."""

    # Critical tasks → GPT-5.2 (latest and smartest)
    if complexity == "critical":
        return "gpt-5.2"

    # Very long context (>200K tokens) → Claude Sonnet (1M ctx)
    if context_size > 200000:
        return "claude-sonnet-4-5"

    # High-volume, simple tasks → GPT-4o-mini or Claude Haiku
    if volume == "high" and complexity == "low":
        return "gpt-4o-mini"
    if volume == "high" and complexity == "medium":
        return "claude-haiku-4-5"

    # Chain-of-thought reasoning → o3 or DeepSeek R1
    if task_type in ["reasoning", "planning", "step_by_step"]:
        if complexity == "high":
            return "o3"
        return "o3-mini"  # or "deepseek-r1" for budget

    # Code-related tasks → Claude Sonnet or GPT-5.1-codex
    if task_type in ["code_review", "code_generation", "debugging"]:
        return "claude-sonnet-4-5" if complexity == "high" else "gpt-5.1-codex"

    # Complex analysis → Claude
    if task_type in ["analysis", "research"]:
        if complexity == "extreme":
            return "claude-opus-4-5"
        return "claude-sonnet-4-5"

    # Creative writing → Claude Sonnet (hybrid reasoning)
    if task_type in ["creative", "narrative", "storytelling"]:
        return "claude-sonnet-4-5"

    # Budget reasoning → DeepSeek or Grok
    if task_type in ["budget_analysis", "cost_effective"]:
        return "deepseek-v3.1"  # or "grok-3"

    # Default to GPT-4o for general tasks
    return "gpt-4o"
```

---

## Model Router & Decision Guide

### Complete Model Inventory (32 Models)

*Last updated: December 2025*

| Model | Provider | Category | Region | Primary Use Case |
|-------|----------|----------|--------|------------------|
| gpt-4o | OpenAI | Flagship | SAN | General content generation |
| gpt-4o-mini | OpenAI | Cost-optimized | SAN | High-volume chat |
| gpt-4.1 | OpenAI | Reasoning | SAN | Structured extraction (1M ctx) |
| gpt-4.1-mini | OpenAI | Reasoning | SAN | Lightweight reasoning (1M ctx) |
| gpt-4.1-nano | OpenAI | Reasoning | SAN | Classification, routing (1M ctx) |
| gpt-5-nano | OpenAI | Next-gen | SAN | Advanced reasoning |
| gpt-5.1 | OpenAI | Next-gen | SAN | Complex multi-step |
| gpt-5.1-codex | OpenAI | Code | SAN | Code generation |
| gpt-5.2 | OpenAI | Latest | SAN | Smartest model (400K ctx) |
| o3 | OpenAI | Reasoning | SAN | Advanced chain-of-thought |
| o3-mini | OpenAI | Reasoning | SAN | Chain-of-thought |
| o4-mini | OpenAI | Reasoning | SAN | Fast reasoning |
| text-embedding-3-large | OpenAI | Embedding | SAN | Production RAG |
| text-embedding-3-small | OpenAI | Embedding | SAN | Draft embeddings |
| dall-e-3 | OpenAI | Image | SAN | Story illustrations |
| gpt-image-1 | OpenAI | Image | SAN | Advanced image gen |
| whisper | OpenAI | Audio | SAN | Speech-to-text |
| tts / tts-hd | OpenAI | Audio | SAN | Text-to-speech |
| claude-haiku-4-5 | Anthropic | Fast | UK South | High-volume ($1/$5 per 1M) |
| claude-sonnet-4-5 | Anthropic | Balanced | UK South | Deep analysis (1M ctx) |
| claude-opus-4-5 | Anthropic | Premium | UK South | Complex research |
| cohere-rerank-v3 | Cohere | RAG | UK South | Search reranking |
| cohere-embed-multilingual | Cohere | Embedding | UK South | 100+ languages |
| codestral-2501 | Mistral | Code | UK South | Code (256K context) |
| deepseek-v3.1 | DeepSeek | Reasoning | UK South | Budget reasoning |
| deepseek-r1 | DeepSeek | Reasoning | UK South | Chain-of-thought |
| deepseek-coder-v2 | DeepSeek | Code | UK South | Budget code |
| jamba-1.5-large | AI21 | Long-context | UK South | 256K context |
| jamba-1.5-mini | AI21 | Long-context | UK South | Budget long-context |
| grok-3 | xAI | Reasoning | UK South | Alternative reasoning |
| llama-4-maverick | Meta | Next-gen | UK South | Latest open-source |

---

### Master Decision Tree

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                          MYSTIRA MODEL DECISION TREE                                 │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│                                   START                                             │
│                                     │                                               │
│                    ┌────────────────┴────────────────┐                              │
│                    ▼                                 ▼                              │
│           ┌───────────────┐                 ┌───────────────┐                       │
│           │ What type of  │                 │ Is it a       │                       │
│           │ content?      │                 │ specialized   │                       │
│           └───────┬───────┘                 │ task?         │                       │
│                   │                         └───────┬───────┘                       │
│    ┌──────┬───────┼───────┬──────┐                 │                               │
│    ▼      ▼       ▼       ▼      ▼          ┌──────┴──────┐                        │
│  Text   Code   Image   Audio  Embed         ▼             ▼                        │
│    │      │       │       │      │        Yes            No                        │
│    ▼      ▼       ▼       ▼      ▼          │             │                        │
│ [TEXT]  [CODE] dall-e-3 [AUDIO] [EMBED]     ▼             └──▶ [TEXT FLOW]         │
│                                        ┌─────────┐                                  │
│                                        │ Which?  │                                  │
│                                        └────┬────┘                                  │
│                              ┌──────┬───────┼───────┬──────┐                       │
│                              ▼      ▼       ▼       ▼      ▼                       │
│                          Rerank  Long-ctx  Multi   Reason  Other                   │
│                              │      │      lang      │      │                      │
│                              ▼      ▼       ▼        ▼      ▼                      │
│                          cohere  jamba  cohere    o3-mini  [TEXT]                  │
│                          rerank  1.5    embed                                      │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────┐
│                              [TEXT FLOW] - Text Generation                           │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│                            Is it high volume?                                       │
│                            (>1000 req/day)                                          │
│                                   │                                                 │
│                    ┌──────────────┴──────────────┐                                  │
│                    ▼                             ▼                                  │
│                  YES                            NO                                  │
│                    │                             │                                  │
│                    ▼                             ▼                                  │
│         ┌─────────────────┐           ┌─────────────────┐                          │
│         │ Needs analysis? │           │ What complexity?│                          │
│         └────────┬────────┘           └────────┬────────┘                          │
│            Yes   │   No                        │                                   │
│             │    │                   ┌─────────┼─────────┐                         │
│             ▼    ▼                   ▼         ▼         ▼                         │
│         claude  gpt-4o            Simple   Medium    Complex                       │
│         haiku   -mini               │         │         │                          │
│                                     ▼         ▼         ▼                          │
│                                 gpt-4o     gpt-4o   ┌─────────┐                    │
│                                 -mini               │Creative?│                    │
│                                                     └────┬────┘                    │
│                                                    Yes   │   No                    │
│                                                     │    │                         │
│                                                     ▼    ▼                         │
│                                                 claude  ┌─────────┐                │
│                                                 sonnet  │Critical?│                │
│                                                         └────┬────┘                │
│                                                        Yes   │   No                │
│                                                         │    │                     │
│                                                         ▼    ▼                     │
│                                                      claude  claude                │
│                                                      opus    sonnet                │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────┐
│                              [CODE FLOW] - Code Tasks                                │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│                           What code task?                                           │
│                                 │                                                   │
│              ┌──────────┬───────┼───────┬──────────┐                               │
│              ▼          ▼       ▼       ▼          ▼                               │
│          Generate    Review   Debug   Explain   Complete                           │
│              │          │       │       │          │                               │
│              ▼          ▼       ▼       ▼          ▼                               │
│         ┌────────┐  ┌────────┐  │   gpt-4o    codestral                            │
│         │Complex?│  │Complex?│  │              -2501                               │
│         └───┬────┘  └───┬────┘  │                                                  │
│         Yes │ No    Yes │ No    │                                                  │
│          │  │        │  │       │                                                  │
│          ▼  ▼        ▼  ▼       ▼                                                  │
│       claude gpt   claude gpt   ┌────────┐                                         │
│       sonnet 5.1   sonnet 5.1   │Budget? │                                         │
│              codex       codex  └───┬────┘                                         │
│                              Yes    │   No                                         │
│                               │     │                                              │
│                               ▼     ▼                                              │
│                          deepseek  claude                                          │
│                          -coder    sonnet                                          │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────┐
│                              [AUDIO FLOW] - Audio Tasks                              │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│                           Audio direction?                                          │
│                                 │                                                   │
│                    ┌────────────┴────────────┐                                      │
│                    ▼                         ▼                                      │
│              Speech → Text              Text → Speech                               │
│                    │                         │                                      │
│                    ▼                         ▼                                      │
│                whisper               ┌─────────────┐                               │
│                                      │ Quality?    │                               │
│                                      └──────┬──────┘                               │
│                                      Standard │ High                               │
│                                          │    │                                    │
│                                          ▼    ▼                                    │
│                                        tts   tts-hd                                │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────────────┐
│                           [EMBED FLOW] - Embedding Tasks                             │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│                           What language?                                            │
│                                 │                                                   │
│                    ┌────────────┴────────────┐                                      │
│                    ▼                         ▼                                      │
│               English                   Multilingual                                │
│                    │                    (African, etc.)                             │
│                    │                         │                                      │
│                    ▼                         ▼                                      │
│         ┌─────────────────┐            cohere-embed                                │
│         │ What purpose?   │            -multilingual                               │
│         └────────┬────────┘                                                        │
│         Production │ Draft/Test                                                    │
│              │     │                                                               │
│              ▼     ▼                                                               │
│          embed-3  embed-3                                                          │
│          -large   -small                                                           │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

---

### Model Router Implementation

```python
"""
Mystira Model Router - Intelligent model selection based on task characteristics.

This module provides automatic routing to the optimal AI model based on:
- Task type (text, code, image, audio, embedding)
- Complexity level (simple, medium, complex, critical)
- Volume requirements (standard, high)
- Special requirements (long-context, multilingual, reasoning)
"""

from enum import Enum
from dataclasses import dataclass
from typing import Optional, List
import logging

logger = logging.getLogger(__name__)


class TaskType(Enum):
    # Text Generation
    CHAT = "chat"
    CONTENT_GENERATION = "content_generation"
    SUMMARIZATION = "summarization"
    TRANSLATION = "translation"
    CLASSIFICATION = "classification"

    # Analysis
    ANALYSIS = "analysis"
    RESEARCH = "research"
    REASONING = "reasoning"

    # Creative
    CREATIVE_WRITING = "creative_writing"
    STORYTELLING = "storytelling"
    NARRATIVE = "narrative"

    # Code
    CODE_GENERATION = "code_generation"
    CODE_REVIEW = "code_review"
    CODE_COMPLETION = "code_completion"
    DEBUGGING = "debugging"
    CODE_EXPLANATION = "code_explanation"

    # Specialized
    DATA_EXTRACTION = "data_extraction"
    STRUCTURED_OUTPUT = "structured_output"

    # Multimodal
    IMAGE_GENERATION = "image_generation"
    SPEECH_TO_TEXT = "speech_to_text"
    TEXT_TO_SPEECH = "text_to_speech"

    # RAG
    EMBEDDING = "embedding"
    RERANKING = "reranking"


class Complexity(Enum):
    SIMPLE = "simple"      # Basic tasks, no reasoning needed
    MEDIUM = "medium"      # Standard tasks, some reasoning
    COMPLEX = "complex"    # Multi-step, requires analysis
    CRITICAL = "critical"  # Mission-critical, highest quality


class Volume(Enum):
    STANDARD = "standard"  # Normal usage
    HIGH = "high"          # >1000 requests/day


@dataclass
class ModelConfig:
    """Configuration for a deployed model."""
    name: str
    provider: str
    region: str
    input_cost_per_1m: float
    output_cost_per_1m: float
    context_window: int
    strengths: List[str]


@dataclass
class RoutingRequest:
    """Request for model routing."""
    task_type: TaskType
    complexity: Complexity = Complexity.MEDIUM
    volume: Volume = Volume.STANDARD

    # Special requirements
    requires_long_context: bool = False
    context_length: int = 0
    requires_multilingual: bool = False
    language: str = "en"
    requires_reasoning: bool = False
    budget_constrained: bool = False

    # Quality preferences
    prefer_speed: bool = False
    prefer_quality: bool = False


class ModelRouter:
    """
    Intelligent model router for Mystira.

    Routes requests to optimal models based on task characteristics,
    cost constraints, and quality requirements.
    """

    # Model definitions with costs and capabilities
    MODELS = {
        # OpenAI - Cost-Optimized
        "gpt-4o-mini": ModelConfig(
            name="gpt-4o-mini",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=0.15,
            output_cost_per_1m=0.60,
            context_window=128000,
            strengths=["high-volume", "chat", "summarization"]
        ),
        "gpt-4.1-nano": ModelConfig(
            name="gpt-4.1-nano",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=0.10,
            output_cost_per_1m=0.40,
            context_window=128000,
            strengths=["classification", "routing", "simple-tasks"]
        ),

        # OpenAI - General Purpose
        "gpt-4o": ModelConfig(
            name="gpt-4o",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=2.50,
            output_cost_per_1m=10.00,
            context_window=128000,
            strengths=["content", "translation", "general"]
        ),
        "gpt-4.1": ModelConfig(
            name="gpt-4.1",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=2.00,
            output_cost_per_1m=8.00,
            context_window=128000,
            strengths=["structured-output", "data-extraction"]
        ),

        # OpenAI - Premium
        "gpt-5.1": ModelConfig(
            name="gpt-5.1",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=5.00,
            output_cost_per_1m=15.00,
            context_window=128000,
            strengths=["complex-tasks", "multi-step"]
        ),
        "gpt-5.1-codex": ModelConfig(
            name="gpt-5.1-codex",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=5.00,
            output_cost_per_1m=15.00,
            context_window=128000,
            strengths=["code-generation", "code-review"]
        ),

        # OpenAI - Reasoning
        "o3-mini": ModelConfig(
            name="o3-mini",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=1.10,
            output_cost_per_1m=4.40,
            context_window=128000,
            strengths=["reasoning", "chain-of-thought", "planning"]
        ),

        # Anthropic
        "claude-haiku-4-5": ModelConfig(
            name="claude-haiku-4-5",
            provider="Anthropic",
            region="UK South",
            input_cost_per_1m=0.25,
            output_cost_per_1m=1.25,
            context_window=200000,
            strengths=["high-volume", "fast-analysis"]
        ),
        "claude-sonnet-4-5": ModelConfig(
            name="claude-sonnet-4-5",
            provider="Anthropic",
            region="UK South",
            input_cost_per_1m=3.00,
            output_cost_per_1m=15.00,
            context_window=200000,
            strengths=["analysis", "creative", "code-review", "reasoning"]
        ),
        "claude-opus-4-5": ModelConfig(
            name="claude-opus-4-5",
            provider="Anthropic",
            region="UK South",
            input_cost_per_1m=15.00,
            output_cost_per_1m=75.00,
            context_window=200000,
            strengths=["critical-tasks", "research", "complex-analysis"]
        ),

        # Specialized - Code
        "codestral-2501": ModelConfig(
            name="codestral-2501",
            provider="Mistral",
            region="UK South",
            input_cost_per_1m=0.30,
            output_cost_per_1m=0.90,
            context_window=256000,
            strengths=["code-completion", "long-context-code"]
        ),
        "deepseek-coder-v2": ModelConfig(
            name="deepseek-coder-v2",
            provider="DeepSeek",
            region="UK South",
            input_cost_per_1m=0.14,
            output_cost_per_1m=0.28,
            context_window=128000,
            strengths=["budget-code", "code-generation"]
        ),

        # Specialized - Long Context
        "jamba-1.5-large": ModelConfig(
            name="jamba-1.5-large",
            provider="AI21",
            region="UK South",
            input_cost_per_1m=2.00,
            output_cost_per_1m=8.00,
            context_window=256000,
            strengths=["long-documents", "manuscript-analysis"]
        ),
        "jamba-1.5-mini": ModelConfig(
            name="jamba-1.5-mini",
            provider="AI21",
            region="UK South",
            input_cost_per_1m=0.20,
            output_cost_per_1m=0.40,
            context_window=256000,
            strengths=["budget-long-context"]
        ),

        # RAG
        "cohere-rerank-v3": ModelConfig(
            name="cohere-rerank-v3",
            provider="Cohere",
            region="UK South",
            input_cost_per_1m=2.00,  # per 1K queries
            output_cost_per_1m=0,
            context_window=4096,
            strengths=["reranking", "search-quality"]
        ),
        "cohere-embed-multilingual": ModelConfig(
            name="cohere-embed-multilingual",
            provider="Cohere",
            region="UK South",
            input_cost_per_1m=0.10,
            output_cost_per_1m=0,
            context_window=512,
            strengths=["multilingual", "african-languages"]
        ),

        # Audio
        "whisper": ModelConfig(
            name="whisper",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=0.006,  # per minute
            output_cost_per_1m=0,
            context_window=0,
            strengths=["speech-to-text"]
        ),
        "tts": ModelConfig(
            name="tts",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=15.00,  # per 1M chars
            output_cost_per_1m=0,
            context_window=0,
            strengths=["text-to-speech"]
        ),
        "tts-hd": ModelConfig(
            name="tts-hd",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=30.00,  # per 1M chars
            output_cost_per_1m=0,
            context_window=0,
            strengths=["high-quality-tts"]
        ),

        # Image
        "dall-e-3": ModelConfig(
            name="dall-e-3",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=0.04,  # per image
            output_cost_per_1m=0,
            context_window=0,
            strengths=["image-generation"]
        ),

        # Embeddings
        "text-embedding-3-large": ModelConfig(
            name="text-embedding-3-large",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=0.13,
            output_cost_per_1m=0,
            context_window=8191,
            strengths=["production-embeddings", "high-accuracy"]
        ),
        "text-embedding-3-small": ModelConfig(
            name="text-embedding-3-small",
            provider="OpenAI",
            region="SAN",
            input_cost_per_1m=0.02,
            output_cost_per_1m=0,
            context_window=8191,
            strengths=["budget-embeddings", "draft"]
        ),
    }

    # Fallback chains for resilience
    FALLBACK_CHAINS = {
        "gpt-4o": ["claude-sonnet-4-5", "gpt-4o-mini"],
        "gpt-5.1-codex": ["claude-sonnet-4-5", "codestral-2501", "gpt-4.1"],
        "claude-opus-4-5": ["claude-sonnet-4-5", "gpt-5.1"],
        "claude-sonnet-4-5": ["gpt-4o", "claude-haiku-4-5"],
        "o3-mini": ["claude-sonnet-4-5", "gpt-4.1"],
        "jamba-1.5-large": ["jamba-1.5-mini", "claude-sonnet-4-5"],
    }

    def __init__(self):
        self.usage_stats = {}

    def route(self, request: RoutingRequest) -> str:
        """
        Route a request to the optimal model.

        Args:
            request: RoutingRequest with task characteristics

        Returns:
            Model name string
        """

        # Handle specialized task types first
        if request.task_type == TaskType.IMAGE_GENERATION:
            return "dall-e-3"

        if request.task_type == TaskType.SPEECH_TO_TEXT:
            return "whisper"

        if request.task_type == TaskType.TEXT_TO_SPEECH:
            return "tts-hd" if request.prefer_quality else "tts"

        if request.task_type == TaskType.RERANKING:
            return "cohere-rerank-v3"

        if request.task_type == TaskType.EMBEDDING:
            return self._route_embedding(request)

        # Handle long context requirements
        if request.requires_long_context or request.context_length > 128000:
            return self._route_long_context(request)

        # Handle code tasks
        if request.task_type in [
            TaskType.CODE_GENERATION, TaskType.CODE_REVIEW,
            TaskType.CODE_COMPLETION, TaskType.DEBUGGING,
            TaskType.CODE_EXPLANATION
        ]:
            return self._route_code(request)

        # Handle reasoning tasks
        if request.requires_reasoning or request.task_type == TaskType.REASONING:
            return self._route_reasoning(request)

        # Handle creative tasks
        if request.task_type in [
            TaskType.CREATIVE_WRITING, TaskType.STORYTELLING,
            TaskType.NARRATIVE
        ]:
            return self._route_creative(request)

        # Handle analysis tasks
        if request.task_type in [TaskType.ANALYSIS, TaskType.RESEARCH]:
            return self._route_analysis(request)

        # Handle general text tasks
        return self._route_general_text(request)

    def _route_embedding(self, request: RoutingRequest) -> str:
        """Route embedding requests."""
        if request.requires_multilingual:
            return "cohere-embed-multilingual"
        if request.budget_constrained or request.prefer_speed:
            return "text-embedding-3-small"
        return "text-embedding-3-large"

    def _route_long_context(self, request: RoutingRequest) -> str:
        """Route requests requiring >128K context."""
        if request.budget_constrained:
            return "jamba-1.5-mini"

        # Code with long context
        if request.task_type in [TaskType.CODE_GENERATION, TaskType.CODE_REVIEW]:
            return "codestral-2501"

        # Premium long-context
        return "jamba-1.5-large"

    def _route_code(self, request: RoutingRequest) -> str:
        """Route code-related tasks."""

        # Simple code completion
        if request.task_type == TaskType.CODE_COMPLETION:
            return "codestral-2501"

        # Code explanation
        if request.task_type == TaskType.CODE_EXPLANATION:
            return "gpt-4o"

        # Budget code tasks
        if request.budget_constrained:
            return "deepseek-coder-v2"

        # Complex code review/debugging
        if request.complexity in [Complexity.COMPLEX, Complexity.CRITICAL]:
            return "claude-sonnet-4-5"

        # Standard code generation
        return "gpt-5.1-codex"

    def _route_reasoning(self, request: RoutingRequest) -> str:
        """Route reasoning-heavy tasks."""

        # Critical reasoning
        if request.complexity == Complexity.CRITICAL:
            return "claude-opus-4-5"

        # Chain-of-thought reasoning
        if request.prefer_quality:
            return "o3-mini"

        # Standard reasoning
        return "claude-sonnet-4-5"

    def _route_creative(self, request: RoutingRequest) -> str:
        """Route creative writing tasks."""

        # High-volume creative (e.g., story variations)
        if request.volume == Volume.HIGH:
            return "claude-haiku-4-5"

        # Claude is generally better for creative
        return "claude-sonnet-4-5"

    def _route_analysis(self, request: RoutingRequest) -> str:
        """Route analysis and research tasks."""

        # Critical analysis
        if request.complexity == Complexity.CRITICAL:
            return "claude-opus-4-5"

        # High-volume analysis
        if request.volume == Volume.HIGH:
            return "claude-haiku-4-5"

        # Standard analysis
        return "claude-sonnet-4-5"

    def _route_general_text(self, request: RoutingRequest) -> str:
        """Route general text generation tasks."""

        # High-volume simple tasks
        if request.volume == Volume.HIGH:
            if request.complexity == Complexity.SIMPLE:
                return "gpt-4.1-nano"
            return "gpt-4o-mini"

        # Classification/routing
        if request.task_type == TaskType.CLASSIFICATION:
            return "gpt-4.1-nano"

        # Structured output
        if request.task_type in [TaskType.DATA_EXTRACTION, TaskType.STRUCTURED_OUTPUT]:
            return "gpt-4.1"

        # Complex tasks
        if request.complexity == Complexity.COMPLEX:
            return "gpt-5.1"

        # Critical tasks
        if request.complexity == Complexity.CRITICAL:
            return "claude-sonnet-4-5"

        # Default general purpose
        return "gpt-4o"

    def get_fallback(self, model: str) -> Optional[str]:
        """Get fallback model if primary is unavailable."""
        chain = self.FALLBACK_CHAINS.get(model, [])
        return chain[0] if chain else None

    def get_cost_estimate(self, model: str, input_tokens: int, output_tokens: int) -> float:
        """Estimate cost for a request."""
        config = self.MODELS.get(model)
        if not config:
            return 0.0

        input_cost = (input_tokens / 1_000_000) * config.input_cost_per_1m
        output_cost = (output_tokens / 1_000_000) * config.output_cost_per_1m
        return input_cost + output_cost


# Convenience function for simple routing
def select_model(
    task_type: str,
    complexity: str = "medium",
    volume: str = "standard",
    **kwargs
) -> str:
    """
    Simple model selection function.

    Args:
        task_type: Type of task (e.g., "chat", "code_review", "analysis")
        complexity: "simple", "medium", "complex", or "critical"
        volume: "standard" or "high"
        **kwargs: Additional options (requires_long_context, budget_constrained, etc.)

    Returns:
        Optimal model name

    Example:
        >>> select_model("code_review", complexity="complex")
        'claude-sonnet-4-5'

        >>> select_model("chat", volume="high")
        'gpt-4o-mini'
    """
    router = ModelRouter()

    request = RoutingRequest(
        task_type=TaskType(task_type),
        complexity=Complexity(complexity),
        volume=Volume(volume),
        requires_long_context=kwargs.get("requires_long_context", False),
        context_length=kwargs.get("context_length", 0),
        requires_multilingual=kwargs.get("requires_multilingual", False),
        requires_reasoning=kwargs.get("requires_reasoning", False),
        budget_constrained=kwargs.get("budget_constrained", False),
        prefer_speed=kwargs.get("prefer_speed", False),
        prefer_quality=kwargs.get("prefer_quality", False),
    )

    return router.route(request)
```

---

### Quick Reference Card

*Last updated: December 2025*

```
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                          MODEL QUICK REFERENCE                                       │
├─────────────────────────────────────────────────────────────────────────────────────┤
│                                                                                     │
│  COST TIERS (per 1M tokens):                                                        │
│  ─────────────────────────────                                                      │
│  💚 Budget     ($0.10-$0.60)  gpt-4.1-nano, deepseek-v3.1, deepseek-r1, jamba-mini │
│  💛 Standard   ($0.15-$2.00)  gpt-4o-mini, claude-haiku, gpt-4.1, o3-mini, o4-mini │
│  🟠 Premium    ($2.50-$7.50)  gpt-4o, gpt-5.1, gpt-5.2, claude-sonnet, grok-3     │
│  🔴 Maximum    ($15.00+)      claude-opus, o3                                       │
│                                                                                     │
│  TASK SHORTCUTS:                                                                    │
│  ─────────────────────────────                                                      │
│  Chat (high vol)     → gpt-4o-mini                                                 │
│  Chat (quality)      → gpt-4o                                                      │
│  Code generation     → gpt-5.1-codex or codestral-2501                             │
│  Code review         → claude-sonnet-4-5                                           │
│  Analysis            → claude-sonnet-4-5                                           │
│  Critical tasks      → gpt-5.2 (latest) or claude-opus-4-5                         │
│  Reasoning (adv)     → o3                                                          │
│  Reasoning (fast)    → o3-mini or o4-mini                                          │
│  Chain-of-thought    → deepseek-r1 (budget) or o3-mini                             │
│  Creative writing    → claude-sonnet-4-5 (hybrid reasoning)                        │
│  Classification      → gpt-4.1-nano                                                │
│  Data extraction     → gpt-4.1 (1M context)                                        │
│  Long docs (256K)    → jamba-1.5-large                                             │
│  Long docs (1M)      → claude-sonnet-4-5 or gpt-4.1                                │
│  Multilingual embed  → cohere-embed-multilingual                                   │
│  Search reranking    → cohere-rerank-v3                                            │
│  Budget reasoning    → deepseek-v3.1 or grok-3                                     │
│                                                                                     │
│  FALLBACK CHAINS:                                                                   │
│  ─────────────────────────────                                                      │
│  gpt-5.2        → gpt-5.1 → gpt-4o                                                 │
│  gpt-4o         → claude-sonnet → gpt-4o-mini                                      │
│  claude-opus    → o3 → claude-sonnet                                               │
│  gpt-5.1-codex  → claude-sonnet → codestral → gpt-4.1                             │
│  o3             → o3-mini → claude-sonnet                                          │
│  deepseek-r1    → o3-mini → gpt-4.1                                                │
│                                                                                     │
└─────────────────────────────────────────────────────────────────────────────────────┘
```

---

### Usage Examples

```python
# Example 1: Simple chat
model = select_model("chat", volume="high")
# Returns: "gpt-4o-mini"

# Example 2: Complex code review
model = select_model("code_review", complexity="complex")
# Returns: "claude-sonnet-4-5"

# Example 3: Long document analysis
model = select_model(
    "analysis",
    complexity="complex",
    requires_long_context=True,
    context_length=200000
)
# Returns: "jamba-1.5-large"

# Example 4: Budget code generation
model = select_model("code_generation", budget_constrained=True)
# Returns: "deepseek-coder-v2"

# Example 5: Critical research
model = select_model("research", complexity="critical")
# Returns: "claude-opus-4-5"

# Example 6: Chain-of-thought reasoning
model = select_model("reasoning", requires_reasoning=True, prefer_quality=True)
# Returns: "o3-mini"

# Example 7: Multilingual embedding
model = select_model("embedding", requires_multilingual=True)
# Returns: "cohere-embed-multilingual"
```

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

## Troubleshooting

### Common Issues

#### 1. Model Deployment Failed: "Model not available in region"

**Problem**: Catalog models (Cohere, Mistral, DeepSeek, AI21) are not available in South Africa North.

**Solution**: These models are configured with `location = "uksouth"` and deployed to a secondary AI Services account. Check:

```bash
# Verify UK South account exists
az cognitiveservices account show \
  --name mys-shared-ai-uks \
  --resource-group <rg-name>

# List deployments on UK South
az cognitiveservices account deployment list \
  --name mys-shared-ai-uks \
  --resource-group <rg-name> \
  --output table
```

#### 2. Quota Exceeded: "InsufficientQuota"

**Problem**: Model deployment fails due to token quota limits.

**Solutions**:
1. Reduce `capacity` in the model configuration
2. Request quota increase at: https://aka.ms/oai/quotaincrease
3. Use `GlobalStandard` SKU for automatic global routing

```bash
# Check current quota usage
az cognitiveservices account list-usages \
  --name mys-shared-ai-san \
  --resource-group <rg-name> \
  --output table
```

#### 3. Marketplace Terms Not Accepted

**Problem**: Catalog models fail with "MarketplaceTermsNotAccepted" or similar.

**Solution**:
1. Visit Azure AI Foundry portal: https://ai.azure.com/explore/models
2. Search for the model (e.g., "Claude")
3. Click "Deploy" to trigger terms acceptance
4. Accept the marketplace agreement

Alternatively, use the deployment script:
```bash
./infra/scripts/deploy-claude-models.sh dev
```

#### 4. API Version Mismatch

**Problem**: AzAPI resource fails with "InvalidApiVersion".

**Solution**: Ensure correct API versions:
- Cognitive Services account: `2025-06-01` (for `allowProjectManagement`)
- Model deployments: `2024-10-01`
- AI Projects: `2025-06-01`

#### 5. Claude Models Not Working via OpenAI SDK

**Problem**: Claude models return errors when called via OpenAI SDK.

**Solution**: Claude models require the Anthropic SDK or direct REST calls:

```python
# WRONG: OpenAI SDK (doesn't work for Claude)
from openai import AzureOpenAI
client = AzureOpenAI(...)
client.chat.completions.create(model="claude-sonnet-4-5", ...)  # Fails

# CORRECT: Use direct REST API
import requests
response = requests.post(
    f"{endpoint}/openai/deployments/claude-sonnet-4-5/chat/completions?api-version=2024-10-01",
    headers={"api-key": api_key, "Content-Type": "application/json"},
    json={"messages": [...], "max_tokens": 1000}
)
```

#### 6. Embedding Dimension Mismatch

**Problem**: Vector search fails with dimension mismatch errors.

**Solution**: Ensure consistent embedding dimensions:
- `text-embedding-3-large`: 3072 dimensions (default)
- `text-embedding-3-small`: 1536 dimensions (default)

Both can be configured with custom dimensions using the `dimensions` parameter:
```python
response = client.embeddings.create(
    model="text-embedding-3-large",
    input="text to embed",
    dimensions=1536  # Reduce from 3072 to 1536
)
```

#### 7. Terraform State Drift

**Problem**: Terraform shows unexpected changes or fails to apply.

**Solution**:
1. Check for manual changes in Azure portal
2. Import existing resources if needed
3. Use `terraform refresh` to update state

```bash
# Refresh state
terraform refresh

# Import existing resource
terraform import module.shared_azure_ai.azurerm_cognitive_account.ai_foundry /subscriptions/.../resourceGroups/.../providers/Microsoft.CognitiveServices/accounts/mys-shared-ai-san
```

### Deployment Verification

Run these commands to verify your deployment:

```bash
# 1. Check AI Services accounts
az cognitiveservices account list \
  --resource-group <rg-name> \
  --output table

# 2. List all model deployments (primary region)
az cognitiveservices account deployment list \
  --name mys-shared-ai-san \
  --resource-group <rg-name> \
  --output table

# 3. List all model deployments (UK South)
az cognitiveservices account deployment list \
  --name mys-shared-ai-uks \
  --resource-group <rg-name> \
  --output table

# 4. Test endpoint connectivity
curl -s -o /dev/null -w "%{http_code}" \
  "https://mys-shared-ai-san.cognitiveservices.azure.com/openai/deployments?api-version=2024-10-01" \
  -H "api-key: $API_KEY"

# 5. Verify model availability
az cognitiveservices model list \
  --location southafricanorth \
  --query "[?kind=='OpenAI'].{name:name,version:version}" \
  --output table
```

### Getting Help

- **Azure Support**: https://azure.microsoft.com/support/
- **OpenAI on Azure**: https://learn.microsoft.com/en-us/azure/ai-services/openai/
- **Mystira Docs**: See ADR-0020 and ADR-0021 for model strategy

---

## References

- [Azure AI Foundry Documentation](https://learn.microsoft.com/en-us/azure/ai-services/)
- [Azure AI Search Vector Search](https://learn.microsoft.com/en-us/azure/search/vector-search-overview)
- [OpenAI Embeddings Guide](https://platform.openai.com/docs/guides/embeddings)
- [RAG Pattern Best Practices](https://learn.microsoft.com/en-us/azure/architecture/ai-ml/guide/rag-solution-design)
- [ADR-0020: AI Model Selection Strategy](../adr/ADR-0020-ai-model-selection-strategy.md)
- [ADR-0021: Specialized & Edge Case Models](../adr/ADR-0021-specialized-edge-case-models.md)
