# ADR-0021: Specialized & Edge Case Models

**Status**: Proposed
**Date**: 2024-12-24
**Decision Makers**: Platform Team, AI/ML Team
**Related**: [ADR-0020: AI Model Selection Strategy](./ADR-0020-ai-model-selection-strategy.md)

## Context

ADR-0020 established Mystira's core model strategy with 18 models covering primary use cases. This ADR addresses specialized models for edge cases, alternative providers, and emerging capabilities not covered by the core deployment.

These models are categorized as:
1. **RAG Enhancement** - Models specifically designed for search and retrieval
2. **Code Specialization** - Dedicated code models beyond GPT-5.1-codex
3. **Long Context** - Models for processing documents >128K tokens
4. **Vision** - Multimodal models for image understanding
5. **Open Source** - Provider-agnostic alternatives (Meta Llama, etc.)
6. **Emerging Providers** - DeepSeek, AI21, Mistral for cost or capability advantages

---

## Proposed Models

### 1. RAG Enhancement Models

#### Cohere Rerank v3.5

**Purpose**: Improve search relevance in RAG pipelines by reranking initial retrieval results.

| Property | Value |
|----------|-------|
| Provider | Cohere |
| Model ID | `rerank-v3.5` |
| SKU | GlobalStandard (Serverless) |
| Pricing | $2.00 per 1K search queries |
| Availability | UK South, East US, West US |

**Why It's Needed**:
- Azure AI Search returns top-K results by vector similarity
- Reranking uses cross-encoder to semantically rank results
- Typically improves relevance by 10-30% for complex queries
- Essential for production RAG quality

**Integration Point**:
```python
# After initial vector search
search_results = ai_search.search(query, top=50)

# Rerank with Cohere
reranked = cohere_client.rerank(
    query=query,
    documents=[r.content for r in search_results],
    top_n=10,
    model="rerank-v3.5"
)
```

**Terraform Configuration**:
```hcl
"cohere-rerank-v3" = {
  model_name    = "rerank-v3.5"
  model_version = "1"
  model_format  = "Cohere"
  sku_name      = "GlobalStandard"
  capacity      = 1
  location      = "uksouth"  # Not available in SAN
}
```

#### Cohere Embed v3 Multilingual

**Purpose**: Embed non-English content for multilingual RAG.

| Property | Value |
|----------|-------|
| Provider | Cohere |
| Model ID | `embed-multilingual-v3.0` |
| SKU | GlobalStandard (Serverless) |
| Pricing | $0.10 per 1M tokens |
| Languages | 100+ languages |

**Why Consider**:
- OpenAI embeddings are English-optimized
- African language support for South African market
- Supports Afrikaans, Zulu, Xhosa, and other regional languages

**Use Case**: Mystira stories in multiple South African languages.

---

### 2. Code Specialization Models

#### Codestral-2501 (Mistral)

**Purpose**: Dedicated code model optimized for code generation, completion, and understanding.

| Property | Value |
|----------|-------|
| Provider | Mistral AI |
| Model ID | `Codestral-2501` |
| Context | 256K tokens |
| SKU | GlobalStandard (Serverless) |
| Pricing | $0.30 input / $0.90 output (per 1M tokens) |

**Why Consider**:
- 80+ programming languages
- Fill-in-the-middle (FIM) support
- Much cheaper than gpt-5.1-codex
- Larger context (256K vs 128K)

**Use Cases**:
- Code completion for Mystira.Publisher
- Documentation generation
- Code review assistance
- Bulk code analysis

**Terraform Configuration**:
```hcl
"codestral-2501" = {
  model_name    = "Codestral-2501"
  model_version = "2501"
  model_format  = "Mistral"
  sku_name      = "GlobalStandard"
  capacity      = 1
  location      = "uksouth"
}
```

#### DeepSeek-Coder-V2

**Purpose**: Alternative code model with strong benchmarks.

| Property | Value |
|----------|-------|
| Provider | DeepSeek |
| Model ID | `DeepSeek-Coder-V2-236B` |
| Context | 128K tokens |
| Pricing | ~$0.14 input / $0.28 output (per 1M tokens) |

**Why Consider**:
- Near GPT-4 code performance
- Significantly cheaper
- Open weights (for local deployment if needed)

---

### 3. Long Context Models

#### Jamba-1.5-Large (AI21)

**Purpose**: Process extremely long documents (up to 256K tokens).

| Property | Value |
|----------|-------|
| Provider | AI21 Labs |
| Model ID | `jamba-1.5-large` |
| Context | 256K tokens |
| SKU | GlobalStandard (Serverless) |
| Pricing | $2.00 input / $8.00 output (per 1M tokens) |

**Why Consider**:
- Novel Mamba architecture (linear scaling with context)
- Process entire books in single call
- Efficient attention for long-form content
- Good for story continuity analysis

**Use Cases**:
- Full story manuscript analysis
- Cross-chapter consistency checking
- Long document summarization
- Book-length content processing

**Terraform Configuration**:
```hcl
"jamba-1.5-large" = {
  model_name    = "jamba-1.5-large"
  model_version = "1"
  model_format  = "AI21"
  sku_name      = "GlobalStandard"
  capacity      = 1
  location      = "uksouth"
}
```

#### Jamba-1.5-Mini

**Purpose**: Cost-effective long context for simpler tasks.

| Property | Value |
|----------|-------|
| Provider | AI21 Labs |
| Model ID | `jamba-1.5-mini` |
| Context | 256K tokens |
| Pricing | $0.20 input / $0.40 output (per 1M tokens) |

**Why Consider**:
- Same 256K context as Large
- 10x cheaper for simpler long-context tasks
- Good for document chunking decisions

---

### 4. Vision Models

#### Llama-3.2-90B-Vision-Instruct

**Purpose**: Understand and analyze images alongside text.

| Property | Value |
|----------|-------|
| Provider | Meta |
| Model ID | `Llama-3.2-90B-Vision-Instruct` |
| Context | 128K tokens |
| SKU | GlobalStandard (Serverless) |
| Pricing | Pay-per-token (serverless) |

**Why Consider**:
- Image understanding without OpenAI dependency
- Open-source model
- Good for analyzing story illustrations
- Can describe images for accessibility

**Use Cases**:
- Analyze DALL-E generated images
- Extract text from story artwork
- Generate alt-text for accessibility
- Visual content moderation

**Terraform Configuration**:
```hcl
"llama-3.2-90b-vision" = {
  model_name    = "Llama-3.2-90B-Vision-Instruct"
  model_version = "1"
  model_format  = "Meta"
  sku_name      = "GlobalStandard"
  capacity      = 1
  location      = "uksouth"
}
```

#### Llama-3.2-11B-Vision-Instruct

**Purpose**: Efficient vision model for high-volume image tasks.

| Property | Value |
|----------|-------|
| Provider | Meta |
| Model ID | `Llama-3.2-11B-Vision-Instruct` |
| Context | 128K tokens |
| Pricing | Lower than 90B variant |

**Why Consider**:
- 8x smaller than 90B version
- Faster inference
- Cost-effective for bulk image processing

#### Pixtral-Large-2411 (Mistral)

**Purpose**: Alternative multimodal model with strong vision capabilities.

| Property | Value |
|----------|-------|
| Provider | Mistral AI |
| Model ID | `Pixtral-Large-2411` |
| Context | 128K tokens |
| Pricing | ~$2.00 input / $6.00 output (per 1M tokens) |

**Why Consider**:
- Strong image understanding
- Good for document analysis with images
- Alternative to Llama vision models

---

### 5. Open Source / Alternative Providers

#### Llama-3.3-70B-Instruct

**Purpose**: High-quality open-source LLM for general tasks.

| Property | Value |
|----------|-------|
| Provider | Meta |
| Model ID | `Llama-3.3-70B-Instruct` |
| Context | 128K tokens |
| SKU | GlobalStandard (Serverless) |
| Pricing | Pay-per-token |

**Why Consider**:
- No per-token markup (just compute cost)
- Strong reasoning and instruction following
- Provider-agnostic (can run locally if needed)
- Fallback when OpenAI is unavailable

**Use Cases**:
- General-purpose fallback
- Batch processing where cost matters
- Tasks where model provider doesn't matter

**Terraform Configuration**:
```hcl
"llama-3.3-70b" = {
  model_name    = "Llama-3.3-70B-Instruct"
  model_version = "1"
  model_format  = "Meta"
  sku_name      = "GlobalStandard"
  capacity      = 1
  location      = "uksouth"
}
```

#### Llama-3.1-8B-Instruct

**Purpose**: Ultra-efficient model for simple tasks.

| Property | Value |
|----------|-------|
| Provider | Meta |
| Model ID | `Llama-3.1-8B-Instruct` |
| Context | 128K tokens |
| Pricing | Very low (smallest model) |

**Why Consider**:
- Fastest inference
- Lowest cost
- Good for classification, extraction, routing

#### Mistral-Large-2411

**Purpose**: Strong reasoning model from Mistral.

| Property | Value |
|----------|-------|
| Provider | Mistral AI |
| Model ID | `Mistral-Large-2411` |
| Context | 128K tokens |
| Pricing | ~$2.00 input / $6.00 output (per 1M tokens) |

**Why Consider**:
- Strong multilingual support
- Good reasoning capabilities
- European provider (data residency considerations)

---

### 6. Reasoning & Research Models

#### DeepSeek-V3

**Purpose**: Advanced reasoning model with strong benchmarks.

| Property | Value |
|----------|-------|
| Provider | DeepSeek |
| Model ID | `DeepSeek-V3` |
| Context | 64K tokens |
| Pricing | ~$0.27 input / $1.10 output (per 1M tokens) |

**Why Consider**:
- Near Claude/GPT reasoning at fraction of cost
- Strong on math and logic
- Good for complex analysis tasks

#### DeepSeek-R1-Distill-Llama-70B

**Purpose**: Reasoning-optimized model distilled from R1.

| Property | Value |
|----------|-------|
| Provider | DeepSeek |
| Model ID | `DeepSeek-R1-Distill-Llama-70B` |
| Context | 64K tokens |
| Pricing | Very low |

**Why Consider**:
- Chain-of-thought reasoning
- Cheaper alternative to o3-mini
- Good for step-by-step analysis

---

## Prioritization Matrix

| Model | Priority | Phase | Use Case | Monthly Est. |
|-------|----------|-------|----------|--------------|
| Cohere Rerank v3.5 | **High** | Phase 1 | RAG quality | $100 |
| Codestral-2501 | Medium | Phase 2 | Code tasks | $50 |
| Jamba-1.5-Large | Medium | Phase 2 | Long docs | $100 |
| Llama-3.2-90B-Vision | Low | Phase 3 | Image analysis | $50 |
| Llama-3.3-70B | Low | Phase 3 | Fallback | $50 |
| DeepSeek-V3 | Low | Phase 3 | Cost savings | $30 |
| Cohere Embed Multilingual | Low | Future | i18n | TBD |

---

## Decision

### Immediate (Phase 1)

Add **Cohere Rerank v3.5** to the Terraform configuration:
- Critical for RAG search quality
- Low cost ($2/1K queries)
- Significant relevance improvement

### Near-term (Phase 2)

Consider adding:
1. **Codestral-2501** - If code generation costs become significant
2. **Jamba-1.5-Large** - If full-story processing is needed

### Future (Phase 3)

Evaluate based on usage patterns:
- Vision models if image analysis demand grows
- Llama models as cost-saving alternatives
- DeepSeek for budget-conscious reasoning

---

## Implementation Notes

### Serverless Model Deployment

Most specialized models use serverless (pay-per-use) deployment:

```bash
# Deploy via Azure ML CLI
az ml serverless-endpoint create \
  --resource-group mystira-shared-san-rg \
  --workspace-name mystira-ai-foundry-san \
  --name cohere-rerank-v3 \
  --model-id azureml://registries/azure-openai/models/cohere-rerank-v3.5
```

### Cost Controls

For edge-case models with unpredictable usage:
- Set up Azure Cost Management alerts
- Implement rate limiting at application layer
- Consider provisioned throughput only if usage stabilizes

### Fallback Strategy

```
Edge Case Request
       ↓
  [Primary Model Available?]
       ↓ No
  [Fallback to Core Model]
       ↓
  gpt-4o or claude-sonnet-4-5
```

---

## Consequences

### Positive

- **Specialized capabilities**: Better tools for specific tasks
- **Cost optimization**: Cheaper alternatives for appropriate workloads
- **Resilience**: More fallback options
- **Future-proofing**: Ready for new use cases

### Negative

- **Complexity**: More models to monitor and maintain
- **Integration work**: Each model needs application integration
- **Testing overhead**: More model combinations to validate

### Risks

- **Underutilization**: May deploy models that aren't used
- **Cost sprawl**: Easy to accumulate unused serverless endpoints
- **Vendor lock-in**: Each provider has different APIs

---

## Action Items

- [ ] Add Cohere Rerank v3.5 to Terraform (Phase 1)
- [ ] Create reranking integration in RAG pipeline
- [ ] Evaluate Codestral-2501 after 3 months of gpt-5.1-codex usage
- [ ] Evaluate Jamba-1.5-Large if long-document use cases emerge
- [ ] Monitor costs and usage monthly
- [ ] Review this ADR quarterly for relevance

---

## References

- [Azure AI Model Catalog](https://ai.azure.com/explore/models)
- [Cohere Rerank Documentation](https://docs.cohere.com/docs/rerank-2)
- [Mistral Models on Azure](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-mistral)
- [Meta Llama on Azure](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-llama)
- [DeepSeek on Azure](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-deepseek)
- [AI21 Jamba on Azure](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-jamba)
- [ADR-0020: AI Model Selection Strategy](./ADR-0020-ai-model-selection-strategy.md)
