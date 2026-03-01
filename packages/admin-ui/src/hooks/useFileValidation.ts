import { useState } from "react";
import * as yaml from "js-yaml";
import { validateScenario, ValidationResult } from "../utils/schemaValidator";
import { showToast } from "../utils/toast";

interface UseFileValidationOptions {
  onValidationComplete?: (result: ValidationResult) => void;
}

export function useFileValidation(options?: UseFileValidationOptions) {
  const [validating, setValidating] = useState(false);
  const [validationResult, setValidationResult] = useState<ValidationResult | null>(null);

  const validateFile = async (file: File) => {
    if (!file) {
      showToast.error("No file selected");
      return;
    }

    setValidating(true);
    setValidationResult(null);

    try {
      const fileContent = await file.text();
      const parsedData = parseFileContent(fileContent, file.name);
      const result = validateScenario(parsedData);

      setValidationResult(result);
      options?.onValidationComplete?.(result);

      if (result.valid) {
        showToast.success("Validation passed!");
      } else {
        showToast.error(`Validation failed with ${result.errors.length} error(s)`);
      }
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : "Failed to parse file";
      showToast.error(`Parse error: ${errorMessage}`);

      const errorResult: ValidationResult = {
        valid: false,
        errors: [{ path: "root", message: errorMessage }],
      };

      setValidationResult(errorResult);
      options?.onValidationComplete?.(errorResult);
    } finally {
      setValidating(false);
    }
  };

  const resetValidation = () => {
    setValidationResult(null);
  };

  return {
    validating,
    validationResult,
    validateFile,
    resetValidation,
  };
}

function parseFileContent(content: string, filename: string): unknown {
  if (filename.endsWith(".json")) {
    return JSON.parse(content);
  }

  if (filename.endsWith(".yaml") || filename.endsWith(".yml")) {
    return yaml.load(content);
  }

  throw new Error("Unsupported file format. Please use .json, .yaml, or .yml files.");
}
