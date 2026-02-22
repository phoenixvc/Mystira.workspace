# Age-Specific Vector Stores: Complete Code Flow

## Overview

This document shows the complete code execution flow when a user requests a story for a specific age group with FileSearch knowledge mode.

---

## Execution Flow Example

**User Request**:
```http
POST /api/story-agent/sessions/start
{
  "storyPrompt": "A brave knight helps villagers",
  "ageGroup": "6-9",
  "knowledgeMode": "FileSearch"
}
```

---

## Step-by-Step Code Execution

### 1. **Configuration Loading** (`Startup.cs`)

```csharp
// On application startup
services.Configure<FoundryAgentConfig>(configuration.GetSection("FoundryAgent"));

// Config loaded:
{
  "VectorStoresByAgeGroup": {
    "1-2": "vs_toddler_abc123",
    "3-5": "vs_preschool_def456",
    "6-9": "vs_elementary_ghi789",    // ← Will be used
    "10-12": "vs_preteen_jkl012"
  }
}
```

---

### 2. **Service Registration** (`FoundryServiceCollectionExtensions.cs:68-72`)

```csharp
// FileSearchKnowledgeProvider is registered with config
var fileSearchConfig = new FileSearchKnowledgeProvider.FileSearchConfiguration
{
    VectorStoreName = "mystira-story-knowledge",  // Fallback
    VectorStoresByAgeGroup = foundryConfig.VectorStoresByAgeGroup  // ← Passed in
};

return new FileSearchKnowledgeProvider(client, fileSearchConfig, logger);
```

---

### 3. **API Endpoint** (`StoryAgentController.cs`)

```csharp
[HttpPost("sessions/start")]
public async Task<IActionResult> StartSession([FromBody] StartSessionRequest request)
{
    var sessionId = Guid.NewGuid().ToString();

    // Calls orchestrator
    var session = await _orchestrator.InitializeSessionAsync(
        sessionId,
        request.KnowledgeMode,   // "FileSearch"
        request.AgeGroup         // "6-9" ← Age group passed
    );

    return Accepted(session);
}
```

---

### 4. **Session Initialization** (`AgentOrchestrator.cs:62-81`)

```csharp
public async Task<StorySession> InitializeSessionAsync(
    string sessionId,
    string knowledgeMode,
    string ageGroup)  // "6-9"
{
    var session = new StorySession { AgeGroup = ageGroup, ... };

    // Check knowledge provider type
    if (_knowledgeProvider is FileSearchKnowledgeProvider fileSearchProvider)
    {
        // Get age-specific vector store ID
        var vectorStoreId = fileSearchProvider.GetVectorStoreIdForAgeGroup(ageGroup);
        // ↑ Returns "vs_elementary_ghi789" for age "6-9"

        // Create thread with vector store attached
        threadResult = await _foundryClient.CreateThreadWithVectorStoresAsync(
            _config.WriterAgentId,
            new[] { vectorStoreId }   // ["vs_elementary_ghi789"]
        );

        _logger.LogInformation(
            "Created thread with vector store {VectorStoreId} for age group {AgeGroup}",
            vectorStoreId,   // vs_elementary_ghi789
            ageGroup         // 6-9
        );
    }

    session.ThreadId = threadResult.ThreadId;
    await _sessionRepository.UpdateAsync(session);
    return session;
}
```

---

### 5. **Vector Store ID Lookup** (`FileSearchKnowledgeProvider.cs:87-110`)

```csharp
public string GetVectorStoreIdForAgeGroup(string? ageGroup)  // "6-9"
{
    // Check if age-specific stores are configured
    if (_config.VectorStoresByAgeGroup != null && !string.IsNullOrEmpty(ageGroup))
    {
        // Lookup in dictionary
        if (_config.VectorStoresByAgeGroup.TryGetValue(ageGroup, out var vectorStoreId))
        {
            _logger.LogDebug(
                "Using age-specific vector store {VectorStoreId} for age group {AgeGroup}",
                vectorStoreId,   // "vs_elementary_ghi789"
                ageGroup         // "6-9"
            );
            return vectorStoreId;  // ← Returns "vs_elementary_ghi789"
        }

        _logger.LogWarning("No vector store for age {AgeGroup}, using fallback", ageGroup);
    }

    // Fallback to default
    return _config.VectorStoreName;  // "mystira-story-knowledge"
}
```

---

### 6. **Thread Creation with Vector Store** (`FoundryAgentClient.cs:185-228`)

```csharp
public async Task<ThreadCreationResult> CreateThreadWithVectorStoresAsync(
    string agentId,                    // "asst_writer_abc123"
    IEnumerable<string> vectorStoreIds // ["vs_elementary_ghi789"]
)
{
    _logger.LogInformation(
        "Creating thread for agent: {AgentId} with vector stores: {VectorStores}",
        agentId,                              // asst_writer_abc123
        string.Join(", ", vectorStoreIds)     // vs_elementary_ghi789
    );

    // Build tool resources
    var toolResources = new ToolResources
    {
        FileSearch = new FileSearchToolResource()
    };

    foreach (var vectorStoreId in vectorStoreIds)
    {
        toolResources.FileSearch.VectorStoreIds.Add(vectorStoreId);
        // ↑ Attaches "vs_elementary_ghi789" to thread
    }

    // Create thread via Azure AI Foundry API
    var threadResponse = await _agentsClient.CreateThreadAsync(
        toolResources: toolResources,  // ← Vector store attached here
        cancellationToken: cancellationToken
    );

    var threadId = threadResponse.Value.Id;  // "thread_xyz789"

    _logger.LogInformation(
        "Created thread: {ThreadId} with {VectorStoreCount} vector stores",
        threadId,                      // thread_xyz789
        vectorStoreIds.Count()         // 1
    );

    return new ThreadCreationResult
    {
        ThreadId = threadId,           // thread_xyz789
        AssistantId = agentId
    };
}
```

---

### 7. **Story Generation** (`AgentOrchestrator.GenerateStoryAsync`)

```csharp
public async Task<(bool Success, string Message)> GenerateStoryAsync(
    string sessionId,
    string storyPrompt,   // "A brave knight helps villagers"
    CancellationToken ct)
{
    var session = await _sessionRepository.GetAsync(sessionId);
    // session.AgeGroup = "6-9"
    // session.ThreadId = "thread_xyz789" (with vs_elementary_ghi789 attached)

    // Build prompt with age-specific knowledge guidance
    var prompt = _promptGenerator.GenerateWriterPrompt(
        storyPrompt,
        session.AgeGroup,           // "6-9"
        session.TargetAxes ?? new()
    );

    // Submit run to thread
    var runResult = await _foundryClient.CreateRunAsync(
        session.ThreadId,           // thread_xyz789 (has vs_elementary_ghi789)
        _config.WriterAgentId,      // asst_writer_abc123
        prompt                      // Contains age-specific guidance
    );

    // Poll for completion...
}
```

---

### 8. **Prompt Generation with Age Context** (`PromptGenerator.cs:19-24`)

```csharp
public string GenerateWriterPrompt(
    string storyPrompt,  // "A brave knight helps villagers"
    string ageGroup,     // "6-9"
    List<string> axes)
{
    var guidelines = _guidelines.GetForAgeGroup(ageGroup);

    // Get age-specific knowledge guidance
    var knowledge = _knowledge.GetContextualGuidance(ageGroup);  // ← Passes "6-9"

    return WriterAgentPrompt.Build(storyPrompt, ageGroup, axes, knowledge, guidelines);
}
```

---

### 9. **Age-Specific Knowledge Guidance** (`FileSearchKnowledgeProvider.cs:72-82`)

```csharp
public string GetContextualGuidance(string? ageGroup)  // "6-9"
{
    var baseGuidance = "Use the file_search tool to retrieve age-appropriate guidelines...";

    if (!string.IsNullOrEmpty(ageGroup))
    {
        return $"{baseGuidance}\n\n" +
               $"The vector store is pre-filtered for age group {ageGroup}. " +
               // ↑ "The vector store is pre-filtered for age group 6-9."
               $"All retrieved documents are appropriate for this age range.";
    }

    return baseGuidance;
}
```

---

### 10. **Final Prompt Sent to Agent**

```
You are a professional children's story writer.

## Your Task
Generate a story for: A brave knight helps villagers

## Age-appropriate content for age group: 6-9
- Use age-appropriate vocabulary, themes, and conflicts
- Ensure characters are relatable to target age group

## Knowledge base context:
Use the file_search tool to retrieve age-appropriate guidelines.

The vector store is pre-filtered for age group 6-9.
All retrieved documents are appropriate for this age range.

## Project guidelines
[Age 6-9 specific guidelines from IProjectGuidelinesService]

[Rest of prompt...]
```

---

### 11. **Agent Execution (Azure AI Foundry)**

```
Agent receives prompt
  ↓
Agent thinks: "I need vocabulary guidance for age 6-9"
  ↓
Agent calls tool: file_search
  Query: "vocabulary and sentence structure for elementary age"
  ↓
Azure AI Foundry searches ONLY in vector store "vs_elementary_ghi789"
  (Cannot access toddler, preschool, or preteen stores)
  ↓
Returns documents:
  - elementary_vocabulary.md (6-9 content)
  - elementary_safety_guidelines.md (6-9 content)
  ↓
Agent uses this knowledge:
  - Compound/complex sentences ✓
  - 3rd-4th grade vocabulary ✓
  - Moral dilemmas with resolutions ✓
  - Character development arcs ✓
  ↓
Agent generates story JSON with age-appropriate content
  ↓
Returns story to orchestrator
```

---

### 12. **Different Age Group Example**

**If user requested age "1-2" instead:**

```
InitializeSessionAsync("1-2")
  ↓
GetVectorStoreIdForAgeGroup("1-2")
  ↓
Returns "vs_toddler_abc123"
  ↓
CreateThreadWithVectorStoresAsync(agentId, ["vs_toddler_abc123"])
  ↓
Thread created with TODDLER vector store only
  ↓
Agent searches file_search tool
  ↓
Only retrieves from toddler_vocabulary.md, toddler_safety.md
  ↓
Agent generates story:
  - 2-4 word sentences ✓
  - Concrete nouns only ✓
  - No scary content ✓
  - Sensory focus ✓
```

---

## Key Isolation Points

1. **Configuration-driven mapping**:
   ```json
   "6-9" → "vs_elementary_ghi789"
   "1-2" → "vs_toddler_abc123"
   ```

2. **Thread-level vector store attachment**:
   ```csharp
   // Each thread gets ONE vector store
   CreateThreadWithVectorStoresAsync(agentId, [ageSpecificVectorStoreId])
   ```

3. **File search scope isolation**:
   - Thread `thread_abc` with `vs_toddler_abc123` → can ONLY search toddler docs
   - Thread `thread_xyz` with `vs_elementary_ghi789` → can ONLY search elementary docs
   - **No cross-contamination possible**

---

## Benefits Summary

| Aspect | Implementation |
|--------|----------------|
| **Single Agent** | `WriterAgentId: asst_writer_abc123` (same for all age groups) |
| **Age Specialization** | Via vector store attachment, not agent configuration |
| **Prompt Awareness** | Agent told "pre-filtered for age group 6-9" |
| **Knowledge Isolation** | file_search can ONLY access attached vector store |
| **Adding Age Groups** | Just add to `VectorStoresByAgeGroup` config, no code changes |
| **Maintenance** | Update markdown files in vector stores, instant effect |

---

## Troubleshooting Checklist

✅ Vector stores created in Azure AI Foundry
✅ Vector store IDs copied to `appsettings.json`
✅ Knowledge mode set to `"FileSearch"` in config
✅ Age group passed in API request matches config keys (`"6-9"`, not `"6-9 years old"`)
✅ Files uploaded and indexed in each vector store
✅ Logs show correct vector store ID: `"Created thread with vector store vs_elementary_ghi789 for age group 6-9"`

---

## Next Steps

1. Create knowledge files: `knowledge/elementary_vocabulary.md`, etc.
2. Upload to vector stores in Azure portal
3. Copy vector store IDs to `appsettings.json`
4. Test with: `POST /api/story-agent/sessions/start` with different age groups
5. Verify logs show correct vector store selection
6. Compare story outputs across age groups to confirm differentiation
