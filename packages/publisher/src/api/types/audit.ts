// Audit types that mirror backend API contracts

export type AuditEventType =
  | 'story_created'
  | 'contributor_added'
  | 'contributor_removed'
  | 'split_updated'
  | 'approval_submitted'
  | 'approval_rejected'
  | 'override_applied'
  | 'registration_initiated'
  | 'registration_completed'
  | 'registration_failed';

export interface AuditLog {
  id: string;
  eventType: AuditEventType;
  storyId: string;
  actorId: string;
  actorName: string;
  targetId?: string;
  targetName?: string;
  details: Record<string, unknown>;
  timestamp: string;
}

export interface AuditLogParams {
  storyId?: string;
  eventType?: AuditEventType;
  actorId?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}
