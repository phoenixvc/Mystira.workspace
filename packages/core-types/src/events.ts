/**
 * Domain events for Mystira platform.
 * Mirrors Mystira.Shared.Messaging.Events in C#.
 */

/**
 * Base interface for all domain events.
 */
export interface DomainEvent {
  /** When the event occurred (ISO 8601) */
  occurredAt: string;
}

/**
 * Base interface for integration events (cross-service).
 */
export interface IntegrationEvent extends DomainEvent {
  /** Unique event ID for idempotency */
  eventId: string;
}

// =============================================================================
// Account Events
// =============================================================================

/**
 * Published when a new user account is created.
 */
export interface AccountCreated extends IntegrationEvent {
  type: 'AccountCreated';
  accountId: string;
  email: string;
  provider?: string;
}

/**
 * Published when a user account is updated.
 */
export interface AccountUpdated extends IntegrationEvent {
  type: 'AccountUpdated';
  accountId: string;
  updatedFields: string[];
}

/**
 * Published when a user account is deleted.
 */
export interface AccountDeleted extends IntegrationEvent {
  type: 'AccountDeleted';
  accountId: string;
  isSoftDelete: boolean;
}

// =============================================================================
// Session Events
// =============================================================================

/**
 * Published when a game session is started.
 */
export interface SessionStarted extends IntegrationEvent {
  type: 'SessionStarted';
  sessionId: string;
  accountId: string;
  scenarioId: string;
}

/**
 * Published when a game session is completed.
 */
export interface SessionCompleted extends IntegrationEvent {
  type: 'SessionCompleted';
  sessionId: string;
  accountId: string;
  scenarioId: string;
  durationSeconds: number;
  outcome?: string;
}

/**
 * Published when a session is abandoned.
 */
export interface SessionAbandoned extends IntegrationEvent {
  type: 'SessionAbandoned';
  sessionId: string;
  accountId: string;
  durationSeconds: number;
  lastProgressPoint?: string;
}

// =============================================================================
// Content Events
// =============================================================================

/**
 * Published when a scenario is created.
 */
export interface ScenarioCreated extends IntegrationEvent {
  type: 'ScenarioCreated';
  scenarioId: string;
  title: string;
  authorId: string;
  isPublished: boolean;
}

/**
 * Published when a scenario is updated.
 */
export interface ScenarioUpdated extends IntegrationEvent {
  type: 'ScenarioUpdated';
  scenarioId: string;
  updatedFields: string[];
  version?: number;
}

/**
 * Published when a scenario is published.
 */
export interface ScenarioPublished extends IntegrationEvent {
  type: 'ScenarioPublished';
  scenarioId: string;
  title: string;
  authorId: string;
}

/**
 * Published when a scenario is unpublished.
 */
export interface ScenarioUnpublished extends IntegrationEvent {
  type: 'ScenarioUnpublished';
  scenarioId: string;
  reason?: string;
}

/**
 * Published when media is uploaded.
 */
export interface MediaUploaded extends IntegrationEvent {
  type: 'MediaUploaded';
  mediaId: string;
  uploaderId: string;
  mimeType: string;
  sizeBytes: number;
}

// =============================================================================
// Cache Events
// =============================================================================

/**
 * Published when cache should be invalidated.
 */
export interface CacheInvalidated extends IntegrationEvent {
  type: 'CacheInvalidated';
  keyPattern: string;
  entityType: string;
  entityId?: string;
  sourceService: string;
}

/**
 * Published when cache warmup is requested.
 */
export interface CacheWarmupRequested extends IntegrationEvent {
  type: 'CacheWarmupRequested';
  entityType: string;
  entityIds?: string[];
  priority: number;
}

// =============================================================================
// Union Types
// =============================================================================

/**
 * All account-related events.
 */
export type AccountEvent = AccountCreated | AccountUpdated | AccountDeleted;

/**
 * All session-related events.
 */
export type SessionEvent = SessionStarted | SessionCompleted | SessionAbandoned;

/**
 * All content-related events.
 */
export type ContentEvent =
  | ScenarioCreated
  | ScenarioUpdated
  | ScenarioPublished
  | ScenarioUnpublished
  | MediaUploaded;

/**
 * All cache-related events.
 */
export type CacheEvent = CacheInvalidated | CacheWarmupRequested;

/**
 * All Mystira domain events.
 */
export type MystiraEvent = AccountEvent | SessionEvent | ContentEvent | CacheEvent;

/**
 * Event type discriminator values.
 */
export type EventType = MystiraEvent['type'];
