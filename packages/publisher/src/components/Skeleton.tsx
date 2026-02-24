import clsx from 'clsx';

interface SkeletonProps {
  width?: string | number;
  height?: string | number;
  className?: string;
  variant?: 'text' | 'circular' | 'rectangular';
  lines?: number;
}

export function Skeleton({
  width,
  height,
  className,
  variant = 'rectangular',
  lines = 1,
}: SkeletonProps) {
  const style: React.CSSProperties = {};
  if (width) style.width = typeof width === 'number' ? `${width}px` : width;
  if (height) style.height = typeof height === 'number' ? `${height}px` : height;

  if (lines > 1) {
    return (
      <div className={clsx('skeleton-container', className)}>
        {Array.from({ length: lines }).map((_, i) => (
          <div
            key={i}
            className={clsx('skeleton', `skeleton--${variant}`, {
              'skeleton--last': i === lines - 1,
            })}
            style={i === lines - 1 ? style : undefined}
          />
        ))}
      </div>
    );
  }

  return (
    <div
      className={clsx('skeleton', `skeleton--${variant}`, className)}
      style={style}
    />
  );
}

