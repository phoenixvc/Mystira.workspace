import { ReactNode } from "react";

interface CardProps {
  title?: string;
  subtitle?: string;
  children: ReactNode;
  className?: string;
}

function Card({ title, subtitle, children, className = "" }: CardProps) {
  return (
    <div className={`card ${className}`}>
      <div className="card-body">
        {title && <h5 className="card-title">{title}</h5>}
        {subtitle && <p className="card-text text-muted">{subtitle}</p>}
        {children}
      </div>
    </div>
  );
}

export default Card;
