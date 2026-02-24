import { roleRequestsApi } from '@/api';
import type { RoleRequest, RoleRequestStatus } from '@/api/types';
import {
  Badge,
  Button,
  Card,
  CardBody,
  CardHeader,
  EmptyState,
  Modal,
  Spinner,
} from '@/components';
import { formatDate } from '@/utils/format';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';

interface RoleRequestListProps {
  storyId?: string;
  openRoleId?: string;
}

function formatRole(role: string): string {
  return role
    .split('_')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ');
}

function getStatusBadgeVariant(
  status: RoleRequestStatus
): 'info' | 'success' | 'danger' | 'warning' {
  switch (status) {
    case 'pending':
      return 'info';
    case 'accepted':
      return 'success';
    case 'rejected':
      return 'danger';
    case 'withdrawn':
      return 'warning';
    default:
      return 'info';
  }
}

export function RoleRequestList({ storyId, openRoleId }: RoleRequestListProps) {
  const [selectedRequest, setSelectedRequest] = useState<RoleRequest | null>(null);
  const [showResponseModal, setShowResponseModal] = useState(false);
  const [responseMessage, setResponseMessage] = useState('');
  const [accepting, setAccepting] = useState(false);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['role-requests', storyId, openRoleId],
    queryFn: async () => {
      if (openRoleId) {
        return await roleRequestsApi.getRoleRequestsByOpenRole(openRoleId);
      }
      if (storyId) {
        return await roleRequestsApi.getRoleRequestsByStory(storyId);
      }
      const result = await roleRequestsApi.getRoleRequests();
      return Array.isArray(result) ? result : (result?.items ?? []);
    },
  });

  const requests = Array.isArray(data) ? data : [];

  const respondMutation = useMutation({
    mutationFn: ({ requestId, accept }: { requestId: string; accept: boolean }) =>
      roleRequestsApi.respondToRoleRequest({
        requestId,
        accept,
        message: responseMessage || undefined,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['role-requests'] });
      queryClient.invalidateQueries({ queryKey: ['open-roles'] });
      setShowResponseModal(false);
      setSelectedRequest(null);
      setResponseMessage('');
    },
  });

  const handleRespond = (request: RoleRequest, accept: boolean) => {
    setSelectedRequest(request);
    setAccepting(accept);
    setShowResponseModal(true);
  };

  const handleSubmitResponse = async () => {
    if (!selectedRequest) return;
    await respondMutation.mutateAsync({
      requestId: selectedRequest.id,
      accept: accepting,
    });
  };

  if (isLoading) {
    return <Spinner />;
  }

  if (requests.length === 0) {
    return (
      <EmptyState
        title="No role requests"
        description="No contributors have applied for roles yet."
      />
    );
  }

  return (
    <>
      <div className="role-request-list">
        {requests.map(request => (
          <Card key={request.id} className="role-request-list__item">
            <CardHeader>
              <div className="role-request-list__header">
                <div>
                  <h4>{request.userName}</h4>
                  <p className="role-request-list__email">{request.userEmail}</p>
                </div>
                <Badge variant={getStatusBadgeVariant(request.status)}>{request.status}</Badge>
              </div>
            </CardHeader>
            <CardBody>
              <div className="role-request-list__content">
                <div className="role-request-list__info">
                  <div>
                    <strong>Story:</strong> {request.storyTitle}
                  </div>
                  <div>
                    <strong>Role:</strong> {formatRole(request.role)}
                  </div>
                  {request.proposedSplit && (
                    <div>
                      <strong>Proposed Split:</strong> {request.proposedSplit}%
                    </div>
                  )}
                  {request.message && (
                    <div className="role-request-list__message">
                      <strong>Message:</strong>
                      <p>{request.message}</p>
                    </div>
                  )}
                  {request.portfolio && (
                    <div>
                      <strong>Portfolio:</strong>{' '}
                      <a href={request.portfolio} target="_blank" rel="noopener noreferrer">
                        View Portfolio
                      </a>
                    </div>
                  )}
                  <div className="role-request-list__meta">
                    <span>Applied {formatDate(request.createdAt)}</span>
                  </div>
                </div>
                {request.status === 'pending' && (
                  <div className="role-request-list__actions">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleRespond(request, false)}
                    >
                      Reject
                    </Button>
                    <Button size="sm" onClick={() => handleRespond(request, true)}>
                      Accept
                    </Button>
                  </div>
                )}
              </div>
            </CardBody>
          </Card>
        ))}
      </div>

      {showResponseModal && selectedRequest && (
        <Modal
          isOpen
          onClose={() => {
            setShowResponseModal(false);
            setSelectedRequest(null);
            setResponseMessage('');
          }}
          title={accepting ? 'Accept Role Request' : 'Reject Role Request'}
          size="md"
        >
          <div className="role-request-response">
            <p>
              {accepting
                ? `Accept ${selectedRequest.userName}'s application for ${formatRole(selectedRequest.role)}?`
                : `Reject ${selectedRequest.userName}'s application?`}
            </p>
            <textarea
              className="role-request-response__message"
              value={responseMessage}
              onChange={e => setResponseMessage(e.target.value)}
              placeholder="Optional message to the contributor..."
              rows={4}
            />
            <div className="role-request-response__actions">
              <Button
                variant="outline"
                onClick={() => {
                  setShowResponseModal(false);
                  setSelectedRequest(null);
                  setResponseMessage('');
                }}
              >
                Cancel
              </Button>
              <Button
                variant={accepting ? 'primary' : 'danger'}
                onClick={handleSubmitResponse}
                loading={respondMutation.isPending}
              >
                {accepting ? 'Accept' : 'Reject'} Request
              </Button>
            </div>
          </div>
        </Modal>
      )}
    </>
  );
}
