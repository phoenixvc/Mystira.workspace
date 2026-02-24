# IMPORTANT: No Default Vector Store Fallback

## Critical Change

**FileSearch mode now requires EXPLICIT configuration for ALL supported age groups.**

There is **NO default/fallback vector store**. If a user requests a story for an age group that is not configured, the system will **fail with a clear error** rather than falling back to potentially inappropriate content.

---

## Error Behavior

### Scenario 1: Age Group Not Configured

**Configuration:**
```json
{
  "FileSearch": {
    "VectorStoresByAgeGroup": {
      "6-9": "vs_elementary_ghi789",
      "10-12": "vs_preteen_jkl012"
    }
  }
}
```

**Request:**
```http
POST /api/story-agent/sessions/start
{
  "ageGroup": "3-5"  // Not configured!
}
```

**Result:**
```
InvalidOperationException:
No vector store configured for age group '3-5'.
Configured age groups: [6-9, 10-12].
Add a vector store for age group '3-5' to FoundryAgent:FileSearch:VectorStoresByAgeGroup configuration.
```

---

### Scenario 2: No Age Group Provided

**Request:**
```http
POST /api/story-agent/sessions/start
{
  "storyPrompt": "A brave knight",
  "ageGroup": null  // Missing!
}
```

**Result:**
```
ArgumentException:
Age group is required for FileSearch knowledge mode.
Ensure the session has a valid age group specified.
```

---

### Scenario 3: Empty Configuration

**Configuration:**
```json
{
  "FileSearch": {
    "VectorStoresByAgeGroup": {}  // Empty!
  }
}
```

**Request:**
```http
POST /api/story-agent/sessions/start
{
  "ageGroup": "6-9"
}
```

**Result:**
```
InvalidOperationException:
No vector stores configured for FileSearch mode.
Set FoundryAgent:FileSearch:VectorStoresByAgeGroup in configuration with age-specific vector store IDs.
```

---

## Correct Configuration

### Minimal Valid Configuration

```json
{
  "FoundryAgent": {
    "KnowledgeMode": "FileSearch",
    "FileSearch": {
      "VectorStoresByAgeGroup": {
        "6-9": "vs_elementary_ghi789"
      }
    }
  }
}
```

**Behavior:**
- Age 6-9 requests: ✅ Uses `vs_elementary_ghi789`
- Any other age: ❌ Fails with clear error message

---

### Production Configuration (All Age Groups)

```json
{
  "FoundryAgent": {
    "KnowledgeMode": "FileSearch",
    "FileSearch": {
      "VectorStoresByAgeGroup": {
        "1-2": "vs_toddler_abc123",
        "3-5": "vs_preschool_def456",
        "6-9": "vs_elementary_ghi789",
        "10-12": "vs_preteen_jkl012",
        "13-15": "vs_teen_mno345"
      },
      "MaxFiles": 20,
      "MaxTokens": 4000
    }
  }
}
```

**Behavior:**
- All configured ages: ✅ Uses age-specific vector store
- Unconfigured ages: ❌ Fails with clear error listing configured ages

---

## Why No Default Fallback?

### Safety First

**Problem with defaults:**
```
User requests: Age 1-2 (toddler)
No config for 1-2 exists
Falls back to "default" store containing 10-12 content
❌ DANGER: Toddler receives preteen-level content (inappropriate!)
```

**Solution: Explicit configuration:**
```
User requests: Age 1-2 (toddler)
No config for 1-2 exists
❌ ERROR: "No vector store configured for age group '1-2'"
✅ SAFE: Administrator knows exactly what's missing
```

### Benefits

1. **Prevent inappropriate content** - No accidental age mismatches
2. **Explicit configuration** - Clear visibility of supported age groups
3. **Fail fast** - Errors at session creation, not during generation
4. **Better error messages** - Shows configured ages, making fix obvious
5. **Production safety** - Can't accidentally deploy with missing age groups

---

## Migration from Old Approach

### Old Code (Had Fallback)

```csharp
// OLD: Fell back to VectorStoreName if age group not found
var vectorStoreId = config.VectorStoresByAgeGroup?.GetValueOrDefault(ageGroup)
    ?? config.VectorStoreName  // ❌ Dangerous fallback
    ?? "mystira-story-knowledge";
```

### New Code (Explicit Only)

```csharp
// NEW: Throws exception if age group not configured
if (!config.VectorStoresByAgeGroup.TryGetValue(ageGroup, out var vectorStoreId))
{
    throw new InvalidOperationException(
        $"No vector store configured for age group '{ageGroup}'. " +
        $"Configured age groups: [{configuredAgeGroups}].");  // ✅ Clear error
}
```

---

## Updated Configuration Schema

### FileSearchConfig (New)

```csharp
public class FileSearchConfig
{
    /// <summary>
    /// REQUIRED: All supported age groups must be explicitly configured.
    /// No fallback/default store exists.
    /// </summary>
    public Dictionary<string, string> VectorStoresByAgeGroup { get; set; } = new();

    public int? MaxFiles { get; set; }
    public int? MaxTokens { get; set; }
}
```

**Key changes:**
- ❌ Removed `DefaultVectorStoreId` property
- ✅ `VectorStoresByAgeGroup` is now required (not nullable)
- ✅ Empty dictionary = clear error at runtime

---

## Testing Your Configuration

### Validation Script

```bash
# Test all configured age groups
for age in "1-2" "3-5" "6-9" "10-12"; do
  echo "Testing age group: $age"
  curl -X POST http://localhost:7001/api/story-agent/sessions/start \
    -H "Content-Type: application/json" \
    -d "{\"ageGroup\": \"$age\", \"storyPrompt\": \"test\"}"
done

# Test unconfigured age group (should fail)
echo "Testing unconfigured age: 13-15"
curl -X POST http://localhost:7001/api/story-agent/sessions/start \
  -H "Content-Type: application/json" \
  -d '{"ageGroup": "13-15", "storyPrompt": "test"}'
# Expected: 400 Bad Request with error message
```

---

## Error Messages Reference

| Error | Cause | Solution |
|-------|-------|----------|
| "Age group is required for FileSearch knowledge mode" | `ageGroup` is null/empty | Include `ageGroup` in request |
| "No vector stores configured for FileSearch mode" | `VectorStoresByAgeGroup` is empty | Add at least one age group mapping |
| "No vector store configured for age group 'X'" | Age group 'X' not in dictionary | Add 'X' to `VectorStoresByAgeGroup` or use different age |

---

## Summary

**Old Behavior:**
- Missing age group → Falls back to default → Risk of inappropriate content

**New Behavior:**
- Missing age group → **Error with clear message** → Safe, explicit configuration required

**Action Required:**
- ✅ Configure ALL age groups your application supports
- ✅ Remove any references to `DefaultVectorStoreId` from configuration
- ✅ Test all age groups to ensure configuration is complete
- ✅ Handle errors gracefully in client UI (show supported age groups)
