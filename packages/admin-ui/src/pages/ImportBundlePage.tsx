import { useMutation, useQueryClient } from "@tanstack/react-query";
import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { bundlesApi } from "../api/bundles";
import { showToast } from "../utils/toast";

function ImportBundlePage() {
  const [file, setFile] = useState<File | null>(null);
  const [validateReferences, setValidateReferences] = useState(true);
  const [overwriteExisting, setOverwriteExisting] = useState(false);
  const [uploading, setUploading] = useState(false);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const uploadMutation = useMutation({
    mutationFn: (file: File) =>
      bundlesApi.uploadBundle(file, validateReferences, overwriteExisting),
    onSuccess: data => {
      queryClient.invalidateQueries({ queryKey: ["bundles"] });
      if (data.success) {
        showToast.success("Bundle uploaded successfully!");
      } else {
        showToast.error(String(data.result));
      }
      navigate("/admin/bundles");
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to upload bundle file");
      setUploading(false);
    },
  });

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0];
    if (selectedFile) {
      setFile(selectedFile);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!file) {
      showToast.error("Please select a file");
      return;
    }

    setUploading(true);
    try {
      await uploadMutation.mutateAsync(file);
    } catch {
      // Error handled in onError
    }
  };

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">📥 Import Bundle</h1>
        <Link to="/admin/bundles" className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to Bundles
        </Link>
      </div>

      <div className="card">
        <div className="card-body">
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label htmlFor="bundleFile" className="form-label">
                Bundle File
              </label>
              <input
                type="file"
                className="form-control"
                id="bundleFile"
                onChange={handleFileChange}
                disabled={uploading}
                required
              />
              <div className="form-text">Select a bundle file to upload and process</div>
            </div>

            <div className="mb-3 form-check">
              <input
                type="checkbox"
                className="form-check-input"
                id="validateReferences"
                checked={validateReferences}
                onChange={e => setValidateReferences(e.target.checked)}
                disabled={uploading}
              />
              <label className="form-check-label" htmlFor="validateReferences">
                Validate references to media and scenarios
              </label>
            </div>

            <div className="mb-3 form-check">
              <input
                type="checkbox"
                className="form-check-input"
                id="overwriteExisting"
                checked={overwriteExisting}
                onChange={e => setOverwriteExisting(e.target.checked)}
                disabled={uploading}
              />
              <label className="form-check-label" htmlFor="overwriteExisting">
                Overwrite existing bundle if name matches
              </label>
            </div>

            <div className="d-flex gap-2">
              <button type="submit" className="btn btn-primary" disabled={!file || uploading}>
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
                    <i className="bi bi-upload"></i> Upload Bundle
                  </>
                )}
              </button>
              <Link to="/admin/bundles" className="btn btn-secondary">
                Cancel
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default ImportBundlePage;
