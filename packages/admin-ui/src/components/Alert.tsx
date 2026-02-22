import { ReactNode } from "react";

interface AlertProps {
  variant: "success" | "danger" | "warning" | "info";
  icon?: string;
  title?: string;
  children: ReactNode;
  className?: string;
}

function Alert({ variant, icon, title, children, className = "" }: AlertProps) {
  const iconMap = {
    success: "bi-check-circle-fill",
    danger: "bi-x-circle-fill",
    warning: "bi-exclamation-triangle-fill",
    info: "bi-info-circle-fill",
  };

  const displayIcon = icon || iconMap[variant];

  return (
    <div className={`alert alert-${variant} ${className}`} role="alert">
      {displayIcon && <i className={`bi ${displayIcon} me-2`}></i>}
      {title && <strong>{title}</strong>}
      {title && " "}
      {children}
    </div>
  );
}

export default Alert;
