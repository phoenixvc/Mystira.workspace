import { useState, useEffect, useMemo, useCallback } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { contributorsApi } from '@/api';
import { Button, Alert, Card, CardBody, CardFooter } from '@/components';
import { useContributors } from '../hooks/useContributors';

interface RoyaltySplitEditorProps {
  storyId: string;
}

export function RoyaltySplitEditor({ storyId }: RoyaltySplitEditorProps) {
  const queryClient = useQueryClient();
  const { contributors, isLoading } = useContributors(storyId);
  const [splits, setSplits] = useState<Record<string, number>>({});
  const [isDirty, setIsDirty] = useState(false);

  // Initialize splits from contributors
  useEffect(() => {
    if (contributors && !isDirty) {
      const initialSplits: Record<string, number> = {};
      contributors.forEach(c => {
        initialSplits[c.id] = c.split;
      });
      setSplits(initialSplits);
    }
  }, [contributors, isDirty]);

  const totalSplit = useMemo(
    () => Object.values(splits).reduce((sum, val) => sum + val, 0),
    [splits]
  );
  const isValid = useMemo(() => totalSplit === 100, [totalSplit]);

  const updateMutation = useMutation({
    mutationFn: async () => {
      const updates = Object.entries(splits).map(([id, split]) =>
        contributorsApi.update(id, { split })
      );
      await Promise.all(updates);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contributors', storyId] });
      setIsDirty(false);
    },
  });

  const handleSplitChange = useCallback((contributorId: string, value: number) => {
    setSplits(prev => ({ ...prev, [contributorId]: value }));
    setIsDirty(true);
  }, []);

  const distributeEvenly = useCallback(() => {
    if (!contributors) return;
    const evenSplit = Math.floor(100 / contributors.length);
    const remainder = 100 - evenSplit * contributors.length;

    const newSplits: Record<string, number> = {};
    contributors.forEach((c, i) => {
      newSplits[c.id] = evenSplit + (i === 0 ? remainder : 0);
    });
    setSplits(newSplits);
    setIsDirty(true);
  }, [contributors]);

  if (isLoading) {
    return <div>Loading contributors...</div>;
  }

  if (!contributors || contributors.length === 0) {
    return (
      <Alert variant="warning">
        Add contributors before setting royalty splits.
      </Alert>
    );
  }

  return (
    <Card className="royalty-split-editor">
      <CardBody>
        <div className="royalty-split-editor__header">
          <h4>Royalty Distribution</h4>
          <Button variant="ghost" size="sm" onClick={distributeEvenly}>
            Distribute Evenly
          </Button>
        </div>

        <div className="royalty-split-editor__total">
          <span>Total:</span>
          <span className={`royalty-split-editor__total-value ${isValid ? 'valid' : 'invalid'}`}>
            {totalSplit}%
          </span>
          {!isValid && (
            <span className="royalty-split-editor__total-hint">
              {totalSplit < 100 ? `${100 - totalSplit}% remaining` : `${totalSplit - 100}% over`}
            </span>
          )}
        </div>

        <ul className="royalty-split-editor__list">
          {contributors.map(contributor => (
            <li key={contributor.id} className="royalty-split-editor__item">
              <div className="royalty-split-editor__contributor">
                <span className="royalty-split-editor__name">{contributor.userId}</span>
                <span className="royalty-split-editor__role">{formatRole(contributor.role)}</span>
              </div>
              <div className="royalty-split-editor__input">
                <input
                  type="range"
                  min={0}
                  max={100}
                  value={splits[contributor.id] ?? 0}
                  onChange={e => handleSplitChange(contributor.id, Number(e.target.value))}
                  className="royalty-split-editor__slider"
                />
                <input
                  type="number"
                  min={0}
                  max={100}
                  value={splits[contributor.id] ?? 0}
                  onChange={e => handleSplitChange(contributor.id, Number(e.target.value))}
                  className="royalty-split-editor__number"
                />
                <span>%</span>
              </div>
            </li>
          ))}
        </ul>

        {!isValid && (
          <Alert variant="warning">
            Royalty splits must total exactly 100% before registration.
          </Alert>
        )}
      </CardBody>

      <CardFooter>
        <Button
          onClick={() => updateMutation.mutate()}
          disabled={!isDirty || !isValid}
          loading={updateMutation.isPending}
        >
          Save Splits
        </Button>
      </CardFooter>
    </Card>
  );
}

function formatRole(role: string): string {
  return role
    .split('_')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
}
