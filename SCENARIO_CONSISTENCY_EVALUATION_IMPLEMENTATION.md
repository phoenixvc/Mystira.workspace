# Scenario Consistency Evaluation Service Implementation

## Overview
Implemented `ScenarioConsistencyEvaluationService` in the `Mystira.StoryGenerator.Application.StoryConsistencyAnalysis` namespace with parallel execution of two validation mechanisms.

## Architecture

### Core Components

#### 1. **IScenarioConsistencyEvaluationService** (Interface)
- **Location**: `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/IScenarioConsistencyEvaluationService.cs`
- **Purpose**: Defines contract for scenario consistency evaluation
- **Key Method**: `EvaluateAsync(ScenarioGraph, string, CancellationToken)`
  - Takes only the scenario graph and path content
  - Internally classifies all scenes and validates entities

#### 2. **ScenarioConsistencyEvaluationService** (Implementation)
- **Location**: `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/ScenarioConsistencyEvaluationService.cs`
- **Purpose**: Orchestrates parallel execution of three operations:
  1. Scene entity classification (all scenes in parallel)
  2. Path consistency evaluation
  3. Entity introduction validation using classifications
- **Dependencies**:
  - `ILlmConsistencyEvaluator` (injected)
  - `IEntityLlmClassificationService` (injected)
  - `ILogger<ScenarioConsistencyEvaluationService>` (injected)

### Validation Pipelines

#### Pipeline 1: Scene Entity Classification (Parallel)
```
ScenarioGraph.Nodes 
    → (parallel) IEntityLlmClassificationService.ClassifyAsync() per scene
    → IReadOnlyDictionary<sceneId, SceneEntityClassificationData>
```

**Configuration**: `appsettings.json:Ai:EntityClassifier`
- Uses `SceneEntityLlmClassifier` (from Llm project)
- Extracts introduced, removed entities and time delta per scene
- All scene classifications run in parallel via `Task.WhenAll`
- Gracefully handles null classifications from individual scenes

#### Pipeline 2: Path Consistency Evaluation
```
scenarioPathContent 
    → ILlmConsistencyEvaluator.EvaluateConsistencyAsync()
    → ConsistencyEvaluationResult?
```

**Configuration**: `appsettings.json:Ai:ConsistencyEvaluator`
- Uses `ScenarioPathConsistencyLlmEvaluator` (from Llm project)
- Evaluates dominator-based compressed scenario paths
- Returns overall assessment (ok, has_minor_issues, has_major_issues, broken)
- Returns detailed consistency issues with severity levels
- Runs in parallel with scene classification

#### Pipeline 3: Entity Introduction Validation
```
Classifications (from Pipeline 1)
    + ScenarioGraph
    → ScenarioEntityIntroductionValidator.FindIntroductionViolations()
    → IReadOnlyList<EntityIntroductionViolation>
```

**Process**:
1. Uses classifications from Pipeline 1 to get introduced/removed entities
2. Extracts "used" entities by matching entity names in scene text
3. Runs graph-theoretic data-flow analysis to detect violations
4. Runs sequentially after Pipelines 1 and 2 complete

### Parallel Execution Model
```csharp
// Step 1: Classification and path consistency run in parallel
var classificationTask = ClassifyScenesAsync(graph, cancellationToken);
var pathConsistencyTask = EvaluatePathConsistencyAsync(scenarioPathContent, cancellationToken);

// Wait for both parallel operations
await Task.WhenAll(classificationTask, pathConsistencyTask);

// Step 2: Entity validation uses classification results
var sceneClassifications = await classificationTask;
var entityIntroductionResult = await ValidateEntityIntroductionAsync(graph, sceneClassifications, cancellationToken);

// Step 3: Combine all results
var result = new ScenarioConsistencyEvaluationResult(
    pathConsistencyResult,
    entityIntroductionResult,
    isSuccessful);
```

**Key Points**:
- Pipelines 1 (classification) and 2 (path consistency) execute concurrently
- Pipeline 3 (entity validation) runs after 1 completes (requires classification results)
- Within Pipeline 1, all scenes are classified in parallel
- Failures in individual classifications don't block others

## Result Data Structures

### ScenarioConsistencyEvaluationResult
Top-level result combining both evaluations:
```csharp
public sealed record ScenarioConsistencyEvaluationResult(
    ConsistencyEvaluationResult? PathConsistencyResult,
    EntityIntroductionEvaluationResult? EntityIntroductionResult,
    bool IsSuccessful)
```

### EntityIntroductionEvaluationResult
Entity-specific validation results:
```csharp
public sealed record EntityIntroductionEvaluationResult(
    IReadOnlyList<EntityIntroductionViolation> Violations,
    IReadOnlyDictionary<string, SceneEntityClassificationData> SceneClassifications)
```

### SceneEntityClassificationData
Per-scene entity classification details:
```csharp
public sealed record SceneEntityClassificationData(
    string SceneId,
    string TimeDelta,
    IReadOnlyList<SceneEntity> IntroducedEntities,
    IReadOnlyList<SceneEntity> RemovedEntities)
```

## Error Handling & Graceful Degradation

- Individual evaluations that fail return `null` without propagating exceptions
- Service logs warnings at evaluation-level failures
- `IsSuccessful` flag indicates whether at least one evaluation completed
- Partial results supported (e.g., path consistency succeeds while entity classification fails)
- Scene classification failures return empty dictionary instead of failing entire validation

## Dependency Injection & Registration

### Service Registration (Program.cs)
```csharp
// Register consistency evaluation services
builder.Services.AddScoped<ILlmConsistencyEvaluator, ScenarioPathConsistencyLlmEvaluator>();
builder.Services.AddScoped<IEntityLlmClassificationService, SceneEntityLlmClassifier>();
builder.Services.AddScoped<IScenarioConsistencyEvaluationService, ScenarioConsistencyEvaluationService>();
```

## Logging

Service includes comprehensive logging at multiple levels:
- **Information**: Service start/completion with result summary
- **Debug**: Individual pipeline progress, violation counts, classification counts
- **Warning**: Graceful failure handling in sub-components

Log messages include:
- Execution flow (start/completion)
- Result availability (PathConsistency present?, EntityIntroduction present?)
- Violation counts
- Scene classification counts
- Failure details and recovery

## Testing

### Unit Tests
- **Location**: `tests/Mystira.StoryGenerator.Application.Tests/Mystira.StoryGenerator.Application.Tests/ScenarioConsistencyEvaluationServiceTests.cs`
- **Framework**: xUnit with Moq mocking
- **Coverage**:
  - ✅ Parallel execution of classification and path consistency
  - ✅ Per-scene classification in parallel
  - ✅ Partial results when one evaluation fails
  - ✅ Handling of null results from both evaluators
  - ✅ Entity introduction violation detection
  - ✅ Null parameter validation
  - ✅ Exception handling and recovery
  - ✅ Scene-specific classification mocking

### Test Cases
1. `EvaluateAsync_ExecutesBothClassificationAndPathConsistencyInParallel` - Verifies parallel execution of classification and path consistency
2. `EvaluateAsync_ReturnsSuccessfulResultWhenBothEvaluationsReturnNull` - Handles graceful null returns
3. `EvaluateAsync_ReturnsPartialResultWhenOneEvaluationFails` - Partial results when one pipeline fails
4. `EvaluateAsync_FindsEntityIntroductionViolations` - Violation detection with mocked per-scene classifications
5. `EvaluateAsync_ThrowsWhenGraphIsNull` - Graph parameter validation
6. `EvaluateAsync_ThrowsWhenScenarioPathContentIsNull` - Path content parameter validation
7. `EvaluateAsync_ClassifiesAllScenesInParallel` - Verifies concurrent execution of scene classifications using interlocked counters

## Configuration

### appsettings.json Settings

#### ConsistencyEvaluator
```json
"ConsistencyEvaluator": {
  "Enabled": true,
  "Provider": "azure-openai",
  "DeploymentName": "gpt-5.1",
  "ModelId": "gpt-5.1",
  "Temperature": 0.0,
  "MaxTokens": 10000
}
```

#### EntityClassifier
```json
"EntityClassifier": {
  "Enabled": true,
  "Provider": "azure-openai",
  "DeploymentName": "gpt-5-nano",
  "ModelId": "gpt-5-nano",
  "Temperature": 0.0,
  "MaxTokens": 1000
}
```

## Usage Example

```csharp
public class MyService
{
    private readonly IScenarioConsistencyEvaluationService _evaluationService;

    public MyService(IScenarioConsistencyEvaluationService evaluationService)
    {
        _evaluationService = evaluationService;
    }

    public async Task EvaluateStoryAsync(Scenario scenario, CancellationToken ct)
    {
        // Create graph from scenario
        var graph = ScenarioGraph.FromScenario(scenario);
        
        // Get compressed paths for path consistency evaluation
        var paths = graph.GetCompressedPaths();
        var pathContent = SerializePathsToContent(paths);

        // Evaluate scenario consistency - internally:
        // 1. Classifies all scenes in parallel
        // 2. Evaluates path consistency in parallel with classification
        // 3. Validates entity introductions using classification results
        var result = await _evaluationService.EvaluateAsync(graph, pathContent, ct);

        if (result.PathConsistencyResult != null)
        {
            Console.WriteLine($"Path Consistency: {result.PathConsistencyResult.OverallAssessment}");
            
            foreach (var issue in result.PathConsistencyResult.Issues)
            {
                Console.WriteLine($"  Issue: {issue.Summary} (Severity: {issue.Severity})");
            }
        }

        if (result.EntityIntroductionResult != null)
        {
            var violations = result.EntityIntroductionResult.Violations;
            Console.WriteLine($"Entity Introduction Violations: {violations.Count}");
            
            foreach (var violation in violations)
            {
                Console.WriteLine($"  Scene {violation.SceneId}: {violation.Entity.Name} used before introduction");
            }

            // Access per-scene classifications
            foreach (var (sceneId, classification) in result.EntityIntroductionResult.SceneClassifications)
            {
                Console.WriteLine($"Scene {sceneId}: {classification.IntroducedEntities.Count} introduced, " +
                    $"{classification.RemovedEntities.Count} removed, Time delta: {classification.TimeDelta}");
            }
        }

        Console.WriteLine($"Overall Success: {result.IsSuccessful}");
    }

    private string SerializePathsToContent(IEnumerable<ScenarioPath> paths)
    {
        // Serialize paths for LLM evaluation
        return string.Join("\n---\n", paths.Select(p => p.Story));
    }
}
```

## Files Created/Modified

### Created Files
1. `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/IScenarioConsistencyEvaluationService.cs`
2. `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/ScenarioConsistencyEvaluationService.cs`
3. `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/ScenarioConsistencyEvaluationResult.cs`
4. `tests/Mystira.StoryGenerator.Application.Tests/Mystira.StoryGenerator.Application.Tests/ScenarioConsistencyEvaluationServiceTests.cs`

### Modified Files
1. `src/Mystira.StoryGenerator.Api/Program.cs` - Added service registrations

## Performance Considerations

- **Parallelization**: Both validation pipelines execute concurrently via `Task.WhenAll`
- **Scene Classification Parallelization**: All scene classifications run in parallel
- **Graceful Degradation**: Failures in individual scenes don't fail entire classification
- **Timeout Management**: Uses configurable cancellation tokens propagated through the call chain

## Notes

- Scene serialization uses Title, Type, and Description fields (extensible as needed)
- Classification results are cached per scene in a dictionary for efficient access
- Service maintains no state between calls (stateless design)
- All result types are immutable records for thread safety
