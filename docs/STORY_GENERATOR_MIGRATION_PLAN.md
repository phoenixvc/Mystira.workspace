# StoryGenerator Package Consolidation Migration Plan

## Overview

This document outlines the migration of `Mystira.StoryGenerator.*` packages into the main workspace as shared libraries that can be consumed by `admin-api`, `publisher`, and other services.

## Phase 1 Progress

| Task | Status | Commit |
|------|--------|--------|
| Add GraphTheory to `Mystira.Shared` | ✅ Complete | 14 files added |
| Create `Mystira.Ai` package | ✅ Complete | 9 files added |
| Create `Mystira.Authoring.Abstractions` package | ✅ Complete | 16 files added |
| Create `Mystira.Authoring` package | ✅ Complete | 7 files added |
| Verify packages build | ⏳ Pending | Requires dotnet CLI |

### Files Created

**Mystira.Shared.GraphTheory** (14 files):
- `IGraph.cs`, `IDirectedGraph.cs`, `IEdge.cs`, `Edge.cs`, `DirectedGraph.cs`
- `SearchAlgorithms.cs`, `SortAlgorithms.cs`, `PathAlgorithms.cs`
- `DataFlowNode.cs`, `DataFlowAnalysis.cs`
- `StateNode.cs`, `StateTransition.cs`, `FrontierMergedGraphBuilder.cs`, `FrontierMergedGraphResult.cs`

**Mystira.Ai** (9 files):
- `Abstractions/ILLMService.cs`, `Abstractions/ILlmServiceFactory.cs`
- `Configuration/AiSettings.cs`
- `Providers/AzureOpenAIService.cs`, `Providers/AnthropicAIService.cs`, `Providers/LLMServiceFactory.cs`
- `RateLimiting/PerMinuteRateLimiter.cs`
- `Extensions/ServiceCollectionExtensions.cs`
- `Mystira.Ai.csproj`

**Mystira.Authoring.Abstractions** (16 files):
- `Commands/ICommand.cs`, `Commands/ICommandHandler.cs`
- `Models/Scenario/Scenario.cs`, `Scene.cs`, `Branch.cs`, `ScenarioCharacter.cs`
- `Models/Entities/SceneEntity.cs`, `EntityClassification.cs`
- `Models/Consistency/ConsistencyEvaluationResult.cs`, `EntityContinuityIssue.cs`
- `Services/AuthoringContext.cs`, `IChatOrchestrationService.cs`, `IStoryValidationService.cs`, `IConsistencyEvaluationService.cs`, `IEntityClassificationService.cs`, `IStorySchemaProvider.cs`
- `Mystira.Authoring.Abstractions.csproj`

**Mystira.Authoring** (7 files):
- `Commands/Stories/GenerateStoryCommand.cs`, `ValidateStoryCommand.cs`
- `Commands/Chat/FreeTextCommand.cs`
- `Graph/ScenarioGraphBuilder.cs`
- `Services/ConsistencyEvaluationService.cs`
- `Extensions/ServiceCollectionExtensions.cs`
- `Mystira.Authoring.csproj`

## Decisions Made

| Decision | Choice | Rationale |
|----------|--------|-----------|
| AI package naming | `Mystira.Ai` | Clean, follows `Microsoft.Extensions.AI` convention |
| Graph Theory location | `Mystira.Shared.GraphTheory` | Generic algorithms reusable beyond stories |
| Authoring structure | Split with Abstractions | Long-term maintainability, follows Microsoft patterns |

## Current State

```
packages/story-generator/src/
├── Mystira.StoryGenerator.Contracts/    → DELETE (duplicated in Mystira.Contracts)
├── Mystira.StoryGenerator.Domain/       → Split into Mystira.Ai + Mystira.Authoring.Abstractions
├── Mystira.StoryGenerator.Application/  → Migrate to Mystira.Authoring
├── Mystira.StoryGenerator.Llm/          → Split into Mystira.Ai + Mystira.Authoring
├── Mystira.StoryGenerator.GraphTheory/  → Migrate to Mystira.Shared.GraphTheory
├── Mystira.StoryGenerator.RagIndexer/   → Migrate to Mystira.Authoring
├── Mystira.StoryGenerator.Api/          → Keep (references new packages)
├── Mystira.StoryGenerator.Console/      → Keep (references new packages)
├── Mystira.StoryGenerator.Llm.Console/  → Keep (references new packages)
└── Mystira.StoryGenerator.Web/          → Keep (references new packages)
```

## Target State

```
packages/
├── ai/
│   └── Mystira.Ai/                           ← Generic LLM infrastructure
├── authoring/
│   ├── Mystira.Authoring.Abstractions/       ← Interfaces, commands, POCOs
│   └── Mystira.Authoring/                    ← Implementations, handlers, services
├── shared/
│   └── Mystira.Shared/                       ← Add GraphTheory namespace
├── contracts/
│   └── Mystira.Contracts/                    ← Already has StoryGenerator/* (keep)
└── story-generator/                          ← Submodule references new packages
```

## Dependency Graph

```
┌─────────────────────────────────────────────────────────────────┐
│                         Consumers                                │
│         (admin-api, publisher, story-generator-api)             │
└─────────────────────────────────────────────────────────────────┘
                                │
                ┌───────────────┼───────────────┐
                ▼               ▼               ▼
        ┌───────────┐   ┌─────────────┐   ┌───────────┐
        │Mystira.Ai │   │  Mystira.   │   │ Mystira.  │
        │           │   │  Authoring  │   │ Contracts │
        └─────┬─────┘   └──────┬──────┘   └───────────┘
              │                │                 ▲
              │         ┌──────┴──────┐          │
              │         ▼             │          │
              │   ┌───────────┐       │          │
              │   │ Mystira.  │       │          │
              │   │ Authoring.│       │          │
              │   │Abstractions│      │          │
              │   └─────┬─────┘       │          │
              │         │             │          │
              └────┬────┴─────────────┘          │
                   ▼                             │
            ┌─────────────┐                      │
            │  Mystira.   │──────────────────────┘
            │   Shared    │
            └─────────────┘
```

---

## Package 1: Mystira.Ai

**Purpose**: Generic LLM/AI infrastructure reusable across all services.

### Files to Include

#### From `Mystira.StoryGenerator.Llm/`
| Source | Target Namespace |
|--------|------------------|
| `Services/LLM/AzureOpenAIService.cs` | `Mystira.Ai.Providers` |
| `Services/LLM/AnthropicAIService.cs` | `Mystira.Ai.Providers` |
| `Services/LLM/LLMServiceFactory.cs` | `Mystira.Ai.Providers` |
| `Extensions/ChatMessageExtensions.cs` | `Mystira.Ai.Extensions` |

#### From `Mystira.StoryGenerator.Domain/Services/`
| Source | Target Namespace |
|--------|------------------|
| `ILlmServiceFactory.cs` (ILLMService, ILlmServiceFactory) | `Mystira.Ai.Abstractions` |

#### From `Mystira.StoryGenerator.Application/Infrastructure/`
| Source | Target Namespace |
|--------|------------------|
| `RateLimiting/PerMinuteRateLimiter.cs` | `Mystira.Ai.RateLimiting` |

### Contracts (use existing `Mystira.Contracts.StoryGenerator`)
The following already exist in `Mystira.Contracts` and should be referenced:
- `Mystira.Contracts.StoryGenerator.Chat.*`
- `Mystira.Contracts.StoryGenerator.Configuration.*`

### Project Structure
```
packages/ai/Mystira.Ai/
├── Mystira.Ai.csproj
├── Abstractions/
│   ├── ILLMService.cs
│   └── ILlmServiceFactory.cs
├── Providers/
│   ├── AzureOpenAIService.cs
│   ├── AnthropicAIService.cs
│   └── LLMServiceFactory.cs
├── RateLimiting/
│   └── PerMinuteRateLimiter.cs
├── Extensions/
│   └── ChatMessageExtensions.cs
└── DependencyInjection.cs
```

### Dependencies
```xml
<ItemGroup>
  <PackageReference Include="Azure.AI.OpenAI" Version="2.5.0-beta.1" />
  <PackageReference Include="Anthropic.SDK" Version="..." />
  <ProjectReference Include="../../contracts/dotnet/Mystira.Contracts/Mystira.Contracts.csproj" />
</ItemGroup>
```

---

## Package 2: Mystira.Shared.GraphTheory

**Purpose**: Generic graph algorithms reusable across projects (not story-specific).

### Files to Include

#### From `Mystira.StoryGenerator.GraphTheory/`

**Generic Graph (fully reusable)**
| Source | Target Namespace |
|--------|------------------|
| `Graph/DirectedGraph.cs` | `Mystira.Shared.GraphTheory` |
| `Graph/Edge.cs` | `Mystira.Shared.GraphTheory` |
| `Algorithms/PathAlgorithms.cs` | `Mystira.Shared.GraphTheory.Algorithms` |
| `Algorithms/SearchAlgorithms.cs` | `Mystira.Shared.GraphTheory.Algorithms` |
| `Algorithms/SortAlgorithms.cs` | `Mystira.Shared.GraphTheory.Algorithms` |
| `DataFlowAnalysis/DataFlowNode.cs` | `Mystira.Shared.GraphTheory.DataFlow` |
| `DataFlowAnalysis/DataFlowAnalysis.cs` | `Mystira.Shared.GraphTheory.DataFlow` |
| `FrontierMergedGraph/SceneStateNode.cs` | `Mystira.Shared.GraphTheory.StateSpace` |
| `FrontierMergedGraph/FrontierMergedGraphBuilder.cs` | `Mystira.Shared.GraphTheory.StateSpace` |
| `FrontierMergedGraph/FrontierMergedGraphResult.cs` | `Mystira.Shared.GraphTheory.StateSpace` |
| `FrontierMergedGraph/SceneTransition.cs` | `Mystira.Shared.GraphTheory.StateSpace` |

#### From `Mystira.StoryGenerator.Domain/Graph/`

**Graph Interfaces**
| Source | Target Namespace |
|--------|------------------|
| `IGraph.cs` | `Mystira.Shared.GraphTheory` |
| `IDirectedGraph.cs` | `Mystira.Shared.GraphTheory` |
| `IEdge.cs` | `Mystira.Shared.GraphTheory` |

### Add to Existing Project
```
packages/shared/Mystira.Shared/
├── Mystira.Shared.csproj          (existing)
├── GraphTheory/                   ← NEW
│   ├── IGraph.cs
│   ├── IDirectedGraph.cs
│   ├── IEdge.cs
│   ├── DirectedGraph.cs
│   ├── Edge.cs
│   ├── Algorithms/
│   │   ├── PathAlgorithms.cs
│   │   ├── SearchAlgorithms.cs
│   │   └── SortAlgorithms.cs
│   ├── DataFlow/
│   │   ├── DataFlowNode.cs
│   │   └── DataFlowAnalysis.cs
│   └── StateSpace/
│       ├── SceneStateNode.cs
│       ├── FrontierMergedGraphBuilder.cs
│       ├── FrontierMergedGraphResult.cs
│       └── SceneTransition.cs
└── ... (existing files)
```

---

## Package 3: Mystira.Authoring.Abstractions

**Purpose**: Interfaces, commands, and POCOs for story authoring. Minimal dependencies.

### Files to Include

#### From `Mystira.StoryGenerator.Domain/`

**Stories (POCOs)**
| Source | Target Namespace |
|--------|------------------|
| `Stories/Scenario.cs` | `Mystira.Authoring.Abstractions.Stories` |
| `Stories/SceneExtensions.cs` | `Mystira.Authoring.Abstractions.Stories` |
| `Stories/ScenarioExtensions.cs` | `Mystira.Authoring.Abstractions.Stories` |
| `Stories/ScenarioDominatorPathAnalysis.cs` | `Mystira.Authoring.Abstractions.Stories` |
| `Stories/StoryContinuityIssue.cs` | `Mystira.Authoring.Abstractions.Stories` |
| `Stories/StoryContinuityAsyncContracts.cs` | `Mystira.Authoring.Abstractions.Stories` |

**Story-Specific Graph (uses generic graph)**
| Source | Target Namespace |
|--------|------------------|
| `Graph/IScenarioGraph.cs` | `Mystira.Authoring.Abstractions.Graph` |
| `Graph/SceneEdge.cs` | `Mystira.Authoring.Abstractions.Graph` |

**Commands**
| Source | Target Namespace |
|--------|------------------|
| `Commands/ICommand.cs` | `Mystira.Authoring.Abstractions.Commands` |
| `Commands/ICommandHandler.cs` | `Mystira.Authoring.Abstractions.Commands` |
| `Commands/Stories/*.cs` | `Mystira.Authoring.Abstractions.Commands.Stories` |
| `Commands/Chat/*.cs` | `Mystira.Authoring.Abstractions.Commands.Chat` |

**Service Interfaces**
| Source | Target Namespace |
|--------|------------------|
| `Services/IChatOrchestrationService.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/IScenarioFactory.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/IStoryValidationService.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/IStoryContinuityService.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/IScenarioConsistencyEvaluationService.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/IScenarioEntityConsistencyEvaluationService.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/IScenarioDominatorPathConsistencyEvaluationService.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/IScenarioSrlAnalysisService.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/IPrefixSummaryService.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/IStorySchemaProvider.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/ICommandRouter.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/IInstructionBlockService.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/ChatContext.cs` | `Mystira.Authoring.Abstractions.Services` |
| `Services/StoryConsistencyEvaluation.cs` | `Mystira.Authoring.Abstractions.Services` |

**LLM Service Interfaces (story-specific)**
| Source | Target Namespace |
|--------|------------------|
| `Services/ILlmClassificationService.cs` | `Mystira.Authoring.Abstractions.Llm` |
| `Services/ILlmIntentLlmClassificationService.cs` | `Mystira.Authoring.Abstractions.Llm` |
| `Services/IEntityLlmClassificationService.cs` | `Mystira.Authoring.Abstractions.Llm` |
| `Services/ISemanticRoleLabellingLlmService.cs` | `Mystira.Authoring.Abstractions.Llm` |
| `Services/IPrefixSummaryLlmService.cs` | `Mystira.Authoring.Abstractions.Llm` |
| `Services/IDominatorPathConsistencyLlmService.cs` | `Mystira.Authoring.Abstractions.Llm` |

### Project Structure
```
packages/authoring/Mystira.Authoring.Abstractions/
├── Mystira.Authoring.Abstractions.csproj
├── Stories/
│   ├── Scenario.cs
│   ├── Scene.cs
│   ├── Branch.cs
│   └── ...
├── Graph/
│   ├── IScenarioGraph.cs
│   └── SceneEdge.cs
├── Commands/
│   ├── ICommand.cs
│   ├── ICommandHandler.cs
│   ├── Stories/
│   └── Chat/
├── Services/
│   ├── IChatOrchestrationService.cs
│   ├── IScenarioFactory.cs
│   └── ...
└── Llm/
    ├── ILlmClassificationService.cs
    └── ...
```

### Dependencies (minimal!)
```xml
<ItemGroup>
  <ProjectReference Include="../../shared/Mystira.Shared/Mystira.Shared.csproj" />
  <ProjectReference Include="../../contracts/dotnet/Mystira.Contracts/Mystira.Contracts.csproj" />
</ItemGroup>
```

---

## Package 4: Mystira.Authoring

**Purpose**: Implementations, handlers, and services for story authoring.

### Files to Include

#### From `Mystira.StoryGenerator.Application/`

**Handlers**
| Source | Target Namespace |
|--------|------------------|
| `Handlers/Stories/*.cs` | `Mystira.Authoring.Handlers.Stories` |
| `Handlers/Chat/*.cs` | `Mystira.Authoring.Handlers.Chat` |

**Services**
| Source | Target Namespace |
|--------|------------------|
| `Services/ChatOrchestrationService.cs` | `Mystira.Authoring.Services` |
| `Services/ScenarioContinuityService.cs` | `Mystira.Authoring.Services` |
| `Services/ScenarioEntityConsistencyEvaluationService.cs` | `Mystira.Authoring.Services` |
| `Services/ScenarioDominatorPathConsistencyEvaluationService.cs` | `Mystira.Authoring.Services` |
| `Services/ScenarioPrefixSummaryService.cs` | `Mystira.Authoring.Services` |
| `Services/ScenarioSrlAnalysisService.cs` | `Mystira.Authoring.Services` |

**Scenarios**
| Source | Target Namespace |
|--------|------------------|
| `Scenarios/ScenarioFactory.cs` | `Mystira.Authoring.Scenarios` |
| `Scenarios/ScenarioGraph.cs` | `Mystira.Authoring.Scenarios` |
| `Scenarios/ScenarioExtensions.cs` | `Mystira.Authoring.Scenarios` |

**Story Consistency Analysis**
| Source | Target Namespace |
|--------|------------------|
| `StoryConsistencyAnalysis/SceneNode.cs` | `Mystira.Authoring.Analysis` |
| `StoryConsistencyAnalysis/EntityConsistency/*.cs` | `Mystira.Authoring.Analysis.EntityConsistency` |
| `StoryConsistencyAnalysis/ContinuityAnalyzer/*.cs` | `Mystira.Authoring.Analysis.Continuity` |
| `StoryConsistencyAnalysis/PrefixSummary/*.cs` | `Mystira.Authoring.Analysis.PrefixSummary` |
| `StoryConsistencyAnalysis/Legacy/*.cs` | `Mystira.Authoring.Analysis.Legacy` |

**Utilities**
| Source | Target Namespace |
|--------|------------------|
| `Utilities/StoryTextSanitizer.cs` | `Mystira.Authoring.Utilities` |

#### From `Mystira.StoryGenerator.Llm/`

**Story-Specific LLM Services**
| Source | Target Namespace |
|--------|------------------|
| `Services/ConsistencyEvaluators/*.cs` | `Mystira.Authoring.Llm.ConsistencyEvaluators` |
| `Services/StoryInstructionsRag/*.cs` | `Mystira.Authoring.Llm.Rag` |
| `Services/StoryIntentClassification/*.cs` | `Mystira.Authoring.Llm.IntentClassification` |

#### From `Mystira.StoryGenerator.RagIndexer/`
| Source | Target Namespace |
|--------|------------------|
| All files | `Mystira.Authoring.Rag` |

### Project Structure
```
packages/authoring/Mystira.Authoring/
├── Mystira.Authoring.csproj
├── Handlers/
│   ├── Stories/
│   │   ├── GenerateStoryCommandHandler.cs
│   │   ├── ValidateStoryCommandHandler.cs
│   │   └── ...
│   └── Chat/
│       ├── FreeTextCommandHandler.cs
│       └── ...
├── Services/
│   ├── ChatOrchestrationService.cs
│   ├── ScenarioFactory.cs
│   └── ...
├── Scenarios/
│   ├── ScenarioFactory.cs
│   ├── ScenarioGraph.cs
│   └── ScenarioExtensions.cs
├── Analysis/
│   ├── SceneNode.cs
│   ├── EntityConsistency/
│   ├── Continuity/
│   └── PrefixSummary/
├── Llm/
│   ├── ConsistencyEvaluators/
│   ├── Rag/
│   └── IntentClassification/
├── Rag/
│   └── ... (from RagIndexer)
├── Utilities/
│   └── StoryTextSanitizer.cs
└── DependencyInjection.cs
```

### Dependencies
```xml
<ItemGroup>
  <PackageReference Include="MediatR" Version="12.1.1" />
  <PackageReference Include="NJsonSchema" Version="11.5.1" />
  <ProjectReference Include="../Mystira.Authoring.Abstractions/Mystira.Authoring.Abstractions.csproj" />
  <ProjectReference Include="../../ai/Mystira.Ai/Mystira.Ai.csproj" />
</ItemGroup>
```

---

## Package to Delete: Mystira.StoryGenerator.Contracts

**Reason**: Already duplicated in `Mystira.Contracts.StoryGenerator.*`

All files in this package have exact copies in the main Contracts package. Delete entirely and update references.

---

## Namespace Mapping Summary

| Old Namespace | New Namespace |
|---------------|---------------|
| `Mystira.StoryGenerator.Contracts.*` | `Mystira.Contracts.StoryGenerator.*` |
| `Mystira.StoryGenerator.Domain.Stories` | `Mystira.Authoring.Abstractions.Stories` |
| `Mystira.StoryGenerator.Domain.Services` (interfaces) | `Mystira.Authoring.Abstractions.Services` |
| `Mystira.StoryGenerator.Domain.Services` (ILLMService) | `Mystira.Ai.Abstractions` |
| `Mystira.StoryGenerator.Domain.Graph` (generic) | `Mystira.Shared.GraphTheory` |
| `Mystira.StoryGenerator.Domain.Graph` (story-specific) | `Mystira.Authoring.Abstractions.Graph` |
| `Mystira.StoryGenerator.Domain.Commands` | `Mystira.Authoring.Abstractions.Commands` |
| `Mystira.StoryGenerator.GraphTheory.Graph` | `Mystira.Shared.GraphTheory` |
| `Mystira.StoryGenerator.GraphTheory.Algorithms` | `Mystira.Shared.GraphTheory.Algorithms` |
| `Mystira.StoryGenerator.GraphTheory.DataFlowAnalysis` | `Mystira.Shared.GraphTheory.DataFlow` |
| `Mystira.StoryGenerator.GraphTheory.FrontierMergedGraph` | `Mystira.Shared.GraphTheory.StateSpace` |
| `Mystira.StoryGenerator.Application.Services` | `Mystira.Authoring.Services` |
| `Mystira.StoryGenerator.Application.Handlers` | `Mystira.Authoring.Handlers` |
| `Mystira.StoryGenerator.Llm.Services.LLM` | `Mystira.Ai.Providers` |
| `Mystira.StoryGenerator.Llm.Services.ConsistencyEvaluators` | `Mystira.Authoring.Llm.ConsistencyEvaluators` |

---

## Migration Steps

### Phase 1: Create New Packages (No Breaking Changes)

1. **Add GraphTheory to `Mystira.Shared`**
   ```bash
   mkdir -p packages/shared/Mystira.Shared/GraphTheory
   ```
   - Copy graph interfaces and implementations
   - Update namespaces to `Mystira.Shared.GraphTheory`

2. **Create `Mystira.Ai` package**
   ```bash
   mkdir -p packages/ai/Mystira.Ai
   ```
   - Copy LLM provider files with new namespaces
   - Add project references to `Mystira.Contracts`
   - Create `DependencyInjection.cs` for service registration

3. **Create `Mystira.Authoring.Abstractions` package**
   ```bash
   mkdir -p packages/authoring/Mystira.Authoring.Abstractions
   ```
   - Copy interfaces, commands, POCOs
   - Add project references to `Mystira.Shared` and `Mystira.Contracts`
   - Keep dependencies minimal

4. **Create `Mystira.Authoring` package**
   ```bash
   mkdir -p packages/authoring/Mystira.Authoring
   ```
   - Copy handlers, services, implementations
   - Add project references to `Mystira.Authoring.Abstractions` and `Mystira.Ai`
   - Create `DependencyInjection.cs` for service registration

5. **Publish new packages**
   - Add to CI/CD pipeline
   - Publish in order: `Mystira.Shared` → `Mystira.Ai` → `Mystira.Authoring.Abstractions` → `Mystira.Authoring`

### Phase 2: Migrate StoryGenerator Submodule

1. **Update `Mystira.StoryGenerator.Api`**
   - Replace project references with package references:
     ```xml
     <PackageReference Include="Mystira.Shared" Version="1.0.0" />
     <PackageReference Include="Mystira.Ai" Version="1.0.0" />
     <PackageReference Include="Mystira.Authoring" Version="1.0.0" />
     <PackageReference Include="Mystira.Contracts" Version="1.0.0" />
     ```
   - Update `using` statements to new namespaces
   - Remove references to deleted local projects

2. **Update other executables** (Console, Llm.Console, Web)
   - Same process as Api

3. **Delete old packages from submodule**
   - `Mystira.StoryGenerator.Contracts` (use `Mystira.Contracts`)
   - `Mystira.StoryGenerator.Domain` (split into `Mystira.Authoring.Abstractions` + `Mystira.Ai`)
   - `Mystira.StoryGenerator.Application` (migrated to `Mystira.Authoring`)
   - `Mystira.StoryGenerator.Llm` (split into `Mystira.Ai` + `Mystira.Authoring`)
   - `Mystira.StoryGenerator.GraphTheory` (migrated to `Mystira.Shared.GraphTheory`)
   - `Mystira.StoryGenerator.RagIndexer` (migrated to `Mystira.Authoring`)

4. **Upgrade to .NET 9.0**
   - Update `TargetFramework` in remaining projects

### Phase 3: Update Consumers

1. **Update `admin-api`**
   ```xml
   <!-- For full authoring capabilities -->
   <PackageReference Include="Mystira.Authoring" Version="1.0.0" />

   <!-- Or just abstractions if only using interfaces -->
   <PackageReference Include="Mystira.Authoring.Abstractions" Version="1.0.0" />
   ```

2. **Update `publisher`** (if .NET backend exists)
   - Add package references as needed

### Phase 4: Cleanup

1. Remove deprecated NuGet packages from feed:
   - `Mystira.StoryGenerator.Contracts`
   - `Mystira.StoryGenerator.Domain`
   - `Mystira.StoryGenerator.Application`
   - `Mystira.StoryGenerator.Llm`
   - `Mystira.StoryGenerator.GraphTheory`
   - `Mystira.StoryGenerator.RagIndexer`

2. Clean up solution files and project references

### Phase 5: Documentation & Tests

1. **XML Documentation Comments**
   - Add `<summary>` to all public types and members
   - Add `<param>` and `<returns>` to all public methods
   - Add `<remarks>` for complex algorithms (especially GraphTheory)
   - Add `<example>` blocks for key APIs
   - Enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in all .csproj files
   - Enable `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` for CS1591 (missing XML comments)

2. **README Documentation**
   - `packages/ai/README.md` - Mystira.Ai usage guide
   - `packages/authoring/README.md` - Mystira.Authoring usage guide
   - `packages/shared/Mystira.Shared/GraphTheory/README.md` - Graph algorithms guide
   - Update root `README.md` with new package structure

3. **Unit Tests**
   ```
   packages/
   ├── ai/
   │   └── Mystira.Ai.Tests/
   │       ├── Providers/
   │       │   ├── AzureOpenAIServiceTests.cs
   │       │   └── LLMServiceFactoryTests.cs
   │       └── RateLimiting/
   │           └── PerMinuteRateLimiterTests.cs
   ├── authoring/
   │   ├── Mystira.Authoring.Abstractions.Tests/
   │   │   ├── Stories/
   │   │   │   └── ScenarioTests.cs
   │   │   └── Commands/
   │   │       └── CommandTests.cs
   │   └── Mystira.Authoring.Tests/
   │       ├── Handlers/
   │       │   ├── GenerateStoryCommandHandlerTests.cs
   │       │   └── ValidateStoryCommandHandlerTests.cs
   │       ├── Services/
   │       │   └── ChatOrchestrationServiceTests.cs
   │       └── Analysis/
   │           ├── EntityContinuityAnalyzerTests.cs
   │           └── ScenarioConsistencyTests.cs
   └── shared/
       └── Mystira.Shared.Tests/
           └── GraphTheory/
               ├── DirectedGraphTests.cs
               ├── Algorithms/
               │   ├── PathAlgorithmsTests.cs
               │   ├── SearchAlgorithmsTests.cs
               │   └── SortAlgorithmsTests.cs
               └── StateSpace/
                   └── FrontierMergedGraphTests.cs
   ```

4. **Integration Tests**
   - LLM provider integration tests (with test doubles)
   - End-to-end story generation tests
   - Consistency analysis integration tests

5. **Test Coverage Targets**
   | Package | Target Coverage |
   |---------|-----------------|
   | `Mystira.Shared.GraphTheory` | 90%+ (algorithms are critical) |
   | `Mystira.Ai` | 80%+ |
   | `Mystira.Authoring.Abstractions` | 70%+ (mostly POCOs) |
   | `Mystira.Authoring` | 85%+ |

### Phase 6: CI/CD Integration

1. **GitHub Actions Workflows**
   ```yaml
   # .github/workflows/packages-ai.yml
   name: Mystira.Ai CI/CD
   on:
     push:
       paths:
         - 'packages/ai/**'
     pull_request:
       paths:
         - 'packages/ai/**'
   jobs:
     build-test-publish:
       # Build, test, and conditionally publish
   ```

2. **Workflow Files to Create**
   | Workflow | Trigger Paths | Purpose |
   |----------|---------------|---------|
   | `packages-shared.yml` | `packages/shared/**` | Build/test Mystira.Shared |
   | `packages-ai.yml` | `packages/ai/**` | Build/test Mystira.Ai |
   | `packages-authoring.yml` | `packages/authoring/**` | Build/test Mystira.Authoring.* |
   | `packages-publish.yml` | Tags `v*` | Publish all packages to NuGet |

3. **Build Pipeline Steps**
   ```yaml
   steps:
     - name: Checkout
       uses: actions/checkout@v4

     - name: Setup .NET
       uses: actions/setup-dotnet@v4
       with:
         dotnet-version: '9.0.x'

     - name: Restore
       run: dotnet restore

     - name: Build
       run: dotnet build --no-restore --configuration Release

     - name: Test
       run: dotnet test --no-build --configuration Release --collect:"XPlat Code Coverage"

     - name: Upload Coverage
       uses: codecov/codecov-action@v4

     - name: Pack (on tag)
       if: startsWith(github.ref, 'refs/tags/v')
       run: dotnet pack --no-build --configuration Release -o ./nupkg

     - name: Publish (on tag)
       if: startsWith(github.ref, 'refs/tags/v')
       run: dotnet nuget push ./nupkg/*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json
   ```

4. **Package Versioning Strategy**
   - Use GitVersion or MinVer for automatic versioning
   - Version format: `{major}.{minor}.{patch}[-{prerelease}]`
   - Pre-release tags: `-alpha`, `-beta`, `-rc`
   - Example: `1.0.0-alpha.1`, `1.0.0-beta.2`, `1.0.0`

5. **Dependency Build Order**
   ```
   1. Mystira.Shared        (no deps)
   2. Mystira.Contracts     (no deps)
   3. Mystira.Ai            (depends on Contracts)
   4. Mystira.Authoring.Abstractions (depends on Shared, Contracts)
   5. Mystira.Authoring     (depends on Abstractions, Ai)
   ```

6. **Quality Gates**
   | Gate | Requirement |
   |------|-------------|
   | Build | Must pass |
   | Unit Tests | Must pass |
   | Code Coverage | Must meet target thresholds |
   | XML Docs | No CS1591 warnings |
   | Security Scan | No high/critical vulnerabilities |

7. **NuGet Package Metadata**
   ```xml
   <PropertyGroup>
     <PackageId>Mystira.Ai</PackageId>
     <Version>1.0.0</Version>
     <Authors>Phoenix VC</Authors>
     <Company>Mystira</Company>
     <PackageProjectUrl>https://github.com/phoenixvc/Mystira.workspace</PackageProjectUrl>
     <RepositoryUrl>https://github.com/phoenixvc/Mystira.workspace</RepositoryUrl>
     <PackageTags>ai;llm;openai;anthropic</PackageTags>
     <PackageLicenseExpression>MIT</PackageLicenseExpression>
     <PackageReadmeFile>README.md</PackageReadmeFile>
     <GenerateDocumentationFile>true</GenerateDocumentationFile>
   </PropertyGroup>
   ```

---

## Breaking Changes

| Change | Impact | Mitigation |
|--------|--------|------------|
| Namespace changes | All consumers | Search & replace using statements |
| Package renames | NuGet references | Update package references |
| .NET 8 → 9 upgrade | StoryGenerator submodule | Required for compatibility |
| `ILLMService` moved to `Mystira.Ai` | LLM consumers | Reference `Mystira.Ai` package |
| Graph interfaces moved to `Mystira.Shared` | Graph consumers | Reference `Mystira.Shared` package |
| Abstractions split | Existing references | Reference `Mystira.Authoring.Abstractions` for interfaces |

---

## Benefits After Migration

1. **Reusability**: `admin-api` and `publisher` can use shared packages
2. **No duplication**: Single source of truth for contracts and services
3. **Clear boundaries**:
   - `Mystira.Ai` → Generic LLM infrastructure
   - `Mystira.Authoring.Abstractions` → Interfaces only (light dependency)
   - `Mystira.Authoring` → Full implementations
   - `Mystira.Shared.GraphTheory` → Reusable algorithms
4. **Consistent versioning**: All packages on .NET 9.0
5. **Flexible consumption**: Consumers can reference just abstractions or full implementations
6. **Future-proof**: Abstractions pattern allows independent evolution

---

## Estimated Effort

| Phase | Task | Complexity | Estimate |
|-------|------|------------|----------|
| **1** | Add GraphTheory to Mystira.Shared | Low | ~11 files |
| **1** | Create Mystira.Ai | Medium | ~6 files |
| **1** | Create Mystira.Authoring.Abstractions | Medium | ~25 files |
| **1** | Create Mystira.Authoring | High | ~35 files |
| **2** | Update StoryGenerator submodule | Medium | Namespace updates |
| **3** | Update admin-api | Low | Package refs only |
| **4** | Cleanup deprecated packages | Low | Remove from feed |
| **5** | XML documentation comments | Medium | All public APIs |
| **5** | README documentation | Low | 4 README files |
| **5** | Unit tests | High | ~50 test files |
| **5** | Integration tests | Medium | ~10 test files |
| **6** | CI/CD workflows | Medium | 4 workflow files |
| **6** | Quality gates setup | Low | Config files |

---

## Final Package Summary

| Package | Purpose | Dependencies |
|---------|---------|--------------|
| `Mystira.Shared` | Core utilities + GraphTheory | None |
| `Mystira.Contracts` | API contracts (existing) | None |
| `Mystira.Ai` | LLM providers, rate limiting | Contracts |
| `Mystira.Authoring.Abstractions` | Interfaces, commands, POCOs | Shared, Contracts |
| `Mystira.Authoring` | Implementations, handlers | Abstractions, Ai |
