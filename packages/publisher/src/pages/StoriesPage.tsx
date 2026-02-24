import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { storiesApi } from '@/api';
import type { StoryStatus } from '@/api/types';
import { Button, Card, CardBody, Input, Select, Badge, EmptyState, SkeletonLoader } from '@/components';
import { useDebounce } from '@/hooks';

const STATUS_OPTIONS = [
  { value: '', label: 'All Statuses' },
  { value: 'draft', label: 'Draft' },
  { value: 'pending_approval', label: 'Pending Approval' },
  { value: 'approved', label: 'Approved' },
  { value: 'registered', label: 'Registered' },
  { value: 'rejected', label: 'Rejected' },
];

export function StoriesPage() {
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<StoryStatus | ''>('');
  const debouncedSearch = useDebounce(search, 300);

  const { data, isLoading } = useQuery({
    queryKey: ['stories', { search: debouncedSearch, status }],
    queryFn: () =>
      storiesApi.list({
        search: debouncedSearch || undefined,
        status: status || undefined,
      }),
  });

  return (
    <div className="page page--stories">
      <header className="page-header">
        <h1>Stories</h1>
        <Link to="/register">
          <Button>Register New Story</Button>
        </Link>
      </header>

      <div className="stories-filters">
        <Input
          placeholder="Search stories..."
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
        <Select
          options={STATUS_OPTIONS}
          value={status}
          onChange={e => setStatus(e.target.value as StoryStatus | '')}
        />
      </div>

      {isLoading ? (
        <SkeletonLoader type="card" count={6} />
      ) : data?.items.length === 0 ? (
        <EmptyState
          title="No stories found"
          description="Create your first story registration or adjust your search filters."
          action={
            <Link to="/register">
              <Button>Register a Story</Button>
            </Link>
          }
        />
      ) : (
        <div className="stories-grid">
          {data?.items.map(story => (
            <Link key={story.id} to={`/stories/${story.id}`}>
              <Card className="story-card">
                <CardBody>
                  <div className="story-card__header">
                    <h3>{story.title}</h3>
                    <Badge variant={getStatusVariant(story.status)}>{story.status}</Badge>
                  </div>
                  <p className="story-card__summary">{story.summary}</p>
                  <div className="story-card__meta">
                    <span>{story.contributors.length} contributors</span>
                    <span>{new Date(story.updatedAt).toLocaleDateString()}</span>
                  </div>
                </CardBody>
              </Card>
            </Link>
          ))}
        </div>
      )}
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
