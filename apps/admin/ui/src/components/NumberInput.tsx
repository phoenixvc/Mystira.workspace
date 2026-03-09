import React, { forwardRef } from "react";

interface NumberInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  error?: string;
}

const NumberInput = forwardRef<HTMLInputElement, NumberInputProps>(
  ({ error, className = "", ...props }, ref) => {
    return (
      <input
        type="number"
        className={`form-control ${error ? "is-invalid" : ""} ${className}`}
        ref={ref}
        {...props}
      />
    );
  }
);

NumberInput.displayName = "NumberInput";

export default NumberInput;
