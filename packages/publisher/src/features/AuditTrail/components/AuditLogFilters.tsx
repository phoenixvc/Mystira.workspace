import { Input, Select, Button } from '@/components';
import type { AuditEventType, AuditLogParams } from '@/api/types';

interface AuditLogFiltersProps {
  filters: AuditLogParams;
  onChange: (filters: AuditLogParams) => void;
  onExport?: () => void;
}

const EVENT_TYPE_OPTIONS = [
  { value: '', label: 'All Events' },
  { value: 'story_created', label: 'Story Created' },
  { value: 'contributor_added', label: 'Contributor Added' },
  { value: 'contributor_removed', label: 'Contributor Removed' },
  { value: 'split_updated', label: 'Split Updated' },
  { value: 'approval_submitted', label: 'Approval Submitted' },
  { value: 'approval_rejected', label: 'Approval Rejected' },
  { value: 'override_applied', label: 'Override Applied' },
  { value: 'registration_initiated', label: 'Registration Initiated' },
  { value: 'registration_completed', label: 'Registration Completed' },
  { value: 'registration_failed', label: 'Registration Failed' },
];

export function AuditLogFilters({ filters, onChange, onExport: _onExport }: AuditLogFiltersProps) {
  const handleChange = (key: keyof AuditLogParams, value: string) => {
    onChange({
      ...filters,
      [key]: value || undefined,
    });
  };

  const hasFilters = filters.eventType || filters.startDate || filters.endDate;

  return (
    <div className="audit-log-filters">
      <div className="audit-log-filters__fields">
        <Select
          label="Event Type"
          options={EVENT_TYPE_OPTIONS}
          value={filters.eventType || ''}
          onChange={e => handleChange('eventType', e.target.value as AuditEventType)}
        />

        <Input
          label="Start Date"
          type="date"
          value={filters.startDate || ''}
          onChange={e => handleChange('startDate', e.target.value)}
        />

        <Input
          label="End Date"
          type="date"
          value={filters.endDate || ''}
          onChange={e => handleChange('endDate', e.target.value)}
        />
      </div>

      <div className="audit-log-filters__actions">
        {hasFilters && (
          <Button
            variant="ghost"
            onClick={() => onChange({})}
          >
            Clear All
          </Button>
        )}
      </div>
    </div>
  );
}
