import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Link } from "react-router-dom";
import { mediaApi } from "../api/media";
import { showToast } from "../utils/toast";

function ImportMediaZipPage() {
  const [file, setFile] = useState<File | null>(null);
  const [overwriteMetadata, setOverwriteMetadata] = useState(false);
  const [overwriteMedia, setOverwriteMedia] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [uploadResult, setUploadResult] = useState<{
    success: boolean;
    message: string;
    processedFiles: number;
    errors: string[];
  } | null>(null);
  const queryClient = useQueryClient();

  const uploadMutation = useMutation({
    mutationFn: (data: { file: File; overwriteMetadata: boolean; overwriteMedia: boolean }) =>
      mediaApi.uploadMediaZip(data.file, data.overwriteMetadata, data.overwriteMedia),
    onSuccess: result => {
      queryClient.invalidateQueries({ queryKey: ["media"] });
      setUploadResult(result);
      setUploading(false);
      if (result.success) {
        showToast.success("Media ZIP uploaded successfully!");
      } else {
        showToast.error("Media ZIP upload completed with errors");
      }
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to upload media ZIP file");
      setUploading(false);
    },
  });

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0];
    if (selectedFile) {
      if (!selectedFile.name.endsWith(".zip")) {
        showToast.error("Please select a ZIP file");
        return;
      }
      setFile(selectedFile);
      setUploadResult(null);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!file) {
      showToast.error("Please select a ZIP file");
      return;
    }

    setUploading(true);
    try {
      await uploadMutation.mutateAsync({ file, overwriteMetadata, overwriteMedia });
    } catch {
      // Error handled in onError
    }
  };

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">📦 Import Media from ZIP</h1>
        <Link to="/admin/media" className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to Media
        </Link>
      </div>

      <div className="card mb-3">
        <div className="card-body">
          <h5 className="card-title">ZIP File Requirements</h5>
          <p className="card-text">The ZIP file must contain:</p>
          <ul>
            <li>
              <code>media-metadata.json</code> - A JSON file containing media metadata with image
              IDs and their corresponding filenames
            </li>
            <li>Media files (images, audio, video) referenced in the metadata file</li>
          </ul>
          <p className="card-text text-muted mb-0">
            The system will process the metadata file and upload the corresponding media files to
            the database.
          </p>
        </div>
      </div>

      <div className="card mb-3">
        <div className="card-body">
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label htmlFor="zipFile" className="form-label">
                ZIP File
              </label>
              <input
                type="file"
                className="form-control"
                id="zipFile"
                accept=".zip"
                onChange={handleFileChange}
                disabled={uploading}
                required
              />
              <div className="form-text">Select a ZIP file containing media and metadata</div>
            </div>

            {file && (
              <div className="mb-3">
                <div className="alert alert-info">
                  <strong>Selected file:</strong> {file.name} (
                  {(file.size / 1024 / 1024).toFixed(2)} MB)
                </div>
              </div>
            )}

            <div className="mb-3">
              <div className="form-check">
                <input
                  className="form-check-input"
                  type="checkbox"
                  id="overwriteMetadata"
                  checked={overwriteMetadata}
                  onChange={e => setOverwriteMetadata(e.target.checked)}
                  disabled={uploading}
                />
                <label className="form-check-label" htmlFor="overwriteMetadata">
                  Overwrite existing metadata
                </label>
              </div>
              <div className="form-text">
                If checked, existing metadata entries will be updated with new values
              </div>
            </div>

            <div className="mb-3">
              <div className="form-check">
                <input
                  className="form-check-input"
                  type="checkbox"
                  id="overwriteMedia"
                  checked={overwriteMedia}
                  onChange={e => setOverwriteMedia(e.target.checked)}
                  disabled={uploading}
                />
                <label className="form-check-label" htmlFor="overwriteMedia">
                  Overwrite existing media files
                </label>
              </div>
              <div className="form-text">
                If checked, existing media files will be replaced with new ones
              </div>
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
                    <i className="bi bi-upload"></i> Upload ZIP
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

      {uploadResult && (
        <div className="card">
          <div className="card-body">
            <h5 className="card-title">Upload Results</h5>
            <div className={`alert ${uploadResult.success ? "alert-success" : "alert-warning"}`}>
              <strong>{uploadResult.message}</strong>
              <p className="mb-0">Processed files: {uploadResult.processedFiles}</p>
            </div>

            {uploadResult.errors.length > 0 && (
              <div className="mt-3">
                <h6>Errors:</h6>
                <ul className="list-group">
                  {uploadResult.errors.map((error, index) => (
                    <li key={index} className="list-group-item list-group-item-danger">
                      {error}
                    </li>
                  ))}
                </ul>
              </div>
            )}

            <div className="mt-3">
              <Link to="/admin/media" className="btn btn-primary">
                Go to Media List
              </Link>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default ImportMediaZipPage;
