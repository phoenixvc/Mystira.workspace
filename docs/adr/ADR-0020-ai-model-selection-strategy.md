# ADR-0020: AI Model Selection Strategy

**Status**: Accepted
**Date**: 2025-12-27 (Updated)
**Decision Makers**: Platform Team, AI/ML Team
**Supersedes**: None
**Last Review**: December 2025

## Context

Mystira is migrating from Azure OpenAI (legacy `kind: OpenAI`) to Azure AI Foundry (`kind: AIServices`), which provides access to a broader model catalog including OpenAI, Anthropic, Meta, Mistral, Cohere, and other providers.

This ADR documents:
1. Available models in Azure AI Foundry
2. Models selected for Mystira
3. Gap analysis and rationale
4. Pricing and regional considerations

## Decision

### Multi-Provider Model Strategy

Mystira will deploy models from multiple providers to optimize for:
- **Cost efficiency** (use cheaper models for simple tasks)
- **Quality** (use premium models for complex analysis)
- **Availability** (fallback options when primary models are unavailable)
- **Specialization** (code models for code, reasoning models for analysis)

---

## Model Inventory

### Currently Configured Models

| Model | Provider | Category | Region | SKU | Use Case |
|-------|----------|----------|--------|-----|----------|
| gpt-4o | OpenAI | Flagship | SAN | GlobalStandard | General content generation |
| gpt-4o-mini | OpenAI | Cost-optimized | SAN | GlobalStandard | Chat, high-volume tasks |
| gpt-4.1 | OpenAI | Reasoning | SAN | GlobalStandard | Structured data extraction (1M context) |
| gpt-4.1-mini | OpenAI | Reasoning | SAN | GlobalStandard | Lightweight reasoning (1M context) |
| gpt-4.1-nano | OpenAI | Reasoning | SAN | GlobalStandard | Classification, routing (1M context) |
| gpt-5-nano | OpenAI | Next-gen | SAN | GlobalStandard | Advanced reasoning (cost-effective) |
| gpt-5.1 | OpenAI | Next-gen | SAN | GlobalStandard | Complex multi-step tasks |
| gpt-5.1-codex | OpenAI | Code | SAN | GlobalStandard | Code generation/review |
| gpt-5.2 | OpenAI | Latest | SAN | GlobalStandard | Smartest model (400K context) |
| o3 | OpenAI | Reasoning | SAN | GlobalStandard | Advanced chain-of-thought |
| o3-mini | OpenAI | Reasoning | SAN | GlobalStandard | Chain-of-thought analysis |
| o4-mini | OpenAI | Reasoning | SAN | GlobalStandard | Fast reasoning |
| text-embedding-3-large | OpenAI | Embedding | SAN | GlobalStandard | Production RAG |
| text-embedding-3-small | OpenAI | Embedding | SAN | GlobalStandard | Draft/test embeddings |
| dall-e-3 | OpenAI | Image | SAN | Standard | Story illustrations |
| gpt-image-1 | OpenAI | Image | SAN | Standard | Advanced image generation |
| whisper | OpenAI | Audio | SAN | Standard | Speech-to-text |
| tts | OpenAI | Audio | SAN | Standard | Text-to-speech |
| tts-hd | OpenAI | Audio | SAN | Standard | High-quality TTS |
| claude-haiku-4-5 | Anthropic | Fast | UK South | Serverless | High-volume analysis ($1/$5 per 1M) |
| claude-sonnet-4-5 | Anthropic | Balanced | UK South | Serverless | Deep analysis, code review (1M context) |
| claude-opus-4-5 | Anthropic | Premium | UK South | Serverless | Complex research ($15/$75 per 1M) |
| cohere-rerank-v3 | Cohere | RAG | UK South | Serverless | Search reranking |
| cohere-embed-multilingual | Cohere | Embedding | UK South | Serverless | Multilingual RAG |
| codestral-2501 | Mistral | Code | UK South | Serverless | Code generation (256K context) |
| deepseek-v3.1 | DeepSeek | Reasoning | UK South | Serverless | Advanced reasoning |
| deepseek-r1 | DeepSeek | Reasoning | UK South | Serverless | Chain-of-thought reasoning |
| deepseek-coder-v2 | DeepSeek | Code | UK South | Serverless | Cost-effective code |
| jamba-1.5-large | AI21 | Long Context | UK South | Serverless | 256K context |
| jamba-1.5-mini | AI21 | Long Context | UK South | Serverless | Efficient long context |
| grok-3 | xAI | Reasoning | UK South | Serverless | Alternative reasoning |
| llama-4-maverick | Meta | Next-gen | UK South | Serverless | Latest Llama model |

**Total: 32 models deployed**

See [ADR-0021](./ADR-0021-specialized-edge-case-models.md) for additional specialized model considerations.

---

## Azure AI Foundry Model Catalog Analysis

### Available OpenAI Models

| Model | Available | Configured | Status | Notes |
|-------|-----------|------------|--------|-------|
| gpt-4o | Yes | Yes | Deployed | Flagship multimodal |
| gpt-4o-mini | Yes | Yes | Deployed | Cost-effective |
| gpt-4-turbo | Yes | No | Not needed | Superseded by gpt-4o |
| gpt-4 | Yes | No | Not needed | Legacy, use gpt-4o |
| gpt-3.5-turbo | Yes | No | Not needed | Legacy, use gpt-4o-mini |
| gpt-4.1 | Yes | Yes | Deployed | Enhanced reasoning, 1M context |
| gpt-4.1-mini | Yes | Yes | Deployed | Lightweight reasoning, 1M context |
| gpt-4.1-nano | Yes | Yes | Deployed | Ultra-lightweight, 1M context |
| gpt-5-nano | Yes | Yes | Deployed | Next-gen efficient |
| gpt-5.1 | Yes | Yes | Deployed | Next-gen flagship |
| gpt-5.1-codex | Yes | Yes | Deployed | Code specialized |
| gpt-5.2 | Yes | Yes | Deployed | Latest (Dec 2025), 400K context |
| gpt-5.2-chat | Yes | No | Consider | Optimized for chat, 400K context |
| gpt-5-codex-mini | Yes | No | Consider | Cost-effective code (4x cheaper) |
| o1-preview | Yes | No | Not needed | Superseded by o3 |
| o1-mini | Yes | No | Not needed | Superseded by o3-mini |
| o3 | Yes | Yes | Deployed | Advanced reasoning |
| o3-mini | Yes | Yes | Deployed | Latest reasoning |
| o4-mini | Yes | Yes | Deployed | Fast reasoning |
| dall-e-3 | Yes | Yes | Deployed | Image generation |
| gpt-image-1 | Yes | Yes | Deployed | Advanced image gen |
| sora | Yes | No | Consider | Video generation |
| sora-2 | Yes | No | Consider | Latest video gen |
| whisper | Yes | Yes | Deployed | Speech-to-text |
| tts | Yes | Yes | Deployed | Text-to-speech |
| tts-hd | Yes | Yes | Deployed | High-quality TTS |
| gpt-4o-realtime-preview | Yes | No | Consider | Real-time audio |
| text-embedding-3-large | Yes | Yes | Deployed | Production embeddings |
| text-embedding-3-small | Yes | Yes | Deployed | Efficient embeddings |
| text-embedding-ada-002 | Yes | No | Not needed | Legacy, use v3 |

### Available Anthropic Models

| Model | Available | Configured | Status | Notes |
|-------|-----------|------------|--------|-------|
| claude-haiku-4-5 | Yes | Yes | Deployed | Fast ($1/$5 per 1M), hybrid reasoning |
| claude-sonnet-4-5 | Yes | Yes | Deployed | Balanced ($3/$15 per 1M), 1M context |
| claude-opus-4-5 | Yes | Yes | Deployed | Premium ($15/$75 per 1M), Nov 2025 |
| claude-3-5-haiku | Yes | No | Superseded | Use claude-haiku-4-5 |
| claude-3-5-sonnet | Yes | No | Superseded | Use claude-sonnet-4-5 |
| claude-3-opus | Yes | No | Retiring | Deprecated Jun 2025, retiring Jan 2026 |
| claude-3-sonnet | No | No | Retired | Retired Jul 2025 |

**Note**: All Claude 4.5 models support hybrid reasoning with "Auto", "Fast", and "Thinking" modes.

### Available Meta Llama Models

| Model | Available | Configured | Status | Notes |
|-------|-----------|------------|--------|-------|
| Llama-4-Maverick-17B-128E-Instruct | Yes | Yes | Deployed | Latest Llama 4, MoE architecture |
| Llama-3.3-70B-Instruct | Yes | No | **Consider** | Open-source, no licensing |
| Llama-3.2-90B-Vision | Yes | No | **Consider** | Vision + text |
| Llama-3.2-11B-Vision | Yes | No | **Consider** | Efficient vision |
| Llama-3.1-405B-Instruct | Yes | No | Not needed | Very large, expensive |
| Llama-3.1-70B-Instruct | Yes | No | **Consider** | Good cost/performance |
| Llama-3.1-8B-Instruct | Yes | No | **Consider** | Ultra-efficient |

### Available Mistral Models

| Model | Available | Configured | Status | Notes |
|-------|-----------|------------|--------|-------|
| Mistral-Large-2411 | Yes | No | **Consider** | Strong reasoning |
| Mistral-Nemo-2407 | Yes | No | **Consider** | 12B efficient |
| Ministral-3B-2410 | Yes | No | **Consider** | Ultra-small, fast |
| Codestral-2501 | Yes | No | **Gap** | Code specialized |
| Pixtral-Large-2411 | Yes | No | **Consider** | Vision model |

### Available Cohere Models

| Model | Available | Configured | Status | Notes |
|-------|-----------|------------|--------|-------|
| Command R+ | Yes | No | **Consider** | RAG optimized |
| Command R | Yes | No | **Consider** | Efficient RAG |
| Embed v3 (English) | Yes | No | **Consider** | Alternative embeddings |
| Embed v3 (Multilingual) | Yes | No | **Consider** | 100+ languages |
| Rerank v3 | Yes | No | **Gap** | Search reranking |

### Available DeepSeek Models

| Model | Available | Configured | Status | Notes |
|-------|-----------|------------|--------|-------|
| DeepSeek-V3.1 | Yes | Yes | Deployed | Latest V3, strong reasoning |
| DeepSeek-V3-0324 | Yes | No | Consider | March 2024 version |
| DeepSeek-R1 | Yes | Yes | Deployed | Chain-of-thought reasoning |
| DeepSeek-R1-0528 | Yes | No | Consider | May 2025 version |
| DeepSeek-Coder-V2 | Yes | Yes | Deployed | Code specialized |
| DeepSeek-R1-Distill | Yes | No | Consider | Reasoning distilled |

### Available xAI Grok Models

| Model | Available | Configured | Status | Notes |
|-------|-----------|------------|--------|-------|
| grok-4 | Yes | No | **Consider** | Latest flagship |
| grok-4-fast-reasoning | Yes | No | **Consider** | Fast reasoning mode |
| grok-4-fast-non-reasoning | Yes | No | **Consider** | Fast general mode |
| grok-3 | Yes | Yes | Deployed | Strong reasoning |
| grok-3-mini | Yes | No | Consider | Cost-effective |
| grok-code-fast-1 | Yes | No | **Consider** | Code specialized |

### Available AI21 Models

| Model | Available | Configured | Status | Notes |
|-------|-----------|------------|--------|-------|
| Jamba-1.5-Large | Yes | No | **Consider** | 256K context |
| Jamba-1.5-Mini | Yes | No | **Consider** | Efficient long-context |

---

## Gap Analysis

### Critical Gaps (Recommended to Add)

| Gap | Model | Why Needed | Priority | Status |
|-----|-------|------------|----------|--------|
| **Reasoning Models** | o3-mini | Deep reasoning for complex analysis | High | ✅ Added |
| **Image Generation** | dall-e-3 | Story illustrations, content creation | High | ✅ Added |
| **Speech-to-Text** | whisper | Voice input for accessibility | Medium | ✅ Added |
| **Text-to-Speech** | tts, tts-hd | Audio narration for stories | Medium | ✅ Added |
| **Code Specialized** | Codestral-2501 | Alternative to gpt-5.1-codex | Low | See ADR-0021 |
| **Reranking** | Cohere Rerank v3 | Improve RAG search quality | Medium | See ADR-0021 |

### Optional Additions (Future Consideration)

| Model | Why Consider | Priority |
|-------|--------------|----------|
| Llama-3.3-70B | No per-token cost (provisioned) | Low |
| Command R+ | RAG optimization | Low |
| DeepSeek-V3 | Cost-effective reasoning | Low |
| Jamba-1.5-Large | 256K context for long documents | Low |
| Embed v3 Multilingual | Non-English content | Low |

---

## Mystira Use Case Mapping

### Storytelling Platform Requirements

| Use Case | Current Model | Alternative | Notes |
|----------|---------------|-------------|-------|
| **Story Generation** | gpt-4o | claude-sonnet-4-5 | Creative writing |
| **Story Editing** | gpt-4o-mini | - | High volume |
| **Character Development** | claude-sonnet-4-5 | gpt-4o | Complex personas |
| **Plot Analysis** | claude-opus-4-5 | o3-mini | Deep reasoning |
| **Content Moderation** | gpt-4.1-nano | - | Fast classification |
| **Translation** | gpt-4o | - | Multilingual |
| **Summarization** | gpt-4o-mini | claude-haiku-4-5 | High volume |
| **Story Illustrations** | **dall-e-3 (GAP)** | - | Visual content |
| **Audio Narration** | **tts (GAP)** | - | Accessibility |
| **Voice Input** | **whisper (GAP)** | - | Accessibility |

### RAG Pipeline Requirements

| Component | Current Model | Alternative | Notes |
|-----------|---------------|-------------|-------|
| **Document Embedding** | text-embedding-3-large | Cohere Embed v3 | Production |
| **Query Embedding** | text-embedding-3-small | - | Fast |
| **Reranking** | **Cohere Rerank (GAP)** | - | Quality boost |
| **Context Processing** | gpt-4o-mini | claude-haiku-4-5 | Fast |
| **Answer Generation** | gpt-4o | claude-sonnet-4-5 | Quality |

### Code/Development Requirements

| Use Case | Current Model | Alternative | Notes |
|----------|---------------|-------------|-------|
| **Code Generation** | gpt-5.1-codex | claude-sonnet-4-5 | Primary |
| **Code Review** | claude-sonnet-4-5 | Codestral-2501 | Analysis |
| **Bug Detection** | gpt-4.1 | - | Structured output |
| **Documentation** | gpt-4o | - | Clear writing |

---

## Pricing Analysis

### Current Model Costs (per 1M tokens)

| Model | Input | Output | Context | Monthly Est.* |
|-------|-------|--------|---------|---------------|
| gpt-4o | $2.50 | $10.00 | 128K | $500 |
| gpt-4o-mini | $0.15 | $0.60 | 128K | $50 |
| gpt-4.1 | $2.00 | $8.00 | 1M | $400 |
| gpt-4.1-nano | $0.10 | $0.40 | 1M | $30 |
| gpt-5.1 | $5.00 | $15.00 | 200K | $800 |
| gpt-5.1-codex | $5.00 | $15.00 | 200K | $400 |
| gpt-5.2 | $7.50 | $22.50 | 400K | $1,000 |
| o3-mini | $1.10 | $4.40 | 200K | $200 |
| o3 | $10.00 | $40.00 | 200K | $800 |
| o4-mini | $1.10 | $4.40 | 128K | $200 |
| text-embedding-3-large | $0.13 | - | 8K | $100 |
| text-embedding-3-small | $0.02 | - | 8K | $20 |
| claude-haiku-4-5 | $1.00 | $5.00 | 200K | $150 |
| claude-sonnet-4-5 | $3.00 | $15.00 | 1M | $600 |
| claude-opus-4-5 | $15.00 | $75.00 | 200K | $1,500 |
| deepseek-v3.1 | $0.27 | $1.10 | 64K | $50 |
| deepseek-r1 | $0.55 | $2.19 | 64K | $80 |
| grok-3 | $3.00 | $15.00 | 131K | $600 |

*Estimated based on moderate usage

### Media Model Costs

| Model | Pricing | Notes |
|-------|---------|-------|
| dall-e-3 | $0.04/image (1024x1024) | Standard quality |
| gpt-image-1 | $0.08/image | Advanced features |
| whisper | $0.006/minute | Speech-to-text |
| tts | $15.00/1M chars | Standard TTS |
| tts-hd | $30.00/1M chars | High-quality TTS |
| sora | $0.05/sec (720p) | Video generation |
| sora-2 | $0.10/sec (1080p) | HD video generation |
| Cohere Rerank | $2.00/1K queries | Search reranking |

---

## Regional Availability

### South Africa North (Primary)

| Category | Available Models |
|----------|------------------|
| OpenAI GPT | gpt-4o, gpt-4o-mini, gpt-4.1 series, gpt-5 series |
| OpenAI Embedding | text-embedding-3-large, text-embedding-3-small |
| OpenAI Image | dall-e-3 |
| OpenAI Audio | whisper, tts |
| Reasoning | o1-mini, o3-mini (limited) |

### UK South (Fallback)

| Category | Available Models |
|----------|------------------|
| All OpenAI | Full availability |
| Anthropic | All Claude models |
| Meta | Llama 3.x series |
| Mistral | All models |
| Cohere | All models |
| DeepSeek | All models |

### Deployment Strategy

```
Primary (SAN):
├── OpenAI GPT models (gpt-4o, gpt-4o-mini, gpt-4.1/5.x)
├── Embedding models (text-embedding-3-large/small)
├── Image generation (dall-e-3)
└── Audio models (whisper, tts)

Fallback (UK South):
├── Anthropic Claude (all models)
├── Models not available in SAN
└── Overflow capacity
```

### Regional Cost Considerations

Azure AI Services pricing is generally **consistent globally** for the same SKU tier:

| SKU | Pricing Model | Regional Variation |
|-----|---------------|-------------------|
| **GlobalStandard** | Pay-per-token | No variation - globally routed |
| **Standard** | Pay-per-token | Minimal variation by region |
| **ProvisionedManaged** | Reserved capacity | May vary by region availability |

**Key Points**:
- `GlobalStandard` SKU automatically routes to optimal datacenter, no regional cost difference
- UK South and South Africa North have identical per-token pricing for OpenAI models
- Catalog models (Anthropic, Cohere, etc.) are billed through Azure Marketplace with global pricing
- Data egress charges may apply for cross-region traffic (minimal for AI workloads)

**Recommendation**: Use `GlobalStandard` for all deployable models to:
1. Avoid regional pricing variations
2. Get automatic load balancing
3. Better availability during regional outages

---

## Recommended Model Additions

### Phase 1: Latest Models (Immediate) ✅ Complete

```hcl
# GPT-5.2 - Latest and smartest OpenAI model
"gpt-5.2" = {
  model_name    = "gpt-5.2"
  model_version = "2025-12-11"
  model_format  = "OpenAI"
  sku_name      = "GlobalStandard"
  capacity      = 10
}

# o3 - Advanced reasoning model
"o3" = {
  model_name    = "o3"
  model_version = "2025-04-16"
  model_format  = "OpenAI"
  sku_name      = "GlobalStandard"
  capacity      = 10
}

# o4-mini - Fast reasoning
"o4-mini" = {
  model_name    = "o4-mini"
  model_version = "2025-04-16"
  model_format  = "OpenAI"
  sku_name      = "GlobalStandard"
  capacity      = 10
}
```

### Phase 2: New Providers (Current Sprint)

```hcl
# xAI Grok-3 for alternative reasoning
"grok-3" = {
  model_name    = "grok-3"
  model_version = "1"
  model_format  = "xAI"
  sku_name      = "GlobalStandard"
  capacity      = 1
  location      = "uksouth"
}

# DeepSeek R1 for chain-of-thought
"deepseek-r1" = {
  model_name    = "DeepSeek-R1"
  model_version = "1"
  model_format  = "DeepSeek"
  sku_name      = "GlobalStandard"
  capacity      = 1
  location      = "uksouth"
}

# Llama 4 Maverick
"llama-4-maverick" = {
  model_name    = "Llama-4-Maverick-17B-128E-Instruct-FP8"
  model_version = "1"
  model_format  = "Meta"
  sku_name      = "GlobalStandard"
  capacity      = 1
  location      = "uksouth"
}
```

### Phase 3: Video & Advanced Media (Future)

```hcl
# Sora video generation
"sora" = {
  model_name    = "sora"
  model_version = "1"
  model_format  = "OpenAI"
  sku_name      = "Standard"
  capacity      = 1
}

# Sora-2 HD video
"sora-2" = {
  model_name    = "sora-2"
  model_version = "1"
  model_format  = "OpenAI"
  sku_name      = "Standard"
  capacity      = 1
}

# Real-time audio
"gpt-4o-realtime-preview" = {
  model_name    = "gpt-4o-realtime-preview"
  model_version = "2024-12-17"
  model_format  = "OpenAI"
  sku_name      = "GlobalStandard"
  capacity      = 1
}
```

---

## Model Routing Strategy

### Intelligent Model Selection

```python
def select_model(request: Request) -> str:
    """Route to optimal model based on task characteristics."""

    task = request.task_type
    complexity = request.complexity
    volume = request.volume_tier

    # Critical/Premium tasks → Latest models
    if complexity == "critical":
        return "gpt-5.2"  # Latest and smartest

    # Reasoning tasks → o-series or Claude
    if task in ["analysis", "reasoning", "planning"]:
        if complexity == "extreme":
            return "claude-opus-4-5"
        elif complexity == "high":
            return "o3"  # Advanced reasoning
        elif volume == "high":
            return "o4-mini"  # Fast reasoning
        else:
            return "claude-sonnet-4-5"

    # Code tasks → Codex or Claude
    if task in ["code_generation", "code_review"]:
        if complexity == "high":
            return "claude-sonnet-4-5"
        else:
            return "gpt-5.1-codex"

    # Creative writing → GPT-4o or Claude (hybrid reasoning)
    if task in ["story", "creative", "narrative"]:
        return "gpt-4o" if volume == "high" else "claude-sonnet-4-5"

    # Long context tasks (>200K tokens) → 1M context models
    if request.context_length > 200000:
        return "claude-sonnet-4-5"  # 1M context
    elif request.context_length > 128000:
        return "gpt-5.2"  # 400K context

    # High-volume tasks → Mini/Nano models
    if volume == "high":
        return "gpt-4o-mini"

    # Default
    return "gpt-4o"
```

### Fallback Chain

```
Primary → Secondary → Tertiary
gpt-5.2 → gpt-5.1 → gpt-4o
gpt-4o → claude-sonnet-4-5 → gpt-4o-mini
gpt-5.1-codex → claude-sonnet-4-5 → codestral-2501
claude-opus-4-5 → o3 → claude-sonnet-4-5
o3 → o3-mini → claude-sonnet-4-5
deepseek-r1 → o3-mini → gpt-4.1
```

---

## Consequences

### Positive

- **Comprehensive coverage**: 32 models for all use cases
- **Cost optimization**: Tier-based model selection with budget options
- **Quality options**: Premium models (GPT-5.2, Claude Opus 4.5) for critical tasks
- **Fallback resilience**: Multi-provider strategy (OpenAI, Anthropic, Meta, xAI, DeepSeek)
- **Future-ready**: Easy to add new models, video generation ready
- **Long context**: Up to 1M tokens (Claude Sonnet 4.5, GPT-4.1 series)
- **Hybrid reasoning**: Claude 4.5 models support Auto/Fast/Thinking modes

### Negative

- **Complexity**: 32 models to manage and monitor
- **Cost monitoring**: Need to track per-model usage across providers
- **Regional split**: Some models only available in UK South
- **Provider diversity**: Different APIs and behaviors across providers

### Risks

- **Model deprecation**: Claude 3 Opus retiring Jan 2026, legacy models being phased out
- **Quota limits**: May need to request increases for new models
- **Pricing changes**: Costs may change, especially for new models
- **Provider stability**: Newer providers (xAI, DeepSeek) may have API changes

---

## Action Items

- [x] Configure OpenAI GPT models (gpt-4o, gpt-4o-mini, gpt-4.1/5.x)
- [x] Configure GPT-5.2 (latest December 2025 model)
- [x] Configure o-series reasoning models (o3, o3-mini, o4-mini)
- [x] Configure embedding models (text-embedding-3-large/small)
- [x] Configure Anthropic Claude 4.5 models (haiku, sonnet, opus)
- [x] Add Claude deployment script for automation
- [x] Add dall-e-3 and gpt-image-1 for image generation
- [x] Add whisper for speech-to-text
- [x] Add tts/tts-hd for text-to-speech
- [x] Add Cohere Rerank for RAG improvement (see ADR-0021)
- [x] Add specialized models (Codestral, DeepSeek, Jamba) - see ADR-0021
- [x] Add xAI Grok-3 for alternative reasoning
- [x] Add Meta Llama 4 Maverick
- [x] Add DeepSeek V3.1 and R1 for cost-effective reasoning
- [x] Implement model routing logic (see azure-ai-foundry-rag-guide.md#model-router)
- [x] Document model selection in developer guide
- [ ] Set up cost monitoring per model
- [ ] Evaluate Sora/Sora-2 for video generation
- [ ] Evaluate gpt-4o-realtime-preview for real-time audio
- [ ] Migrate away from Claude 3 Opus before Jan 2026 retirement

---

## References

- [Azure AI Model Catalog](https://ai.azure.com/explore/models)
- [OpenAI Pricing](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/openai-service/)
- [Anthropic Claude on Azure](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-anthropic)
- [Mystira RAG Guide](../infrastructure/azure-ai-foundry-rag-guide.md)
