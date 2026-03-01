import { useMutation } from "@tanstack/react-query";
import { useState } from "react";
import { Link } from "react-router-dom";
import { scenariosApi, ScenarioReferenceValidation } from "../api/scenarios";
import { showToast } from "../utils/toast";

function ValidateScenariosPage() {
  const [includeMetadataValidation, setIncludeMetadataValidation] = useState(true);
  const [validationResults, setValidationResults] = useState<ScenarioReferenceValidation[] | null>(
    null
  );
  const [isValidating, setIsValidating] = useState(false);

  const validateMutation = useMutation({
    mutationFn: (includeMetadata: boolean) =>
      scenariosApi.validateAllScenarioReferences(includeMetadata),
    onSuccess: results => {
      setValidationResults(results);
      setIsValidating(false);
      const invalidCount = results.filter(r => !r.isValid).length;
      if (invalidCount === 0) {
        showToast.success("All scenarios validated successfully!");
      } else {
        showToast.error(`Found ${invalidCount} scenario(s) with validation issues`);
      }
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to validate scenarios");
      setIsValidating(false);
    },
  });

  const handleValidate = async () => {
    setIsValidating(true);
    setValidationResults(null);
    await validateMutation.mutateAsync(includeMetadataValidation);
  };

  const getValidationSummary = () => {
    if (!validationResults) return null;

    const totalScenarios = validationResults.length;
    const validScenarios = validationResults.filter(r => r.isValid).length;
    const invalidScenarios = totalScenarios - validScenarios;

    return { totalScenarios, validScenarios, invalidScenarios };
  };

  const summary = getValidationSummary();

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">✅ Validate Scenarios</h1>
        <Link to="/admin/scenarios" className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to Scenarios
        </Link>
      </div>

      <div className="card mb-3">
        <div className="card-body">
          <h5 className="card-title">Media Reference Validation</h5>
          <p className="card-text">
            This tool validates all media references (images, audio, video) in scenario scenes and
            characters against the media metadata in the database.
          </p>
          <p className="card-text text-muted mb-0">
            It checks for missing media files, invalid references, and metadata consistency issues.
          </p>
        </div>
      </div>

      <div className="card mb-3">
        <div className="card-body">
          <div className="mb-3">
            <div className="form-check">
              <input
                className="form-check-input"
                type="checkbox"
                id="includeMetadataValidation"
                checked={includeMetadataValidation}
                onChange={e => setIncludeMetadataValidation(e.target.checked)}
                disabled={isValidating}
              />
              <label className="form-check-label" htmlFor="includeMetadataValidation">
                Include metadata validation
              </label>
            </div>
            <div className="form-text">
              If checked, the validation will also check media metadata consistency
            </div>
          </div>

          <button className="btn btn-primary" onClick={handleValidate} disabled={isValidating}>
            {isValidating ? (
              <>
                <span
                  className="spinner-border spinner-border-sm me-2"
                  role="status"
                  aria-hidden="true"
                ></span>
                Validating...
              </>
            ) : (
              <>
                <i className="bi bi-check-circle"></i> Validate All Scenarios
              </>
            )}
          </button>
        </div>
      </div>

      {summary && (
        <div className="card mb-3">
          <div className="card-body">
            <h5 className="card-title">Validation Summary</h5>
            <div className="row">
              <div className="col-md-4">
                <div className="card bg-light">
                  <div className="card-body text-center">
                    <h3 className="mb-0">{summary.totalScenarios}</h3>
                    <p className="text-muted mb-0">Total Scenarios</p>
                  </div>
                </div>
              </div>
              <div className="col-md-4">
                <div className="card bg-success text-white">
                  <div className="card-body text-center">
                    <h3 className="mb-0">{summary.validScenarios}</h3>
                    <p className="mb-0">Valid</p>
                  </div>
                </div>
              </div>
              <div className="col-md-4">
                <div className="card bg-danger text-white">
                  <div className="card-body text-center">
                    <h3 className="mb-0">{summary.invalidScenarios}</h3>
                    <p className="mb-0">Invalid</p>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {validationResults && validationResults.length > 0 && (
        <div className="card">
          <div className="card-body">
            <h5 className="card-title">Validation Results</h5>
            <div className="accordion" id="validationAccordion">
              {validationResults.map((result, index) => (
                <div className="accordion-item" key={result.scenarioId}>
                  <h2 className="accordion-header">
                    <button
                      className={`accordion-button ${index !== 0 ? "collapsed" : ""}`}
                      type="button"
                      data-bs-toggle="collapse"
                      data-bs-target={`#collapse-${index}`}
                      aria-expanded={index === 0}
                    >
                      <span className="me-2">
                        {result.isValid ? (
                          <i className="bi bi-check-circle-fill text-success"></i>
                        ) : (
                          <i className="bi bi-x-circle-fill text-danger"></i>
                        )}
                      </span>
                      <strong>{result.scenarioName}</strong>
                      {!result.isValid && (
                        <span className="badge bg-danger ms-2">
                          {result.missingMediaReferences.length +
                            result.invalidMediaReferences.length +
                            result.metadataIssues.length}{" "}
                          issue(s)
                        </span>
                      )}
                    </button>
                  </h2>
                  <div
                    id={`collapse-${index}`}
                    className={`accordion-collapse collapse ${index === 0 ? "show" : ""}`}
                    data-bs-parent="#validationAccordion"
                  >
                    <div className="accordion-body">
                      {result.isValid ? (
                        <div className="alert alert-success mb-0">
                          <i className="bi bi-check-circle me-2"></i>
                          All media references are valid!
                        </div>
                      ) : (
                        <>
                          {result.missingMediaReferences.length > 0 && (
                            <div className="mb-3">
                              <h6 className="text-danger">
                                <i className="bi bi-exclamation-triangle me-2"></i>
                                Missing Media References ({result.missingMediaReferences.length})
                              </h6>
                              <ul className="list-group">
                                {result.missingMediaReferences.map((ref, idx) => (
                                  <li key={idx} className="list-group-item list-group-item-danger">
                                    <code>{ref}</code>
                                  </li>
                                ))}
                              </ul>
                            </div>
                          )}

                          {result.invalidMediaReferences.length > 0 && (
                            <div className="mb-3">
                              <h6 className="text-danger">
                                <i className="bi bi-exclamation-triangle me-2"></i>
                                Invalid Media References ({result.invalidMediaReferences.length})
                              </h6>
                              <ul className="list-group">
                                {result.invalidMediaReferences.map((ref, idx) => (
                                  <li key={idx} className="list-group-item list-group-item-danger">
                                    <code>{ref}</code>
                                  </li>
                                ))}
                              </ul>
                            </div>
                          )}

                          {result.metadataIssues.length > 0 && (
                            <div className="mb-3">
                              <h6 className="text-warning">
                                <i className="bi bi-info-circle me-2"></i>
                                Metadata Issues ({result.metadataIssues.length})
                              </h6>
                              <ul className="list-group">
                                {result.metadataIssues.map((issue, idx) => (
                                  <li key={idx} className="list-group-item list-group-item-warning">
                                    {issue}
                                  </li>
                                ))}
                              </ul>
                            </div>
                          )}

                          <Link
                            to={`/admin/scenarios/edit/${result.scenarioId}`}
                            className="btn btn-sm btn-primary"
                          >
                            <i className="bi bi-pencil"></i> Edit Scenario
                          </Link>
                        </>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default ValidateScenariosPage;
