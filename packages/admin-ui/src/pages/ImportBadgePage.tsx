import { useMutation, useQueryClient } from "@tanstack/react-query";
import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { badgesApi } from "../api/badges";
import { mediaApi } from "../api/media";
import { showToast } from "../utils/toast";

function ImportBadgePage() {
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const uploadMutation = useMutation({
    mutationFn: async (file: File) => {
      // First upload the image to media
      const mediaAsset = await mediaApi.uploadMedia(file);

      // Then create a badge with the uploaded image ID
      const badgeName = file.name.replace(/\.[^/.]+$/, ""); // Remove extension
      return badgesApi.createBadge({
        name: badgeName,
        imageId: mediaAsset.id,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["badges"] });
      queryClient.invalidateQueries({ queryKey: ["media"] });
      showToast.success("Badge uploaded successfully!");
      navigate("/admin/badges");
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to upload badge file");
      setUploading(false);
    },
  });

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = e.target.files?.[0];
    if (selectedFile) {
      // Badge files are typically images
      if (!selectedFile.type.startsWith("image/")) {
        showToast.error("Please select an image file");
        return;
      }
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
        <h1 className="h2">📥 Import Badge</h1>
        <Link to="/admin/badges" className="btn btn-sm btn-outline-secondary">
          <i className="bi bi-arrow-left"></i> Back to Badges
        </Link>
      </div>

      <div className="card">
        <div className="card-body">
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label htmlFor="badgeFile" className="form-label">
                Badge Image File
              </label>
              <input
                type="file"
                className="form-control"
                id="badgeFile"
                accept="image/*"
                onChange={handleFileChange}
                disabled={uploading}
                required
              />
              <div className="form-text">Select an image file for the badge</div>
            </div>

            {file && (
              <div className="mb-3">
                <div className="alert alert-info">
                  <strong>Selected file:</strong> {file.name} ({(file.size / 1024).toFixed(2)} KB)
                </div>
                {file.type.startsWith("image/") && (
                  <div className="mt-2">
                    <img
                      src={URL.createObjectURL(file)}
                      alt="Preview"
                      style={{ maxWidth: "200px", maxHeight: "200px", objectFit: "contain" }}
                      className="img-thumbnail"
                    />
                  </div>
                )}
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
                    <i className="bi bi-upload"></i> Upload Badge
                  </>
                )}
              </button>
              <Link to="/admin/badges" className="btn btn-secondary">
                Cancel
              </Link>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

export default ImportBadgePage;
