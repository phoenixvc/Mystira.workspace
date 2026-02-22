import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Link } from "react-router-dom";
import { characterMapsApi } from "../api/characterMaps";
import ConfirmationDialog from "../components/ConfirmationDialog";
import ErrorAlert from "../components/ErrorAlert";
import LoadingSpinner from "../components/LoadingSpinner";
import Pagination from "../components/Pagination";
import SearchBar from "../components/SearchBar";
import { showToast } from "../utils/toast";

function CharacterMapsPage() {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [searchTerm, setSearchTerm] = useState("");
  const [deleteConfirm, setDeleteConfirm] = useState<{ isOpen: boolean; id: string | null }>({
    isOpen: false,
    id: null,
  });
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useQuery({
    queryKey: ["characterMaps", page, pageSize, searchTerm],
    queryFn: () =>
      characterMapsApi.getCharacterMaps({
        page,
        pageSize,
        searchTerm: searchTerm || undefined,
      }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => characterMapsApi.deleteCharacterMap(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["characterMaps"] });
      showToast.success("Character map deleted successfully");
    },
    onError: () => {
      showToast.error("Failed to delete character map");
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
    return <LoadingSpinner message="Loading character maps..." />;
  }

  if (error) {
    return (
      <ErrorAlert
        error={error}
        title="Error loading character maps"
        onRetry={() => queryClient.invalidateQueries({ queryKey: ["characterMaps"] })}
      />
    );
  }

  const totalPages = data ? Math.ceil(data.totalCount / pageSize) : 0;

  return (
    <div>
      <ConfirmationDialog
        isOpen={deleteConfirm.isOpen}
        title="Delete Character Map"
        message="Are you sure you want to delete this character map? This action cannot be undone."
        confirmText="Delete"
        cancelText="Cancel"
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
        variant="danger"
      />
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">🗺️ Character Maps</h1>
        <div className="btn-toolbar mb-2 mb-md-0">
          <div className="btn-group me-2">
            <Link to="/admin/character-maps/create" className="btn btn-sm btn-primary">
              <i className="bi bi-plus-circle"></i> Create Character Map
            </Link>
            <Link to="/admin/character-maps/import" className="btn btn-sm btn-outline-primary">
              <i className="bi bi-upload"></i> Import Character Map
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
        placeholder="Search character maps..."
        onSearchReset={handleSearchReset}
      />

      <div className="card">
        <div className="card-body">
          {data && data.characterMaps.length > 0 ? (
            <>
              <div className="table-responsive">
                <table className="table table-hover">
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>Description</th>
                      <th>Image ID</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.characterMaps.map(map => (
                      <tr key={map.id}>
                        <td>{map.name}</td>
                        <td>{map.description || "-"}</td>
                        <td>{map.imageId || "-"}</td>
                        <td>
                          <div className="btn-group btn-group-sm">
                            <Link
                              to={`/admin/character-maps/edit/${map.id}`}
                              className="btn btn-outline-primary"
                            >
                              <i className="bi bi-pencil"></i> Edit
                            </Link>
                            <button
                              className="btn btn-outline-danger"
                              onClick={() => handleDeleteClick(map.id)}
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
              <p className="text-muted">No character maps found.</p>
              <div className="d-flex gap-2 justify-content-center">
                <Link to="/admin/character-maps/create" className="btn btn-primary">
                  Create Your First Character Map
                </Link>
                <Link to="/admin/character-maps/import" className="btn btn-outline-primary">
                  Import Character Map
                </Link>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default CharacterMapsPage;
