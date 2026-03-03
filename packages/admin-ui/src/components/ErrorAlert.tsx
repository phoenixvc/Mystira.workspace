interface ErrorAlertProps {
  error: Error | unknown;
  title?: string;
  onRetry?: () => void;
}

function ErrorAlert({ error, title = "Error", onRetry }: ErrorAlertProps) {
  const message = error instanceof Error ? error.message : "An unknown error occurred";

  return (
    <div className="alert alert-danger" role="alert">
      <h5 className="alert-heading">
        <i className="bi bi-exclamation-triangle me-2"></i>
        {title}
      </h5>
      <p className="mb-0">{message}</p>
      {onRetry && (
        <>
          <hr />
          <button className="btn btn-outline-danger btn-sm" onClick={onRetry}>
            <i className="bi bi-arrow-clockwise me-1"></i>
            Retry
          </button>
        </>
      )}
    </div>
  );
}

export default ErrorAlert;
