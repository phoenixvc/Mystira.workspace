import { useQuery } from '@tanstack/react-query';
import { chainApi, type RegistrationResponse } from '@/api';
import { Alert, Spinner, Badge, Card, CardBody } from '@/components';

interface RegistrationStatusProps {
  storyId: string;
  result?: RegistrationResponse | null;
  isLoading?: boolean;
}

export function RegistrationStatus({ storyId, result, isLoading }: RegistrationStatusProps) {
  const { data: status } = useQuery({
    queryKey: ['registration-status', result?.transactionId],
    queryFn: () => chainApi.getRegistrationStatus(result!.transactionId),
    enabled: !!result?.transactionId && result.status === 'pending',
    refetchInterval: 5000, // Poll every 5 seconds for pending transactions
  });

  if (isLoading) {
    return (
      <div className="registration-status registration-status--loading">
        <Spinner size="lg" />
        <h3>Registering your story on-chain...</h3>
        <p>This may take a few moments. Please don't close this page.</p>
      </div>
    );
  }

  if (!result) {
    return (
      <Alert variant="info">
        Ready to register story {storyId} on the blockchain.
      </Alert>
    );
  }

  const currentStatus = status?.status || result.status;

  return (
    <div className="registration-status">
      <Card>
        <CardBody>
          <div className="registration-status__header">
            <h3>Registration {getStatusText(currentStatus)}</h3>
            <Badge variant={getStatusVariant(currentStatus)}>{currentStatus}</Badge>
          </div>

          {currentStatus === 'confirmed' && (
            <Alert variant="success" title="Successfully Registered!">
              Your story has been permanently recorded on the blockchain.
            </Alert>
          )}

          {currentStatus === 'failed' && (
            <Alert variant="error" title="Registration Failed">
              {status?.errorMessage || 'An error occurred during registration.'}
            </Alert>
          )}

          {currentStatus === 'pending' && (
            <Alert variant="warning" title="Pending Confirmation">
              Your transaction is being processed. Confirmations: {status?.confirmations || 0}
            </Alert>
          )}

          <dl className="registration-status__details">
            <dt>Transaction ID</dt>
            <dd>
              <code>{result.transactionId}</code>
            </dd>

            {result.blockNumber && (
              <>
                <dt>Block Number</dt>
                <dd>{result.blockNumber}</dd>
              </>
            )}

            <dt>Timestamp</dt>
            <dd>{new Date(result.timestamp).toLocaleString()}</dd>
          </dl>
        </CardBody>
      </Card>
    </div>
  );
}

function getStatusText(status: string): string {
  switch (status) {
    case 'confirmed':
      return 'Complete';
    case 'pending':
      return 'In Progress';
    case 'failed':
      return 'Failed';
    default:
      return status;
  }
}

function getStatusVariant(status: string) {
  switch (status) {
    case 'confirmed':
      return 'success' as const;
    case 'pending':
      return 'warning' as const;
    case 'failed':
      return 'danger' as const;
    default:
      return 'default' as const;
  }
}
