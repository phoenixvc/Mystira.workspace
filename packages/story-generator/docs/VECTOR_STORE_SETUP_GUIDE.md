# Vector Store Setup Guide for Age-Specific Story Generation

## Overview

This guide explains how to configure multiple vector stores (one per age group) for the Mystira Story Generator, enabling age-appropriate knowledge retrieval during story generation.

## Architecture

```
User Request (age: 6-9)
    ↓
AgentOrchestrator.InitializeSessionAsync()
    ↓
FileSearchKnowledgeProvider.GetVectorStoreIdForAgeGroup("6-9")
    ↓
    Returns: "vs_elementary_ghi789"
    ↓
FoundryAgentClient.CreateThreadWithVectorStoresAsync(agentId, ["vs_elementary_ghi789"])
    ↓
Thread created with 6-9 specific knowledge attached
    ↓
Writer Agent can only access 6-9 age-appropriate documents via file_search tool
```

## Step 1: Prepare Your Knowledge Documents

Create separate markdown files for each age group with appropriate content:

### Toddler (Ages 1-2)

**File: `knowledge/toddler_vocabulary.md`**
```markdown
# Vocabulary Guidelines for Ages 1-2

## Sentence Structure
- Use 2-4 word sentences only
- Simple subject-verb patterns: "Dog runs", "Baby eats"
- No complex grammar

## Vocabulary
- Concrete nouns: ball, dog, mom, milk, toy
- Basic verbs: eat, run, sleep, play
- Simple adjectives: big, small, hot, cold
- NO abstract concepts (hope, bravery, justice)

## Repetition
- Repeat key phrases for memory building
- Use consistent character names
- Pattern-based storytelling (e.g., "Brown bear, brown bear, what do you see?")
```

**File: `knowledge/toddler_safety_guidelines.md`**
```markdown
# Safety Guidelines for Ages 1-2

## Forbidden Content
- NO separation anxiety themes (lost parent, being alone)
- NO scary animals or monsters
- NO loud noises or sudden events
- NO bedtime resistance themes

## Appropriate Themes
- Daily routines (eating, bathing, playing)
- Sensory exploration (textures, colors, sounds)
- Familiar objects and people
- Positive emotions only (happy, excited, curious)
```

### Elementary (Ages 6-9)

**File: `knowledge/elementary_vocabulary.md`**
```markdown
# Vocabulary Guidelines for Ages 6-9

## Sentence Structure
- Use compound and complex sentences
- Introduce subordinate clauses
- Vary sentence length for rhythm

## Vocabulary
- Reading level: 3rd-4th grade
- Introduce figurative language: simple metaphors ("brave as a lion")
- Abstract concepts OK: courage, honesty, friendship, fairness
- Context clues for new words

## Narrative Techniques
- Character development arcs
- Foreshadowing (basic)
- Multiple perspectives (if clear)
- Dialogue-driven storytelling
```

**File: `knowledge/elementary_safety_guidelines.md`**
```markdown
# Safety Guidelines for Ages 6-9

## Forbidden Content
- NO graphic violence or death
- NO romantic relationships beyond friendship
- NO adult themes (substance use, etc.)
- NO horror or psychological terror

## Appropriate Themes
- Moral dilemmas with clear resolutions
- Overcoming fears (age-appropriate: dark, first day of school)
- Friendship conflicts and resolution
- Family dynamics (sibling rivalry, new baby)
- Adventure with safe outcomes
```

---

## Step 2: Create Vector Stores in Azure AI Foundry

### Option A: Via Azure Portal

1. Navigate to https://ai.azure.com
2. Select your Azure AI Foundry project
3. Go to **Storage & Indexes** → **Vector Stores**
4. Click **Create vector store**

**Create the following 4 vector stores:**

#### Store 1: Toddler (1-2 years)
- **Name**: `vs-mystira-age-1-2`
- **Description**: Story guidelines and examples for toddlers (ages 1-2)
- **Files to upload**:
  - `knowledge/toddler_vocabulary.md`
  - `knowledge/toddler_safety_guidelines.md`
  - `knowledge/toddler_story_examples.md`
- **Copy the Vector Store ID** (e.g., `vs_abc123xyz456`)

#### Store 2: Preschool (3-5 years)
- **Name**: `vs-mystira-age-3-5`
- **Description**: Story guidelines and examples for preschoolers (ages 3-5)
- **Files to upload**:
  - `knowledge/preschool_vocabulary.md`
  - `knowledge/preschool_safety_guidelines.md`
  - `knowledge/preschool_story_examples.md`
- **Copy the Vector Store ID**

#### Store 3: Elementary (6-9 years)
- **Name**: `vs-mystira-age-6-9`
- **Description**: Story guidelines and examples for elementary age (ages 6-9)
- **Files to upload**:
  - `knowledge/elementary_vocabulary.md`
  - `knowledge/elementary_safety_guidelines.md`
  - `knowledge/elementary_story_examples.md`
- **Copy the Vector Store ID**

#### Store 4: Preteen (10-12 years)
- **Name**: `vs-mystira-age-10-12`
- **Description**: Story guidelines and examples for preteens (ages 10-12)
- **Files to upload**:
  - `knowledge/preteen_vocabulary.md`
  - `knowledge/preteen_safety_guidelines.md`
  - `knowledge/preteen_story_examples.md`
- **Copy the Vector Store ID**

---

## Step 3: Configure appsettings.json

Update your `appsettings.json` with the vector store IDs you copied:

```json
{
  "FoundryAgent": {
    "WriterAgentId": "asst_your_writer_agent_id",
    "JudgeAgentId": "asst_your_judge_agent_id",
    "RefinerAgentId": "asst_your_refiner_agent_id",
    "RubricSummaryAgentId": "asst_your_summary_agent_id",
    "Endpoint": "https://your-foundry-project.azure.com",
    "ApiKey": "your-api-key",
    "ProjectId": "your-project-id",
    "MaxIterations": 5,
    "RunTimeout": "00:05:00",
    "KnowledgeMode": "FileSearch",

    "VectorStoresByAgeGroup": {
      "1-2": "vs_abc123xyz456",      // Replace with your actual IDs
      "3-5": "vs_def456abc789",      // from Azure portal
      "6-9": "vs_ghi789def012",
      "10-12": "vs_jkl012ghi345"
    }
  }
}
```

---

## Step 4: Verify the Integration

### Test Request

Make a request to create a story for age group 6-9:

```bash
POST /api/story-agent/sessions/start
Content-Type: application/json

{
  "storyPrompt": "A young explorer discovers a magical forest",
  "knowledgeMode": "FileSearch",
  "ageGroup": "6-9",
  "targetAxes": ["wonder", "courage"]
}
```

### What Happens Under the Hood

1. **Session Initialization** (`AgentOrchestrator.InitializeSessionAsync`)
   ```csharp
   // Gets vector store ID for age 6-9
   var vectorStoreId = "vs_ghi789def012";

   // Creates thread with that specific vector store attached
   var thread = await CreateThreadWithVectorStoresAsync(writerAgentId, [vectorStoreId]);
   ```

2. **Agent Receives Age-Specific Guidance** (in prompt)
   ```
   Use the file_search tool to retrieve age-appropriate guidelines.
   The vector store is pre-filtered for age group 6-9.
   All retrieved documents are appropriate for this age range.
   ```

3. **Agent Searches Vector Store**
   - Agent decides: "I need vocabulary guidance for 6-9 year olds"
   - Calls `file_search` tool with query: "vocabulary and sentence structure"
   - Only retrieves from `vs-mystira-age-6-9` (isolated to that vector store)

4. **Story Generation**
   - Agent uses 6-9 appropriate vocabulary
   - Applies 6-9 safety guidelines
   - Creates age-appropriate moral dilemmas

### Expected Log Output

```
[10:23:45 INF] Initializing session sess_abc123 with knowledge mode FileSearch for age group 6-9
[10:23:45 DBG] Using age-specific vector store vs_ghi789def012 for age group 6-9
[10:23:46 INF] Created thread with vector store vs_ghi789def012 for age group 6-9
[10:23:46 INF] Created thread: thread_xyz789 with 1 vector stores
[10:23:47 INF] Session sess_abc123 initialized successfully with thread ID thread_xyz789
```

---

## Step 5: Verify Vector Store Isolation

Create test stories for different age groups to verify isolation:

### Test 1: Toddler Story (Should use simple 2-4 word sentences)

```bash
POST /api/story-agent/sessions/start
{
  "storyPrompt": "A bunny finds a carrot",
  "ageGroup": "1-2"
}
```

**Expected Output**:
```json
{
  "scenes": [
    {
      "narrative": "Bunny is hungry. Bunny looks around. Bunny sees carrot!",
      "choices": [
        { "text": "Eat carrot", ... },
        { "text": "Share carrot", ... }
      ]
    }
  ]
}
```

### Test 2: Elementary Story (Should use complex sentences)

```bash
POST /api/story-agent/sessions/start
{
  "storyPrompt": "A brave knight helps villagers",
  "ageGroup": "6-9"
}
```

**Expected Output**:
```json
{
  "scenes": [
    {
      "narrative": "Sir Galahad noticed the village was in trouble when he heard the desperate cries echoing from the town square. The dragon had been terrorizing the farmers for weeks, and though he felt a flutter of fear in his chest, he knew he had to act.",
      "choices": [
        {
          "text": "Confront the dragon directly to protect the villagers",
          "consequence_axes_delta": { "courage": 0.8, "compassion": 0.5 }
        }
      ]
    }
  ]
}
```

---

## Troubleshooting

### Issue: Agent not retrieving age-appropriate content

**Symptom**: 6-9 year old story has toddler-level vocabulary

**Solution**:
1. Check logs for vector store ID: `grep "vector store" logs/app.log`
2. Verify configuration mapping in appsettings.json
3. Confirm vector store has files indexed:
   ```bash
   GET https://your-foundry-endpoint/openai/vector_stores/vs_ghi789def012/files
   ```

### Issue: "No vector store configured" error

**Symptom**: Exception thrown during session initialization

**Solution**:
- Ensure EITHER `VectorStoreName` (fallback) OR `VectorStoresByAgeGroup` is configured
- Check appsettings.json syntax (valid JSON)

### Issue: All age groups use same content

**Symptom**: No difference between toddler and elementary stories

**Solution**:
- Verify different files were uploaded to each vector store
- Check file content is actually different
- Test vector store search directly via Foundry portal

---

## Advanced Configuration

### Adding New Age Groups

To add a new age group (e.g., "13-15" for teens):

1. Create vector store in Azure portal
2. Add to configuration:
   ```json
   "VectorStoresByAgeGroup": {
     "1-2": "vs_abc123",
     "3-5": "vs_def456",
     "6-9": "vs_ghi789",
     "10-12": "vs_jkl012",
     "13-15": "vs_mno345"  // New!
   }
   ```
3. No code changes required - system automatically uses it

### Fallback Behavior

If an age group is requested but not configured:
- System logs warning: `"No vector store configured for age group X, falling back to default"`
- Uses `VectorStoreName` as fallback
- Story may not be age-appropriate (monitor evaluation scores)

---

## Maintenance

### Updating Knowledge Documents

To update guidelines for an age group:

1. Navigate to Azure AI Foundry → Vector Stores
2. Select the appropriate store (e.g., `vs-mystira-age-6-9`)
3. Click **Upload files** or **Replace file**
4. Upload new/updated markdown file
5. Wait for re-indexing (usually <1 minute)
6. New sessions will use updated content immediately

### Version Control

Recommended practice:
- Store knowledge files in Git: `knowledge/elementary_vocabulary_v2.md`
- Tag vector store versions in descriptions: "Elementary guidelines v1.2"
- Keep audit log of when files were updated

---

## Summary

You've now configured:
- ✅ **4 isolated vector stores** (one per age group)
- ✅ **Automatic vector store selection** based on session age group
- ✅ **Age-appropriate knowledge retrieval** via FileSearch
- ✅ **Zero code changes needed** to add new age groups

**Architecture Benefits**:
- **Single Writer Agent** (no need for 4 separate agents)
- **Knowledge-driven specialization** (not prompt-driven)
- **Easy maintenance** (update markdown files, not agent configs)
- **Scalable** (add age groups without deploying new agents)

Next steps:
1. Create your knowledge markdown files
2. Upload to vector stores in Azure
3. Copy vector store IDs to appsettings.json
4. Test with different age groups
5. Monitor evaluation scores to verify age-appropriateness
