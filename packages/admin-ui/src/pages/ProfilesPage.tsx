import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { profilesApi } from "../api/accounts";
import ErrorAlert from "../components/ErrorAlert";
import LoadingSpinner from "../components/LoadingSpinner";
import Pagination from "../components/Pagination";
import SearchBar from "../components/SearchBar";

function ProfilesPage() {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [searchTerm, setSearchTerm] = useState("");
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useQuery({
    queryKey: ["profiles", page, pageSize, searchTerm],
    queryFn: () =>
      profilesApi.getProfiles({
        page,
        pageSize,
        searchTerm: searchTerm || undefined,
      }),
  });

  const handleSearchReset = () => {
    setSearchTerm("");
    setPage(1);
  };

  if (isLoading) {
    return <LoadingSpinner message="Loading profiles..." />;
  }

  if (error) {
    return (
      <ErrorAlert
        error={error}
        title="Error loading profiles"
        onRetry={() => queryClient.invalidateQueries({ queryKey: ["profiles"] })}
      />
    );
  }

  const totalPages = data ? Math.ceil(data.totalCount / pageSize) : 0;

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">
          <i className="bi bi-person-badge me-2"></i>
          User Profiles
        </h1>
        <span className="badge bg-secondary">Read-Only</span>
      </div>

      <SearchBar
        value={searchTerm}
        onChange={value => {
          setSearchTerm(value);
          setPage(1);
        }}
        placeholder="Search profiles..."
        onSearchReset={handleSearchReset}
      />

      <div className="card">
        <div className="card-body">
          {data && data.profiles.length > 0 ? (
            <>
              <div className="table-responsive">
                <table className="table table-hover">
                  <thead>
                    <tr>
                      <th>ID</th>
                      <th>Display Name</th>
                      <th>Age Group</th>
                      <th>Avatar</th>
                      <th>Created</th>
                      <th>Updated</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.profiles.map(profile => (
                      <tr key={profile.id}>
                        <td>
                          <code className="small">{profile.id.slice(0, 8)}...</code>
                        </td>
                        <td>{profile.displayName || "-"}</td>
                        <td>{profile.ageGroup || "-"}</td>
                        <td>
                          {profile.avatarId ? (
                            <code className="small">{profile.avatarId.slice(0, 8)}...</code>
                          ) : (
                            "-"
                          )}
                        </td>
                        <td>
                          {profile.createdAt
                            ? new Date(profile.createdAt).toLocaleDateString()
                            : "-"}
                        </td>
                        <td>
                          {profile.updatedAt
                            ? new Date(profile.updatedAt).toLocaleDateString()
                            : "-"}
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
              <i className="bi bi-person-badge text-muted" style={{ fontSize: "3rem" }}></i>
              <p className="text-muted mt-3">No profiles found.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default ProfilesPage;
