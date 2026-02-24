interface CheckboxProps {
  id: string;
  label: string;
  helpText?: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
  disabled?: boolean;
}

function Checkbox({ id, label, helpText, checked, onChange, disabled = false }: CheckboxProps) {
  return (
    <div className="mb-3">
      <div className="form-check">
        <input
          className="form-check-input"
          type="checkbox"
          id={id}
          checked={checked}
          onChange={e => onChange(e.target.checked)}
          disabled={disabled}
        />
        <label className="form-check-label" htmlFor={id}>
          {label}
        </label>
      </div>
      {helpText && <div className="form-text">{helpText}</div>}
    </div>
  );
}

export default Checkbox;
