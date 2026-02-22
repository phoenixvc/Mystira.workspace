import { useRouteError, isRouteErrorResponse, useNavigate } from "react-router-dom";
import Card from "../components/Card";
import Alert from "../components/Alert";
import { useState } from "react";

function ErrorPage() {
  const error = useRouteError();
  const navigate = useNavigate();
  const [showDetails, setShowDetails] = useState(import.meta.env.DEV);

  const getErrorInfo = () => {
    if (isRouteErrorResponse(error)) {
      return {
        status: error.status,
        statusText: error.statusText,
        message: error.data?.message || error.statusText,
        stack: null,
      };
    }

    if (error instanceof Error) {
      return {
        status: 500,
        statusText: "Internal Error",
        message: error.message,
        stack: error.stack,
      };
    }

    return {
      status: 500,
      statusText: "Unknown Error",
      message: "An unexpected error occurred",
      stack: null,
    };
  };

  const errorInfo = getErrorInfo();

  const handleGoHome = () => {
    navigate("/admin");
  };

  const handleReload = () => {
    window.location.reload();
  };

  const copyErrorToClipboard = () => {
    const errorText = `
Error ${errorInfo.status}: ${errorInfo.statusText}
Message: ${errorInfo.message}

${errorInfo.stack ? `Stack Trace:\n${errorInfo.stack}` : "No stack trace available"}
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
              <i className="bi bi-bug-fill text-danger" style={{ fontSize: "4rem" }}></i>
              <h1 className="display-4 fw-bold text-danger mt-3">{errorInfo.status}</h1>
              <h2 className="h3 mb-3">{errorInfo.statusText}</h2>
              <p className="text-muted">We encountered an error while processing your request.</p>
            </div>

            <Alert variant="danger" title="Error Message">
              {errorInfo.message}
            </Alert>

            {errorInfo.stack && (
              <div className="mt-3">
                <button
                  className="btn btn-sm btn-outline-secondary mb-2"
                  onClick={() => setShowDetails(!showDetails)}
                >
                  <i className={`bi bi-chevron-${showDetails ? "up" : "down"} me-2`}></i>
                  {showDetails ? "Hide" : "Show"} Stack Trace
                </button>

                {showDetails && (
                  <>
                    <pre
                      className="bg-light p-3 border rounded"
                      style={{ fontSize: "0.85rem", maxHeight: "400px", overflow: "auto" }}
                    >
                      <code>{errorInfo.stack}</code>
                    </pre>
                    <button
                      className="btn btn-sm btn-outline-primary mt-2"
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

export default ErrorPage;
