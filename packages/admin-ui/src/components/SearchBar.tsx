interface SearchBarProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  onSearchReset?: () => void;
}

function SearchBar({
  value,
  onChange,
  placeholder = "Search...",
  onSearchReset,
}: SearchBarProps) {
  return (
    <div className="mb-3">
      <div className="input-group">
        <span className="input-group-text">
          <i className="bi bi-search"></i>
        </span>
        <input
          type="text"
          className="form-control"
          placeholder={placeholder}
          value={value}
          onChange={(e) => onChange(e.target.value)}
        />
        {value && onSearchReset && (
          <button
            className="btn btn-outline-secondary"
            type="button"
            onClick={onSearchReset}
          >
            <i className="bi bi-x"></i>
          </button>
        )}
      </div>
    </div>
  );
}

export default SearchBar;
