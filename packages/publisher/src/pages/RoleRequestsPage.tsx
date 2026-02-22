import { useSearchParams } from 'react-router-dom';
import { RoleRequestList } from '@/features/Contributor/components/RoleRequestList';
import { Card, CardBody, CardHeader } from '@/components';

export function RoleRequestsPage() {
  const [searchParams] = useSearchParams();
  const storyId = searchParams.get('storyId') || undefined;

  return (
    <div className="page page--role-requests">
      <Card>
        <CardHeader>
          <h1>Role Requests</h1>
          <p>Review and respond to contributor applications</p>
        </CardHeader>
        <CardBody>
          <RoleRequestList storyId={storyId} />
        </CardBody>
      </Card>
    </div>
  );
}

