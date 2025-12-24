# ADR-0020: AI Model Selection Strategy

**Status**: Accepted
**Date**: 2024-12-24
**Decision Makers**: Platform Team, AI/ML Team
**Supersedes**: None

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
| gpt-4.1 | OpenAI | Reasoning | SAN | GlobalStandard | Structured data extraction |
| gpt-4.1-mini | OpenAI | Reasoning | SAN | GlobalStandard | Lightweight reasoning |
| gpt-4.1-nano | OpenAI | Reasoning | SAN | GlobalStandard | Classification, routing |
| gpt-5-nano | OpenAI | Next-gen | SAN | GlobalStandard | Advanced reasoning (cost-effective) |
| gpt-5.1 | OpenAI | Next-gen | SAN | GlobalStandard | Complex multi-step tasks |
| gpt-5.1-codex | OpenAI | Code | SAN | GlobalStandard | Code generation/review |
| o3-mini | OpenAI | Reasoning | SAN | GlobalStandard | Chain-of-thought analysis |
| text-embedding-3-large | OpenAI | Embedding | SAN | GlobalStandard | Production RAG |
| text-embedding-3-small | OpenAI | Embedding | SAN | GlobalStandard | Draft/test embeddings |
| dall-e-3 | OpenAI | Image | SAN | Standard | Story illustrations |
| whisper | OpenAI | Audio | SAN | Standard | Speech-to-text |
| tts | OpenAI | Audio | SAN | Standard | Text-to-speech |
| tts-hd | OpenAI | Audio | SAN | Standard | High-quality TTS |
| claude-haiku-4-5 | Anthropic | Fast | UK South | Serverless | High-volume analysis |
| claude-sonnet-4-5 | Anthropic | Balanced | UK South | Serverless | Deep analysis, code review |
| claude-opus-4-5 | Anthropic | Premium | UK South | Serverless | Complex research |

**Total: 18 models configured**

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
| gpt-4.1 | Yes | Yes | Deployed | Enhanced reasoning |
| gpt-4.1-mini | Yes | Yes | Deployed | Lightweight reasoning |
| gpt-4.1-nano | Yes | Yes | Deployed | Ultra-lightweight |
| gpt-5-nano | Yes | Yes | Deployed | Next-gen efficient |
| gpt-5.1 | Yes | Yes | Deployed | Next-gen flagship |
| gpt-5.1-codex | Yes | Yes | Deployed | Code specialized |
| o1-preview | Yes | No | Not needed | Superseded by o3-mini |
| o1-mini | Yes | No | Not needed | Superseded by o3-mini |
| o3-mini | Yes | Yes | Deployed | Latest reasoning |
| dall-e-3 | Yes | Yes | Deployed | Image generation |
| whisper | Yes | Yes | Deployed | Speech-to-text |
| tts | Yes | Yes | Deployed | Text-to-speech |
| tts-hd | Yes | Yes | Deployed | High-quality TTS |
| text-embedding-3-large | Yes | Yes | Deployed | Production embeddings |
| text-embedding-3-small | Yes | Yes | Deployed | Efficient embeddings |
| text-embedding-ada-002 | Yes | No | Not needed | Legacy, use v3 |

### Available Anthropic Models

| Model | Available | Configured | Status | Notes |
|-------|-----------|------------|--------|-------|
| claude-3-5-haiku | Yes | Yes | Deployed | Fast, cost-effective |
| claude-3-5-sonnet | Yes | No | Superseded | Use claude-sonnet-4-5 |
| claude-sonnet-4-5 | Yes | Yes | Deployed | Latest balanced |
| claude-opus-4-5 | Yes | Yes | Deployed | Maximum capability |
| claude-3-opus | Yes | No | Superseded | Use claude-opus-4-5 |

### Available Meta Llama Models

| Model | Available | Configured | Status | Notes |
|-------|-----------|------------|--------|-------|
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
| DeepSeek-V3 | Yes | No | **Consider** | Strong reasoning |
| DeepSeek-Coder-V2 | Yes | No | **Consider** | Code specialized |
| DeepSeek-R1-Distill | Yes | No | **Consider** | Reasoning distilled |

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

| Model | Input | Output | Monthly Est.* |
|-------|-------|--------|---------------|
| gpt-4o | $2.50 | $10.00 | $500 |
| gpt-4o-mini | $0.15 | $0.60 | $50 |
| gpt-4.1 | $2.00 | $8.00 | $400 |
| gpt-4.1-nano | $0.10 | $0.40 | $30 |
| gpt-5.1 | $5.00 | $15.00 | $800 |
| gpt-5.1-codex | $5.00 | $15.00 | $400 |
| text-embedding-3-large | $0.13 | - | $100 |
| text-embedding-3-small | $0.02 | - | $20 |
| claude-haiku-4-5 | $0.25 | $1.25 | $100 |
| claude-sonnet-4-5 | $3.00 | $15.00 | $600 |
| claude-opus-4-5 | $15.00 | $75.00 | $1,500 |

*Estimated based on moderate usage

### Gap Model Costs

| Model | Input | Output | Priority |
|-------|-------|--------|----------|
| o1-mini | $3.00 | $12.00 | High |
| o3-mini | $1.10 | $4.40 | High |
| dall-e-3 | $0.04/image (1024x1024) | - | High |
| whisper | $0.006/minute | - | Medium |
| tts | $15.00/1M chars | - | Medium |
| Cohere Rerank | $2.00/1K queries | - | Medium |

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

---

## Recommended Model Additions

### Phase 1: Critical (Immediate)

```hcl
# Add to variables.tf model_deployments

# Reasoning models for complex analysis
"o3-mini" = {
  model_name    = "o3-mini"
  model_version = "2025-01-31"
  model_format  = "OpenAI"
  sku_name      = "GlobalStandard"
  capacity      = 10
}

# Image generation for story illustrations
"dall-e-3" = {
  model_name    = "dall-e-3"
  model_version = "3.0"
  model_format  = "OpenAI"
  sku_name      = "Standard"
  capacity      = 1
}
```

### Phase 2: Accessibility (Next Sprint)

```hcl
# Speech-to-text for voice input
"whisper" = {
  model_name    = "whisper"
  model_version = "001"
  model_format  = "OpenAI"
  sku_name      = "Standard"
  capacity      = 1
}

# Text-to-speech for audio narration
"tts" = {
  model_name    = "tts"
  model_version = "001"
  model_format  = "OpenAI"
  sku_name      = "Standard"
  capacity      = 1
}
```

### Phase 3: RAG Enhancement (Future)

```hcl
# Cohere Rerank for improved search quality
"cohere-rerank-v3" = {
  model_name    = "rerank-v3.5"
  model_version = "1"
  model_format  = "Cohere"
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

    # Reasoning tasks → o-series or Claude
    if task in ["analysis", "reasoning", "planning"]:
        if complexity == "extreme":
            return "claude-opus-4-5"
        elif complexity == "high":
            return "o3-mini"
        else:
            return "claude-sonnet-4-5"

    # Code tasks → Codex or Claude
    if task in ["code_generation", "code_review"]:
        if complexity == "high":
            return "claude-sonnet-4-5"
        else:
            return "gpt-5.1-codex"

    # Creative writing → GPT-4o or Claude
    if task in ["story", "creative", "narrative"]:
        return "gpt-4o" if volume == "high" else "claude-sonnet-4-5"

    # High-volume tasks → Mini/Nano models
    if volume == "high":
        return "gpt-4o-mini"

    # Default
    return "gpt-4o"
```

### Fallback Chain

```
Primary → Secondary → Tertiary
gpt-4o → claude-sonnet-4-5 → gpt-4o-mini
gpt-5.1-codex → claude-sonnet-4-5 → gpt-4.1
claude-opus-4-5 → o3-mini → claude-sonnet-4-5
```

---

## Consequences

### Positive

- **Comprehensive coverage**: 13+ models for all use cases
- **Cost optimization**: Tier-based model selection
- **Quality options**: Premium models for critical tasks
- **Fallback resilience**: Multi-provider strategy
- **Future-ready**: Easy to add new models

### Negative

- **Complexity**: Multiple models to manage
- **Cost monitoring**: Need to track per-model usage
- **Regional split**: Some models only in UK South

### Risks

- **Model deprecation**: OpenAI may retire models
- **Quota limits**: May need to request increases
- **Pricing changes**: Costs may increase

---

## Action Items

- [x] Configure OpenAI GPT models (gpt-4o, gpt-4o-mini, gpt-4.1/5.x)
- [x] Configure embedding models (text-embedding-3-large/small)
- [x] Configure Anthropic Claude models (haiku, sonnet, opus)
- [x] Add Claude deployment script for automation
- [x] Add o3-mini for reasoning tasks
- [x] Add dall-e-3 for image generation
- [x] Add whisper for speech-to-text
- [x] Add tts/tts-hd for text-to-speech
- [x] Add Cohere Rerank for RAG improvement (see ADR-0021)
- [x] Add specialized models (Codestral, DeepSeek-Coder, Jamba) - see ADR-0021
- [x] Implement model routing logic (see azure-ai-foundry-rag-guide.md#model-router)
- [x] Document model selection in developer guide
- [ ] Set up cost monitoring per model

---

## References

- [Azure AI Model Catalog](https://ai.azure.com/explore/models)
- [OpenAI Pricing](https://azure.microsoft.com/en-us/pricing/details/cognitive-services/openai-service/)
- [Anthropic Claude on Azure](https://learn.microsoft.com/en-us/azure/ai-studio/how-to/deploy-models-anthropic)
- [Mystira RAG Guide](../infrastructure/azure-ai-foundry-rag-guide.md)
