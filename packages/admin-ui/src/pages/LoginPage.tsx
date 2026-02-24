import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth";

function LoginPage() {
  const { isAuthenticated, isLoading, login } = useAuth();
  const navigate = useNavigate();
  const [loginInProgress, setLoginInProgress] = useState(false);

  // Redirect to admin if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      navigate("/admin");
    }
  }, [isAuthenticated, navigate]);

  const handleLogin = async () => {
    setLoginInProgress(true);
    try {
      await login();
      // Navigation will happen automatically via useEffect once authenticated
    } catch (error) {
      console.error("Login failed:", error);
    } finally {
      setLoginInProgress(false);
    }
  };

  if (isLoading) {
    return (
      <div
        className="d-flex justify-content-center align-items-center"
        style={{ minHeight: "100vh" }}
      >
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="container">
      <div className="row justify-content-center">
        <div className="col-md-6 col-lg-4">
          <div className="card shadow mt-5">
            <div className="card-body p-5">
              <h2 className="card-title text-center mb-4">Mystira Admin</h2>
              <p className="text-center text-muted mb-4">
                Sign in with your Microsoft account to continue
              </p>
              <button
                type="button"
                className="btn btn-primary w-100"
                onClick={handleLogin}
                disabled={loginInProgress}
              >
                {loginInProgress ? (
                  <>
                    <span
                      className="spinner-border spinner-border-sm me-2"
                      role="status"
                      aria-hidden="true"
                    ></span>
                    Signing in...
                  </>
                ) : (
                  <>
                    <i className="bi bi-microsoft me-2"></i>
                    Sign in with Microsoft
                  </>
                )}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default LoginPage;
