/**
 * Entity types for Mystira platform.
 * Mirrors Mystira.Shared.Data.Entities in C#.
 */

/**
 * Base entity interface with ID.
 */
export interface Entity {
  /** Unique identifier (ULID recommended) */
  id: string;
}

/**
 * Entity with audit fields.
 */
export interface AuditableEntity extends Entity {
  /** When the entity was created */
  createdAt: string;
  /** Who created the entity */
  createdBy?: string;
  /** When the entity was last updated */
  updatedAt?: string;
  /** Who last updated the entity */
  updatedBy?: string;
}

/**
 * Entity with soft delete support.
 */
export interface SoftDeletableEntity extends AuditableEntity {
  /** Whether the entity is deleted */
  isDeleted: boolean;
  /** When the entity was deleted */
  deletedAt?: string;
  /** Who deleted the entity */
  deletedBy?: string;
}

/**
 * Value object base (immutable, compared by value).
 */
export interface ValueObject {
  /** Value objects should implement equals() */
  equals(other: unknown): boolean;
}

/**
 * Database target for polyglot persistence.
 * Mirrors Mystira.Shared.Data.Polyglot.DatabaseTarget in C#.
 */
export type DatabaseTarget = 'cosmosdb' | 'postgresql' | 'redis';

/**
 * Entity metadata for polyglot routing.
 */
export interface EntityMetadata {
  /** Target database for this entity */
  databaseTarget: DatabaseTarget;
  /** Collection/table name */
  collectionName: string;
  /** Partition key (for Cosmos DB) */
  partitionKey?: string;
}
