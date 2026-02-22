import { useEffect } from 'react';
import { Alert } from './Alert';
import { Button } from './Button';
import clsx from 'clsx';

export type ToastVariant = 'success' | 'error' | 'warning' | 'info';

export interface ToastProps {
  id: string;
  variant: ToastVariant;
  title: string;
  message?: string;
  duration?: number;
  onClose: (id: string) => void;
}

export function Toast({ id, variant, title, message, duration = 5000, onClose }: ToastProps) {
  useEffect(() => {
    if (duration > 0) {
      const timer = setTimeout(() => {
        onClose(id);
      }, duration);

      return () => clearTimeout(timer);
    }
  }, [id, duration, onClose]);

  return (
    <div className={clsx('toast', `toast--${variant}`)} role="alert" aria-live="polite">
      <Alert variant={variant} title={title} className="toast__alert">
        {message && <p>{message}</p>}
      </Alert>
      <Button
        variant="ghost"
        size="sm"
        className="toast__close"
        onClick={() => onClose(id)}
        aria-label="Close notification"
      >
        ×
      </Button>
    </div>
  );
}

