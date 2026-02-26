import { Component, type ReactNode } from "react";
import { Alert } from "./Alert";
import { Button } from "./Button";

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  hasError: boolean;
  error?: Error;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    // Dynamic import to avoid circular dependencies
    import("@/utils/logger").then(({ logger }) => {
      logger.error("ErrorBoundary caught an error:", error, errorInfo);
    });
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
        <div className="error-boundary">
          <Alert variant="error" title="Something went wrong">
            <p>
              An unexpected error occurred. Please try refreshing the page or
              contact support if the problem persists.
            </p>
            {import.meta.env.DEV && this.state.error && (
              <pre className="error-boundary__details">
                {this.state.error.message}
              </pre>
            )}
          </Alert>
          <Button onClick={this.handleReset} variant="outline">
            Try again
          </Button>
        </div>
      );
    }

    return this.props.children;
  }
}
