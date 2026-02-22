import { forwardRef, type ButtonHTMLAttributes } from 'react';
import clsx from 'clsx';
import { Spinner } from './Spinner';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'outline' | 'ghost' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  loading?: boolean;
  fullWidth?: boolean;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      variant = 'primary',
      size = 'md',
      loading = false,
      fullWidth = false,
      disabled,
      className,
      children,
      ...props
    },
    ref
  ) => {
    return (
      <button
        ref={ref}
        disabled={disabled || loading}
        className={clsx(
          'btn',
          `btn--${variant}`,
          `btn--${size}`,
          fullWidth && 'btn--full-width',
          loading && 'btn--loading',
          className
        )}
        aria-busy={loading}
        {...props}
      >
        {loading && <Spinner size="sm" className="btn__spinner" />}
        <span className={clsx(loading && 'btn__content--hidden')}>{children}</span>
      </button>
    );
  }
);

Button.displayName = 'Button';
