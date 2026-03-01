import { useState } from "react";
import Alert from "./Alert";
import {
  ValidationError,
  formatValidationErrors,
  groupErrorsByPath,
} from "../utils/schemaValidator";

interface ValidationResultsProps {
  valid: boolean;
  errors: ValidationError[];
}

function ValidationResults({ valid, errors }: ValidationResultsProps) {
  const [expandedPath, setExpandedPath] = useState<string | null>(null);

  if (valid) {
    return (
      <Alert variant="success" title="Validation Passed!">
        The scenario file is valid and ready to upload.
      </Alert>
    );
  }

  const groupedErrors = groupErrorsByPath(errors);
  const errorEntries = Array.from(groupedErrors.entries());

  return (
    <>
      <Alert variant="danger" title="Validation Failed!">
        Found {errors.length} error(s) in the scenario file.
      </Alert>

      <div className="accordion mb-3" id="validationErrorsAccordion">
        {errorEntries.map(([path, pathErrors], index) => {
          const isExpanded = expandedPath === path;
          const pathId = `error-path-${index}`;

          return (
            <div className="accordion-item" key={path}>
              <h2 className="accordion-header">
                <button
                  className={`accordion-button ${isExpanded ? "" : "collapsed"}`}
                  type="button"
                  onClick={() => setExpandedPath(isExpanded ? null : path)}
                  aria-expanded={isExpanded}
                  aria-controls={pathId}
                >
                  <strong>{path === "root" ? "Root Level" : path}</strong>
                  <span className="badge bg-danger ms-2">{pathErrors.length}</span>
                </button>
              </h2>
              <div
                id={pathId}
                className={`accordion-collapse collapse ${isExpanded ? "show" : ""}`}
              >
                <div className="accordion-body">
                  <ul className="list-group">
                    {pathErrors.map((error, idx) => (
                      <li key={idx} className="list-group-item list-group-item-danger">
                        {error.message}
                        {error.keyword && (
                          <small className="text-muted d-block mt-1">Rule: {error.keyword}</small>
                        )}
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            </div>
          );
        })}
      </div>

      <details>
        <summary className="btn btn-sm btn-outline-secondary">View All Errors as Text</summary>
        <pre className="mt-2 p-3 bg-light border rounded">
          <code>{formatValidationErrors(errors)}</code>
        </pre>
      </details>
    </>
  );
}

export default ValidationResults;
