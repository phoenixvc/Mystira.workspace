import { storiesApi } from '@/api';
import {
  Alert,
  Badge,
  Button,
  Card,
  CardBody,
  CardHeader,
  FeatureErrorBoundary,
  SkeletonLoader,
  Spinner,
} from '@/components';
import { AuditLogList, useAuditLogs } from '@/features/AuditTrail';
import {
  ApprovalPanel,
  ContributorList,
  OpenRoleManager,
  RoleRequestList,
} from '@/features/Contributor';
import { useAuth } from '@/hooks';
import { useQuery } from '@tanstack/react-query';
import { Link, useParams } from 'react-router-dom';

export function StoryDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();

  const {
    data: story,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['story', id],
    queryFn: () => storiesApi.get(id!),
    enabled: !!id,
  });

  const { logs: auditLogs, isLoading: isLoadingLogs } = useAuditLogs(id);

  if (isLoading) {
    return (
      <div className="page page--story-detail">
        <SkeletonLoader type="form" />
      </div>
    );
  }

  if (error || !story) {
    return (
      <div className="page page--error">
        <Alert variant="error" title="Story not found">
          The requested story could not be found.
        </Alert>
        <Link to="/stories">
          <Button variant="outline">Back to Stories</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="page page--story-detail">
      <header className="story-detail__header">
        <div>
          <Link to="/stories" className="story-detail__back">
            Back to Stories
          </Link>
          <h1>{story.title}</h1>
          <Badge variant={getStatusVariant(story.status)} size="md">
            {story.status}
          </Badge>
        </div>
        {story.status === 'draft' && (
          <Link to={`/register?story=${story.id}`}>
            <Button>Continue Registration</Button>
          </Link>
        )}
      </header>

      <div className="story-detail__grid">
        <div className="story-detail__main">
          <Card>
            <CardHeader>
              <h2>Details</h2>
            </CardHeader>
            <CardBody>
              <p>{story.summary}</p>
              <dl className="story-detail__meta">
                <dt>Created</dt>
                <dd>{new Date(story.createdAt).toLocaleString()}</dd>
                <dt>Last Updated</dt>
                <dd>{new Date(story.updatedAt).toLocaleString()}</dd>
                {story.registeredAt && (
                  <>
                    <dt>Registered</dt>
                    <dd>{new Date(story.registeredAt).toLocaleString()}</dd>
                  </>
                )}
                {story.transactionId && (
                  <>
                    <dt>Transaction ID</dt>
                    <dd>
                      <code>{story.transactionId}</code>
                    </dd>
                  </>
                )}
              </dl>
            </CardBody>
          </Card>

          <Card>
            <CardHeader>
              <h2>Contributors</h2>
            </CardHeader>
            <CardBody>
              <ContributorList
                contributors={story.contributors.map((c, i) => ({
                  id: `${story.id}-${i}`,
                  storyId: story.id,
                  userId: c.userId,
                  role: c.role,
                  split: c.split,
                  approvalStatus: c.approvalStatus,
                  createdAt: story.createdAt,
                  updatedAt: story.updatedAt,
                }))}
              />
            </CardBody>
          </Card>

          {user && (
            <>
              <FeatureErrorBoundary featureName="Open Roles">
                <OpenRoleManager storyId={story.id} storyTitle={story.title} />
              </FeatureErrorBoundary>
              <Card>
                <CardHeader>
                  <h2>Role Requests</h2>
                </CardHeader>
                <CardBody>
                  <FeatureErrorBoundary featureName="Role Requests">
                    <RoleRequestList storyId={story.id} />
                  </FeatureErrorBoundary>
                </CardBody>
              </Card>
            </>
          )}

          {user && story.status === 'pending_approval' && (
            <ApprovalPanel story={story} currentUserId={user.id} />
          )}
        </div>

        <aside className="story-detail__sidebar">
          <Card>
            <CardHeader>
              <h2>Activity</h2>
            </CardHeader>
            <CardBody>
              {isLoadingLogs ? <Spinner /> : <AuditLogList logs={auditLogs.slice(0, 10)} />}
            </CardBody>
          </Card>
        </aside>
      </div>
    </div>
  );
}

function getStatusVariant(status: string) {
  switch (status) {
    case 'registered':
      return 'success' as const;
    case 'pending_approval':
      return 'warning' as const;
    case 'rejected':
      return 'danger' as const;
    default:
      return 'default' as const;
  }
}
