import { Link } from 'react-router-dom';
import { Button, EmptyState } from '@/components';

export function NotFoundPage() {
  return (
    <div className="page page--not-found">
      <EmptyState
        title="Page Not Found"
        description="The page you're looking for doesn't exist or has been moved."
        action={
          <Link to="/">
            <Button>Go to Home</Button>
          </Link>
        }
      />
    </div>
  );
}
