import { useState } from 'react';
import { useNavigate, useLocation, Link } from 'react-router-dom';
import { Button, Input, Alert, Card, CardBody, ThemeToggle } from '@/components';
import { useAuth } from '@/hooks';

export function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { login, isLoggingIn, loginError } = useAuth();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/dashboard';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await login({ email, password });
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

          {loginError && (
            <Alert variant="error">
              {loginError instanceof Error ? loginError.message : 'Login failed. Please try again.'}
            </Alert>
          )}

          <form onSubmit={handleSubmit} className="login-form">
            <Input
              label="Email"
              type="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
              autoComplete="email"
              autoFocus
            />

            <Input
              label="Password"
              type="password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="Enter your password"
              required
              autoComplete="current-password"
            />

            <div className="login-form__footer">
              <Button type="submit" loading={isLoggingIn} fullWidth size="lg">
                Sign In
              </Button>
              <p className="login-form__help">
                Don't have an account?{' '}
                <Link to="/login" style={{ color: 'var(--color-primary-600)', fontWeight: 'var(--font-weight-medium)' }}>
                  Contact us
                </Link>
              </p>
            </div>
          </form>
        </CardBody>
      </Card>
      </div>
    </>
  );
}
