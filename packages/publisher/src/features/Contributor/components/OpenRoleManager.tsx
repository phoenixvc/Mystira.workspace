import { useState } from 'react';
import { Button, Card, CardBody, CardHeader } from '@/components';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { roleRequestsApi } from '@/api';
import type { OpenRole } from '@/api/types';
import { OpenRoleForm } from './OpenRoleForm';
import { OpenRoleList } from './OpenRoleList';

interface OpenRoleManagerProps {
  storyId: string;
  storyTitle: string;
  readonly?: boolean;
}

export function OpenRoleManager({ storyId, storyTitle, readonly = false }: OpenRoleManagerProps) {
  const [showForm, setShowForm] = useState(false);
  const [editingRole, setEditingRole] = useState<OpenRole | null>(null);
  const queryClient = useQueryClient();

  const { data: openRoles, isLoading } = useQuery({
    queryKey: ['open-roles', storyId],
    queryFn: () => roleRequestsApi.getOpenRolesByStory(storyId),
    enabled: !!storyId,
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => roleRequestsApi.deleteOpenRole(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['open-roles', storyId] });
    },
  });

  const handleEdit = (role: OpenRole) => {
    setEditingRole(role);
    setShowForm(true);
  };

  const handleClose = () => {
    setShowForm(false);
    setEditingRole(null);
  };

  return (
    <div className="open-role-manager">
      <Card>
        <CardHeader>
          <div className="open-role-manager__header">
            <div>
              <h3>Open Roles</h3>
              <p className="open-role-manager__subtitle">
                Make roles available for contributors to apply
              </p>
            </div>
            {!readonly && (
              <Button variant="outline" size="sm" onClick={() => setShowForm(true)}>
                Add Open Role
              </Button>
            )}
          </div>
        </CardHeader>
        <CardBody>
          <OpenRoleList
            roles={openRoles ?? []}
            isLoading={isLoading}
            onEdit={readonly ? undefined : handleEdit}
            onDelete={readonly ? undefined : id => deleteMutation.mutate(id)}
          />
        </CardBody>
      </Card>

      {showForm && (
        <OpenRoleForm
          storyId={storyId}
          storyTitle={storyTitle}
          initialData={editingRole}
          onSubmit={async data => {
            if (editingRole) {
              await roleRequestsApi.updateOpenRole(editingRole.id, data);
            } else {
              await roleRequestsApi.createOpenRole(data);
            }
            queryClient.invalidateQueries({ queryKey: ['open-roles', storyId] });
            handleClose();
          }}
          onCancel={handleClose}
        />
      )}
    </div>
  );
}

