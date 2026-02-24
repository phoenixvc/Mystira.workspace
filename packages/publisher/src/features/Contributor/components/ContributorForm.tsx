import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { contributorsApi } from '@/api';
import type { AddContributorRequest, ContributorRole } from '@/api/types';
import { Button, Input, Select, Modal } from '@/components';

interface ContributorFormProps {
  storyId: string;
  onSubmit: (data: AddContributorRequest) => Promise<void>;
  onCancel: () => void;
  isSubmitting?: boolean;
}

const ROLE_OPTIONS = [
  { value: 'primary_author', label: 'Primary Author' },
  { value: 'co_author', label: 'Co-Author' },
  { value: 'illustrator', label: 'Illustrator' },
  { value: 'editor', label: 'Editor' },
  { value: 'moderator', label: 'Moderator' },
  { value: 'publisher', label: 'Publisher' },
];

export function ContributorForm({
  storyId,
  onSubmit,
  onCancel,
  isSubmitting = false,
}: ContributorFormProps) {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const [email, setEmail] = useState('');
  const [role, setRole] = useState<ContributorRole>('co_author');
  const [split, setSplit] = useState(10);
  const [useEmail, setUseEmail] = useState(false);

  const { data: searchResults, isLoading: isSearching } = useQuery({
    queryKey: ['user-search', searchQuery],
    queryFn: () => contributorsApi.searchUsers({ query: searchQuery, limit: 5 }),
    enabled: searchQuery.length >= 2 && !useEmail,
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    await onSubmit({
      storyId,
      userId: useEmail ? undefined : selectedUserId ?? undefined,
      email: useEmail ? email : undefined,
      role,
      split,
    });
  };

  return (
    <Modal isOpen onClose={onCancel} title="Add Contributor" size="md">
      <form onSubmit={handleSubmit} className="contributor-form">
        <div className="contributor-form__toggle">
          <button
            type="button"
            className={`contributor-form__toggle-btn ${!useEmail ? 'active' : ''}`}
            onClick={() => setUseEmail(false)}
          >
            Search Users
          </button>
          <button
            type="button"
            className={`contributor-form__toggle-btn ${useEmail ? 'active' : ''}`}
            onClick={() => setUseEmail(true)}
          >
            Invite by Email
          </button>
        </div>

        {useEmail ? (
          <Input
            label="Email Address"
            type="email"
            value={email}
            onChange={e => setEmail(e.target.value)}
            placeholder="contributor@example.com"
            required
          />
        ) : (
          <div className="contributor-form__search">
            <Input
              label="Search Users"
              value={searchQuery}
              onChange={e => {
                setSearchQuery(e.target.value);
                setSelectedUserId(null);
              }}
              placeholder="Search by name or handle..."
            />
            {isSearching && <span className="contributor-form__searching">Searching...</span>}
            {searchResults && searchResults.length > 0 && (
              <ul className="contributor-form__results">
                {searchResults.map(user => (
                  <li key={user.id}>
                    <button
                      type="button"
                      className={`contributor-form__result ${selectedUserId === user.id ? 'selected' : ''}`}
                      onClick={() => {
                        setSelectedUserId(user.id);
                        setSearchQuery(user.name);
                      }}
                    >
                      <span>{user.name}</span>
                      <span className="contributor-form__result-email">{user.email}</span>
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>
        )}

        <Select label="Role" options={ROLE_OPTIONS} value={role} onChange={e => setRole(e.target.value as ContributorRole)} />

        <div className="contributor-form__split">
          <Input
            label="Royalty Split (%)"
            type="number"
            min={0}
            max={100}
            value={split}
            onChange={e => setSplit(Number(e.target.value))}
          />
          <input
            type="range"
            min={0}
            max={100}
            value={split}
            onChange={e => setSplit(Number(e.target.value))}
            className="contributor-form__slider"
          />
        </div>

        <div className="contributor-form__actions">
          <Button variant="outline" type="button" onClick={onCancel}>
            Cancel
          </Button>
          <Button
            type="submit"
            loading={isSubmitting}
            disabled={(!useEmail && !selectedUserId) || (useEmail && !email)}
          >
            Add Contributor
          </Button>
        </div>
      </form>
    </Modal>
  );
}
