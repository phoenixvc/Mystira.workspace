import { useQuery } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { adminApi } from "../api/admin";
import ErrorAlert from "../components/ErrorAlert";
import LoadingSpinner from "../components/LoadingSpinner";

function DashboardPage() {
  const [stats, setStats] = useState({
    totalScenarios: 0,
    totalMedia: 0,
    totalBadges: 0,
    totalBundles: 0,
  });

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["admin", "stats"],
    queryFn: () => adminApi.getStats(),
  });

  useEffect(() => {
    if (data) {
      setStats(data);
    }
  }, [data]);

  const handleRefresh = () => {
    refetch();
  };

  if (isLoading) {
    return <LoadingSpinner message="Loading dashboard statistics..." />;
  }

  if (error) {
    return <ErrorAlert error={error} title="Error loading dashboard" onRetry={() => refetch()} />;
  }

  return (
    <div>
      <div className="d-flex justify-content-between flex-wrap flex-md-nowrap align-items-center pt-3 pb-2 mb-3 border-bottom">
        <h1 className="h2">📊 Content Dashboard</h1>
        <div className="btn-toolbar mb-2 mb-md-0">
          <div className="btn-group me-2">
            <button
              type="button"
              id="refreshStatsBtn"
              className="btn btn-sm btn-outline-secondary"
              onClick={handleRefresh}
            >
              <i className="bi bi-arrow-clockwise"></i> Refresh
            </button>
          </div>
        </div>
      </div>

      {/* Stats Cards */}
      <div className="row mb-4">
        <div className="col-xl-3 col-md-6 mb-4">
          <div className="card border-left-primary shadow h-100 py-2 card-stat">
            <div className="card-body">
              <div className="row no-gutters align-items-center">
                <div className="col mr-2">
                  <div className="text-xs font-weight-bold text-primary text-uppercase mb-1">
                    Total Scenarios
                  </div>
                  <div className="h5 mb-0 font-weight-bold text-gray-800">
                    {isLoading ? "-" : stats.totalScenarios}
                  </div>
                </div>
                <div className="col-auto">
                  <i className="bi bi-book fa-2x text-gray-300"></i>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="col-xl-3 col-md-6 mb-4">
          <div className="card border-left-success shadow h-100 py-2 card-stat">
            <div className="card-body">
              <div className="row no-gutters align-items-center">
                <div className="col mr-2">
                  <div className="text-xs font-weight-bold text-success text-uppercase mb-1">
                    Media Files
                  </div>
                  <div className="h5 mb-0 font-weight-bold text-gray-800">
                    {isLoading ? "-" : stats.totalMedia}
                  </div>
                </div>
                <div className="col-auto">
                  <i className="bi bi-image fa-2x text-gray-300"></i>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="col-xl-3 col-md-6 mb-4">
          <div className="card border-left-info shadow h-100 py-2 card-stat">
            <div className="card-body">
              <div className="row no-gutters align-items-center">
                <div className="col mr-2">
                  <div className="text-xs font-weight-bold text-info text-uppercase mb-1">
                    Badges
                  </div>
                  <div className="h5 mb-0 font-weight-bold text-gray-800">
                    {isLoading ? "-" : stats.totalBadges}
                  </div>
                </div>
                <div className="col-auto">
                  <i className="bi bi-award fa-2x text-gray-300"></i>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="col-xl-3 col-md-6 mb-4">
          <div className="card border-left-warning shadow h-100 py-2 card-stat">
            <div className="card-body">
              <div className="row no-gutters align-items-center">
                <div className="col mr-2">
                  <div className="text-xs font-weight-bold text-warning text-uppercase mb-1">
                    Bundles
                  </div>
                  <div className="h5 mb-0 font-weight-bold text-gray-800">
                    {isLoading ? "-" : stats.totalBundles}
                  </div>
                </div>
                <div className="col-auto">
                  <i className="bi bi-box fa-2x text-gray-300"></i>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="row">
        <div className="col-12">
          <div className="card">
            <div className="card-body">
              <h5 className="card-title">Welcome to Mystira Admin</h5>
              <p className="card-text">
                This is the admin dashboard. Use the navigation menu to manage scenarios, media,
                badges, and other content.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default DashboardPage;
