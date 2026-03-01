import { useMutation } from "@tanstack/react-query";
import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { scenariosApi } from "../api/scenarios";
import Alert from "../components/Alert";
import Card from "../components/Card";
import Checkbox from "../components/Checkbox";
import ConfirmationDialog from "../components/ConfirmationDialog";
import FileInput from "../components/FileInput";
import ValidationResults from "../components/ValidationResults";
import { useFileValidation } from "../hooks/useFileValidation";
import { useFileUpload } from "../hooks/useFileUpload";
import { showToast } from "../utils/toast";

function ImportScenarioPage() {
  const [file, setFile] = useState<File | null>(null);
  const [overwriteExisting, setOverwriteExisting] = useState(false);
  const navigate = useNavigate();

  const { validating, validationResult, validateFile, resetValidation } = useFileValidation();

  const uploadMutation = useMutation({
    mutationFn: (file: File) => scenariosApi.uploadScenario(file, overwriteExisting),
    onSuccess: data => {
      showToast.success(data.message || "Scenario uploaded successfully!");
      navigate("/admin/scenarios");
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to upload scenario file");
    },
  });

  const { uploading, uploadFile, confirmationProps } = useFileUpload({
    onUpload: async file => {
      await uploadMutation.mutateAsync(file);
    },
    validationResult,
  });

  const handleFileChange = (selectedFile: File | null) => {
    setFile(selectedFile);
    resetValidation();
  };

  const handleValidate = () => {
    if (file) {
      validateFile(file);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (file) {
      await uploadFile(file);
    }
  };

  const isProcessing = uploading || validating;

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">📥 Import Scenario</h1>
        <Link to="/admin/scenarios" className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to Scenarios
        </Link>
      </div>

      <Card
        title="Schema Validation"
        subtitle="Validate your scenario file against the Mystira story schema to catch structural or formatting issues before uploading."
        className="mb-3"
      >
        <Alert variant="info">
          Validation checks for required fields, correct data types, valid enums, and schema
          constraints.
        </Alert>
      </Card>

      <Card className="mb-3">
        <form onSubmit={handleSubmit}>
          <FileInput
            id="scenarioFile"
            label="Scenario File"
            accept=".yaml,.yml,.json"
            helpText="Select a YAML (.yaml, .yml) or JSON (.json) file containing scenario definition"
            selectedFile={file}
            onChange={handleFileChange}
            disabled={isProcessing}
            required
          />

          <div className="mb-3">
            <button
              type="button"
              className="btn btn-outline-primary"
              onClick={handleValidate}
              disabled={!file || isProcessing}
            >
              {validating ? (
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
                  <i className="bi bi-check-circle"></i> Validate Schema
                </>
              )}
            </button>
          </div>

          <Checkbox
            id="overwriteExisting"
            label="Overwrite existing scenario if title matches"
            checked={overwriteExisting}
            onChange={setOverwriteExisting}
            disabled={isProcessing}
          />

          <div className="d-flex gap-2">
            <button type="submit" className="btn btn-primary" disabled={!file || isProcessing}>
              {uploading ? (
                <>
                  <span
                    className="spinner-border spinner-border-sm me-2"
                    role="status"
                    aria-hidden="true"
                  ></span>
                  Uploading...
                </>
              ) : (
                <>
                  <i className="bi bi-upload"></i> Upload Scenario
                </>
              )}
            </button>
            <Link to="/admin/scenarios" className="btn btn-secondary">
              Cancel
            </Link>
          </div>
        </form>
      </Card>

      {validationResult && (
        <Card title="Validation Results">
          <ValidationResults valid={validationResult.valid} errors={validationResult.errors} />
        </Card>
      )}

      <ConfirmationDialog {...confirmationProps} />
    </div>
  );
}

export default ImportScenarioPage;
