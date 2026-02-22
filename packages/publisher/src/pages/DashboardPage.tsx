import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { storiesApi } from '@/api';
import { Button, Card, CardBody, CardHeader, Badge, EmptyState, SkeletonLoader } from '@/components';
import { useAuth } from '@/hooks';

export function DashboardPage() {
  const { user } = useAuth();

  const { data: stories, isLoading } = useQuery({
    queryKey: ['stories', { pageSize: 5 }],
    queryFn: () => storiesApi.list({ pageSize: 5 }),
  });

  const registeredCount = stories?.items.filter(s => s.status === 'registered').length || 0;
  const pendingCount = stories?.items.filter(s => s.status === 'pending_approval').length || 0;
  const totalCount = stories?.items.length || 0;

  return (
    <div className="page page--dashboard">
      <header className="dashboard-header">
        <div>
          <h1>Welcome back, {user?.name || 'User'}</h1>
          <p className="dashboard-header__subtitle">Here's what's happening with your stories</p>
        </div>
        <Link to="/register">
          <Button size="lg">+ Register New Story</Button>
        </Link>
      </header>

      {totalCount > 0 && (
        <div className="dashboard-stats">
          <Card className="dashboard-stat-card">
            <div className="dashboard-stat-card__icon dashboard-stat-card__icon--primary">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M9 11l3 3L22 4" strokeLinecap="round" strokeLinejoin="round"/>
                <path d="M21 12v7a2 2 0 01-2 2H5a2 2 0 01-2-2V5a2 2 0 012-2h11" strokeLinecap="round" strokeLinejoin="round"/>
              </svg>
            </div>
            <div className="dashboard-stat-card__content">
              <div className="dashboard-stat-card__value">{registeredCount}</div>
              <div className="dashboard-stat-card__label">Registered</div>
            </div>
          </Card>
          <Card className="dashboard-stat-card">
            <div className="dashboard-stat-card__icon dashboard-stat-card__icon--warning">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <circle cx="12" cy="12" r="10"/>
                <path d="M12 6v6M12 16h.01" strokeLinecap="round"/>
              </svg>
            </div>
            <div className="dashboard-stat-card__content">
              <div className="dashboard-stat-card__value">{pendingCount}</div>
              <div className="dashboard-stat-card__label">Pending</div>
            </div>
          </Card>
          <Card className="dashboard-stat-card">
            <div className="dashboard-stat-card__icon dashboard-stat-card__icon--info">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M4 19.5A2.5 2.5 0 016.5 17H20" strokeLinecap="round" strokeLinejoin="round"/>
                <path d="M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z" strokeLinecap="round" strokeLinejoin="round"/>
              </svg>
            </div>
            <div className="dashboard-stat-card__content">
              <div className="dashboard-stat-card__value">{totalCount}</div>
              <div className="dashboard-stat-card__label">Total Stories</div>
            </div>
          </Card>
        </div>
      )}

      <div className="dashboard-grid">
        <Card className="card--elevated dashboard-card">
          <CardHeader>
            <h2>Recent Stories</h2>
            <Link to="/stories" className="dashboard-card__link">
              View All →
            </Link>
          </CardHeader>
          <CardBody>
            {isLoading ? (
              <SkeletonLoader type="list" count={5} />
            ) : stories?.items.length === 0 ? (
              <EmptyState
                title="No stories yet"
                description="Get started by registering your first creative story on-chain."
                action={
                  <Link to="/register">
                    <Button>Register Your First Story</Button>
                  </Link>
                }
              />
            ) : (
              <ul className="dashboard-stories">
                {stories?.items.map(story => (
                  <li key={story.id}>
                    <Link to={`/stories/${story.id}`} className="dashboard-story">
                      <div className="dashboard-story__content">
                        <span className="dashboard-story__title">{story.title}</span>
                        {story.summary && (
                          <span className="dashboard-story__summary">{story.summary}</span>
                        )}
                      </div>
                      <Badge variant={getStatusVariant(story.status)}>
                        {story.status.replace('_', ' ')}
                      </Badge>
                    </Link>
                  </li>
                ))}
              </ul>
            )}
          </CardBody>
        </Card>

        <Card className="card--elevated dashboard-card">
          <CardHeader>
            <h2>Quick Actions</h2>
          </CardHeader>
          <CardBody>
            <nav className="dashboard-actions">
              <Link to="/register" className="dashboard-action">
                <div className="dashboard-action__icon">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <path d="M12 5v14M5 12h14" strokeLinecap="round"/>
                  </svg>
                </div>
                <Button variant="outline" fullWidth>Start Registration</Button>
              </Link>
              <Link to="/stories" className="dashboard-action">
                <div className="dashboard-action__icon">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <path d="M4 19.5A2.5 2.5 0 016.5 17H20" strokeLinecap="round"/>
                    <path d="M6.5 2H20v20H6.5A2.5 2.5 0 014 19.5v-15A2.5 2.5 0 016.5 2z" strokeLinecap="round"/>
                  </svg>
                </div>
                <Button variant="outline" fullWidth>View All Stories</Button>
              </Link>
              <Link to="/audit" className="dashboard-action">
                <div className="dashboard-action__icon">
                  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                    <path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z" strokeLinecap="round"/>
                    <path d="M14 2v6h6M16 13H8M16 17H8M10 9H8" strokeLinecap="round"/>
                  </svg>
                </div>
                <Button variant="outline" fullWidth>Audit Trail</Button>
              </Link>
            </nav>
          </CardBody>
        </Card>
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
