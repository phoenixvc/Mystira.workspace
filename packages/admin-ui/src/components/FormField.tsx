import React, { forwardRef } from "react";

interface FormFieldProps {
  label: string;
  error?: string;
  required?: boolean;
  helpText?: string;
  children: React.ReactNode;
}

const FormField = forwardRef<HTMLDivElement, FormFieldProps>(
  ({ label, error, required, helpText, children }, ref) => {
    return (
      <div className="mb-3" ref={ref}>
        <label className="form-label">
          {label} {required && <span className="text-danger">*</span>}
        </label>
        {children}
        {error && <div className="invalid-feedback d-block">{error}</div>}
        {helpText && !error && <div className="form-text">{helpText}</div>}
      </div>
    );
  }
);

FormField.displayName = "FormField";

export default FormField;
