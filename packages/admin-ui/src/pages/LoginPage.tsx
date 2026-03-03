import type { FormEvent } from "react";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth";

function LoginPage() {
  const { isAuthenticated, isLoading, login } = useAuth();
  const navigate = useNavigate();
  const [loginInProgress, setLoginInProgress] = useState(false);
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Redirect to admin if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      navigate("/admin");
    }
  }, [isAuthenticated, navigate]);

  const handleLogin = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setErrorMessage(null);

    if (!username.trim() || !password) {
      setErrorMessage("Username and password are required.");
      return;
    }

    setLoginInProgress(true);
    try {
      await login(username.trim(), password);
      // Navigation will happen automatically via useEffect once authenticated
    } catch (error) {
      console.error("Login failed:", error);
      setErrorMessage("Invalid credentials. Please try again.");
    } finally {
      setLoginInProgress(false);
    }
  };

  if (isLoading) {
    return (
      <div className="d-flex justify-content-center align-items-center admin-full-height">
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
              <p className="text-center text-muted mb-4">Sign in with your admin credentials</p>
              <form onSubmit={handleLogin}>
                <div className="mb-3">
                  <label htmlFor="username" className="form-label">
                    Username
                  </label>
                  <input
                    id="username"
                    type="text"
                    className="form-control"
                    value={username}
                    onChange={event => setUsername(event.target.value)}
                    autoComplete="username"
                    disabled={loginInProgress}
                    required
                  />
                </div>

                <div className="mb-3">
                  <label htmlFor="password" className="form-label">
                    Password
                  </label>
                  <input
                    id="password"
                    type="password"
                    className="form-control"
                    value={password}
                    onChange={event => setPassword(event.target.value)}
                    autoComplete="current-password"
                    disabled={loginInProgress}
                    required
                  />
                </div>

                {errorMessage ? (
                  <div className="alert alert-danger" role="alert">
                    {errorMessage}
                  </div>
                ) : null}

                <button type="submit" className="btn btn-primary w-100" disabled={loginInProgress}>
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
                    "Sign in"
                  )}
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default LoginPage;
