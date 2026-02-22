import { Skeleton } from './Skeleton';

interface SkeletonLoaderProps {
  type?: 'list' | 'card' | 'table' | 'form';
  count?: number;
}

export function SkeletonLoader({ type = 'list', count = 3 }: SkeletonLoaderProps) {
  switch (type) {
    case 'list':
      return (
        <div className="skeleton-loader skeleton-loader--list">
          {Array.from({ length: count }).map((_, i) => (
            <div key={i} className="skeleton-loader__item">
              <Skeleton variant="circular" width={40} height={40} />
              <div className="skeleton-loader__content">
                <Skeleton width="60%" height={16} />
                <Skeleton width="40%" height={14} lines={1} />
              </div>
            </div>
          ))}
        </div>
      );

    case 'card':
      return (
        <div className="skeleton-loader skeleton-loader--card">
          {Array.from({ length: count }).map((_, i) => (
            <div key={i} className="skeleton-loader__card">
              <Skeleton height={200} />
              <div className="skeleton-loader__card-content">
                <Skeleton width="80%" height={20} />
                <Skeleton width="100%" height={14} lines={2} />
              </div>
            </div>
          ))}
        </div>
      );

    case 'table':
      return (
        <div className="skeleton-loader skeleton-loader--table">
          <Skeleton height={40} />
          {Array.from({ length: count }).map((_, i) => (
            <Skeleton key={i} height={50} />
          ))}
        </div>
      );

    case 'form':
      return (
        <div className="skeleton-loader skeleton-loader--form">
          <Skeleton width="100%" height={40} />
          <Skeleton width="100%" height={40} />
          <Skeleton width="100%" height={100} />
          <Skeleton width="30%" height={40} />
        </div>
      );

    default:
      return <Skeleton />;
  }
}

