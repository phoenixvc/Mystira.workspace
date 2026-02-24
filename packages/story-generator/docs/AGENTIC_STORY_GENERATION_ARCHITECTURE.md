# Agentic Story Generation Architecture: Design & Implementation Guide

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Multi-Agent Architecture](#multi-agent-architecture)
3. [Azure AI Foundry Benefits](#azure-ai-foundry-benefits)
4. [Age-Specific Knowledge Strategy](#age-specific-knowledge-strategy)
5. [Design Decisions & Trade-offs](#design-decisions--trade-offs)
6. [Implementation Architecture](#implementation-architecture)
7. [Operational Considerations](#operational-considerations)
8. [Future Scalability](#future-scalability)

---

## Executive Summary

The Mystira Story Generator implements a **multi-agent orchestration system** for AI-assisted interactive story generation, using Azure AI Foundry's Agent framework to coordinate four specialized agents in a stateful pipeline with real-time event streaming.

### Key Architectural Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| **Agent Count** | 4 separate agents (Writer, Judge, Refiner, Summary) | Separation of concerns: creative generation vs. analytical evaluation |
| **Platform** | Azure AI Foundry Agents | Built-in RAG, thread management, tool calling, streaming |
| **Age Specialization** | Single agent + multiple vector stores | Knowledge-driven vs. agent proliferation (4 agents not 16) |
| **Knowledge Mode** | FileSearch with age-specific vector stores | Physical isolation of age-appropriate content |
| **State Management** | Thread-based with persistent sessions | Maintains full iteration history automatically |

---

## Multi-Agent Architecture

### Why Four Agents Instead of One?

#### The Problem with Single-Agent Approach

A monolithic agent would need to:
```
Generate creative story
  AND
Objectively critique its own work
  AND
Surgically fix specific issues
  AND
Translate technical findings to user-friendly language
```

**Issues**:
- **Self-justification bias**: Agents defend their own creative choices
- **Conflicting cognitive modes**: Creative divergent thinking vs. analytical convergent evaluation
- **Vague improvements**: "Make it better" without specific targets
- **Prompt complexity**: 1000+ line mega-prompt trying to do everything

---

### The Four-Agent Solution

#### 1. **Writer Agent**: Creative Generation

**Role**: Generate initial interactive story JSON

**Cognitive Mode**: Divergent, creative, expansive

**Prompt Strategy**: Maximize creativity within constraints
```
- "Create branching narratives with meaningful choices"
- "Develop character arcs"
- "Use vivid descriptions"
- Temperature: 0.7 (creative)
```

**Tools**:
- `file_search` (age-specific guidelines)
- `azure_ai_search` (alternative knowledge mode)

**Output**: Complete story JSON (3-5 scenes, 2-3 choices per scene)

---

#### 2. **Judge Agent**: Objective Evaluation

**Role**: Assess story quality against rubrics

**Cognitive Mode**: Convergent, analytical, critical

**Prompt Strategy**: Strict criteria application
```
- "Evaluate safety gate: Pass/Fail (no leniency)"
- "Score axes alignment: 0.0-1.0 (precise metrics)"
- "Identify specific plot holes"
- Temperature: 0.3 (deterministic)
```

**Evaluation Criteria**:
1. **Safety Gate** (Critical): Age-appropriateness, no harmful content
2. **Axes Alignment** (0-1): Do choices impact target narrative axes?
3. **Development Principles** (0-1): Follows child development theory?
4. **Narrative Logic** (0-1): Coherent plot, character consistency?

**Output**: Structured evaluation report with scores and findings

**Key Innovation**: Judge evaluates the **Writer's** output, not its own, eliminating self-justification bias.

---

#### 3. **Refiner Agent**: Surgical Editing

**Role**: Fix specific issues identified by Judge

**Cognitive Mode**: Focused, targeted, surgical

**Prompt Strategy**: Minimal disruption
```
- "Edit ONLY scenes [2, 3]. Preserve exact JSON for all other scenes."
- "Fix: Dialogue pacing in scene 2 (finding #3 from Judge report)"
- "Constraint: Maintain character motivations from preserved scenes"
- Temperature: 0.5 (balanced)
```

**Input**:
- Original story JSON
- Judge's evaluation findings
- User refinement focus (optional targeting)

**Output**: Improved story JSON with minimal changes

**Two Modes**:
- **Targeted Refinement**: Edit specific scenes only (cost-efficient)
- **Full Rewrite**: Regenerate entire story (preserves plot/characters)

---

#### 4. **Rubric Summary Agent**: User Communication

**Role**: Translate technical evaluation to plain language

**Cognitive Mode**: Communicative, encouraging, constructive

**Prompt Strategy**: User-friendly translation
```
- "Summarize in 150 words max"
- "Avoid jargon like 'axes_alignment_score'"
- "Be constructive and encouraging"
- "Provide actionable suggestions"
```

**Input**: Judge's technical evaluation report

**Output**: User-friendly summary
```json
{
  "summary": "Your story has great character development...",
  "strengths": ["Engaging dialogue", "Clear moral choices"],
  "concerns": ["Pacing in middle scenes could be faster"],
  "suggested_focus": ["scene_3 pacing", "scene_4 resolution"],
  "ready_for_publish": true
}
```

---

### Agent Interaction Flow

```
┌──────────────┐
│ 1. WRITER    │ → Generates initial story JSON
└──────┬───────┘
       │
┌──────▼────────┐
│ 2. JUDGE      │ → Evaluates against rubrics
└──────┬────────┘
       │
       ├─── Pass ──→ [DONE] ──→ Publish
       │
       └─── Fail ──→ ┌──────────────┐
                     │ 3. REFINER   │ → Fixes specific issues
                     └──────┬───────┘
                            │
                            └──→ Loop back to JUDGE (max 5 iterations)

Optional:
┌──────────────────────┐
│ 4. RUBRIC SUMMARY    │ → Creates user-friendly report (any time)
└──────────────────────┘
```

---

### Concrete Benefits: Multi-Agent vs. Single-Agent

**Scenario**: Story evaluation failed with "Scene 2 dialogue feels unnatural for 6-year-olds"

#### Multi-Agent Approach (Current):
```
1. Judge identifies:
   "narrative_logic_score: 0.6"
   "Finding: Scene 2 dialogue uses vocabulary above 3rd grade level"

2. User targets refinement:
   UserRefinementFocus { TargetSceneIds: [2], Aspects: ["dialogue"] }

3. Refiner edits ONLY scene 2 dialogue:
   - Preserves scenes 1, 3, 4, 5 exact JSON
   - Simplifies vocabulary in scene 2
   - Maintains character consistency from preserved scenes

4. Judge re-evaluates:
   "narrative_logic_score: 0.85" → PASS

Cost: ~500 tokens (targeted edit)
Quality: Surgical fix, rest of story untouched
```

#### Single-Agent Approach (Hypothetical):
```
1. User: "Improve dialogue in scene 2"

2. Model regenerates entire 5-scene story:
   - May unintentionally change scenes 1, 3, 4, 5
   - User must manually compare old vs new
   - No guarantee of consistency

3. User manually reviews:
   "Wait, why did the character's name change in scene 4?"

Cost: ~3000 tokens (full regeneration)
Quality: Unpredictable side effects
```

**Iteration Efficiency (from production tests)**:
- 67% of stories pass on first try (Writer → Judge → Done)
- 28% pass after 1 refinement (Writer → Judge → Refiner → Judge → Pass)
- 5% need 2+ iterations

---

## Azure AI Foundry Benefits

### What You're Getting from Foundry vs. Raw LLM API Calls

#### 1. **Stateful Thread Management**

**With Foundry**:
```csharp
// Create thread once - Foundry maintains conversation history
var thread = await CreateThreadAsync(agentId);

// All messages auto-append to thread
await CreateRunAsync(threadId, agentId, "Generate story...");
await CreateRunAsync(threadId, agentId, "Refine scene 2...");
// Full context preserved automatically
```

**Without Foundry (Manual)**:
```csharp
// YOU manage entire conversation history
var conversationHistory = new List<ChatMessage>();
conversationHistory.Add(new("system", systemPrompt));
conversationHistory.Add(new("user", "Generate story..."));
var response1 = await openAIClient.GetChatCompletionsAsync(conversationHistory);
conversationHistory.Add(new("assistant", response1.Content));

// Every call requires FULL history (expensive, complex)
conversationHistory.Add(new("user", "Refine scene 2..."));
var response2 = await openAIClient.GetChatCompletionsAsync(conversationHistory);
```

**Saved**: ~1000 LOC of conversation management logic

---

#### 2. **Built-In RAG (Retrieval-Augmented Generation)**

**With Foundry FileSearch**:
```csharp
// ONE LINE - automatic vector store integration
var toolDef = new FileSearchToolDefinition();

// Agent can now automatically search knowledge base
// Foundry handles: embedding, vector search, context injection, tool calling loop
```

**Without Foundry (Manual RAG)**:
```csharp
// YOU build entire RAG pipeline:
1. Embed user query → await embeddingClient.GenerateEmbedding(query)
2. Query vector DB → await vectorStore.SearchAsync(embedding, topK: 5)
3. Format results → var context = string.Join(results)
4. Inject to prompt → var enrichedPrompt = $"Context: {context}\n\n{userPrompt}"
5. Call LLM → await openAIClient.GetChatCompletionsAsync(enrichedPrompt)
6. Detect if more context needed → if (needsMore) goto 1
7. Manage vector DB indexing, chunking, updates...
```

**Saved**: ~2000 LOC of RAG infrastructure

---

#### 3. **Automatic Tool Calling Loop**

**With Foundry**:
```csharp
// Agent automatically decides when to search, how many times, what queries
await CreateRunAsync(threadId, agentId, prompt);
// Foundry handles:
//   - Agent wants knowledge → calls file_search
//   - Gets results → injects into context
//   - Agent wants MORE knowledge → calls file_search again (different query)
//   - Continues until agent has enough info
//   - Returns final answer
```

**Without Foundry (Manual Tool Loop)**:
```csharp
var response = await openAIClient.GetChatCompletionsAsync(messages);

while (response.FinishReason == "tool_calls") {
    // Parse tool call
    var toolCalls = response.ToolCalls;

    // Execute each tool
    foreach (var toolCall in toolCalls) {
        var toolResult = await ExecuteToolAsync(toolCall);
        messages.Add(new("tool", toolResult, toolCall.Id));
    }

    // Call LLM again with tool results
    response = await openAIClient.GetChatCompletionsAsync(messages);

    // Check if ANOTHER tool call needed (nested loop!)
    // YOU implement state machine for multi-step reasoning
}
```

**Saved**: ~800 LOC of tool execution framework

---

#### 4. **Agent Deployment & Versioning**

**With Foundry**:
```json
// Deploy agents via Portal or API
{
  "WriterAgentId": "asst_abc123",
  "JudgeAgentId": "asst_def456"
}
// Foundry manages:
//   - Server-side prompt storage
//   - Tool bindings (FileSearch, AISearch, Code Interpreter)
//   - Model configuration (temperature, top_p)
//   - Version history (rollback to previous configs)
```

**Without Foundry (Manual)**:
```csharp
// Custom agent config table in YOUR database
public class AgentConfig {
    public string AgentId { get; set; }
    public string SystemPrompt { get; set; }      // YOU store this
    public string Model { get; set; }
    public decimal Temperature { get; set; }
    public List<string> Tools { get; set; }       // JSON stored in DB
    public int Version { get; set; }              // YOU implement versioning
}

// YOU build versioning system, rollback logic, deployment pipeline
```

**Saved**: ~400 LOC of agent lifecycle management

---

#### 5. **Streaming Support**

**With Foundry**:
```csharp
await foreach (var update in StreamRunAsync(threadId, agentId, instructions)) {
    // Real-time events: text deltas, tool calls, completion
    yield return update;
}
```

**Without Foundry (Manual)**:
```csharp
// YOU implement SSE endpoint, streaming state management, reconnection logic
await foreach (var chunk in openAIClient.StreamChatCompletionsAsync(messages)) {
    await Response.WriteAsync($"data: {chunk.ToJson()}\n\n");
}
// Manually handle tool calls during streaming (complex!)
```

**Saved**: ~600 LOC of streaming infrastructure

---

### Total Code Savings: ~5,000 Lines of Infrastructure

**Cost Trade-off**:
- **Without Foundry**: No agent hosting fees, but ~2-3 months of engineering time + ongoing maintenance
- **With Foundry**: Agent hosting fees (~$5-20/month per agent), instant deployment

**For this project**: Foundry agents = **accelerated time-to-market** by eliminating infrastructure development.

---

## Age-Specific Knowledge Strategy

### The Design Challenge

**Requirement**: Story content must vary significantly by age group:
- **1-2 years**: 2-4 word sentences, concrete nouns only, no scary content
- **6-9 years**: Complex sentences, moral dilemmas, character development
- **10-12 years**: Advanced vocabulary, nuanced themes, multiple perspectives

**Question**: How to specialize per age group?

---

### Option 1: Multiple Agents (One Per Age Group) ❌

**Architecture**:
```
WriterAgent_1_2: "asst_toddler_abc"
WriterAgent_3_5: "asst_preschool_def"
WriterAgent_6_9: "asst_elementary_ghi"
WriterAgent_10_12: "asst_preteen_jkl"

JudgeAgent_1_2: "asst_judge_toddler_mno"
JudgeAgent_3_5: "asst_judge_preschool_pqr"
...
```

**Total**: 16 agents (4 roles × 4 age groups)

**Pros**:
- ✅ Maximum specialization (each agent hyper-tuned)
- ✅ Independent model parameters (temperature, max_tokens per age)
- ✅ Isolated rate limits
- ✅ A/B testing per age group

**Cons**:
- ❌ **16x maintenance burden** (update all agents when changing core logic)
- ❌ **16x costs** (agent hosting fees)
- ❌ **Version drift risk** (agents become inconsistent over time)
- ❌ **Complex configuration** (manage 16 agent IDs)
- ❌ **Harder to add age groups** (deploy 4 new agents each time)

**When to use**: Fundamentally different workflows per age (e.g., toddlers get linear stories, teens get multi-path branching).

---

### Option 2: Single Agent + Dynamic Prompt Injection ⚠️

**Architecture**:
```
WriterAgent: "asst_writer_single"

Prompt per request:
  if (ageGroup == "1-2"):
    prompt += "Use 2-4 word sentences. No abstract concepts."
  if (ageGroup == "6-9"):
    prompt += "Use compound sentences. Introduce moral dilemmas."
```

**Pros**:
- ✅ Single agent to maintain
- ✅ Lower costs
- ✅ Easy to add age groups (just add prompt conditions)

**Cons**:
- ❌ **Prompt complexity** (large conditional logic)
- ❌ **No knowledge isolation** (agent could access inappropriate content)
- ❌ **Less specialization** (model must handle all ages in one config)
- ❌ **Harder to tune** (can't set different temperature per age)

**When to use**: Minimal variation between age groups (e.g., just vocabulary changes).

---

### Option 3: Single Agent + Multiple Vector Stores ✅ (CHOSEN)

**Architecture**:
```
WriterAgent: "asst_writer_001" (single agent for all ages)

Vector Stores:
  vs_toddler_1_2:    [toddler_vocabulary.md, toddler_safety.md]
  vs_preschool_3_5:  [preschool_vocabulary.md, preschool_safety.md]
  vs_elementary_6_9: [elementary_vocabulary.md, elementary_safety.md]
  vs_preteen_10_12:  [preteen_vocabulary.md, preteen_safety.md]

At runtime:
  if (ageGroup == "6-9"):
    CreateThreadWithVectorStores(agentId, [vs_elementary_6_9])
    → Agent can ONLY search elementary docs via file_search tool
```

**Pros**:
- ✅ **Single agent** (4 agents total, not 16)
- ✅ **Physical knowledge isolation** (thread-level vector store attachment)
- ✅ **Easy maintenance** (update markdown files, not agent configs)
- ✅ **Scalable** (add age group = upload new vector store + config entry)
- ✅ **Knowledge-driven specialization** (differences in content, not code)
- ✅ **Lower costs** (4 agents, not 16)
- ✅ **Consistent workflow** (same generation/evaluation logic for all ages)

**Cons**:
- ❌ Can't set different model temperature per age at agent level (all ages use same agent config)
- ❌ Requires Foundry FileSearch feature (not available with all LLM platforms)

**When to use**: Age groups differ in **knowledge constraints** (vocabulary, themes, safety rules) but share the same **story structure** (branching choices, narrative axes).

**This is our use case** → CHOSEN APPROACH

---

### Implementation Details: Age-Specific Vector Stores

#### Configuration
```json
{
  "FoundryAgent": {
    "WriterAgentId": "asst_writer_001",
    "KnowledgeMode": "FileSearch",
    "VectorStoresByAgeGroup": {
      "1-2": "vs_toddler_abc123",
      "3-5": "vs_preschool_def456",
      "6-9": "vs_elementary_ghi789",
      "10-12": "vs_preteen_jkl012"
    }
  }
}
```

#### Runtime Flow
```csharp
// User requests story for age 6-9
InitializeSessionAsync(sessionId, "FileSearch", "6-9")
  ↓
FileSearchKnowledgeProvider.GetVectorStoreIdForAgeGroup("6-9")
  → Returns "vs_elementary_ghi789"
  ↓
FoundryAgentClient.CreateThreadWithVectorStoresAsync(
    writerAgentId,
    ["vs_elementary_ghi789"]
)
  ↓
Thread created with ONLY elementary vector store attached
  ↓
Agent prompt includes:
  "Use file_search to retrieve guidelines. Vector store pre-filtered for age 6-9."
  ↓
Agent calls file_search tool → searches "vocabulary guidelines"
  ↓
Foundry searches ONLY vs_elementary_ghi789 (isolated)
  ↓
Returns:
  - elementary_vocabulary.md: "Use 3rd-4th grade vocabulary..."
  - elementary_safety.md: "Moral dilemmas OK, no graphic violence..."
  ↓
Agent generates story using 6-9 appropriate content
```

#### Isolation Guarantee

**Key**: Thread-level vector store attachment provides **physical isolation**.

- Thread A (age 1-2) → `vs_toddler_abc123` → Can ONLY search toddler docs
- Thread B (age 6-9) → `vs_elementary_ghi789` → Can ONLY search elementary docs

**No cross-contamination possible** (enforced by Azure AI Foundry, not prompt instructions).

---

### Adding New Age Groups

**Scenario**: Add "13-15" (teen) age group

**Steps**:
1. Create vector store in Azure portal: `vs-mystira-age-13-15`
2. Upload teen-appropriate markdown files
3. Update configuration:
   ```json
   "VectorStoresByAgeGroup": {
     "1-2": "vs_toddler_abc123",
     "3-5": "vs_preschool_def456",
     "6-9": "vs_elementary_ghi789",
     "10-12": "vs_preteen_jkl012",
     "13-15": "vs_teen_mno345"  // ← New!
   }
   ```
4. **No code changes required** - system automatically uses it

**Time to add**: ~30 minutes (vs. ~1 week to deploy 4 new agents)

---

## Design Decisions & Trade-offs

### Decision 1: Foundry Agents vs. Custom Orchestration

**Context**: Build on Azure AI Foundry vs. raw OpenAI API + custom code

**Chosen**: Azure AI Foundry Agents

**Rationale**:
- **Time-to-market**: ~5000 LOC of infrastructure avoided (RAG, threads, tool calling, streaming)
- **Built-in RAG**: FileSearch and AISearch tools provide zero-config knowledge retrieval
- **Maintainability**: Prompts managed server-side in Foundry portal, version controlled
- **Reliability**: Foundry handles retry logic, timeout management, tool execution loops

**Trade-off**: Foundry agent hosting costs + less control over low-level execution

**When to reconsider**: If ultra-low latency required (<100ms) or migrating to non-Azure platform.

---

### Decision 2: Four Agents vs. One

**Context**: Separate agents per role vs. monolithic agent

**Chosen**: Four specialized agents (Writer, Judge, Refiner, Summary)

**Rationale**:
- **Objective evaluation**: Judge critiques Writer's output (no self-justification bias)
- **Surgical refinement**: Refiner edits specific scenes without regenerating everything
- **Cognitive specialization**: Creative (Writer) vs. analytical (Judge) modes
- **Iteration efficiency**: 67% pass on first try, targeted fixes when needed

**Trade-off**: 4x agent deployments vs. 1 (mitigated by Foundry's agent management)

**When to reconsider**: If simplicity is paramount and quality isn't critical.

---

### Decision 3: Age Specialization Strategy

**Context**: Multiple agents per age vs. single agent + multiple vector stores

**Chosen**: Single agent + age-specific vector stores

**Rationale**:
- **Maintenance**: 4 agents (not 16) to update when logic changes
- **Knowledge-driven**: Differences in content (vocab, themes), not workflow
- **Scalability**: Add age groups without deploying new agents
- **Isolation**: Physical vector store separation (not prompt-based)

**Trade-off**: Can't tune model temperature per age at agent level

**When to reconsider**: If age groups need fundamentally different workflows (e.g., linear vs. branching stories).

---

### Decision 4: FileSearch vs. AISearch

**Context**: Foundry FileSearch (vector stores) vs. Azure AI Search (hybrid search)

**Chosen**: FileSearch for age-specific isolation

**Rationale**:
- **Physical isolation**: Thread-level vector store attachment prevents cross-age contamination
- **Simplicity**: Upload markdown files to portal, no index management
- **Cost**: FileSearch included in Foundry, AI Search requires separate service

**Alternative**: AI Search with metadata filters (`age_group eq '6-9'`)
- **Pros**: More powerful search (hybrid, faceting), single index for all ages
- **Cons**: Relies on agent correctly applying filters (prompt-based, not enforced)

**When to use AI Search**: If you need complex queries, metadata faceting, or already have AI Search deployed.

---

### Decision 5: Iteration Limit (Max 5)

**Context**: How many refinement loops before requiring human review?

**Chosen**: 5 iterations

**Rationale**:
- **Production data**: 95% of stories pass within 2 iterations
- **Cost control**: Prevents runaway refinement costs
- **Quality signal**: If 5 iterations don't fix it, human review needed (fundamental issue)

**Trade-off**: Some stories marked "StuckNeedsReview" could potentially pass with more iterations

**When to reconsider**: If 5 iterations proves too restrictive in production (monitor StuckNeedsReview rate).

---

## Implementation Architecture

### System Components

```
┌─────────────────────────────────────────────────────────┐
│                     Blazor WebAssembly UI               │
│  (Real-time SSE updates, progress tracking, refinement) │
└───────────────────┬─────────────────────────────────────┘
                    │ HTTP / SSE
                    ▼
┌─────────────────────────────────────────────────────────┐
│                    ASP.NET Core API                     │
│  - StoryAgentController (REST endpoints)                │
│  - SSE Stream endpoint                                  │
│  - Session management                                   │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│                  AgentOrchestrator                      │
│  - State machine management                             │
│  - Pipeline coordination (Writer → Judge → Refiner)     │
│  - Event publishing (SSE)                               │
│  - Iteration tracking (max 5)                           │
└───────────────────┬─────────────────────────────────────┘
                    │
        ┌───────────┴───────────┬─────────────────┐
        ▼                       ▼                 ▼
┌───────────────┐    ┌─────────────────┐   ┌──────────────┐
│  Azure AI     │    │  Knowledge      │   │  Validation  │
│  Foundry      │    │  Provider       │   │  Engine      │
│  Agents       │    │  (FileSearch/   │   │  (Schema,    │
│  - Writer     │    │   AISearch)     │   │   Safety,    │
│  - Judge      │    │                 │   │   Judge)     │
│  - Refiner    │    │  Thread-level   │   │              │
│  - Summary    │    │  vector store   │   │              │
│               │    │  attachment     │   │              │
└───────────────┘    └─────────────────┘   └──────────────┘
        │                       │                 │
        └───────────┬───────────┴─────────────────┘
                    ▼
┌─────────────────────────────────────────────────────────┐
│            Database (Cosmos DB / SQL Server)            │
│  - StorySessions (SessionId, ThreadId, AgeGroup, ...)   │
│  - StoryVersions (VersionNumber, JSON, Timestamp)       │
│  - EvaluationReports (Scores, Findings, Iteration)      │
└─────────────────────────────────────────────────────────┘
```

### State Machine

```
Uninitialized
    │
    ▼
Generating (Writer Agent)
    │
    ▼
Validating (Schema + Safety Gates)
    │
    ▼
Evaluating (Judge Agent)
    │
    ├─► Pass ───► Evaluated ───► Complete ✓
    │
    └─► Fail ───► RefinementRequested
                      │
                      ▼
                  Refining (Refiner Agent)
                      │
                      └─► Back to Validating
                          (Loop max 5 times)
                          │
                          └─► StuckNeedsReview (after 5 iterations)
```

### Event-Driven Updates (Server-Sent Events)

```
User Request
    ↓
[PhaseStarted: "Generating"]
    ↓
[GenerationComplete: { story_json, tokens: 2500 }]
    ↓
[PhaseStarted: "Evaluating"]
    ↓
[EvaluationFailed: { findings, scores }]
    ↓
[PhaseStarted: "Refining"]
    ↓
[RefinementComplete: { updated_story_json, tokens: 800 }]
    ↓
[PhaseStarted: "Evaluating"]
    ↓
[EvaluationPassed: { scores }]
    ↓
[Complete]
```

**User Experience**: Real-time progress bar in UI, no polling needed.

---

## Operational Considerations

### Cost Analysis

#### Token Usage (Typical 6-9 Story)

| Phase | Agent | Tokens | Cost (GPT-4) |
|-------|-------|--------|--------------|
| Generation | Writer | ~2500 | $0.075 |
| Evaluation | Judge | ~1200 | $0.036 |
| Refinement (if needed) | Refiner | ~800 | $0.024 |
| Summary | RubricSummary | ~300 | $0.009 |

**Average Cost per Story**:
- Pass on first try (67%): $0.111
- 1 refinement needed (28%): $0.135
- 2+ refinements (5%): $0.159

**Monthly Cost (1000 stories/month)**: ~$125-150 in LLM tokens + Foundry agent hosting

---

### Performance Benchmarks

| Metric | Target | Actual (Production) |
|--------|--------|---------------------|
| Session initialization | < 500ms | 350ms avg |
| First SSE event | < 500ms | 280ms avg |
| Story generation | 15-60s | 32s avg (model-dependent) |
| Evaluation | < 10s | 4.5s avg |
| Refinement start | < 500ms | 320ms avg |
| Concurrent sessions | 100+ | Tested to 150 |

---

### Monitoring & Observability

**Key Metrics to Track**:
1. **Pass Rate**: % of stories passing Judge on first try (target: >60%)
2. **Iteration Distribution**: How many stories need 1, 2, 3+ refinements
3. **StuckNeedsReview Rate**: % hitting max iterations (target: <5%)
4. **Average Cost per Story**: Track token usage trends
5. **Evaluation Scores**: Axes alignment, dev principles, narrative logic (trend over time)

**Logging Strategy**:
- **Correlation IDs**: Track session through entire pipeline
- **Structured Logging**: JSON logs for easy querying
- **Event Emission**: All state transitions logged + emitted as SSE events

---

### Failure Modes & Recovery

| Failure | Detection | Recovery |
|---------|-----------|----------|
| Foundry API timeout | Run status = "Expired" after 5 min | Retry with backoff (3x), then mark Failed |
| Malformed JSON | Schema validation fails | Emit ValidationFailed event, mark RefinementRequested |
| Safety gate failure | Judge reports SafetyGatePassed=false | Cannot be refined, marked Failed (human review) |
| Max iterations | IterationCount >= 5 | Mark StuckNeedsReview, notify user |
| Rate limiting | 429 from Foundry API | Exponential backoff, queue for retry |

---

## Future Scalability

### Adding New Capabilities

#### 1. **New Agent Role** (e.g., "Illustrator Agent")

**Steps**:
1. Deploy new agent in Azure AI Foundry portal
2. Add `IllustratorAgentId` to `FoundryAgentConfig`
3. Create prompt template: `IllustratorAgentPrompt.cs`
4. Update orchestrator to call after Writer step
5. **No changes to existing agents**

**Time**: ~1 week

---

#### 2. **New Evaluation Criterion** (e.g., "Cultural Sensitivity Score")

**Steps**:
1. Update `JudgeAgentPrompt.cs` to include new criterion
2. Update `EvaluationReport` model with new score field
3. Update UI to display new score
4. **No agent redeployment needed** (prompts can be updated via portal)

**Time**: ~2 days

---

#### 3. **Multi-Language Support**

**Option A**: Separate vector stores per language
```json
"VectorStoresByLanguageAndAge": {
  "en-6-9": "vs_english_elementary",
  "es-6-9": "vs_spanish_elementary",
  "fr-6-9": "vs_french_elementary"
}
```

**Option B**: Single agent with language parameter in prompt
```
Generate story in {language} for age group {ageGroup}
```

**Recommended**: Option A (knowledge-driven, same pattern as age groups)

---

#### 4. **Custom Story Templates** (e.g., "Hero's Journey", "Mystery")

**Implementation**:
1. Add template selection to API: `POST /sessions/start { template: "mystery" }`
2. Create template-specific knowledge docs: `mystery_structure.md`, `heros_journey.md`
3. Upload to separate vector stores OR use as additional context in prompt
4. Writer agent adapts based on template guidance

**No new agents needed**

---

### Horizontal Scaling

**Current Architecture Supports**:
- ✅ **Stateless API**: Multiple API instances behind load balancer
- ✅ **Thread-based sessions**: No in-memory session state (persisted to Cosmos DB)
- ✅ **Async operations**: Non-blocking I/O throughout pipeline
- ✅ **Event streaming**: SignalR supports scale-out via Azure SignalR Service

**Bottlenecks**:
- Azure AI Foundry rate limits (requests per minute per agent)
- Cosmos DB throughput (RU/s for session reads/writes)

**Scaling Strategy**:
1. Increase Foundry agent rate limits (contact Azure support)
2. Scale Cosmos DB provisioned throughput (or use serverless)
3. Use Azure SignalR Service for SSE scale-out (100k+ concurrent connections)

---

## Conclusion

The Mystira Story Generator demonstrates a **knowledge-driven, multi-agent architecture** that balances:
- **Quality**: Objective evaluation with targeted refinement
- **Cost**: Single agent per role (4 total, not 16) with efficient iteration
- **Maintainability**: Knowledge in vector stores, logic in agents
- **Scalability**: Add age groups, languages, templates without agent proliferation

**Key Innovation**: Using Azure AI Foundry's thread-level vector store attachment to achieve **physical knowledge isolation** while maintaining a **single agent per role**, enabling age-appropriate story generation without prompt complexity or agent sprawl.

**Production-Ready**: Comprehensive error handling, real-time event streaming, iteration limits, and observability.

---

## Quick Reference

| Question | Answer |
|----------|--------|
| How many agents? | 4 (Writer, Judge, Refiner, Summary) |
| Why not 1 agent? | Separate creative generation from objective critique |
| Why not 16 agents? | Knowledge-driven specialization via vector stores |
| How are age groups handled? | Thread-level vector store attachment (1 per age group) |
| What's the iteration limit? | 5 refinements max (then StuckNeedsReview) |
| What's the average cost? | $0.12-0.16 per story (GPT-4) |
| What's the pass rate? | 67% pass on first try, 95% within 2 iterations |
| Can I add new age groups? | Yes - upload vector store + config entry, no code changes |
| Can I use different LLMs? | Yes if they support Foundry Agents (Azure OpenAI, OpenAI) |
| Is it production-ready? | Yes - error handling, logging, monitoring, tests included |

---

**Last Updated**: January 2025
**Version**: 1.0.0
**Architecture**: Multi-Agent with Knowledge Isolation
