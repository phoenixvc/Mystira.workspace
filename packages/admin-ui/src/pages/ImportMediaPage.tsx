import { useMutation, useQueryClient } from "@tanstack/react-query";
import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { mediaApi } from "../api/media";
import { showToast } from "../utils/toast";

function ImportMediaPage() {
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const uploadMutation = useMutation({
    mutationFn: (file: File) => mediaApi.uploadMedia(file),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["media"] });
      showToast.success("Media uploaded successfully!");
      navigate("/admin/media");
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to upload media file");
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
        <h1 className="h2">📥 Import Media</h1>
        <Link to="/admin/media" className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to Media
        </Link>
      </div>

      <div className="card">
        <div className="card-body">
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label htmlFor="mediaFile" className="form-label">
                Media File
              </label>
              <input
                type="file"
                className="form-control"
                id="mediaFile"
                onChange={handleFileChange}
                disabled={uploading}
                required
              />
              <div className="form-text">Select an image, audio, or video file to upload</div>
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
                    <i className="bi bi-upload"></i> Upload Media
                  </>
                )}
              </button>
              <Link to="/admin/media" className="btn btn-secondary">
                Cancel
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default ImportMediaPage;
