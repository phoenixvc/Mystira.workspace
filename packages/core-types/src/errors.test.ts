/**
 * Tests for error handling utilities
 */

import { describe, it, expect } from "vitest";
import {
  validationError,
  notFoundError,
  conflictError,
  unauthorizedError,
  forbiddenError,
  rateLimitError,
  internalError,
  toErrorResponse,
  type MystiraError,
  type ErrorCode,
} from "./errors";

describe("Error Utilities", () => {
  describe("validationError", () => {
    it("should create a validation error without field errors", () => {
      const error = validationError("Invalid input");

      expect(error.code).toBe("VALIDATION");
      expect(error.message).toBe("Invalid input");
      expect(error.details).toBeUndefined();
    });

    it("should create a validation error with field errors", () => {
      const fieldErrors = {
        email: ["Invalid email format"],
        password: ["Password too short", "Must contain special character"],
      };
      const error = validationError("Validation failed", fieldErrors);

      expect(error.code).toBe("VALIDATION");
      expect(error.message).toBe("Validation failed");
      expect(error.details).toEqual({ errors: fieldErrors });
    });
  });

  describe("notFoundError", () => {
    it("should create a not found error without ID", () => {
      const error = notFoundError("User");

      expect(error.code).toBe("NOT_FOUND");
      expect(error.message).toBe("User not found");
      expect(error.details).toEqual({ resource: "User", id: undefined });
    });

    it("should create a not found error with ID", () => {
      const error = notFoundError("User", "123");

      expect(error.code).toBe("NOT_FOUND");
      expect(error.message).toBe("User with ID '123' not found");
      expect(error.details).toEqual({ resource: "User", id: "123" });
    });
  });

  describe("conflictError", () => {
    it("should create a conflict error", () => {
      const error = conflictError("Resource already exists");

      expect(error.code).toBe("CONFLICT");
      expect(error.message).toBe("Resource already exists");
    });
  });

  describe("unauthorizedError", () => {
    it("should create an unauthorized error with default message", () => {
      const error = unauthorizedError();

      expect(error.code).toBe("UNAUTHORIZED");
      expect(error.message).toBe("Authentication required");
    });

    it("should create an unauthorized error with custom message", () => {
      const error = unauthorizedError("Invalid token");

      expect(error.code).toBe("UNAUTHORIZED");
      expect(error.message).toBe("Invalid token");
    });
  });

  describe("forbiddenError", () => {
    it("should create a forbidden error with default message", () => {
      const error = forbiddenError();

      expect(error.code).toBe("FORBIDDEN");
      expect(error.message).toBe("Access denied");
    });

    it("should create a forbidden error with custom message", () => {
      const error = forbiddenError("Insufficient permissions");

      expect(error.code).toBe("FORBIDDEN");
      expect(error.message).toBe("Insufficient permissions");
    });
  });

  describe("rateLimitError", () => {
    it("should create a rate limit error without retry time", () => {
      const error = rateLimitError();

      expect(error.code).toBe("RATE_LIMITED");
      expect(error.message).toBe("Rate limit exceeded");
      expect(error.details).toBeUndefined();
    });

    it("should create a rate limit error with retry time", () => {
      const error = rateLimitError(60);

      expect(error.code).toBe("RATE_LIMITED");
      expect(error.message).toBe("Rate limit exceeded");
      expect(error.details).toEqual({ retryAfterSeconds: 60 });
    });
  });

  describe("internalError", () => {
    it("should create an internal error with default message", () => {
      const error = internalError();

      expect(error.code).toBe("INTERNAL");
      expect(error.message).toBe("An unexpected error occurred");
    });

    it("should create an internal error with custom message", () => {
      const error = internalError("Database connection failed");

      expect(error.code).toBe("INTERNAL");
      expect(error.message).toBe("Database connection failed");
    });
  });

  describe("toErrorResponse", () => {
    it("should convert VALIDATION error to response with correct status", () => {
      const error = validationError("Invalid input", {
        email: ["Invalid format"],
      });
      const response = toErrorResponse(error);

      expect(response.status).toBe(400);
      expect(response.code).toBe("VALIDATION");
      expect(response.title).toBe("VALIDATION");
      expect(response.detail).toBe("Invalid input");
      expect(response.errors).toEqual({ email: ["Invalid format"] });
    });

    it("should convert NOT_FOUND error to response with correct status", () => {
      const error = notFoundError("User", "123");
      const response = toErrorResponse(error);

      expect(response.status).toBe(404);
      expect(response.code).toBe("NOT_FOUND");
      expect(response.title).toBe("NOT_FOUND");
      expect(response.detail).toBe("User with ID '123' not found");
    });

    it("should convert UNAUTHORIZED error to response with correct status", () => {
      const error = unauthorizedError();
      const response = toErrorResponse(error);

      expect(response.status).toBe(401);
      expect(response.code).toBe("UNAUTHORIZED");
    });

    it("should convert FORBIDDEN error to response with correct status", () => {
      const error = forbiddenError();
      const response = toErrorResponse(error);

      expect(response.status).toBe(403);
      expect(response.code).toBe("FORBIDDEN");
    });

    it("should convert CONFLICT error to response with correct status", () => {
      const error = conflictError("Already exists");
      const response = toErrorResponse(error);

      expect(response.status).toBe(409);
      expect(response.code).toBe("CONFLICT");
    });

    it("should convert RATE_LIMITED error to response with correct status", () => {
      const error = rateLimitError(60);
      const response = toErrorResponse(error);

      expect(response.status).toBe(429);
      expect(response.code).toBe("RATE_LIMITED");
    });

    it("should convert INTERNAL error to response with correct status", () => {
      const error = internalError();
      const response = toErrorResponse(error);

      expect(response.status).toBe(500);
      expect(response.code).toBe("INTERNAL");
    });

    it("should convert SERVICE_UNAVAILABLE error to response with correct status", () => {
      const error: MystiraError = {
        code: "SERVICE_UNAVAILABLE",
        message: "Service is down",
      };
      const response = toErrorResponse(error);

      expect(response.status).toBe(503);
      expect(response.code).toBe("SERVICE_UNAVAILABLE");
    });

    it("should use custom status when provided", () => {
      const error = internalError();
      const response = toErrorResponse(error, 502);

      expect(response.status).toBe(502);
    });

    it("should include correlationId when present", () => {
      const error: MystiraError = {
        code: "INTERNAL",
        message: "Error",
        correlationId: "abc-123",
      };
      const response = toErrorResponse(error);

      expect(response.correlationId).toBe("abc-123");
    });

    it("should include metadata from details", () => {
      const error: MystiraError = {
        code: "RATE_LIMITED",
        message: "Too many requests",
        details: { retryAfterSeconds: 60, endpoint: "/api/users" },
      };
      const response = toErrorResponse(error);

      expect(response.metadata).toEqual({
        retryAfterSeconds: 60,
        endpoint: "/api/users",
      });
    });
  });

  describe("ErrorCode type", () => {
    it("should include all standard error codes", () => {
      const codes: ErrorCode[] = [
        "VALIDATION",
        "NOT_FOUND",
        "CONFLICT",
        "UNAUTHORIZED",
        "FORBIDDEN",
        "RATE_LIMITED",
        "INTERNAL",
        "SERVICE_UNAVAILABLE",
        "BAD_REQUEST",
      ];

      // This test validates that all error codes are properly typed
      codes.forEach((code) => {
        const error: MystiraError = {
          code,
          message: "Test",
        };
        expect(error.code).toBe(code);
      });
    });
  });
});
