import { useNavigate, useLocation } from "react-router-dom";
import Card from "../components/Card";

function NotFoundPage() {
  const navigate = useNavigate();
  const location = useLocation();

  const handleGoHome = () => {
    navigate("/admin");
  };

  const handleGoBack = () => {
    navigate(-1);
  };

  return (
    <div className="container mt-5">
      <div className="row justify-content-center">
        <div className="col-lg-6">
          <Card>
            <div className="text-center">
              <div className="mb-4">
                <i
                  className="bi bi-signpost-split-fill text-warning"
                  style={{ fontSize: "5rem" }}
                ></i>
              </div>
              <h1 className="display-1 fw-bold text-primary">404</h1>
              <h2 className="h3 mb-3">Page Not Found</h2>
              <p className="text-muted mb-4">
                The page you're looking for doesn't exist or has been moved.
              </p>

              <div className="alert alert-light border">
                <small className="text-muted">
                  <strong>Requested path:</strong>
                  <br />
                  <code>{location.pathname}</code>
                </small>
              </div>

              <div className="d-flex gap-2 justify-content-center mt-4">
                <button className="btn btn-primary" onClick={handleGoHome}>
                  <i className="bi bi-house me-2"></i>
                  Go to Dashboard
                </button>
                <button className="btn btn-outline-secondary" onClick={handleGoBack}>
                  <i className="bi bi-arrow-left me-2"></i>
                  Go Back
                </button>
              </div>

              <div className="mt-4">
                <h6 className="text-muted mb-3">Quick Links</h6>
                <div className="d-flex flex-wrap gap-2 justify-content-center">
                  <button
                    className="btn btn-sm btn-outline-primary"
                    onClick={() => navigate("/admin/scenarios")}
                  >
                    Scenarios
                  </button>
                  <button
                    className="btn btn-sm btn-outline-primary"
                    onClick={() => navigate("/admin/media")}
                  >
                    Media
                  </button>
                  <button
                    className="btn btn-sm btn-outline-primary"
                    onClick={() => navigate("/admin/badges")}
                  >
                    Badges
                  </button>
                  <button
                    className="btn btn-sm btn-outline-primary"
                    onClick={() => navigate("/admin/bundles")}
                  >
                    Bundles
                  </button>
                  <button
                    className="btn btn-sm btn-outline-primary"
                    onClick={() => navigate("/admin/avatars")}
                  >
                    Avatars
                  </button>
                </div>
              </div>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}

export default NotFoundPage;
