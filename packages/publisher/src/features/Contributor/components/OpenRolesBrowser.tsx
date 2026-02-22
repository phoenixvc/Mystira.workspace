import { useState } from 'react';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  Badge,
  Select,
  Spinner,
  EmptyState,
  Modal,
  Input,
} from '@/components';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { roleRequestsApi } from '@/api';
import type { OpenRole, ContributorRole, SubmitRoleRequestRequest } from '@/api/types';
import { formatDate } from '@/utils/format';
import { useAuthStore } from '@/state/authStore';

function formatRole(role: string): string {
  return role
    .split('_')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
}

export function OpenRolesBrowser() {
  const [selectedRole, setSelectedRole] = useState<ContributorRole | ''>('');
  const [selectedOpenRole, setSelectedOpenRole] = useState<OpenRole | null>(null);
  const [showApplicationModal, setShowApplicationModal] = useState(false);
  const [proposedSplit, setProposedSplit] = useState<number | undefined>();
  const [message, setMessage] = useState('');
  const [portfolio, setPortfolio] = useState('');
  const { user } = useAuthStore();
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['open-roles', selectedRole],
    queryFn: () =>
      roleRequestsApi.getOpenRoles({
        role: selectedRole || undefined,
        page: 1,
        pageSize: 50,
      }),
  });

  const openRoles = data?.items ?? [];

  const submitMutation = useMutation({
    mutationFn: (data: SubmitRoleRequestRequest) => roleRequestsApi.submitRoleRequest(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['open-roles'] });
      queryClient.invalidateQueries({ queryKey: ['role-requests'] });
      setShowApplicationModal(false);
      setSelectedOpenRole(null);
      setProposedSplit(undefined);
      setMessage('');
      setPortfolio('');
    },
  });

  const handleApply = (openRole: OpenRole) => {
    setSelectedOpenRole(openRole);
    setProposedSplit(openRole.splitPercentage);
    setShowApplicationModal(true);
  };

  const handleSubmitApplication = async () => {
    if (!selectedOpenRole) return;

    await submitMutation.mutateAsync({
      openRoleId: selectedOpenRole.id,
      proposedSplit: proposedSplit,
      message: message || undefined,
      portfolio: portfolio || undefined,
    });
  };

  const ROLE_FILTER_OPTIONS = [
    { value: '', label: 'All Roles' },
    { value: 'primary_author', label: 'Primary Author' },
    { value: 'co_author', label: 'Co-Author' },
    { value: 'illustrator', label: 'Illustrator' },
    { value: 'editor', label: 'Editor' },
    { value: 'moderator', label: 'Moderator' },
    { value: 'publisher', label: 'Publisher' },
  ];

  if (!user) {
    return (
      <EmptyState
        title="Sign in required"
        description="Please sign in to browse and apply for open roles."
      />
    );
  }

  return (
    <>
      <div className="open-roles-browser">
        <div className="open-roles-browser__header">
          <h2>Open Roles</h2>
          <p>Browse available roles and apply to contribute to stories</p>
        </div>

        <div className="open-roles-browser__filters">
          <Select
            label="Filter by Role"
            options={ROLE_FILTER_OPTIONS}
            value={selectedRole}
            onChange={e => setSelectedRole(e.target.value as ContributorRole | '')}
          />
        </div>

        {isLoading ? (
          <Spinner />
        ) : openRoles.length === 0 ? (
          <EmptyState
            title="No open roles available"
            description="Check back later for new opportunities."
          />
        ) : (
          <div className="open-roles-browser__list">
            {openRoles.map(role => (
              <Card key={role.id} className="open-roles-browser__item">
                <CardHeader>
                  <div className="open-roles-browser__item-header">
                    <div>
                      <h3>{role.storyTitle}</h3>
                      <h4>{formatRole(role.role)}</h4>
                    </div>
                    <Badge variant="info">{role.splitPercentage}% split</Badge>
                  </div>
                </CardHeader>
                <CardBody>
                  {role.description && (
                    <p className="open-roles-browser__description">{role.description}</p>
                  )}
                  {role.requirements && (
                    <div className="open-roles-browser__requirements">
                      <strong>Requirements:</strong> {role.requirements}
                    </div>
                  )}
                  <div className="open-roles-browser__meta">
                    <span>Posted {formatDate(role.createdAt)}</span>
                    {role.deadline && (
                      <span className="open-roles-browser__deadline">
                        Deadline: {formatDate(role.deadline)}
                      </span>
                    )}
                  </div>
                  <div className="open-roles-browser__actions">
                    <Button onClick={() => handleApply(role)}>Apply for Role</Button>
                  </div>
                </CardBody>
              </Card>
            ))}
          </div>
        )}
      </div>

      {showApplicationModal && selectedOpenRole && (
        <Modal
          isOpen
          onClose={() => {
            setShowApplicationModal(false);
            setSelectedOpenRole(null);
            setProposedSplit(undefined);
            setMessage('');
            setPortfolio('');
          }}
          title={`Apply for ${formatRole(selectedOpenRole.role)}`}
          size="md"
        >
          <div className="role-application-form">
            <div className="role-application-form__story-info">
              <strong>Story:</strong> {selectedOpenRole.storyTitle}
            </div>

            <Input
              label="Proposed Royalty Split (%)"
              type="number"
              min={0}
              max={100}
              step={0.1}
              value={proposedSplit ?? selectedOpenRole.splitPercentage}
              onChange={e => setProposedSplit(Number(e.target.value))}
              hint={`Default: ${selectedOpenRole.splitPercentage}%`}
            />

            <div className="role-application-form__field">
              <label htmlFor="message" className="input-label">
                Message (optional)
              </label>
              <textarea
                id="message"
                value={message}
                onChange={e => setMessage(e.target.value)}
                placeholder="Tell the publisher why you're a good fit for this role..."
                rows={4}
                className="input"
              />
            </div>

            <Input
              label="Portfolio Link (optional)"
              type="url"
              value={portfolio}
              onChange={e => setPortfolio(e.target.value)}
              placeholder="https://your-portfolio.com"
            />

            <div className="role-application-form__actions">
              <Button
                variant="outline"
                onClick={() => {
                  setShowApplicationModal(false);
                  setSelectedOpenRole(null);
                  setProposedSplit(undefined);
                  setMessage('');
                  setPortfolio('');
                }}
              >
                Cancel
              </Button>
              <Button onClick={handleSubmitApplication} loading={submitMutation.isPending}>
                Submit Application
              </Button>
            </div>
          </div>
        </Modal>
      )}
    </>
  );
}
