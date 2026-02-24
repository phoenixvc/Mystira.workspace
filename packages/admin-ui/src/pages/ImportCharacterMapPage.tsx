import { useMutation, useQueryClient } from "@tanstack/react-query";
import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { characterMapsApi } from "../api/characterMaps";
import { showToast } from "../utils/toast";

function ImportCharacterMapPage() {
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const uploadMutation = useMutation({
    mutationFn: (file: File) => characterMapsApi.uploadCharacterMap(file),
    onSuccess: data => {
      queryClient.invalidateQueries({ queryKey: ["characterMaps"] });
      showToast.success(data.message || "Character map uploaded successfully!");
      navigate("/admin/character-maps");
    },
    onError: error => {
      showToast.error(
        error instanceof Error ? error.message : "Failed to upload character map file"
      );
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
        <h1 className="h2">📥 Import Character Map</h1>
        <Link to="/admin/character-maps" className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to Character Maps
        </Link>
      </div>

      <div className="card">
        <div className="card-body">
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label htmlFor="characterMapFile" className="form-label">
                Character Map File
              </label>
              <input
                type="file"
                className="form-control"
                id="characterMapFile"
                onChange={handleFileChange}
                disabled={uploading}
                required
              />
              <div className="form-text">Select a character map file to upload</div>
            </div>

            {file && (
              <div className="mb-3">
                <div className="alert alert-info">
                  <strong>Selected file:</strong> {file.name} ({(file.size / 1024).toFixed(2)} KB)
                </div>
              </div>
            )}

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
                    <i className="bi bi-upload"></i> Upload Character Map
                  </>
                )}
              </button>
              <Link to="/admin/character-maps" className="btn btn-secondary">
                Cancel
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default ImportCharacterMapPage;
