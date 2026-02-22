import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Link } from "react-router-dom";
import { mediaApi } from "../api/media";
import ConfirmationDialog from "../components/ConfirmationDialog";
import ErrorAlert from "../components/ErrorAlert";
import LoadingSpinner from "../components/LoadingSpinner";
import Pagination from "../components/Pagination";
import SearchBar from "../components/SearchBar";
import { showToast } from "../utils/toast";

function MediaPage() {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [searchTerm, setSearchTerm] = useState("");
  const [deleteConfirm, setDeleteConfirm] = useState<{ isOpen: boolean; id: string | null }>({
    isOpen: false,
    id: null,
  });
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useQuery({
    queryKey: ["media", page, pageSize, searchTerm],
    queryFn: () =>
      mediaApi.getMedia({
        page,
        pageSize,
        searchTerm: searchTerm || undefined,
      }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => mediaApi.deleteMedia(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["media"] });
      showToast.success("Media file deleted successfully");
    },
    onError: () => {
      showToast.error("Failed to delete media file");
    },
  });

  const handleDeleteClick = (id: string) => {
    setDeleteConfirm({ isOpen: true, id });
  };

  const handleDeleteConfirm = async () => {
    if (deleteConfirm.id) {
      try {
        await deleteMutation.mutateAsync(deleteConfirm.id);
        setDeleteConfirm({ isOpen: false, id: null });
      } catch {
        // Error handled by onError callback
      }
    }
  };

  const handleDeleteCancel = () => {
    setDeleteConfirm({ isOpen: false, id: null });
  };

  const handleSearchReset = () => {
    setSearchTerm("");
    setPage(1);
  };

  if (isLoading) {
    return <LoadingSpinner message="Loading media..." />;
  }

  if (error) {
    return (
      <ErrorAlert
        error={error}
        title="Error loading media"
        onRetry={() => queryClient.invalidateQueries({ queryKey: ["media"] })}
      />
    );
  }

  const totalPages = data ? Math.ceil(data.totalCount / pageSize) : 0;

  return (
    <div>
      <ConfirmationDialog
        isOpen={deleteConfirm.isOpen}
        title="Delete Media File"
        message="Are you sure you want to delete this media file? This action cannot be undone."
        confirmText="Delete"
        cancelText="Cancel"
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
        variant="danger"
      />
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">🖼️ Media</h1>
        <div className="btn-toolbar mb-2 mb-md-0">
          <div className="btn-group me-2">
            <Link to="/admin/media/import" className="btn btn-sm btn-primary">
              <i className="bi bi-upload"></i> Upload Media
            </Link>
            <Link to="/admin/media/import-zip" className="btn btn-sm btn-outline-primary">
              <i className="bi bi-file-zip"></i> Import ZIP
            </Link>
          </div>
        </div>
      </div>

      <SearchBar
        value={searchTerm}
        onChange={value => {
          setSearchTerm(value);
          setPage(1);
        }}
        placeholder="Search media..."
        onSearchReset={handleSearchReset}
      />

      <div className="card">
        <div className="card-body">
          {data && data.media.length > 0 ? (
            <>
              <div className="table-responsive">
                <table className="table table-hover">
                  <thead>
                    <tr>
                      <th>File Name</th>
                      <th>Type</th>
                      <th>Size</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.media.map(media => (
                      <tr key={media.id}>
                        <td>{media.fileName}</td>
                        <td>{media.contentType}</td>
                        <td>{media.size ? `${(media.size / 1024).toFixed(2)} KB` : "-"}</td>
                        <td>
                          <div className="btn-group btn-group-sm">
                            <a
                              href={`/api/admin/media/${media.id}`}
                              className="btn btn-outline-primary"
                              target="_blank"
                              rel="noopener noreferrer"
                            >
                              <i className="bi bi-download"></i> Download
                            </a>
                            <button
                              className="btn btn-outline-danger"
                              onClick={() => handleDeleteClick(media.id)}
                              disabled={deleteMutation.isPending}
                            >
                              <i className="bi bi-trash"></i> Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <Pagination currentPage={page} totalPages={totalPages} onPageChange={setPage} />
            </>
          ) : (
            <div className="text-center py-5">
              <p className="text-muted">No media files found.</p>
              <Link to="/admin/media/import" className="btn btn-primary">
                Upload Your First Media File
              </Link>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default MediaPage;
