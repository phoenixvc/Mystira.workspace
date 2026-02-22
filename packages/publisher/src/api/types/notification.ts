// Notification types

export type NotificationType =
  | 'role_request_received'
  | 'role_request_accepted'
  | 'role_request_rejected'
  | 'open_role_created'
  | 'contributor_added'
  | 'story_approved'
  | 'story_registered'
  | 'approval_required';

export type NotificationStatus = 'unread' | 'read';

export interface Notification {
  id: string;
  userId: string;
  type: NotificationType;
  title: string;
  message: string;
  status: NotificationStatus;
  link?: string;
  metadata?: Record<string, unknown>;
  createdAt: string;
  readAt?: string;
}

export interface NotificationListParams {
  status?: NotificationStatus;
  type?: NotificationType;
  page?: number;
  pageSize?: number;
}

export interface MarkNotificationReadRequest {
  notificationIds: string[];
}

