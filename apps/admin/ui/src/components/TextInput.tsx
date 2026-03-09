import React, { forwardRef } from "react";

interface TextInputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  error?: string;
}

const TextInput = forwardRef<HTMLInputElement, TextInputProps>(
  ({ error, className = "", ...props }, ref) => {
    return (
      <input
        type="text"
        className={`form-control ${error ? "is-invalid" : ""} ${className}`}
        ref={ref}
        {...props}
      />
    );
  }
);

TextInput.displayName = "TextInput";

export default TextInput;
