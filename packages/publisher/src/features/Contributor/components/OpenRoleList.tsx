import type { OpenRole } from '@/api/types';
import { Badge, EmptyState, Spinner } from '@/components';
import { formatDate } from '@/utils/format';

interface OpenRoleListProps {
  roles: OpenRole[];
  isLoading?: boolean;
  onEdit?: (role: OpenRole) => void;
  onDelete?: (id: string) => void;
}

function formatRole(role: string): string {
  return role
    .split('_')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
}

export function OpenRoleList({ roles, isLoading, onEdit, onDelete }: OpenRoleListProps) {
  if (isLoading) {
    return <Spinner />;
  }

  if (roles.length === 0) {
    return (
      <EmptyState
        title="No open roles"
        description="Create an open role to allow contributors to apply for this story."
      />
    );
  }

  return (
    <div className="open-role-list">
      {roles.map(role => (
        <div key={role.id} className="open-role-list__item">
          <div className="open-role-list__content">
            <div className="open-role-list__header">
              <h4>{formatRole(role.role)}</h4>
              <Badge variant="info">{role.splitPercentage}% split</Badge>
            </div>
            {role.description && <p className="open-role-list__description">{role.description}</p>}
            {role.requirements && (
              <div className="open-role-list__requirements">
                <strong>Requirements:</strong> {role.requirements}
              </div>
            )}
            <div className="open-role-list__meta">
              <span>Created {formatDate(role.createdAt)}</span>
              {role.deadline && (
                <span className="open-role-list__deadline">
                  Deadline: {formatDate(role.deadline)}
                </span>
              )}
            </div>
          </div>
          {(onEdit || onDelete) && (
            <div className="open-role-list__actions">
              {onEdit && (
                <button
                  type="button"
                  className="open-role-list__action-btn"
                  onClick={() => onEdit(role)}
                >
                  Edit
                </button>
              )}
              {onDelete && (
                <button
                  type="button"
                  className="open-role-list__action-btn open-role-list__action-btn--danger"
                  onClick={() => onDelete(role.id)}
                >
                  Delete
                </button>
              )}
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
