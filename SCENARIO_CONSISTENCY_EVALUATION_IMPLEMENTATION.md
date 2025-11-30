# Scenario Consistency Evaluation Service Implementation

## Overview
Implemented a three-service architecture in the `Mystira.StoryGenerator.Application.StoryConsistencyAnalysis` namespace:
1. **IScenarioEntityConsistencyEvaluationService** - Evaluates entity consistency
2. **IScenarioDominatorPathConsistencyEvaluationService** - Evaluates path consistency  
3. **IScenarioConsistencyEvaluationService** - Orchestrator that runs both in parallel

All services take a `Scenario` as input and manage their own graph construction and path generation internally.

## Architecture

### Core Components

#### 1. **IScenarioEntityConsistencyEvaluationService** (Interface)
- **Location**: `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/IScenarioEntityConsistencyEvaluationService.cs`
- **Purpose**: Defines contract for entity consistency evaluation
- **Key Method**: `EvaluateAsync(Scenario, CancellationToken)`
  - Takes only the scenario
  - Internally constructs ScenarioGraph and classifies entities
  - Returns `EntityIntroductionEvaluationResult?`

#### 2. **ScenarioEntityConsistencyEvaluationService** (Implementation)
- **Location**: `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/ScenarioEntityConsistencyEvaluationService.cs`
- **Purpose**: Evaluates entity consistency within a scenario
- **Process**:
  1. Builds ScenarioGraph from Scenario
  2. Classifies all scenes in parallel using LLM
  3. Validates entity introduction violations using graph-theoretic data-flow analysis
- **Dependencies**:
  - `IEntityLlmClassificationService` (injected)
  - `ILogger<ScenarioEntityConsistencyEvaluationService>` (injected)

#### 3. **IScenarioDominatorPathConsistencyEvaluationService** (Interface)
- **Location**: `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/IScenarioDominatorPathConsistencyEvaluationService.cs`
- **Purpose**: Defines contract for path consistency evaluation
- **Key Method**: `EvaluateAsync(Scenario, CancellationToken)`
  - Takes only the scenario
  - Internally constructs ScenarioGraph and gets compressed paths
  - Returns `ConsistencyEvaluationResult?`

#### 4. **ScenarioDominatorPathConsistencyEvaluationService** (Implementation)
- **Location**: `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/ScenarioDominatorPathConsistencyEvaluationService.cs`
- **Purpose**: Evaluates path consistency on compressed scenario paths
- **Process**:
  1. Builds ScenarioGraph from Scenario
  2. Generates dominator-based compressed paths via `GetCompressedPaths()`
  3. Serializes paths to content string
  4. Uses LLM to evaluate consistency
- **Dependencies**:
  - `ILlmConsistencyEvaluator` (injected)
  - `ILogger<ScenarioDominatorPathConsistencyEvaluationService>` (injected)

#### 5. **IScenarioConsistencyEvaluationService** (Orchestrator Interface)
- **Location**: `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/IScenarioConsistencyEvaluationService.cs`
- **Purpose**: Orchestrates complete scenario consistency evaluation
- **Key Method**: `EvaluateAsync(Scenario, CancellationToken)`
  - Takes only the scenario
  - Runs both entity and path consistency evaluations in parallel

#### 6. **ScenarioConsistencyEvaluationService** (Orchestrator Implementation)
- **Location**: `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/ScenarioConsistencyEvaluationService.cs`
- **Purpose**: Orchestrates parallel execution of entity and path consistency evaluations
- **Dependencies**:
  - `IScenarioEntityConsistencyEvaluationService` (injected)
  - `IScenarioDominatorPathConsistencyEvaluationService` (injected)
  - `ILogger<ScenarioConsistencyEvaluationService>` (injected)

### Execution Flow

#### Entity Consistency Service Flow
```
Scenario 
    → Build ScenarioGraph
    → (parallel) ClassifyAsync() for each scene
    → Create lookup functions from classifications
    → Run ScenarioEntityIntroductionValidator.FindIntroductionViolations()
    → Return EntityIntroductionEvaluationResult
```

**Configuration**: `appsettings.json:Ai:EntityClassifier`
- Uses `SceneEntityLlmClassifier` (from Llm project)
- All scene classifications run in parallel via `Task.WhenAll`
- Gracefully handles null classifications from individual scenes
- Graph-theoretic data-flow analysis finds violations

#### Path Consistency Service Flow
```
Scenario 
    → Build ScenarioGraph
    → Call GetCompressedPaths() to generate dominator-based paths
    → Serialize paths to content string
    → Call ILlmConsistencyEvaluator.EvaluateConsistencyAsync()
    → Return ConsistencyEvaluationResult
```

**Configuration**: `appsettings.json:Ai:ConsistencyEvaluator`
- Uses `ScenarioPathConsistencyLlmEvaluator` (from Llm project)
- Evaluates dominator-based compressed scenario paths
- Returns overall assessment (ok, has_minor_issues, has_major_issues, broken)
- Returns detailed consistency issues with severity levels

#### Orchestrator Parallel Execution Model
```csharp
// Both services run in parallel on the same Scenario
var entityEvaluationTask = _entityEvaluationService.EvaluateAsync(scenario, cancellationToken);
var pathEvaluationTask = _pathEvaluationService.EvaluateAsync(scenario, cancellationToken);

// Wait for both to complete
await Task.WhenAll(entityEvaluationTask, pathEvaluationTask);

// Collect results
var entityIntroductionResult = await entityEvaluationTask;
var pathConsistencyResult = await pathEvaluationTask;

// Combine results
var result = new ScenarioConsistencyEvaluationResult(
    pathConsistencyResult,
    entityIntroductionResult,
    isSuccessful);
```

**Key Benefits**:
- Each service is independent and can be used separately
- Both specialized services run in parallel at orchestrator level
- Within entity service, all scene classifications run in parallel
- Each service manages its own ScenarioGraph construction
- No redundant graph construction or path generation
- Clear separation of concerns

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

### Using Individual Services
```csharp
public class EntityValidationService
{
    private readonly IScenarioEntityConsistencyEvaluationService _entityService;

    public EntityValidationService(IScenarioEntityConsistencyEvaluationService entityService)
    {
        _entityService = entityService;
    }

    public async Task ValidateEntitiesAsync(Scenario scenario, CancellationToken ct)
    {
        // Service internally builds graph and classifies entities
        var result = await _entityService.EvaluateAsync(scenario, ct);

        if (result != null)
        {
            Console.WriteLine($"Entity Introduction Violations: {result.Violations.Count}");
            
            foreach (var violation in result.Violations)
            {
                Console.WriteLine($"  Scene {violation.SceneId}: {violation.Entity.Name} used before introduction");
            }

            foreach (var (sceneId, classification) in result.SceneClassifications)
            {
                Console.WriteLine($"Scene {sceneId}: {classification.IntroducedEntities.Count} introduced, " +
                    $"Time delta: {classification.TimeDelta}");
            }
        }
    }
}

public class PathValidationService
{
    private readonly IScenarioDominatorPathConsistencyEvaluationService _pathService;

    public PathValidationService(IScenarioDominatorPathConsistencyEvaluationService pathService)
    {
        _pathService = pathService;
    }

    public async Task ValidatePathsAsync(Scenario scenario, CancellationToken ct)
    {
        // Service internally builds graph and generates compressed paths
        var result = await _pathService.EvaluateAsync(scenario, ct);

        if (result != null)
        {
            Console.WriteLine($"Path Consistency: {result.OverallAssessment}");
            
            foreach (var issue in result.Issues)
            {
                Console.WriteLine($"  Issue: {issue.Summary} (Severity: {issue.Severity})");
            }
        }
    }
}
```

### Using the Orchestrator Service
```csharp
public class CompleteConsistencyService
{
    private readonly IScenarioConsistencyEvaluationService _consistencyService;

    public CompleteConsistencyService(IScenarioConsistencyEvaluationService consistencyService)
    {
        _consistencyService = consistencyService;
    }

    public async Task EvaluateCompleteConsistencyAsync(Scenario scenario, CancellationToken ct)
    {
        // Orchestrator runs both entity and path evaluations in parallel
        var result = await _consistencyService.EvaluateAsync(scenario, ct);

        Console.WriteLine($"Overall Success: {result.IsSuccessful}");

        if (result.PathConsistencyResult != null)
        {
            Console.WriteLine($"\nPath Consistency: {result.PathConsistencyResult.OverallAssessment}");
            foreach (var issue in result.PathConsistencyResult.Issues)
            {
                Console.WriteLine($"  Issue: {issue.Summary} (Severity: {issue.Severity})");
            }
        }

        if (result.EntityIntroductionResult != null)
        {
            Console.WriteLine($"\nEntity Introduction Violations: {result.EntityIntroductionResult.Violations.Count}");
            foreach (var violation in result.EntityIntroductionResult.Violations)
            {
                Console.WriteLine($"  Scene {violation.SceneId}: {violation.Entity.Name} used before introduction");
            }
        }
    }
}
```

## Files Created/Modified

### Created Files
1. `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/IScenarioEntityConsistencyEvaluationService.cs`
2. `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/ScenarioEntityConsistencyEvaluationService.cs`
3. `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/IScenarioDominatorPathConsistencyEvaluationService.cs`
4. `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/ScenarioDominatorPathConsistencyEvaluationService.cs`
5. `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/IScenarioConsistencyEvaluationService.cs`
6. `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/ScenarioConsistencyEvaluationService.cs`
7. `src/Mystira.StoryGenerator.Application/StoryConsistencyAnalysis/ScenarioConsistencyEvaluationResult.cs`
8. `tests/Mystira.StoryGenerator.Application.Tests/Mystira.StoryGenerator.Application.Tests/ScenarioEntityConsistencyEvaluationServiceTests.cs`
9. `tests/Mystira.StoryGenerator.Application.Tests/Mystira.StoryGenerator.Application.Tests/ScenarioDominatorPathConsistencyEvaluationServiceTests.cs`
10. `tests/Mystira.StoryGenerator.Application.Tests/Mystira.StoryGenerator.Application.Tests/ScenarioConsistencyEvaluationServiceTests.cs`

### Modified Files
1. `src/Mystira.StoryGenerator.Api/Program.cs` - Added service registrations for all three services

## Performance Considerations

- **Orchestrator-Level Parallelization**: Entity and path consistency services run concurrently
- **Scene Classification Parallelization**: All scene classifications run in parallel within entity service
- **Path Generation**: Only performed once per service call (no redundancy)
- **Graceful Degradation**: Failures in individual scenes don't fail entire classification
- **Timeout Management**: Uses configurable cancellation tokens propagated through the call chain
- **Single Scenario Per Call**: No graph construction redundancy since both services receive same scenario

## Design Patterns

### Single Responsibility Principle
- Each service has a single, well-defined responsibility
- Entity service focuses on entity validation
- Path service focuses on path consistency
- Orchestrator focuses on coordination

### Dependency Injection
- All services are registered in DI container
- Can be injected individually or via orchestrator
- Loose coupling between services

### Immutable Results
- All result types are immutable records for thread safety
- Results can be safely shared across async contexts

## Notes

- Scene serialization uses Title, Type, and Description fields (extensible as needed)
- Each service builds its own ScenarioGraph to avoid shared state
- Services maintain no state between calls (stateless design)
- All result types are immutable records for thread safety
- Services use dependency injection for testability
