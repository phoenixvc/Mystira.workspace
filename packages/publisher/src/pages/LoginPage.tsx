import {
  Alert,
  Button,
  Card,
  CardBody,
  Input,
  ThemeToggle,
} from "@/components";
import { useAuth } from "@/hooks";
import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const {
    login,
    loginWithEntra,
    requestMagicLink,
    loginWithMagicLink,
    isLoggingIn,
    isRequestingMagicLink,
    loginError,
    magicLinkRequestError,
  } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [entraToken, setEntraToken] = useState("");
  const [magicLinkEmail, setMagicLinkEmail] = useState("");
  const [authMethod, setAuthMethod] = useState<
    "credentials" | "entra" | "magic"
  >("credentials");
  const [magicLinkSent, setMagicLinkSent] = useState(false);

  const from =
    (location.state as { from?: { pathname: string } })?.from?.pathname ||
    "/dashboard";

  const handleCredentialsLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await login({ email, password, provider: "credentials" });
      navigate(from, { replace: true });
    } catch {
      // Error is handled by loginError state
    }
  };

  const handleEntraLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await loginWithEntra(entraToken);
      navigate(from, { replace: true });
    } catch {
      // Error is handled by loginError state
    }
  };

  const handleRequestMagicLink = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await requestMagicLink(magicLinkEmail);
      setMagicLinkSent(true);
    } catch {
      // Error is handled by magicLinkRequestError state
    }
  };

  const handleMagicLinkLogin = async () => {
    try {
      await loginWithMagicLink(magicLinkEmail);
      navigate(from, { replace: true });
    } catch {
      // Error is handled by loginError state
    }
  };

  return (
    <>
      <header className="app__header">
        <div className="header">
          <Link to="/" className="header__logo">
            Mystira Publisher
          </Link>
          <nav className="header__nav">
            <Link to="/" className="header__link">
              Home
            </Link>
          </nav>
          <div className="header__user">
            <ThemeToggle />
          </div>
        </div>
      </header>
      <div className="page page--login">
        <Card className="login-card">
          <CardBody>
            <h1>Sign In</h1>
            <p>Welcome back to Mystira Publisher</p>

            {/* Auth Method Selector */}
            <div className="auth-method-selector">
              <Button
                variant={authMethod === "credentials" ? "primary" : "outline"}
                size="sm"
                onClick={() => setAuthMethod("credentials")}
              >
                Email & Password
              </Button>
              <Button
                variant={authMethod === "entra" ? "primary" : "outline"}
                size="sm"
                onClick={() => setAuthMethod("entra")}
              >
                Microsoft Entra
              </Button>
              <Button
                variant={authMethod === "magic" ? "primary" : "outline"}
                size="sm"
                onClick={() => setAuthMethod("magic")}
              >
                Magic Link
              </Button>
            </div>

            {loginError && (
              <Alert variant="error">
                {loginError instanceof Error
                  ? loginError.message
                  : "Login failed. Please try again."}
              </Alert>
            )}

            {magicLinkRequestError && (
              <Alert variant="error">
                {magicLinkRequestError instanceof Error
                  ? magicLinkRequestError.message
                  : "Failed to send magic link. Please try again."}
              </Alert>
            )}

            {/* Credentials Login Form */}
            {authMethod === "credentials" && (
              <form onSubmit={handleCredentialsLogin} className="login-form">
                <Input
                  label="Email"
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@example.com"
                  required
                  autoComplete="email"
                  autoFocus
                />

                <Input
                  label="Password"
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Enter your password"
                  required
                  autoComplete="current-password"
                />

                <div className="login-form__footer">
                  <Button
                    type="submit"
                    loading={isLoggingIn}
                    fullWidth
                    size="lg"
                  >
                    Sign In
                  </Button>
                </div>
              </form>
            )}

            {/* Entra Login Form */}
            {authMethod === "entra" && (
              <form onSubmit={handleEntraLogin} className="login-form">
                <div className="textarea-wrapper">
                  <label htmlFor="entra-token" className="input-label">
                    JWT Token
                  </label>
                  <textarea
                    id="entra-token"
                    className="input"
                    value={entraToken}
                    onChange={(e) => setEntraToken(e.target.value)}
                    placeholder="Paste your JWT token from Entra"
                    required
                    rows={4}
                    autoFocus
                  />
                </div>

                <div className="entra-help">
                  <p>
                    Get your JWT token from the{" "}
                    <a
                      href="/signin"
                      target="_blank"
                      rel="noopener noreferrer"
                      className="entra-link"
                    >
                      Mystira Identity Service
                    </a>
                  </p>
                </div>

                <div className="login-form__footer">
                  <Button
                    type="submit"
                    loading={isLoggingIn}
                    fullWidth
                    size="lg"
                  >
                    Sign In with Entra
                  </Button>
                </div>
              </form>
            )}

            {/* Magic Link Form */}
            {authMethod === "magic" && (
              <form onSubmit={handleRequestMagicLink} className="login-form">
                {!magicLinkSent ? (
                  <>
                    <Input
                      label="Email"
                      type="email"
                      value={magicLinkEmail}
                      onChange={(e) => setMagicLinkEmail(e.target.value)}
                      placeholder="you@example.com"
                      required
                      autoComplete="email"
                      autoFocus
                    />

                    <div className="magic-link-help">
                      <p>
                        We'll send you a magic link that will instantly sign you
                        in.
                      </p>
                    </div>

                    <div className="login-form__footer">
                      <Button
                        type="submit"
                        loading={isRequestingMagicLink}
                        fullWidth
                        size="lg"
                      >
                        Send Magic Link
                      </Button>
                    </div>
                  </>
                ) : (
                  <div className="magic-link-sent">
                    <Alert variant="success">
                      Magic link sent to {magicLinkEmail}! Check your email and
                      click the link to sign in.
                    </Alert>

                    <div className="magic-link-actions">
                      <Button
                        variant="outline"
                        onClick={() => setMagicLinkSent(false)}
                        size="sm"
                      >
                        Send to different email
                      </Button>

                      <Button
                        onClick={handleMagicLinkLogin}
                        loading={isLoggingIn}
                        size="sm"
                      >
                        I've clicked the link
                      </Button>
                    </div>
                  </div>
                )}
              </form>
            )}

            <p className="login-form__help">
              Don't have an account?{" "}
              <a
                href="mailto:support@mystira.com"
                style={{
                  color: "var(--color-primary-600)",
                  fontWeight: "var(--font-weight-medium)",
                }}
              >
                Contact us
              </a>
            </p>
          </CardBody>
        </Card>
      </div>
    </>
  );
}
