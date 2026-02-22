import { useState } from 'react';
import { Button, Input, Select, Modal } from '@/components';
import type { OpenRole, CreateOpenRoleRequest, ContributorRole } from '@/api/types';

interface OpenRoleFormProps {
  storyId: string;
  storyTitle: string;
  initialData?: OpenRole | null;
  onSubmit: (data: CreateOpenRoleRequest) => Promise<void>;
  onCancel: () => void;
}

const ROLE_OPTIONS = [
  { value: 'primary_author', label: 'Primary Author' },
  { value: 'co_author', label: 'Co-Author' },
  { value: 'illustrator', label: 'Illustrator' },
  { value: 'editor', label: 'Editor' },
  { value: 'moderator', label: 'Moderator' },
  { value: 'publisher', label: 'Publisher' },
];

export function OpenRoleForm({
  storyId,
  storyTitle,
  initialData,
  onSubmit,
  onCancel,
}: OpenRoleFormProps) {
  const [role, setRole] = useState<ContributorRole>(initialData?.role ?? 'co_author');
  const [splitPercentage, setSplitPercentage] = useState(initialData?.splitPercentage ?? 10);
  const [description, setDescription] = useState(initialData?.description ?? '');
  const [requirements, setRequirements] = useState(initialData?.requirements ?? '');
  const [deadline, setDeadline] = useState(
    initialData?.deadline ? initialData.deadline.split('T')[0] : ''
  );
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);

    try {
      await onSubmit({
        storyId,
        role,
        splitPercentage,
        description: description || undefined,
        requirements: requirements || undefined,
        deadline: deadline ? `${deadline}T23:59:59Z` : undefined,
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen
      onClose={onCancel}
      title={initialData ? 'Edit Open Role' : 'Create Open Role'}
      size="md"
    >
      <form onSubmit={handleSubmit} className="open-role-form">
        <div className="open-role-form__story-info">
          <strong>Story:</strong> {storyTitle}
        </div>

        <Select
          label="Role"
          options={ROLE_OPTIONS}
          value={role}
          onChange={e => setRole(e.target.value as ContributorRole)}
          required
        />

        <div className="open-role-form__split">
          <Input
            label="Royalty Split (%)"
            type="number"
            min={0}
            max={100}
            step={0.1}
            value={splitPercentage}
            onChange={e => setSplitPercentage(Number(e.target.value))}
            required
          />
          <input
            type="range"
            min={0}
            max={100}
            step={0.1}
            value={splitPercentage}
            onChange={e => setSplitPercentage(Number(e.target.value))}
            className="open-role-form__slider"
          />
        </div>

        <div className="open-role-form__field">
          <label htmlFor="description" className="input-label">
            Description
          </label>
          <textarea
            id="description"
            value={description}
            onChange={e => setDescription(e.target.value)}
            placeholder="Describe what you're looking for in this role..."
            rows={3}
            className="input"
          />
        </div>

        <div className="open-role-form__field">
          <label htmlFor="requirements" className="input-label">
            Requirements (optional)
          </label>
          <textarea
            id="requirements"
            value={requirements}
            onChange={e => setRequirements(e.target.value)}
            placeholder="List any specific requirements or qualifications..."
            rows={3}
            className="input"
          />
        </div>

        <Input
          label="Application Deadline (optional)"
          type="date"
          value={deadline}
          onChange={e => setDeadline(e.target.value)}
          min={new Date().toISOString().split('T')[0]}
        />

        <div className="open-role-form__actions">
          <Button variant="outline" type="button" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" loading={isSubmitting}>
            {initialData ? 'Update' : 'Create'} Open Role
          </Button>
        </div>
      </form>
    </Modal>
  );
}

