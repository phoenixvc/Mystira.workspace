import { type ReactNode } from 'react';
import clsx from 'clsx';

export interface AlertProps {
  children: ReactNode;
  variant?: 'info' | 'success' | 'warning' | 'error';
  title?: string;
  dismissible?: boolean;
  onDismiss?: () => void;
  className?: string;
}

export function Alert({
  children,
  variant = 'info',
  title,
  dismissible = false,
  onDismiss,
  className,
}: AlertProps) {
  return (
    <div
      className={clsx('alert', `alert--${variant}`, className)}
      role="alert"
      aria-live="polite"
    >
      <div className="alert__content">
        {title && <strong className="alert__title">{title}</strong>}
        <div className="alert__message">{children}</div>
      </div>
      {dismissible && onDismiss && (
        <button
          type="button"
          className="alert__dismiss"
          onClick={onDismiss}
          aria-label="Dismiss alert"
        >
          ×
        </button>
      )}
    </div>
  );
}
