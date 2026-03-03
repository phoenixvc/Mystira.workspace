interface LoadingSpinnerProps {
  message?: string;
  size?: "sm" | "md" | "lg";
}

function LoadingSpinner({ message = "Loading...", size = "md" }: LoadingSpinnerProps) {
  const sizeClass = size === "sm" ? "spinner-border-sm" : size === "lg" ? "" : "";

  return (
    <div className="text-center py-5">
      <div className={`spinner-border ${sizeClass}`} role="status">
        <span className="visually-hidden">{message}</span>
      </div>
      {message && <p className="mt-2 text-muted">{message}</p>}
    </div>
  );
}

export default LoadingSpinner;
