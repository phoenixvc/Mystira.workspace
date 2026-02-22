import type { AuditLog } from '@/api/types';
import { Modal, Badge } from '@/components';

interface AuditLogDetailProps {
  log: AuditLog | null;
  onClose: () => void;
}

export function AuditLogDetail({ log, onClose }: AuditLogDetailProps) {
  if (!log) return null;

  return (
    <Modal isOpen={!!log} onClose={onClose} title="Audit Log Details" size="md">
      <div className="audit-log-detail">
        <dl className="audit-log-detail__list">
          <dt>Event Type</dt>
          <dd>
            <Badge variant="info">{formatEventType(log.eventType)}</Badge>
          </dd>

          <dt>Timestamp</dt>
          <dd>{new Date(log.timestamp).toLocaleString()}</dd>

          <dt>Actor</dt>
          <dd>{log.actorName} ({log.actorId})</dd>

          {log.targetName && (
            <>
              <dt>Target</dt>
              <dd>{log.targetName} ({log.targetId})</dd>
            </>
          )}

          <dt>Story ID</dt>
          <dd><code>{log.storyId}</code></dd>

          <dt>Log ID</dt>
          <dd><code>{log.id}</code></dd>
        </dl>

        {Object.keys(log.details).length > 0 && (
          <div className="audit-log-detail__details">
            <h4>Additional Details</h4>
            <pre className="audit-log-detail__json">
              {JSON.stringify(log.details, null, 2)}
            </pre>
          </div>
        )}
      </div>
    </Modal>
  );
}

function formatEventType(eventType: string): string {
  return eventType
    .split('_')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
}
