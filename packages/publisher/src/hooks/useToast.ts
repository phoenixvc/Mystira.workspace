import { useCallback } from 'react';
import { useUIStore } from '@/state/uiStore';
import type { ToastVariant } from '@/components/Toast';

interface ToastOptions {
  variant?: ToastVariant;
  duration?: number;
}

export function useToast() {
  const addNotification = useUIStore(state => state.addNotification);

  const toast = useCallback(
    (title: string, message?: string, options?: ToastOptions) => {
      addNotification({
        type: options?.variant || 'info',
        title,
        message,
        duration: options?.duration,
      });
    },
    [addNotification]
  );

  const success = useCallback(
    (title: string, message?: string, duration?: number) => {
      toast(title, message, { variant: 'success', duration });
    },
    [toast]
  );

  const error = useCallback(
    (title: string, message?: string, duration?: number) => {
      toast(title, message, { variant: 'error', duration });
    },
    [toast]
  );

  const warning = useCallback(
    (title: string, message?: string, duration?: number) => {
      toast(title, message, { variant: 'warning', duration });
    },
    [toast]
  );

  const info = useCallback(
    (title: string, message?: string, duration?: number) => {
      toast(title, message, { variant: 'info', duration });
    },
    [toast]
  );

  return { toast, success, error, warning, info };
}

