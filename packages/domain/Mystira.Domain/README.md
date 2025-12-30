# Mystira.Domain

Core domain types for the Mystira platform. This package contains shared primitives, entities, and value objects used across Contracts, Shared, and application layers.

## Installation

```bash
dotnet add package Mystira.Domain --version 0.5.0-alpha
```

## Contents

### Enums

**Scenario Enums** (`Mystira.Domain.Enums`)
- `DifficultyLevel` - Scenario difficulty (Easy, Medium, Hard, Expert)
- `SessionLength` - Session duration (Quick, Short, Medium, Long, Extended)
- `ScenarioGameState` - Game state (NotStarted, InProgress, Paused, Completed, Abandoned)
- `PublicationStatus` - Publication state (Draft, UnderReview, Published, Archived, Rejected)
- `SceneType` - Scene types (Standard, Intro, Decision, Action, Dialogue, Puzzle, etc.)

**Game Session Enums**
- `SessionStatus` - Session state (Creating, Pending, Active, Paused, Completed, Abandoned, Failed, Expired)
- `AchievementType` - Achievement types (ScenarioCompletion, StoryChoice, CompassMilestone, etc.)
- `SessionEndReason` - How sessions end (Completed, PlayerQuit, Inactivity, Error, etc.)
- `TransactionStatus` - Blockchain transaction state (Pending, Submitted, Confirmed, Failed)

**Account Enums**
- `AccountStatus` - Account state (Active, PendingVerification, Suspended, Deactivated, Deleted)
- `AccountType` - Account type (Free, Premium, Educational, Enterprise)
- `AuthProvider` - Authentication providers (Local, Google, Apple, Microsoft, Passwordless)

**Contributor Enums**
- `ContributorRole` - Contributor roles (Author, Artist, Editor, Writer, Designer, Composer, etc.)
- `ContributorVerificationStatus` - Verification state (Pending, Verified, Rejected, Expired)

**Payment Enums**
- `PaymentStatus` - Payment state (Pending, Processing, Completed, Failed, Refunded, Cancelled, Disputed)
- `PaymentType` - Payment types (Subscription, Purchase, RoyaltyPayout, Refund)
- `PaymentMethod` - Payment methods (Card, BankTransfer, PayPal, ApplePay, GooglePay, Crypto)

### Entity Primitives (`Mystira.Domain.Entities`)

- `EntityId` - ULID-based entity ID generator and validator
- `IEntity<TId>` - Base entity interface
- `IAuditableEntity` - Interface for entities with audit fields
- `ISoftDeletable` - Interface for soft-deletable entities
- `Entity` / `Entity<TId>` - Base entity classes with audit fields
- `SoftDeletableEntity` - Base class for soft-deletable entities

### Value Objects (`Mystira.Domain.ValueObjects`)

- `AgeGroup` - Age group with min/max ages (EarlyChildhood, MiddleChildhood, Preteen, Teen, Adult)
- `CompassAxis` - Moral compass axis (Courage, Kindness, Honesty, etc.)
- `Archetype` - Character archetypes (Hero, Sage, Explorer, Rebel, Magician, etc.)
- `CoreAxis` - Core moral/ethical axes with positive/negative labels
- `EchoType` - Echo types (Memory, Vision, Secret, Emotion, Connection, Warning, etc.)
- `FantasyTheme` - Fantasy themes (HighFantasy, LowFantasy, UrbanFantasy, FairyTale, etc.)

### Primitives (`Mystira.Domain.Primitives`)

- `StringEnum<T>` - Base class for type-safe string-based enumerations

### Serialization (`Mystira.Domain.Serialization`)

- `StringEnumJsonConverter<T>` - JSON converter for StringEnum types
- `StringEnumJsonConverterFactory` - Factory for automatic StringEnum converter creation

### Domain Models (`Mystira.Domain.Models`)

**Core Entities**
- `Account` - User account with authentication, status, and security
- `UserProfile` - User profile with preferences, progress, and settings
- `Scenario` - Playable scenario with scenes, characters, and branches
- `GameSession` - Active or completed game session with choices and achievements
- `ContentBundle` - Bundle of scenarios for purchase/subscription
- `CharacterMap` - Player-to-character mapping in sessions

**Scenario Sub-Models**
- `ScenarioCharacter` - Character in a scenario
- `Scene` - Scene with content, branches, and media
- `Branch` - Choice/branch from a scene
- `MediaReference` - Media asset reference
- `EchoLog` / `EchoReveal` - Echo system models
- `CompassChange` / `CompassTracking` - Compass system models

**Game Session Sub-Models**
- `SessionPlayerAssignment` - Player assignment to session
- `SessionCharacterAssignment` - Character assignment in session
- `SessionChoice` - Choice made during session
- `SessionAchievement` - Achievement earned in session

**Badge System**
- `Badge` - Badge definition
- `BadgeImage` - Badge image variants
- `BadgeConfiguration` - Badge earning criteria
- `BadgeThreshold` - Multi-tier badge thresholds
- `UserBadge` - Badge earned by user
- `AxisAchievement` - Achievement on compass axis

**Contributors & Payments**
- `Contributor` - Content contributor
- `StoryProtocolMetadata` - Story Protocol blockchain metadata
- `RoyaltyPayment` - Royalty payment record

**Media & Configuration**
- `MediaAsset` - Media asset (image, audio, video)
- `AvatarConfiguration` - Avatar options
- `OnboardingStep` - Onboarding flow step

**Progress Tracking**
- `PlayerScenarioScore` - Player's score on a scenario
- `PlayerCompassProgress` - Player's overall compass progress

## Usage

```csharp
using Mystira.Domain.Enums;
using Mystira.Domain.Entities;
using Mystira.Domain.ValueObjects;
using Mystira.Domain.Models;

// Generate a new entity ID
var id = EntityId.NewId();

// Use enums
var difficulty = DifficultyLevel.Medium;
var status = SessionStatus.Active;

// Use value objects
var ageGroup = AgeGroup.ForAge(12); // Returns Preteen
var archetype = Archetype.Hero;

// Create entities
var session = new GameSession
{
    ScenarioId = scenarioId,
    HostPlayerId = playerId,
    Status = SessionStatus.Active
};
```

## Package Hierarchy

```
Mystira.Domain (this package)
    ↓
Mystira.Contracts (references Domain, re-exports enums)
    ↓
Mystira.Shared (references Domain and optionally Contracts)
    ↓
Application Layer (references all above)
```

## Migration from Mystira.App.Domain

If you're migrating from `Mystira.App.Domain`:

1. Replace `Mystira.App.Domain.Models` namespace with `Mystira.Domain.Models`
2. Enums are now in `Mystira.Domain.Enums`
3. Entity base classes are in `Mystira.Domain.Entities`
4. Contracts provides extension methods (`ToDomain()`, `ToContracts()`) for enum conversion

## Related Packages

- [Mystira.Contracts](https://github.com/phoenixvc/Mystira.workspace/tree/main/packages/contracts) - API contracts and DTOs
- [Mystira.Shared](https://github.com/phoenixvc/Mystira.workspace/tree/main/packages/shared) - Shared infrastructure
