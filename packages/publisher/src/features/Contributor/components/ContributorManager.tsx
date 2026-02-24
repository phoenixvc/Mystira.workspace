import { useState } from 'react';
import { Button, Card, CardBody, CardHeader } from '@/components';
import { ContributorList } from './ContributorList';
import { ContributorForm } from './ContributorForm';
import { useContributors } from '../hooks/useContributors';

interface ContributorManagerProps {
  storyId: string;
  readonly?: boolean;
}

export function ContributorManager({ storyId, readonly = false }: ContributorManagerProps) {
  const [showForm, setShowForm] = useState(false);
  const { contributors, isLoading, addContributor, removeContributor, isAdding } =
    useContributors(storyId);

  return (
    <div className="contributor-manager">
      <Card>
        <CardHeader>
          <div className="contributor-manager__header">
            <h3>Contributors</h3>
            {!readonly && (
              <Button variant="outline" size="sm" onClick={() => setShowForm(true)}>
                Add Contributor
              </Button>
            )}
          </div>
        </CardHeader>
        <CardBody>
          <ContributorList
            contributors={contributors ?? []}
            isLoading={isLoading}
            onRemove={readonly ? undefined : removeContributor}
          />
        </CardBody>
      </Card>

      {showForm && (
        <ContributorForm
          storyId={storyId}
          onSubmit={async data => {
            await addContributor(data);
            setShowForm(false);
          }}
          onCancel={() => setShowForm(false)}
          isSubmitting={isAdding}
        />
      )}
    </div>
  );
}
