# Mystira.Domain

Core domain types for the Mystira platform. This package contains shared primitives used across Contracts, Shared, and application layers.

## Installation

```bash
dotnet add package Mystira.Domain --version 0.5.0-alpha
```

## Contents

### Enums

- `DifficultyLevel` - Scenario difficulty levels (Easy, Medium, Hard, Expert)
- `SessionLength` - Game session durations (Quick, Short, Medium, Long, Extended)
- `ScenarioGameState` - Game state (NotStarted, InProgress, Paused, Completed, Abandoned)
- `AccountStatus` - Account states (Active, Suspended, Deleted)
- `ContributorRole` - Contributor roles (Author, Illustrator, Editor, etc.)
- `PaymentStatus` - Payment states (Pending, Completed, Failed, Refunded)

### Entity Primitives

- `EntityId` - ULID-based entity ID generator and validator
- `IEntity<TId>` - Base entity interface
- `Entity<TId>` - Base entity class with audit fields

### Value Objects

- `AgeGroup` - Age group value object
- `CompassAxis` - Moral compass axis value object

## Usage

```csharp
using Mystira.Domain;
using Mystira.Domain.Enums;
using Mystira.Domain.Entities;

// Generate a new entity ID
var id = EntityId.NewId();

// Validate an ID
if (EntityId.IsValid(someId))
{
    // Process
}

// Use enums
var difficulty = DifficultyLevel.Medium;
var sessionLength = SessionLength.Short;
```

## Package Hierarchy

```
Mystira.Domain (this package)
    ↓
Mystira.Contracts (references Domain)
    ↓
Mystira.Shared (references Domain and optionally Contracts)
```

## Related Packages

- [Mystira.Contracts](https://github.com/phoenixvc/Mystira.workspace/tree/main/packages/contracts) - API contracts and DTOs
- [Mystira.Shared](https://github.com/phoenixvc/Mystira.workspace/tree/main/packages/shared) - Shared infrastructure
