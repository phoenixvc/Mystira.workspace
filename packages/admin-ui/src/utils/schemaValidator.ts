import Ajv from "ajv";
import addFormats from "ajv-formats";
import storySchema from "../schemas/story-schema.json";

// Initialize Ajv with formats support
const ajv = new Ajv({ allErrors: true, verbose: true });
addFormats(ajv);

// Compile the story schema
const validateStorySchema = ajv.compile(storySchema);

export interface ValidationError {
  path: string;
  message: string;
  keyword?: string;
  params?: Record<string, unknown>;
}

export interface ValidationResult {
  valid: boolean;
  errors: ValidationError[];
}

/**
 * Validates a scenario/story object against the Mystira story schema
 * @param data The scenario data to validate (parsed JSON/YAML)
 * @returns ValidationResult with valid flag and any errors
 */
export function validateScenario(data: unknown): ValidationResult {
  const valid = validateStorySchema(data);

  if (valid) {
    return { valid: true, errors: [] };
  }

  const errors: ValidationError[] = (validateStorySchema.errors || []).map(error => {
    const path = error.instancePath || error.schemaPath || "root";
    let message = error.message || "Validation error";

    // Enhance error messages for better user understanding
    if (error.keyword === "required") {
      const missingProperty = error.params?.missingProperty;
      message = `Missing required field: ${missingProperty}`;
    } else if (error.keyword === "enum") {
      const allowedValues = error.params?.allowedValues;
      message = `${message}. Allowed values: ${allowedValues?.join(", ")}`;
    } else if (error.keyword === "type") {
      const expectedType = error.params?.type;
      message = `${message}. Expected type: ${expectedType}`;
    } else if (error.keyword === "minLength") {
      const limit = error.params?.limit;
      message = `${message}. Minimum length: ${limit}`;
    } else if (error.keyword === "maxLength") {
      const limit = error.params?.limit;
      message = `${message}. Maximum length: ${limit}`;
    } else if (error.keyword === "pattern") {
      const pattern = error.params?.pattern;
      message = `${message}. Must match pattern: ${pattern}`;
    } else if (error.keyword === "minimum") {
      const limit = error.params?.limit;
      message = `${message}. Minimum value: ${limit}`;
    } else if (error.keyword === "maximum") {
      const limit = error.params?.limit;
      message = `${message}. Maximum value: ${limit}`;
    } else if (error.keyword === "minItems") {
      const limit = error.params?.limit;
      message = `${message}. Minimum items: ${limit}`;
    }

    return {
      path: path.replace(/^\//, "").replace(/\//g, ".") || "root",
      message,
      keyword: error.keyword,
      params: error.params,
    };
  });

  return { valid: false, errors };
}

/**
 * Formats validation errors into a human-readable string
 * @param errors Array of validation errors
 * @returns Formatted error message string
 */
export function formatValidationErrors(errors: ValidationError[]): string {
  if (errors.length === 0) return "";

  return errors
    .map((error, index) => {
      const location = error.path === "root" ? "Root level" : `At ${error.path}`;
      return `${index + 1}. ${location}: ${error.message}`;
    })
    .join("\n");
}

/**
 * Groups validation errors by path for easier display
 * @param errors Array of validation errors
 * @returns Map of path to errors
 */
export function groupErrorsByPath(errors: ValidationError[]): Map<string, ValidationError[]> {
  const grouped = new Map<string, ValidationError[]>();

  errors.forEach(error => {
    const path = error.path || "root";
    if (!grouped.has(path)) {
      grouped.set(path, []);
    }
    grouped.get(path)!.push(error);
  });

  return grouped;
}
