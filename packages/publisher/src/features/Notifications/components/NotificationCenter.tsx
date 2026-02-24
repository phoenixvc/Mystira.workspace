import { notificationsApi } from '@/api';
import type { Notification } from '@/api/types';
import { Badge, Button, EmptyState, Modal, Spinner } from '@/components';
import { NOTIFICATION_POLL_INTERVAL } from '@/constants';
import { formatDate } from '@/utils/format';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import clsx from 'clsx';
import { useNavigate } from 'react-router-dom';

interface NotificationCenterProps {
  isOpen: boolean;
  onClose: () => void;
}

function getNotificationIcon(type: string): string {
  switch (type) {
    case 'role_request_received':
      return '📥';
    case 'role_request_accepted':
      return '✅';
    case 'role_request_rejected':
      return '❌';
    case 'open_role_created':
      return '📢';
    case 'contributor_added':
      return '👤';
    case 'story_approved':
      return '✓';
    case 'story_registered':
      return '🔗';
    case 'approval_required':
      return '⚠️';
    default:
      return '🔔';
  }
}

export function NotificationCenter({ isOpen, onClose }: NotificationCenterProps) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['notifications'],
    queryFn: () => notificationsApi.getNotifications({ page: 1, pageSize: 50 }),
    enabled: isOpen,
    refetchInterval: NOTIFICATION_POLL_INTERVAL,
  });

  const markAsReadMutation = useMutation({
    mutationFn: (ids: string[]) => notificationsApi.markAsRead({ notificationIds: ids }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });

  const markAllAsReadMutation = useMutation({
    mutationFn: () => notificationsApi.markAllAsRead(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => notificationsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] });
    },
  });

  const notifications = data?.items ?? [];
  const unreadCount = data?.unreadCount ?? 0;

  const handleNotificationClick = async (notification: Notification) => {
    if (notification.status === 'unread') {
      await markAsReadMutation.mutateAsync([notification.id]);
    }

    if (notification.link) {
      navigate(notification.link);
      onClose();
    }
  };

  const handleMarkAllAsRead = async () => {
    await markAllAsReadMutation.mutateAsync();
  };

  const handleDelete = async (e: React.MouseEvent, id: string) => {
    e.stopPropagation();
    await deleteMutation.mutateAsync(id);
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Notifications" size="md">
      <div className="notification-center">
        {unreadCount > 0 && (
          <div className="notification-center__header">
            <Button variant="outline" size="sm" onClick={handleMarkAllAsRead}>
              Mark all as read
            </Button>
          </div>
        )}

        {isLoading ? (
          <Spinner />
        ) : notifications.length === 0 ? (
          <EmptyState title="No notifications" description="You're all caught up!" />
        ) : (
          <div className="notification-center__list">
            {notifications.map(notification => (
              <div
                key={notification.id}
                className={clsx('notification-center__item', {
                  'notification-center__item--unread': notification.status === 'unread',
                })}
                onClick={() => handleNotificationClick(notification)}
              >
                <div className="notification-center__icon">
                  {getNotificationIcon(notification.type)}
                </div>
                <div className="notification-center__content">
                  <div className="notification-center__header-row">
                    <h4>{notification.title}</h4>
                    {notification.status === 'unread' && (
                      <Badge variant="info" size="sm">
                        New
                      </Badge>
                    )}
                  </div>
                  <p className="notification-center__message">{notification.message}</p>
                  <span className="notification-center__time">
                    {formatDate(notification.createdAt)}
                  </span>
                </div>
                <button
                  type="button"
                  className="notification-center__delete"
                  onClick={e => handleDelete(e, notification.id)}
                  aria-label="Delete notification"
                >
                  ×
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </Modal>
  );
}
