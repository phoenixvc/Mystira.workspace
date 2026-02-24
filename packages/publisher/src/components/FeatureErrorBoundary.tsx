import { Component, type ReactNode } from 'react';
import { Alert, Button } from './index';
import { logger } from '@/utils/logger';

interface FeatureErrorBoundaryProps {
  children: ReactNode;
  featureName: string;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class FeatureErrorBoundary extends Component<FeatureErrorBoundaryProps, State> {
  constructor(props: FeatureErrorBoundaryProps) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    logger.error(`Error in ${this.props.featureName}:`, error, errorInfo);
  }

  handleReset = () => {
    this.setState({ hasError: false, error: undefined });
  };

  render() {
    if (this.state.hasError) {
      if (this.props.fallback) {
        return this.props.fallback;
      }

      return (
        <div className="feature-error-boundary">
          <Alert variant="error" title={`Error in ${this.props.featureName}`}>
            <p>
              An error occurred in {this.props.featureName}. Please try refreshing this section or
              contact support if the problem persists.
            </p>
            {import.meta.env.DEV && this.state.error && (
              <pre className="feature-error-boundary__details">
                {this.state.error.message}
              </pre>
            )}
          </Alert>
          <Button onClick={this.handleReset} variant="outline">
            Try Again
          </Button>
        </div>
      );
    }

    return this.props.children;
  }
}

