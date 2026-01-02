# StoryGenerator Package Consolidation Migration Plan

## Overview

This document outlines the migration of `Mystira.StoryGenerator.*` packages into the main workspace as shared libraries that can be consumed by `admin-api`, `publisher`, and other services.

## Current State

```
packages/story-generator/src/
├── Mystira.StoryGenerator.Contracts/    → DELETE (duplicated in Mystira.Contracts)
├── Mystira.StoryGenerator.Domain/       → Split into Mystira.Ai + Mystira.Authoring
├── Mystira.StoryGenerator.Application/  → Migrate to Mystira.Authoring
├── Mystira.StoryGenerator.Llm/          → Migrate to Mystira.Ai
├── Mystira.StoryGenerator.GraphTheory/  → Migrate to Mystira.Authoring
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
│   └── Mystira.Ai/                      ← Generic LLM infrastructure
├── authoring/
│   └── Mystira.Authoring/               ← Story generation & analysis
├── contracts/
│   └── Mystira.Contracts/               ← Already has StoryGenerator/* (keep)
└── story-generator/                     ← Submodule references new packages
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

## Package 2: Mystira.Authoring

**Purpose**: Story generation, validation, and consistency analysis.

### Files to Include

#### From `Mystira.StoryGenerator.Domain/`

**Stories (POCOs)**
| Source | Target Namespace |
|--------|------------------|
| `Stories/Scenario.cs` | `Mystira.Authoring.Stories` |
| `Stories/SceneExtensions.cs` | `Mystira.Authoring.Stories` |
| `Stories/ScenarioExtensions.cs` | `Mystira.Authoring.Stories` |
| `Stories/ScenarioDominatorPathAnalysis.cs` | `Mystira.Authoring.Stories` |
| `Stories/StoryContinuityIssue.cs` | `Mystira.Authoring.Stories` |
| `Stories/StoryContinuityAsyncContracts.cs` | `Mystira.Authoring.Stories` |

**Graph**
| Source | Target Namespace |
|--------|------------------|
| `Graph/IGraph.cs` | `Mystira.Authoring.Graph` |
| `Graph/IDirectedGraph.cs` | `Mystira.Authoring.Graph` |
| `Graph/IEdge.cs` | `Mystira.Authoring.Graph` |
| `Graph/IScenarioGraph.cs` | `Mystira.Authoring.Graph` |
| `Graph/SceneEdge.cs` | `Mystira.Authoring.Graph` |

**Commands**
| Source | Target Namespace |
|--------|------------------|
| `Commands/ICommand.cs` | `Mystira.Authoring.Commands` |
| `Commands/ICommandHandler.cs` | `Mystira.Authoring.Commands` |
| `Commands/Stories/*.cs` | `Mystira.Authoring.Commands.Stories` |
| `Commands/Chat/*.cs` | `Mystira.Authoring.Commands.Chat` |

**Service Interfaces**
| Source | Target Namespace |
|--------|------------------|
| `Services/IChatOrchestrationService.cs` | `Mystira.Authoring.Services` |
| `Services/IScenarioFactory.cs` | `Mystira.Authoring.Services` |
| `Services/IStoryValidationService.cs` | `Mystira.Authoring.Services` |
| `Services/IStoryContinuityService.cs` | `Mystira.Authoring.Services` |
| `Services/IScenarioConsistencyEvaluationService.cs` | `Mystira.Authoring.Services` |
| `Services/IScenarioEntityConsistencyEvaluationService.cs` | `Mystira.Authoring.Services` |
| `Services/IScenarioDominatorPathConsistencyEvaluationService.cs` | `Mystira.Authoring.Services` |
| `Services/IScenarioSrlAnalysisService.cs` | `Mystira.Authoring.Services` |
| `Services/IPrefixSummaryService.cs` | `Mystira.Authoring.Services` |
| `Services/IStorySchemaProvider.cs` | `Mystira.Authoring.Services` |
| `Services/ICommandRouter.cs` | `Mystira.Authoring.Services` |
| `Services/IInstructionBlockService.cs` | `Mystira.Authoring.Services` |
| `Services/ChatContext.cs` | `Mystira.Authoring.Services` |
| `Services/StoryConsistencyEvaluation.cs` | `Mystira.Authoring.Services` |

**LLM Service Interfaces (story-specific)**
| Source | Target Namespace |
|--------|------------------|
| `Services/ILlmClassificationService.cs` | `Mystira.Authoring.Llm` |
| `Services/ILlmIntentLlmClassificationService.cs` | `Mystira.Authoring.Llm` |
| `Services/IEntityLlmClassificationService.cs` | `Mystira.Authoring.Llm` |
| `Services/ISemanticRoleLabellingLlmService.cs` | `Mystira.Authoring.Llm` |
| `Services/IPrefixSummaryLlmService.cs` | `Mystira.Authoring.Llm` |
| `Services/IDominatorPathConsistencyLlmService.cs` | `Mystira.Authoring.Llm` |

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

#### From `Mystira.StoryGenerator.GraphTheory/`
| Source | Target Namespace |
|--------|------------------|
| All files | `Mystira.Authoring.GraphTheory` |

#### From `Mystira.StoryGenerator.RagIndexer/`
| Source | Target Namespace |
|--------|------------------|
| All files | `Mystira.Authoring.Rag` |

### Project Structure
```
packages/authoring/Mystira.Authoring/
├── Mystira.Authoring.csproj
├── Stories/
│   ├── Scenario.cs
│   ├── Scene.cs
│   ├── Branch.cs
│   └── ...
├── Graph/
│   ├── IGraph.cs
│   ├── IScenarioGraph.cs
│   └── ...
├── Commands/
│   ├── ICommand.cs
│   ├── Stories/
│   └── Chat/
├── Handlers/
│   ├── Stories/
│   └── Chat/
├── Services/
│   ├── ChatOrchestrationService.cs
│   ├── ScenarioFactory.cs
│   └── ...
├── Analysis/
│   ├── EntityConsistency/
│   ├── Continuity/
│   └── PrefixSummary/
├── Llm/
│   ├── ConsistencyEvaluators/
│   ├── Rag/
│   └── IntentClassification/
├── GraphTheory/
│   └── ...
├── Rag/
│   └── ...
├── Utilities/
│   └── StoryTextSanitizer.cs
└── DependencyInjection.cs
```

### Dependencies
```xml
<ItemGroup>
  <PackageReference Include="MediatR" Version="12.1.1" />
  <PackageReference Include="NJsonSchema" Version="11.5.1" />
  <ProjectReference Include="../../ai/Mystira.Ai/Mystira.Ai.csproj" />
  <ProjectReference Include="../../contracts/dotnet/Mystira.Contracts/Mystira.Contracts.csproj" />
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
| `Mystira.StoryGenerator.Contracts.Chat` | `Mystira.Contracts.StoryGenerator.Chat` |
| `Mystira.StoryGenerator.Contracts.Stories` | `Mystira.Contracts.StoryGenerator.Stories` |
| `Mystira.StoryGenerator.Contracts.Configuration` | `Mystira.Contracts.StoryGenerator.Configuration` |
| `Mystira.StoryGenerator.Contracts.StoryConsistency` | `Mystira.Contracts.StoryGenerator.StoryConsistency` |
| `Mystira.StoryGenerator.Contracts.Intent` | `Mystira.Contracts.StoryGenerator.Intent` |
| `Mystira.StoryGenerator.Contracts.Entities` | `Mystira.Contracts.StoryGenerator.Entities` |
| `Mystira.StoryGenerator.Domain.Stories` | `Mystira.Authoring.Stories` |
| `Mystira.StoryGenerator.Domain.Services` | `Mystira.Authoring.Services` or `Mystira.Ai.Abstractions` |
| `Mystira.StoryGenerator.Domain.Graph` | `Mystira.Authoring.Graph` |
| `Mystira.StoryGenerator.Domain.Commands` | `Mystira.Authoring.Commands` |
| `Mystira.StoryGenerator.Application.Services` | `Mystira.Authoring.Services` |
| `Mystira.StoryGenerator.Application.Handlers` | `Mystira.Authoring.Handlers` |
| `Mystira.StoryGenerator.Llm.Services.LLM` | `Mystira.Ai.Providers` |

---

## Migration Steps

### Phase 1: Create New Packages (No Breaking Changes)

1. **Create `Mystira.Ai` package**
   ```bash
   mkdir -p packages/ai/Mystira.Ai
   ```
   - Copy LLM provider files with new namespaces
   - Add project references to `Mystira.Contracts`
   - Create `DependencyInjection.cs` for service registration

2. **Create `Mystira.Authoring` package**
   ```bash
   mkdir -p packages/authoring/Mystira.Authoring
   ```
   - Copy story/command/handler files with new namespaces
   - Add project references to `Mystira.Ai` and `Mystira.Contracts`
   - Create `DependencyInjection.cs` for service registration

3. **Publish new packages**
   - Add to CI/CD pipeline
   - Publish `Mystira.Ai` and `Mystira.Authoring` to NuGet feed

### Phase 2: Migrate StoryGenerator Submodule

1. **Update `Mystira.StoryGenerator.Api`**
   - Replace project references with package references:
     ```xml
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
   - `Mystira.StoryGenerator.Domain` (migrated to `Mystira.Authoring`)
   - `Mystira.StoryGenerator.Application` (migrated to `Mystira.Authoring`)
   - `Mystira.StoryGenerator.Llm` (split into `Mystira.Ai` + `Mystira.Authoring`)
   - `Mystira.StoryGenerator.GraphTheory` (migrated to `Mystira.Authoring`)
   - `Mystira.StoryGenerator.RagIndexer` (migrated to `Mystira.Authoring`)

4. **Upgrade to .NET 9.0**
   - Update `TargetFramework` in remaining projects

### Phase 3: Update Consumers

1. **Update `admin-api`**
   ```xml
   <PackageReference Include="Mystira.Ai" Version="1.0.0" />
   <PackageReference Include="Mystira.Authoring" Version="1.0.0" />
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

2. Update documentation

---

## Breaking Changes

| Change | Impact | Mitigation |
|--------|--------|------------|
| Namespace changes | All consumers | Search & replace using statements |
| Package renames | NuGet references | Update package references |
| .NET 8 → 9 upgrade | StoryGenerator submodule | Required for compatibility |
| `ILLMService` moved to `Mystira.Ai` | LLM consumers | Reference `Mystira.Ai` package |

---

## Benefits After Migration

1. **Reusability**: `admin-api` and `publisher` can use `Mystira.Ai` and `Mystira.Authoring`
2. **No duplication**: Single source of truth for contracts and services
3. **Clear boundaries**: AI infrastructure vs. story-specific logic
4. **Consistent versioning**: All packages on .NET 9.0
5. **Simpler dependency graph**: Fewer packages to manage

---

## Estimated Effort

| Task | Complexity |
|------|------------|
| Create Mystira.Ai | Medium |
| Create Mystira.Authoring | High (many files) |
| Update StoryGenerator submodule | Medium |
| Update admin-api | Low |
| Testing & validation | Medium |

---

## Questions to Resolve

1. Should `Mystira.Authoring` be split further (e.g., `Mystira.Authoring.Analysis`)?
2. Should graph theory utilities move to `Mystira.Shared` or stay in `Mystira.Authoring`?
3. NuGet package naming: `Mystira.Ai` or `Mystira.App.Ai` for consistency?
