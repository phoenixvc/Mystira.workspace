# @mystira/contracts

Unified API contracts for the Mystira platform.

## Overview

This package consolidates all Mystira API type definitions into a single, well-organized package. It replaces the following deprecated packages:

- `@mystira/app-contracts` → `@mystira/contracts/app`
- `@mystira/story-generator-contracts` → `@mystira/contracts/story-generator`

## Installation

```bash
npm install @mystira/contracts
# or
pnpm add @mystira/contracts
# or
yarn add @mystira/contracts
```

## Usage

### Import Specific Modules

```typescript
import { StoryRequest, StoryResponse } from '@mystira/contracts/app';
import { GeneratorConfig, GeneratorResult } from '@mystira/contracts/story-generator';
```

### Import via Namespaces

```typescript
import { App, StoryGenerator } from '@mystira/contracts';

const request: App.StoryRequest = {
  title: 'My Story',
  content: 'Once upon a time...',
};

const config: StoryGenerator.GeneratorConfig = {
  model: 'gpt-4',
  maxTokens: 2000,
};
```

## Package Structure

```
@mystira/contracts
├── /app                    # App API types
│   ├── ApiRequest
│   ├── ApiResponse
│   ├── ApiError
│   ├── StoryRequest
│   └── StoryResponse
└── /story-generator        # Story Generator API types
    ├── GeneratorConfig
    ├── GeneratorRequest
    ├── GeneratorContext
    ├── GeneratorResult
    └── GeneratorMetadata
```

## Migration from Legacy Packages

See the [Migration Guide](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/guides/contracts-migration.md) for step-by-step instructions.

### Quick Migration

```typescript
// Before
import { StoryRequest } from '@mystira/app-contracts';

// After
import { StoryRequest } from '@mystira/contracts/app';
```

## NuGet Package

The corresponding NuGet package is `Mystira.Contracts`:

```bash
dotnet add package Mystira.Contracts
```

```csharp
using Mystira.Contracts.App;
using Mystira.Contracts.StoryGenerator;
```

## Related Documentation

- [ADR-0020: Package Consolidation Strategy](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/architecture/adr/0020-package-consolidation-strategy.md)
- [Package Releases Guide](https://github.com/phoenixvc/Mystira.workspace/blob/main/docs/guides/package-releases.md)

## License

MIT
