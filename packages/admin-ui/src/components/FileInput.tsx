import { ChangeEvent } from "react";

interface FileInputProps {
  id: string;
  label: string;
  accept: string;
  helpText?: string;
  selectedFile?: File | null;
  onChange: (file: File | null) => void;
  disabled?: boolean;
  required?: boolean;
}

function FileInput({
  id,
  label,
  accept,
  helpText,
  selectedFile,
  onChange,
  disabled = false,
  required = false,
}: FileInputProps) {
  const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0] || null;
    onChange(file);
  };

  return (
    <div className="mb-3">
      <label htmlFor={id} className="form-label">
        {label}
      </label>
      <input
        type="file"
        className="form-control"
        id={id}
        accept={accept}
        onChange={handleChange}
        disabled={disabled}
        required={required}
      />
      {helpText && <div className="form-text">{helpText}</div>}
      {selectedFile && (
        <div className="alert alert-info mt-2 mb-0">
          <strong>Selected file:</strong> {selectedFile.name} (
          {(selectedFile.size / 1024).toFixed(2)} KB)
        </div>
      )}
    </div>
  );
}

export default FileInput;
