import { useState } from 'react';
import type { Story } from '@/api/types';
import { Button, Card, CardBody, Alert } from '@/components';
import { useApproval } from '../hooks/useApproval';

interface ApprovalPanelProps {
  story: Story;
  currentUserId: string;
}

export function ApprovalPanel({ story, currentUserId }: ApprovalPanelProps) {
  const [comment, setComment] = useState('');
  const { submitApproval, isSubmitting } = useApproval();

  const currentContributor = story.contributors.find(c => c.userId === currentUserId);

  if (!currentContributor) {
    return (
      <Alert variant="info">
        You are not a contributor on this story.
      </Alert>
    );
  }

  if (currentContributor.approvalStatus !== 'pending') {
    return (
      <Alert
        variant={currentContributor.approvalStatus === 'approved' ? 'success' : 'warning'}
      >
        You have already {currentContributor.approvalStatus} this registration.
      </Alert>
    );
  }

  const handleApprove = () => {
    submitApproval({
      storyId: story.id,
      approved: true,
      comment: comment || undefined,
    });
  };

  const handleReject = () => {
    if (!comment.trim()) {
      alert('Please provide a reason for rejection');
      return;
    }
    submitApproval({
      storyId: story.id,
      approved: false,
      comment,
    });
  };

  return (
    <Card className="approval-panel">
      <CardBody>
        <h3>Your Approval Required</h3>
        <p>
          Please review the story details and your attribution before approving registration.
        </p>

        <div className="approval-panel__attribution">
          <h4>Your Attribution</h4>
          <dl>
            <dt>Role</dt>
            <dd>{formatRole(currentContributor.role)}</dd>
            <dt>Royalty Split</dt>
            <dd>{currentContributor.split}%</dd>
          </dl>
        </div>

        <div className="approval-panel__comment">
          <label htmlFor="approval-comment">Comment (optional for approval, required for rejection)</label>
          <textarea
            id="approval-comment"
            value={comment}
            onChange={e => setComment(e.target.value)}
            placeholder="Add any comments or concerns..."
            rows={3}
          />
        </div>

        <div className="approval-panel__actions">
          <Button
            variant="danger"
            onClick={handleReject}
            loading={isSubmitting}
          >
            Reject
          </Button>
          <Button
            variant="primary"
            onClick={handleApprove}
            loading={isSubmitting}
          >
            Approve Registration
          </Button>
        </div>
      </CardBody>
    </Card>
  );
}

function formatRole(role: string): string {
  return role
    .split('_')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
}
