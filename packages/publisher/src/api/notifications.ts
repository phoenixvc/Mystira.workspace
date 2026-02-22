import { request } from './client';
import type {
  Notification,
  NotificationListParams,
  MarkNotificationReadRequest,
} from './types/notification';

const NOTIFICATIONS_PATH = '/notifications';

export const notificationsApi = {
  // Get all notifications for current user
  getNotifications: (params?: NotificationListParams) =>
    request<{ items: Notification[]; total: number; unreadCount: number }>({
      method: 'GET',
      url: NOTIFICATIONS_PATH,
      params,
    }),

  // Mark notifications as read
  markAsRead: (data: MarkNotificationReadRequest) =>
    request<void>({
      method: 'POST',
      url: `${NOTIFICATIONS_PATH}/read`,
      data,
    }),

  // Mark all notifications as read
  markAllAsRead: () =>
    request<void>({
      method: 'POST',
      url: `${NOTIFICATIONS_PATH}/read-all`,
    }),

  // Delete a notification
  delete: (id: string) =>
    request<void>({
      method: 'DELETE',
      url: `${NOTIFICATIONS_PATH}/${id}`,
    }),

  // Get unread count
  getUnreadCount: () =>
    request<{ count: number }>({
      method: 'GET',
      url: `${NOTIFICATIONS_PATH}/unread-count`,
    }),
};

