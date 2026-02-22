import { ErrorInfo, useState } from "react";
import Alert from "./Alert";
import Card from "./Card";

interface ErrorDisplayProps {
  error: Error | null;
  errorInfo?: ErrorInfo | null;
  onReset?: () => void;
  showStackTrace?: boolean;
}

function ErrorDisplay({
  error,
  errorInfo,
  onReset,
  showStackTrace = import.meta.env.DEV,
}: ErrorDisplayProps) {
  const [expanded, setExpanded] = useState(false);

  const handleGoHome = () => {
    if (onReset) {
      onReset();
    }
    window.location.href = "/admin";
  };

  const handleReload = () => {
    if (onReset) {
      onReset();
    }
    window.location.reload();
  };

  const copyErrorToClipboard = () => {
    const errorText = `
Error: ${error?.message || "Unknown error"}

Stack Trace:
${error?.stack || "No stack trace available"}

Component Stack:
${errorInfo?.componentStack || "No component stack available"}
    `.trim();

    navigator.clipboard
      .writeText(errorText)
      .then(() => {
        alert("Error details copied to clipboard");
      })
      .catch(err => {
        console.error("Failed to copy to clipboard:", err);
        // Fallback for non-secure contexts or clipboard access denied
        try {
          const textArea = document.createElement("textarea");
          textArea.value = errorText;
          textArea.style.position = "fixed";
          textArea.style.left = "-999999px";
          document.body.appendChild(textArea);
          textArea.select();
          const success = document.execCommand("copy");
          document.body.removeChild(textArea);
          if (success) {
            alert("Error details copied to clipboard");
          } else {
            alert("Failed to copy error details. Please copy manually from the console.");
            console.log("Error details:", errorText);
          }
        } catch (fallbackErr) {
          alert("Failed to copy error details. Please copy manually from the console.");
          console.log("Error details:", errorText);
          console.error("Fallback copy failed:", fallbackErr);
        }
      });
  };

  return (
    <div className="container mt-5">
      <div className="row justify-content-center">
        <div className="col-lg-8">
          <Card>
            <div className="text-center mb-4">
              <i
                className="bi bi-exclamation-triangle-fill text-danger"
                style={{ fontSize: "4rem" }}
              ></i>
              <h1 className="h2 mt-3">Something went wrong</h1>
              <p className="text-muted">
                We encountered an unexpected error. Please try again or contact support if the
                problem persists.
              </p>
            </div>

            <Alert variant="danger" title="Error Details">
              <strong>Message:</strong> {error?.message || "Unknown error occurred"}
            </Alert>

            {showStackTrace && error && (
              <div className="mt-3">
                <button
                  className="btn btn-sm btn-outline-secondary mb-2"
                  onClick={() => setExpanded(!expanded)}
                >
                  <i className={`bi bi-chevron-${expanded ? "up" : "down"} me-2`}></i>
                  {expanded ? "Hide" : "Show"} Technical Details
                </button>

                {expanded && (
                  <>
                    <div className="mb-3">
                      <h6>Stack Trace</h6>
                      <pre
                        className="bg-light p-3 border rounded"
                        style={{ fontSize: "0.85rem", maxHeight: "300px", overflow: "auto" }}
                      >
                        <code>{error.stack || "No stack trace available"}</code>
                      </pre>
                    </div>

                    {errorInfo?.componentStack && (
                      <div className="mb-3">
                        <h6>Component Stack</h6>
                        <pre
                          className="bg-light p-3 border rounded"
                          style={{ fontSize: "0.85rem", maxHeight: "300px", overflow: "auto" }}
                        >
                          <code>{errorInfo.componentStack}</code>
                        </pre>
                      </div>
                    )}

                    <button
                      className="btn btn-sm btn-outline-primary"
                      onClick={copyErrorToClipboard}
                    >
                      <i className="bi bi-clipboard me-2"></i>
                      Copy Error Details
                    </button>
                  </>
                )}
              </div>
            )}

            <div className="d-flex gap-2 justify-content-center mt-4">
              <button className="btn btn-primary" onClick={handleGoHome}>
                <i className="bi bi-house me-2"></i>
                Go to Dashboard
              </button>
              <button className="btn btn-outline-secondary" onClick={handleReload}>
                <i className="bi bi-arrow-clockwise me-2"></i>
                Reload Page
              </button>
              {onReset && (
                <button className="btn btn-outline-secondary" onClick={onReset}>
                  <i className="bi bi-x-circle me-2"></i>
                  Try Again
                </button>
              )}
            </div>

            <div className="mt-4 text-center">
              <small className="text-muted">
                If this problem persists, please contact support with the error details above.
              </small>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}

export default ErrorDisplay;
