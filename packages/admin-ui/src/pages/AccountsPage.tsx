import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { accountsApi } from "../api/accounts";
import ErrorAlert from "../components/ErrorAlert";
import LoadingSpinner from "../components/LoadingSpinner";
import Pagination from "../components/Pagination";
import SearchBar from "../components/SearchBar";

function AccountsPage() {
  const [page, setPage] = useState(1);
  const [pageSize] = useState(20);
  const [searchTerm, setSearchTerm] = useState("");
  const queryClient = useQueryClient();

  const { data, isLoading, error } = useQuery({
    queryKey: ["accounts", page, pageSize, searchTerm],
    queryFn: () =>
      accountsApi.getAccounts({
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
    return <LoadingSpinner message="Loading accounts..." />;
  }

  if (error) {
    return (
      <ErrorAlert
        error={error}
        title="Error loading accounts"
        onRetry={() => queryClient.invalidateQueries({ queryKey: ["accounts"] })}
      />
    );
  }

  const totalPages = data ? Math.ceil(data.totalCount / pageSize) : 0;

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">
          <i className="bi bi-people me-2"></i>
          User Accounts
        </h1>
        <span className="badge bg-secondary">Read-Only</span>
      </div>

      <SearchBar
        value={searchTerm}
        onChange={value => {
          setSearchTerm(value);
          setPage(1);
        }}
        placeholder="Search accounts..."
        onSearchReset={handleSearchReset}
      />

      <div className="card">
        <div className="card-body">
          {data && data.accounts.length > 0 ? (
            <>
              <div className="table-responsive">
                <table className="table table-hover">
                  <thead>
                    <tr>
                      <th>ID</th>
                      <th>Email</th>
                      <th>Display Name</th>
                      <th>Status</th>
                      <th>Created</th>
                      <th>Last Login</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.accounts.map(account => (
                      <tr key={account.id}>
                        <td>
                          <code className="small">{account.id.slice(0, 8)}...</code>
                        </td>
                        <td>{account.email || "-"}</td>
                        <td>{account.displayName || "-"}</td>
                        <td>
                          {account.isActive !== undefined ? (
                            <span
                              className={`badge ${account.isActive ? "bg-success" : "bg-danger"}`}
                            >
                              {account.isActive ? "Active" : "Inactive"}
                            </span>
                          ) : (
                            "-"
                          )}
                        </td>
                        <td>
                          {account.createdAt
                            ? new Date(account.createdAt).toLocaleDateString()
                            : "-"}
                        </td>
                        <td>
                          {account.lastLoginAt
                            ? new Date(account.lastLoginAt).toLocaleDateString()
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
              <i className="bi bi-people text-muted" style={{ fontSize: "3rem" }}></i>
              <p className="text-muted mt-3">No accounts found.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default AccountsPage;
