import { useState } from 'react';
import { Card, CardBody, CardHeader, Button } from '@/components';
import { AuditLogList, AuditLogFilters, AuditLogDetail, useAuditLogs } from '@/features/AuditTrail';
import type { AuditLog } from '@/api/types';

export function AuditPage() {
  const { logs, isLoading, filters, setFilters, exportLogs } = useAuditLogs();
  const [selectedLog, setSelectedLog] = useState<AuditLog | null>(null);
  const [showFilters, setShowFilters] = useState(false);

  const hasActiveFilters = filters.eventType || filters.startDate || filters.endDate;

  return (
    <div className="page page--audit">
      <header className="audit-header">
        <div>
          <h1>Audit Trail</h1>
          <p className="audit-header__subtitle">
            Complete history of all actions across registered stories
          </p>
        </div>
        <div className="audit-header__actions">
          <Button
            variant="outline"
            onClick={() => setShowFilters(!showFilters)}
            style={{ position: 'relative' }}
          >
            {showFilters ? 'Hide' : 'Show'} Filters
            {hasActiveFilters && <span className="audit-header__filter-badge" />}
          </Button>
          {exportLogs && (
            <Button variant="outline" onClick={exportLogs}>
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4M7 10l5 5 5-5M12 15V3" strokeLinecap="round" strokeLinejoin="round"/>
              </svg>
              Export
            </Button>
          )}
        </div>
      </header>

      {showFilters && (
        <Card className="audit-filters-card">
          <CardHeader>
            <h2>Filters</h2>
          </CardHeader>
          <CardBody>
            <AuditLogFilters
              filters={filters}
              onChange={setFilters}
            />
          </CardBody>
        </Card>
      )}

      <Card className="audit-log-card">
        <CardHeader>
          <div>
            <h2>Activity Log</h2>
            <span className="audit-log-card__count">
              {isLoading ? 'Loading...' : `${logs.length} ${logs.length === 1 ? 'event' : 'events'}`}
            </span>
          </div>
        </CardHeader>
        <CardBody>
          <AuditLogList
            logs={logs}
            isLoading={isLoading}
            onSelect={setSelectedLog}
          />
        </CardBody>
      </Card>

      <AuditLogDetail
        log={selectedLog}
        onClose={() => setSelectedLog(null)}
      />
    </div>
  );
}
