# ADR-0021: Specialized & Edge Case Models

**Status**: Accepted
**Date**: 2025-12-27 (Updated)
**Decision Makers**: Platform Team, AI/ML Team
**Related**: [ADR-0020: AI Model Selection Strategy](./ADR-0020-ai-model-selection-strategy.md)
**Last Review**: December 2025

## Context

ADR-0020 established Mystira's core model strategy with 32 models covering primary use cases. This ADR addresses specialized models for edge cases, alternative providers, and emerging capabilities not covered by the core deployment.

These models are categorized as:
1. **RAG Enhancement** - Models specifically designed for search and retrieval
2. **Code Specialization** - Dedicated code models beyond GPT-5.1-codex
3. **Long Context** - Models for processing documents >128K tokens (up to 1M)
4. **Vision** - Multimodal models for image understanding
5. **Video Generation** - Sora and emerging video models
6. **Open Source** - Provider-agnostic alternatives (Meta Llama 4, etc.)
7. **Emerging Providers** - DeepSeek, AI21, Mistral, xAI for cost or capability advantages
8. **Real-time** - Audio streaming and real-time interaction models

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

#### DeepSeek-V3.1 ✅ Deployed

**Purpose**: Latest advanced reasoning model with strong benchmarks.

| Property | Value |
|----------|-------|
| Provider | DeepSeek |
| Model ID | `DeepSeek-V3.1` |
| Context | 64K tokens |
| Pricing | ~$0.27 input / $1.10 output (per 1M tokens) |
| Status | **Deployed** in UK South |

**Why Deployed**:
- Near Claude/GPT reasoning at fraction of cost
- Strong on math and logic
- Good for complex analysis tasks
- Excellent cost-to-performance ratio

#### DeepSeek-R1 ✅ Deployed

**Purpose**: Chain-of-thought reasoning model.

| Property | Value |
|----------|-------|
| Provider | DeepSeek |
| Model ID | `DeepSeek-R1` |
| Context | 64K tokens |
| Pricing | ~$0.55 input / $2.19 output (per 1M tokens) |
| Status | **Deployed** in UK South |

**Why Deployed**:
- Explicit chain-of-thought reasoning
- Alternative to o3-mini for step-by-step analysis
- Excellent for complex problem-solving

#### DeepSeek-R1-Distill-Llama-70B

**Purpose**: Reasoning-optimized model distilled from R1.

| Property | Value |
|----------|-------|
| Provider | DeepSeek |
| Model ID | `DeepSeek-R1-Distill-Llama-70B` |
| Context | 64K tokens |
| Pricing | Very low |

**Why Consider**:
- Smaller and faster than full R1
- Cheaper alternative to o3-mini
- Good for batch step-by-step analysis

### 7. xAI Grok Models

#### Grok-3 ✅ Deployed

**Purpose**: Strong reasoning model from xAI.

| Property | Value |
|----------|-------|
| Provider | xAI |
| Model ID | `grok-3` |
| Context | 131K tokens |
| Pricing | ~$3.00 input / $15.00 output (per 1M tokens) |
| Status | **Deployed** in UK South |

**Why Deployed**:
- Strong reasoning capabilities
- Alternative to Claude Sonnet for analysis
- Good performance on complex tasks

#### Grok-4 (Consider)

**Purpose**: Latest flagship from xAI.

| Property | Value |
|----------|-------|
| Provider | xAI |
| Model ID | `grok-4` |
| Context | 131K tokens |
| Availability | Available in Azure AI Foundry |

**Why Consider**:
- Latest xAI model
- Multiple modes: fast-reasoning, fast-non-reasoning
- Potential alternative to GPT-5.2

#### Grok-Code-Fast-1 (Consider)

**Purpose**: Code-specialized Grok model.

| Property | Value |
|----------|-------|
| Provider | xAI |
| Model ID | `grok-code-fast-1` |

**Why Consider**:
- Specialized for code generation
- Alternative to gpt-5.1-codex
- Optimized for speed

### 8. Video Generation Models

#### Sora (Consider)

**Purpose**: Video generation from text prompts.

| Property | Value |
|----------|-------|
| Provider | OpenAI |
| Model ID | `sora` |
| Pricing | ~$0.05 per second (720p) |
| Availability | Available in East US 2 |

**Why Consider**:
- Generate story visualizations
- Create animated content for Mystira stories
- Marketing and promotional content

#### Sora-2 (Consider)

**Purpose**: HD video generation.

| Property | Value |
|----------|-------|
| Provider | OpenAI |
| Model ID | `sora-2` |
| Pricing | ~$0.10 per second (1080p) |
| Availability | Available in East US 2 |

**Why Consider**:
- Higher resolution video
- Better quality for production content
- Extended duration support

### 9. Real-time Audio Models

#### GPT-4o-Realtime-Preview (Consider)

**Purpose**: Real-time audio streaming and conversation.

| Property | Value |
|----------|-------|
| Provider | OpenAI |
| Model ID | `gpt-4o-realtime-preview` |
| Features | Streaming audio, voice activity detection |
| Availability | Available in Global Standard |

**Why Consider**:
- Real-time voice interactions for Mystira
- Live narration capabilities
- Interactive storytelling experiences

---

## Prioritization Matrix

| Model | Priority | Phase | Use Case | Status | Monthly Est. |
|-------|----------|-------|----------|--------|--------------|
| Cohere Rerank v3.5 | **High** | Phase 1 | RAG quality | ✅ Deployed | $100 |
| Codestral-2501 | **High** | Phase 1 | Code tasks | ✅ Deployed | $50 |
| DeepSeek-V3.1 | **High** | Phase 1 | Cost-effective reasoning | ✅ Deployed | $50 |
| DeepSeek-R1 | **High** | Phase 1 | Chain-of-thought | ✅ Deployed | $80 |
| Grok-3 | Medium | Phase 2 | Alternative reasoning | ✅ Deployed | $100 |
| Jamba-1.5-Large | Medium | Phase 2 | Long docs (256K) | ✅ Deployed | $100 |
| Jamba-1.5-Mini | Medium | Phase 2 | Efficient long context | ✅ Deployed | $30 |
| Llama-4-Maverick | Medium | Phase 2 | Latest open-source | ✅ Deployed | $50 |
| Cohere Embed Multilingual | Medium | Phase 2 | i18n | ✅ Deployed | $20 |
| Grok-4 | Low | Phase 3 | Latest xAI | Consider | TBD |
| Sora | Low | Phase 3 | Video generation | Consider | $200 |
| Sora-2 | Low | Phase 3 | HD video | Consider | $400 |
| GPT-4o-Realtime | Low | Phase 3 | Real-time audio | Consider | $150 |
| Llama-3.2-90B-Vision | Low | Future | Image analysis | Consider | $50 |

---

## Decision

### Completed (Phase 1) ✅

All high-priority models have been deployed:
- **Cohere Rerank v3.5** - RAG search quality improvement
- **Codestral-2501** - Cost-effective code generation (256K context)
- **DeepSeek-V3.1** - Budget-friendly reasoning
- **DeepSeek-R1** - Chain-of-thought reasoning

### Completed (Phase 2) ✅

Medium-priority models deployed:
1. **Grok-3** - Alternative reasoning from xAI
2. **Jamba-1.5-Large/Mini** - 256K context for long documents
3. **Llama-4-Maverick** - Latest Meta model with MoE architecture
4. **Cohere Embed Multilingual** - International content support

### Upcoming (Phase 3)

Evaluate based on usage patterns:
- **Sora/Sora-2** - Video generation for story visualizations
- **GPT-4o-Realtime** - Real-time audio interactions
- **Grok-4** - Latest xAI flagship when available
- **Vision models** - If image analysis demand grows

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

### Completed ✅

- [x] Add Cohere Rerank v3.5 to Terraform (Phase 1)
- [x] Add Cohere Embed Multilingual to Terraform
- [x] Add Codestral-2501 to Terraform (Phase 1)
- [x] Add DeepSeek-V3.1 to Terraform (Phase 1)
- [x] Add DeepSeek-R1 to Terraform (Phase 1)
- [x] Add DeepSeek-Coder-V2 to Terraform (Phase 1)
- [x] Add Jamba-1.5-Large/Mini to Terraform (Phase 2)
- [x] Add Grok-3 to Terraform (Phase 2)
- [x] Add Llama-4-Maverick to Terraform (Phase 2)
- [x] Implement model routing logic for specialized models (see azure-ai-foundry-rag-guide.md#model-router)

### In Progress

- [ ] Create reranking integration in RAG pipeline
- [ ] Monitor costs and usage monthly

### Future

- [ ] Evaluate Sora/Sora-2 for video generation when demand arises
- [ ] Evaluate GPT-4o-Realtime for interactive storytelling
- [ ] Consider Grok-4 when generally available
- [ ] Review this ADR quarterly for relevance (Next: March 2026)

---

## References

- [Azure AI Model Catalog](https://ai.azure.com/explore/models)
- [Cohere Rerank Documentation](https://docs.cohere.com/docs/rerank-2)
- [Mistral Models on Azure](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-mistral)
- [Meta Llama on Azure](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-llama)
- [DeepSeek on Azure](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-deepseek)
- [AI21 Jamba on Azure](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-jamba)
- [ADR-0020: AI Model Selection Strategy](./ADR-0020-ai-model-selection-strategy.md)
