import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { storiesApi } from '@/api';
import type { Story, StoryStatus } from '@/api/types';
import { Card, CardBody, Input, Select, Badge, Spinner, EmptyState } from '@/components';

interface StoryPickerProps {
  onSelect: (story: Story) => void;
  selectedId?: string;
}

const STATUS_OPTIONS = [
  { value: '', label: 'All Statuses' },
  { value: 'draft', label: 'Draft' },
  { value: 'pending_approval', label: 'Pending Approval' },
  { value: 'approved', label: 'Approved' },
];

export function StoryPicker({ onSelect, selectedId }: StoryPickerProps) {
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<StoryStatus | ''>('');

  const { data, isLoading, error } = useQuery({
    queryKey: ['stories', { search, status }],
    queryFn: () =>
      storiesApi.list({
        search: search || undefined,
        status: status || undefined,
      }),
  });

  if (isLoading) {
    return (
      <div className="story-picker__loading">
        <Spinner size="lg" />
      </div>
    );
  }

  if (error) {
    return (
      <EmptyState
        title="Failed to load stories"
        description="There was an error loading your stories. Please try again."
      />
    );
  }

  return (
    <div className="story-picker">
      <div className="story-picker__filters">
        <Input
          placeholder="Search stories..."
          value={search}
          onChange={e => setSearch(e.target.value)}
          className="story-picker__search"
        />
        <Select
          options={STATUS_OPTIONS}
          value={status}
          onChange={e => setStatus(e.target.value as StoryStatus | '')}
          className="story-picker__status"
        />
      </div>

      <div className="story-picker__list">
        {data?.items.length === 0 ? (
          <EmptyState
            title="No stories found"
            description="Create a new story or adjust your filters to find stories ready for registration."
          />
        ) : (
          data?.items.map(story => (
            <Card
              key={story.id}
              onClick={() => onSelect(story)}
              className={`story-picker__item ${selectedId === story.id ? 'story-picker__item--selected' : ''}`}
            >
              <CardBody>
                <div className="story-picker__item-header">
                  <h3 className="story-picker__item-title">{story.title}</h3>
                  <Badge variant={getStatusVariant(story.status)}>
                    {story.status.replace('_', ' ').toUpperCase()}
                  </Badge>
                </div>
                {story.summary && (
                  <p className="story-picker__item-summary">{story.summary}</p>
                )}
                <div className="story-picker__item-meta">
                  <span className="story-picker__item-meta-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M17 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2" strokeLinecap="round" strokeLinejoin="round"/>
                      <circle cx="9" cy="7" r="4" strokeLinecap="round" strokeLinejoin="round"/>
                      <path d="M23 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75" strokeLinecap="round" strokeLinejoin="round"/>
                    </svg>
                    {story.contributors.length} {story.contributors.length === 1 ? 'contributor' : 'contributors'}
                  </span>
                  <span className="story-picker__item-meta-item">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <circle cx="12" cy="12" r="10" strokeLinecap="round" strokeLinejoin="round"/>
                      <path d="M12 6v6l4 2" strokeLinecap="round" strokeLinejoin="round"/>
                    </svg>
                    Updated {new Date(story.updatedAt).toLocaleDateString()}
                  </span>
                </div>
              </CardBody>
            </Card>
          ))
        )}
      </div>
    </div>
  );
}

function getStatusVariant(status: StoryStatus) {
  switch (status) {
    case 'draft':
      return 'default';
    case 'pending_approval':
      return 'warning';
    case 'approved':
      return 'success';
    case 'registered':
      return 'primary';
    case 'rejected':
      return 'danger';
    default:
      return 'default';
  }
}
