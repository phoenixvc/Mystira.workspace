# Scenario Consistency Evaluation Service Implementation

## Overview
Implemented `ScenarioConsistencyEvaluationService` in the `Mystira.StoryGenerator.Application.StoryConsistencyAnalysis` namespace with parallel execution of two validation mechanisms.

## Architecture

### Core Components

#### 1. **IScenarioConsistencyEvaluationService** (Interface)
- **Location**: `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/IScenarioConsistencyEvaluationService.cs`
- **Purpose**: Defines contract for scenario consistency evaluation
- **Key Method**: `EvaluateAsync(ScenarioGraph, string, Func<Scene, IEnumerable<SceneEntity>>...)`

#### 2. **ScenarioConsistencyEvaluationService** (Implementation)
- **Location**: `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/ScenarioConsistencyEvaluationService.cs`
- **Purpose**: Orchestrates parallel execution of two validations
- **Dependencies**:
  - `ILlmConsistencyEvaluator` (injected)
  - `IEntityLlmClassificationService` (injected)
  - `ILogger<ScenarioConsistencyEvaluationService>` (injected)

### Validation Pipelines

#### Pipeline 1: Path Consistency Evaluation
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

#### Pipeline 2: Entity Introduction Validation
```
ScenarioGraph 
    → ScenarioEntityIntroductionValidator.FindIntroductionViolations()
    → IReadOnlyList<EntityIntroductionViolation>
    ↓
ScenarioGraph.Nodes 
    → (parallel) IEntityLlmClassificationService.ClassifyAsync() per scene
    → IReadOnlyDictionary<sceneId, SceneEntityClassificationData>
```

**Configuration**: `appsettings.json:Ai:EntityClassifier`
- Uses graph-theoretic data-flow analysis for violation detection
- Uses `SceneEntityLlmClassifier` (from Llm project) for entity classification
- Extracts introduced, removed, and time delta data per scene
- Runs classification in parallel across all scenes

### Parallel Execution Model
```csharp
// Both tasks start immediately
var pathConsistencyTask = EvaluatePathConsistencyAsync(...);
var entityIntroductionTask = ValidateEntityIntroductionAsync(...);

// Wait for both to complete
await Task.WhenAll(pathConsistencyTask, entityIntroductionTask);

// Results collected after parallel execution
var pathResult = await pathConsistencyTask;
var entityResult = await entityIntroductionTask;
```

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
  - ✅ Parallel execution of both validations
  - ✅ Partial results when one evaluation fails
  - ✅ Handling of null results from both evaluators
  - ✅ Entity introduction violation detection
  - ✅ Scene classification in parallel
  - ✅ Null parameter validation
  - ✅ Exception handling and recovery

### Test Cases
1. `EvaluateAsync_ExecutesBothValidationsInParallel` - Verifies parallel execution
2. `EvaluateAsync_ReturnsSuccessfulResultWhenBothEvaluationsReturnNull` - Handles graceful null
3. `EvaluateAsync_ReturnsPartialResultWhenOneEvaluationFails` - Partial results
4. `EvaluateAsync_FindsEntityIntroductionViolations` - Violation detection
5. `EvaluateAsync_ThrowsWhenGraphIsNull` - Parameter validation
6. `EvaluateAsync_ThrowsWhenScenarioPathContentIsNull` - Parameter validation

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
        var graph = ScenarioGraph.FromScenario(scenario);
        var paths = graph.GetCompressedPaths();
        var pathContent = SerializePathsToContent(paths);

        var result = await _evaluationService.EvaluateAsync(
            graph,
            pathContent,
            scene => ExtractIntroducedEntities(scene),
            scene => ExtractRemovedEntities(scene),
            scene => ExtractUsedEntities(scene),
            ct);

        if (result.PathConsistencyResult != null)
        {
            Console.WriteLine($"Path Consistency: {result.PathConsistencyResult.OverallAssessment}");
        }

        if (result.EntityIntroductionResult != null)
        {
            var violations = result.EntityIntroductionResult.Violations;
            Console.WriteLine($"Entity Introduction Violations: {violations.Count}");
        }
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
