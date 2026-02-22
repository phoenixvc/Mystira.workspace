import { Link, Outlet } from "react-router-dom";
import { useAuth } from "./auth";
import { ThemeSelector } from "./components/ThemeSelector";

function Layout() {
  const { logout } = useAuth();

  const handleLogout = async () => {
    await logout();
  };

  return (
    <>
      <header>
        <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
          <div className="container">
            <Link className="navbar-brand" to="/admin">
              <i className="bi bi-dice-6-fill me-2"></i>
              Mystira Admin
            </Link>
            <button
              className="navbar-toggler"
              type="button"
              data-bs-toggle="collapse"
              data-bs-target="#navbarNav"
              aria-controls="navbarNav"
              aria-expanded="false"
              aria-label="Toggle navigation"
            >
              <span className="navbar-toggler-icon"></span>
            </button>
            <div className="collapse navbar-collapse" id="navbarNav">
              <ul className="navbar-nav me-auto">
                <li className="nav-item">
                  <Link className="nav-link" to="/admin">
                    Dashboard
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/admin/scenarios">
                    Scenarios
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/admin/media">
                    Media
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/admin/badges">
                    Badges
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/admin/bundles">
                    Bundles
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/admin/avatars">
                    Avatars
                  </Link>
                </li>
                <li className="nav-item">
                  <Link className="nav-link" to="/admin/character-maps">
                    Character Maps
                  </Link>
                </li>
                <li className="nav-item dropdown">
                  <a
                    className="nav-link dropdown-toggle"
                    href="#"
                    role="button"
                    data-bs-toggle="dropdown"
                    aria-expanded="false"
                  >
                    Master Data
                  </a>
                  <ul className="dropdown-menu">
                    <li>
                      <Link className="dropdown-item" to="/admin/master-data/age-groups">
                        Age Groups
                      </Link>
                    </li>
                    <li>
                      <Link className="dropdown-item" to="/admin/master-data/archetypes">
                        Archetypes
                      </Link>
                    </li>
                    <li>
                      <Link className="dropdown-item" to="/admin/master-data/compass-axes">
                        Compass Axes
                      </Link>
                    </li>
                    <li>
                      <Link className="dropdown-item" to="/admin/master-data/echo-types">
                        Echo Types
                      </Link>
                    </li>
                    <li>
                      <Link className="dropdown-item" to="/admin/master-data/fantasy-themes">
                        Fantasy Themes
                      </Link>
                    </li>
                  </ul>
                </li>
                <li className="nav-item dropdown">
                  <a
                    className="nav-link dropdown-toggle"
                    href="#"
                    role="button"
                    data-bs-toggle="dropdown"
                    aria-expanded="false"
                  >
                    Users
                  </a>
                  <ul className="dropdown-menu">
                    <li>
                      <Link className="dropdown-item" to="/admin/accounts">
                        Accounts
                      </Link>
                    </li>
                    <li>
                      <Link className="dropdown-item" to="/admin/profiles">
                        Profiles
                      </Link>
                    </li>
                  </ul>
                </li>
              </ul>
              <ul className="navbar-nav align-items-center">
                <li className="nav-item">
                  <ThemeSelector />
                </li>
                <li className="nav-item">
                  <button
                    id="logoutBtn"
                    className="nav-link btn btn-link"
                    onClick={handleLogout}
                    style={{
                      border: "none",
                      background: "none",
                      color: "inherit",
                    }}
                  >
                    Logout
                  </button>
                </li>
              </ul>
            </div>
          </div>
        </nav>
      </header>

      <div className="container mt-4">
        <Outlet />
      </div>

      <footer className="border-top footer text-muted mt-auto">
        <div className="container">
          <div className="text-center py-3">
            <small>&copy; 2025 Mystira. All rights reserved.</small>
          </div>
        </div>
      </footer>
    </>
  );
}

export default Layout;
