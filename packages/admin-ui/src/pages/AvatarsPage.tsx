import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { avatarsApi } from "../api/avatars";
import ConfirmationDialog from "../components/ConfirmationDialog";
import ErrorAlert from "../components/ErrorAlert";
import LoadingSpinner from "../components/LoadingSpinner";
import TextInput from "../components/TextInput";
import { showToast } from "../utils/toast";

function AvatarsPage() {
  const [selectedAgeGroup, setSelectedAgeGroup] = useState<string>("");
  const [newMediaId, setNewMediaId] = useState("");
  const [deleteConfirm, setDeleteConfirm] = useState<{
    isOpen: boolean;
    ageGroup: string | null;
    mediaId: string | null;
  }>({
    isOpen: false,
    ageGroup: null,
    mediaId: null,
  });
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useQuery({
    queryKey: ["avatars"],
    queryFn: () => avatarsApi.getAllAvatars(),
  });

  const addMutation = useMutation({
    mutationFn: ({ ageGroup, mediaId }: { ageGroup: string; mediaId: string }) =>
      avatarsApi.addAvatarToAgeGroup(ageGroup, mediaId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["avatars"] });
      showToast.success("Avatar added successfully");
      setNewMediaId("");
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to add avatar");
    },
  });

  const removeMutation = useMutation({
    mutationFn: ({ ageGroup, mediaId }: { ageGroup: string; mediaId: string }) =>
      avatarsApi.removeAvatarFromAgeGroup(ageGroup, mediaId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["avatars"] });
      showToast.success("Avatar removed successfully");
    },
    onError: error => {
      showToast.error(error instanceof Error ? error.message : "Failed to remove avatar");
    },
  });

  const handleAddAvatar = async (ageGroup: string) => {
    if (!newMediaId.trim()) {
      showToast.error("Please enter a media ID");
      return;
    }
    await addMutation.mutateAsync({ ageGroup, mediaId: newMediaId.trim() });
  };

  const handleRemoveClick = (ageGroup: string, mediaId: string) => {
    setDeleteConfirm({ isOpen: true, ageGroup, mediaId });
  };

  const handleRemoveConfirm = async () => {
    if (deleteConfirm.ageGroup && deleteConfirm.mediaId) {
      try {
        await removeMutation.mutateAsync({
          ageGroup: deleteConfirm.ageGroup,
          mediaId: deleteConfirm.mediaId,
        });
        setDeleteConfirm({ isOpen: false, ageGroup: null, mediaId: null });
      } catch {
        // Error handled by onError callback
      }
    }
  };

  const handleRemoveCancel = () => {
    setDeleteConfirm({ isOpen: false, ageGroup: null, mediaId: null });
  };

  if (isLoading) {
    return <LoadingSpinner message="Loading avatars..." />;
  }

  if (error) {
    return (
      <ErrorAlert
        error={error}
        title="Error loading avatars"
        onRetry={() => queryClient.invalidateQueries({ queryKey: ["avatars"] })}
      />
    );
  }

  const avatarConfigs = data?.avatars || [];

  return (
    <div>
      <ConfirmationDialog
        isOpen={deleteConfirm.isOpen}
        title="Remove Avatar"
        message="Are you sure you want to remove this avatar from the age group?"
        confirmText="Remove"
        cancelText="Cancel"
        onConfirm={handleRemoveConfirm}
        onCancel={handleRemoveCancel}
        variant="danger"
      />
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">👤 Avatars</h1>
      </div>

      <div className="card mb-3">
        <div className="card-body">
          <p className="text-muted mb-0">
            Manage avatar media IDs for different age groups. Each age group can have multiple
            avatars that users can choose from.
          </p>
        </div>
      </div>

      {avatarConfigs.length > 0 ? (
        <div className="accordion" id="avatarAccordion">
          {avatarConfigs.map(config => (
            <div className="accordion-item" key={config.ageGroup}>
              <h2 className="accordion-header">
                <button
                  className={`accordion-button ${selectedAgeGroup === config.ageGroup ? "" : "collapsed"}`}
                  type="button"
                  onClick={() => {
                    setNewMediaId("");
                    setSelectedAgeGroup(
                      selectedAgeGroup === config.ageGroup ? "" : config.ageGroup
                    );
                  }}
                >
                  <strong>{config.ageGroup}</strong>
                  <span className="badge bg-primary ms-2">{config.avatarMediaIds.length}</span>
                </button>
              </h2>
              <div
                className={`accordion-collapse collapse ${selectedAgeGroup === config.ageGroup ? "show" : ""}`}
              >
                <div className="accordion-body">
                  {config.avatarMediaIds.length > 0 ? (
                    <div className="table-responsive mb-3">
                      <table className="table table-sm">
                        <thead>
                          <tr>
                            <th>Media ID</th>
                            <th style={{ width: "100px" }}>Actions</th>
                          </tr>
                        </thead>
                        <tbody>
                          {config.avatarMediaIds.map(mediaId => (
                            <tr key={mediaId}>
                              <td>
                                <code>{mediaId}</code>
                              </td>
                              <td>
                                <button
                                  className="btn btn-sm btn-outline-danger"
                                  onClick={() => handleRemoveClick(config.ageGroup, mediaId)}
                                  disabled={removeMutation.isPending}
                                >
                                  <i className="bi bi-trash"></i>
                                </button>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  ) : (
                    <div className="alert alert-info mb-3">
                      No avatars configured for this age group yet.
                    </div>
                  )}

                  <div className="card bg-light">
                    <div className="card-body">
                      <h6 className="card-title">Add Avatar</h6>
                      <div className="input-group">
                        <TextInput
                          id={`mediaId-${config.ageGroup}`}
                          value={selectedAgeGroup === config.ageGroup ? newMediaId : ""}
                          onChange={e => setNewMediaId(e.target.value)}
                          placeholder="Enter media ID"
                          disabled={addMutation.isPending}
                        />
                        <button
                          className="btn btn-primary"
                          onClick={() => handleAddAvatar(config.ageGroup)}
                          disabled={addMutation.isPending || !newMediaId.trim()}
                        >
                          {addMutation.isPending ? (
                            <>
                              <span
                                className="spinner-border spinner-border-sm me-1"
                                role="status"
                                aria-hidden="true"
                              ></span>
                              Adding...
                            </>
                          ) : (
                            <>
                              <i className="bi bi-plus-circle"></i> Add
                            </>
                          )}
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <div className="card">
          <div className="card-body text-center py-5">
            <p className="text-muted">No avatar configurations found.</p>
          </div>
        </div>
      )}
    </div>
  );
}

export default AvatarsPage;
