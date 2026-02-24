import { memo } from 'react';
import type { Attribution } from '@/api/types';
import { Avatar, Badge, Button, Spinner, EmptyState } from '@/components';

interface ContributorListProps {
  contributors: Attribution[];
  isLoading?: boolean;
  onRemove?: (id: string) => void;
}

export const ContributorList = memo(function ContributorList({
  contributors,
  isLoading,
  onRemove,
}: ContributorListProps) {
  if (isLoading) {
    return (
      <div className="contributor-list__loading">
        <Spinner />
      </div>
    );
  }

  if (contributors.length === 0) {
    return (
      <EmptyState
        title="No contributors yet"
        description="Add contributors to this story to begin the registration process."
      />
    );
  }

  return (
    <ul className="contributor-list">
      {contributors.map(contributor => (
        <li key={contributor.id} className="contributor-list__item">
          <div className="contributor-list__info">
            <Avatar name={contributor.userId} size="sm" />
            <div className="contributor-list__details">
              <span className="contributor-list__name">{contributor.userId}</span>
              <span className="contributor-list__role">{formatRole(contributor.role)}</span>
            </div>
          </div>

          <div className="contributor-list__meta">
            <span className="contributor-list__split">{contributor.split}%</span>
            <Badge variant={getApprovalVariant(contributor.approvalStatus)}>
              {contributor.approvalStatus}
            </Badge>
            {onRemove && contributor.approvalStatus === 'pending' && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => onRemove(contributor.id)}
                aria-label={`Remove contributor ${contributor.userId}`}
              >
                Remove
              </Button>
            )}
          </div>
        </li>
      ))}
    </ul>
  );
});

function formatRole(role: string): string {
  return role
    .split('_')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
}

function getApprovalVariant(status: string) {
  switch (status) {
    case 'approved':
      return 'success' as const;
    case 'rejected':
      return 'danger' as const;
    case 'overridden':
      return 'warning' as const;
    default:
      return 'default' as const;
  }
}
