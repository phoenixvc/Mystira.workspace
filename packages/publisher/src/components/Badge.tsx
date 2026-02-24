import { type ReactNode } from 'react';
import clsx from 'clsx';

export interface BadgeProps {
  children: ReactNode;
  variant?: 'default' | 'primary' | 'success' | 'warning' | 'danger' | 'info';
  size?: 'sm' | 'md';
  className?: string;
}

export function Badge({ children, variant = 'default', size = 'md', className }: BadgeProps) {
  return (
    <span className={clsx('badge', `badge--${variant}`, `badge--${size}`, className)}>
      {children}
    </span>
  );
}
