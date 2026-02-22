# Mystira Story Consistency Engine
### Prefix Summaries • Must-Sets • SRL • Issue Detection

---

## Overview

Mystira’s story-consistency engine ensures logical coherence in highly branching interactive stories.  
Because different player choices and roll outcomes can produce many possible paths, the system must detect issues such as:

- Entities appearing before being introduced
- Entities disappearing and reappearing
- Time gaps and timeline contradictions
- Causal inconsistencies
- Incorrect roll/choice outcomes
- Style inconsistencies affecting early-childhood comprehension

To achieve this, Mystira uses three major layers:

1. **Prefix summaries** – global world-state reconstruction
2. **Prefix-aggregated must/maybe/absent sets** – merged world-state constraints
3. **Local SRL (Semantic Role Labeling)** – per-scene entity usage classification

The results are combined to detect issues across every frontier node in the story graph.

---

# 1. Prefix Path Generation

## 1.1 The Problem: Branching Explosion

A branching story graph can explode combinatorially.  
If each branch node has *b* outgoing edges and the depth is *d*, the total number of root→leaf paths can be as high as:

**bᵈ**

Enumerating *all* unique paths is often unnecessary and too expensive.

---

## 1.2 The Solution: Prefix Paths

A **prefix path** is the sequence of scenes from the root up to a *frontier point* where multiple branches converge.

Mystira does **not** enumerate every full path through the graph.  
Instead, it enumerates only:

- Sequential prefixes
- Up to points of convergence where states become equivalent

This reduces computational complexity enormously.

---

## 1.3 Why Prefix Count < Full Path Count

Two key reasons:

### 1. Frontier Merging

If many different story branches eventually reach the same scene *with the same effective world-state*, we only keep one prefix.

Example:  
Five different ways to reach `scene_bridge` → **1 prefix summary**, not 5 paths.

### 2. Early Collapse of Equivalent Contexts

If two paths introduce/remove the same entities and result in identical world-states, they collapse.

Thus:

**Number of prefix paths ≪ Number of full paths**

This is the foundational scalability property of the Mystira consistency engine.

---

# 2. Prefix Summary LLM (Global State Snapshot)

The prefix summary engine uses a large-model LLM to interpret an ordered sequence of scenes and output the **canonical world-state** accumulated so far.

Each summary contains:

- The canonical set of entities
- First introduction scene
- Last mention scene
- Status at the end of the prefix
- Known-by list
- Proper-noun analysis
- Notes helpful for downstream reasoning
- A prefix-level time-span estimate

The LLM must emit structured JSON compliant with the `ScenarioPathPrefixSummary` schema.

---

## 2.1 What the Prefix LLM Prompt Contains

The prompt instructs the model:

- How to identify entities
- When something counts as an introduction
- How to recognize removal
- How to estimate time spans
- How to canonicalize names
- How to assign knowledge sets
- What schema to output

It must output valid JSON, not prose.

### Why a Large Model?

Prefix summarization requires global reasoning:

- Multi-scene memory
- Entity lifecycle reconstruction
- Avoiding hallucinations
- Canonical naming stability
- Detecting implicit references

This makes models like **GPT-4.1, GPT-5.1, Claude 3.5 Sonnet, Gemini 2 Pro** ideal.  
Smaller models tend to miss entities, over-trigger introductions, or violate the schema.

---

# 3. Prefix Summary Aggregation

After generating prefix summaries for all frontier prefixes, Mystira merges them to compute:

- **Must-active sets** – entities present in *every* prefix to a scene
- **Maybe-active sets** – entities present in *some but not all* prefixes
- **Definitely-absent sets** – entities removed in *all* prefixes

Merging is performed by the `PrefixSummaryAggregator`.

---

## 3.1 Must-Active Set

An entity is **must-active** at scene `v` if:

- It appears in all prefix summaries ending at `v`.

This is computed via set intersection (⋂).

These entities:

- Must always be considered already-known
- Cannot be newly introduced
- Cannot disappear unless explicitly removed

---

## 3.2 Maybe-Active Set

An entity is **maybe-active** at `v` if:

- It appears in *some but not all* prefixes.

Computed as:

**(union − intersection)**

These entities are candidates for local SRL evaluation.

---

## 3.3 Definitely-Absent Set

An entity is **definitely absent** if *all* prefixes classify it as removed/unavailable.

This supports:

- Reappearance checks
- Correct SRL reintroduction logic

---

# 4. Local SRL (Scene-Level Entity Classification)

Each scene is processed independently by the SRL engine using the `SemanticRoleLabellingLlmService`.

SRL evaluates:

- Whether each candidate entity appears
- Whether the scene treats it as known or new
- Whether it is removed
- Its semantic roles
- Whether it is used as a proper noun
- A short evidence span

## 4.1 Purpose

Prefix summaries provide *global* truth.  
SRL provides *local* truth.

The combination enables highly accurate issue detection.

---

## 4.2 What the SRL Prompt Contains

The SRL prompt includes:

- The scene text
- The must-active set
- The definitely-absent set
- The full candidate set
- Very strict rules for:
    - Introduction vs already-known vs reintroduced
    - Disallowing contradictory labels for known entities
    - Style-based proper noun detection
    - Local usage style categorization

The LLM must output structured JSON matching the `SemanticRoleLabellingClassification` schema.

---

# 5. Issue Identification

After SRL classification, the system compares:

- SRL interpretation
- Prefix summary truths
- Must/maybe/absent sets

The `SceneContinuityAnalyzer` detects issues such as:

### • Used but not introduced
Entity is treated as `"already_known"` in SRL, but not guaranteed to be active.

### • Reintroduced but already guaranteed
Entity appears as `"new"` but must-active says it has always been known.

### • Entity disappears then reappears
Removed earlier, then treated as known with no reintroduction.

### • Roll/choice contradictions
Outcome text disagrees with roll instructions.

### • Improper introduction style
Scene uses descriptive scaffolding inconsistent with knowledge state.

This multi-level analysis provides robust and child-safe consistency validation.

---

# 6. Why the System Works

## 6.1 Prefix Summaries Reduce Complexity
Only frontier prefixes are analyzed.  
Convergent paths collapse.

## 6.2 SRL Adds Local Precision
Even subtle misuse (“the sad turtle”) is caught.

## 6.3 Global + Local = Consistency
By merging global constraints with local classification:

- Errors cannot slip through
- Ambiguities are minimized
- Style and logic remain consistent

This approach is uniquely powerful for branching stories.

---

# 7. Prompt Categories Used in Mystira

Mystira uses two families of LLM prompts:

## 7.1 Prefix Summary Prompts (Global)

Purpose: Build a canonical world-state snapshot.

Characteristics:

- Multi-scene reasoning
- Entity lifecycle analysis
- Time-span estimation
- Schema-enforced output

Model requirements:  
High reasoning depth and stable JSON output.

---

## 7.2 SRL Prompts (Local)

Purpose: Evaluate *one scene* with minimal context.

Characteristics:

- Strict rule-based classification
- Cannot contradict prefix summary truth
- Detects new introductions, semantic roles, removal, style, proper-noun usage

Model requirements:  
Medium reasoning depth, high determinism, strong compliance with rules.

---

# Conclusion

Mystira’s consistency engine integrates:

- Graph frontier merging
- Prefix LLM state reconstruction
- Must/maybe/absent set computation
- Local SRL classification
- Multi-layer issue detection

This ensures that branching stories remain logically coherent, emotionally appropriate, and safe for young readers.
