import { useState } from 'react';
import { Badge } from '@/components';
import { useQuery } from '@tanstack/react-query';
import { notificationsApi } from '@/api';
import { NotificationCenter } from './NotificationCenter';
import { NOTIFICATION_POLL_INTERVAL } from '@/constants';
import clsx from 'clsx';

export function NotificationBell() {
  const [isOpen, setIsOpen] = useState(false);

  const { data } = useQuery({
    queryKey: ['notifications', 'unread-count'],
    queryFn: () => notificationsApi.getUnreadCount(),
    refetchInterval: NOTIFICATION_POLL_INTERVAL,
  });

  const unreadCount = data?.count ?? 0;

  return (
    <>
      <button
        type="button"
        className={clsx('notification-bell', {
          'notification-bell--has-unread': unreadCount > 0,
        })}
        onClick={() => setIsOpen(true)}
        aria-label={`Notifications${unreadCount > 0 ? ` (${unreadCount} unread)` : ''}`}
      >
        <span className="notification-bell__icon">🔔</span>
        {unreadCount > 0 && (
          <Badge variant="danger" className="notification-bell__badge">
            {unreadCount > 99 ? '99+' : unreadCount}
          </Badge>
        )}
      </button>
      <NotificationCenter isOpen={isOpen} onClose={() => setIsOpen(false)} />
    </>
  );
}

