import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Link } from "react-router-dom";
import { scenariosApi } from "../api/scenarios";
import ConfirmationDialog from "../components/ConfirmationDialog";
import ErrorAlert from "../components/ErrorAlert";
import LoadingSpinner from "../components/LoadingSpinner";
import Pagination from "../components/Pagination";
import SearchBar from "../components/SearchBar";
import { showToast } from "../utils/toast";

function ScenariosPage() {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [searchTerm, setSearchTerm] = useState("");
  const [deleteConfirm, setDeleteConfirm] = useState<{ isOpen: boolean; id: string | null }>({
    isOpen: false,
    id: null,
  });
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useQuery({
    queryKey: ["scenarios", page, pageSize, searchTerm],
    queryFn: () =>
      scenariosApi.getScenarios({
        page,
        pageSize,
        searchTerm: searchTerm || undefined,
      }),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => scenariosApi.deleteScenario(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["scenarios"] });
      showToast.success("Scenario deleted successfully");
    },
    onError: () => {
      showToast.error("Failed to delete scenario");
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
    return <LoadingSpinner message="Loading scenarios..." />;
  }

  if (error) {
    return (
      <ErrorAlert
        error={error}
        title="Error loading scenarios"
        onRetry={() => queryClient.invalidateQueries({ queryKey: ["scenarios"] })}
      />
    );
  }

  const totalPages = data ? Math.ceil(data.totalCount / pageSize) : 0;

  return (
    <div>
      <ConfirmationDialog
        isOpen={deleteConfirm.isOpen}
        title="Delete Scenario"
        message="Are you sure you want to delete this scenario? This action cannot be undone."
        confirmText="Delete"
        cancelText="Cancel"
        onConfirm={handleDeleteConfirm}
        onCancel={handleDeleteCancel}
        variant="danger"
      />
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">📚 Scenarios</h1>
        <div className="btn-toolbar mb-2 mb-md-0">
          <div className="btn-group me-2">
            <Link to="/admin/scenarios/create" className="btn btn-sm btn-primary">
              <i className="bi bi-plus-circle"></i> Create
            </Link>
            <Link to="/admin/scenarios/import" className="btn btn-sm btn-outline-primary">
              <i className="bi bi-upload"></i> Import
            </Link>
            <Link to="/admin/scenarios/validate" className="btn btn-sm btn-outline-success">
              <i className="bi bi-check-circle"></i> Validate
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
        placeholder="Search scenarios..."
        onSearchReset={handleSearchReset}
      />

      <div className="card">
        <div className="card-body">
          {data && data.scenarios.length > 0 ? (
            <>
              <div className="table-responsive">
                <table className="table table-hover">
                  <thead>
                    <tr>
                      <th>Title</th>
                      <th>Age Rating</th>
                      <th>Tags</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.scenarios.map(scenario => (
                      <tr key={scenario.id}>
                        <td>
                          <Link to={`/admin/scenarios/edit/${scenario.id}`}>{scenario.title}</Link>
                        </td>
                        <td>{scenario.ageRating}</td>
                        <td>
                          {scenario.tags && scenario.tags.length > 0
                            ? scenario.tags.join(", ")
                            : "-"}
                        </td>
                        <td>
                          <div className="btn-group btn-group-sm">
                            <Link
                              to={`/admin/scenarios/edit/${scenario.id}`}
                              className="btn btn-outline-primary"
                            >
                              <i className="bi bi-pencil"></i> Edit
                            </Link>
                            <button
                              className="btn btn-outline-danger"
                              onClick={() => handleDeleteClick(scenario.id)}
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
              <p className="text-muted">No scenarios found.</p>
              <div className="d-flex gap-2 justify-content-center">
                <Link to="/admin/scenarios/create" className="btn btn-primary">
                  Create Your First Scenario
                </Link>
                <Link to="/admin/scenarios/import" className="btn btn-outline-primary">
                  Import Scenario
                </Link>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default ScenariosPage;
