import { useTheme, Theme } from "../contexts/ThemeContext";

const themeOptions: { value: Theme; label: string; icon: string }[] = [
  { value: "light", label: "Light", icon: "bi-sun" },
  { value: "dark", label: "Dark", icon: "bi-moon-stars" },
  { value: "system", label: "System", icon: "bi-display" },
];

export function ThemeSelector() {
  const { theme, setTheme } = useTheme();

  return (
    <div className="dropdown">
      <button
        className="btn btn-link nav-link dropdown-toggle d-flex align-items-center"
        type="button"
        data-bs-toggle="dropdown"
        aria-expanded="false"
        aria-label="Toggle theme"
      >
        <i
          className={`bi ${
            theme === "light" ? "bi-sun" : theme === "dark" ? "bi-moon-stars" : "bi-display"
          }`}
        />
      </button>
      <ul className="dropdown-menu dropdown-menu-end">
        {themeOptions.map(option => (
          <li key={option.value}>
            <button
              className={`dropdown-item d-flex align-items-center gap-2 ${
                theme === option.value ? "active" : ""
              }`}
              onClick={() => setTheme(option.value)}
            >
              <i className={`bi ${option.icon}`} />
              {option.label}
            </button>
          </li>
        ))}
      </ul>
    </div>
  );
}
