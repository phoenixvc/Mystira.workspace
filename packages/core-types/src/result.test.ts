/**
 * Tests for Result pattern implementation
 */

import { describe, it, expect } from "vitest";
import {
  ok,
  err,
  isOk,
  isErr,
  unwrap,
  unwrapOr,
  map,
  mapErr,
  flatMap,
  type Result,
} from "./result";
import { MystiraError } from "./errors";

describe("Result Pattern", () => {
  describe("ok", () => {
    it("should create a successful result", () => {
      const result = ok(42);
      expect(result.success).toBe(true);
      if (result.success) {
        expect(result.value).toBe(42);
      }
    });

    it("should work with different types", () => {
      const stringResult = ok("hello");
      const objectResult = ok({ id: 1, name: "test" });
      const arrayResult = ok([1, 2, 3]);

      expect(stringResult.success).toBe(true);
      expect(objectResult.success).toBe(true);
      expect(arrayResult.success).toBe(true);
    });
  });

  describe("err", () => {
    it("should create a failed result", () => {
      const error: MystiraError = {
        code: "INTERNAL",
        message: "Something went wrong",
      };
      const result = err(error);

      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error).toEqual(error);
      }
    });
  });

  describe("isOk", () => {
    it("should return true for successful results", () => {
      const result = ok(42);
      expect(isOk(result)).toBe(true);
    });

    it("should return false for failed results", () => {
      const result = err({ code: "INTERNAL", message: "error" });
      expect(isOk(result)).toBe(false);
    });

    it("should narrow type correctly", () => {
      const result: Result<number> = ok(42);
      if (isOk(result)) {
        // TypeScript should know result.value is number
        expect(result.value).toBe(42);
      }
    });
  });

  describe("isErr", () => {
    it("should return false for successful results", () => {
      const result = ok(42);
      expect(isErr(result)).toBe(false);
    });

    it("should return true for failed results", () => {
      const result = err({ code: "INTERNAL", message: "error" });
      expect(isErr(result)).toBe(true);
    });

    it("should narrow type correctly", () => {
      const result: Result<number> = err({
        code: "INTERNAL",
        message: "error",
      });
      if (isErr(result)) {
        // TypeScript should know result.error exists
        expect(result.error.code).toBe("INTERNAL");
      }
    });
  });

  describe("unwrap", () => {
    it("should return value for successful results", () => {
      const result = ok(42);
      expect(unwrap(result)).toBe(42);
    });

    it("should throw error for failed results", () => {
      const error: MystiraError = {
        code: "INTERNAL",
        message: "Something went wrong",
      };
      const result = err(error);

      expect(() => unwrap(result)).toThrow();
    });
  });

  describe("unwrapOr", () => {
    it("should return value for successful results", () => {
      const result = ok(42);
      expect(unwrapOr(result, 0)).toBe(42);
    });

    it("should return default value for failed results", () => {
      const result: Result<number> = err({
        code: "INTERNAL",
        message: "error",
      });
      expect(unwrapOr(result, 99)).toBe(99);
    });
  });

  describe("map", () => {
    it("should transform successful results", () => {
      const result = ok(42);
      const mapped = map(result, (n) => n * 2);

      expect(isOk(mapped)).toBe(true);
      if (isOk(mapped)) {
        expect(mapped.value).toBe(84);
      }
    });

    it("should preserve failed results", () => {
      const error: MystiraError = { code: "INTERNAL", message: "error" };
      const result: Result<number> = err(error);
      const mapped = map(result, (n) => n * 2);

      expect(isErr(mapped)).toBe(true);
      if (isErr(mapped)) {
        expect(mapped.error).toEqual(error);
      }
    });

    it("should work with type transformations", () => {
      const result = ok(42);
      const mapped = map(result, (n) => `Number: ${n}`);

      expect(isOk(mapped)).toBe(true);
      if (isOk(mapped)) {
        expect(mapped.value).toBe("Number: 42");
      }
    });
  });

  describe("mapErr", () => {
    it("should preserve successful results", () => {
      const result = ok(42);
      const mapped = mapErr(result, (e) => ({ ...e, message: "transformed" }));

      expect(isOk(mapped)).toBe(true);
      if (isOk(mapped)) {
        expect(mapped.value).toBe(42);
      }
    });

    it("should transform failed results", () => {
      const error: MystiraError = { code: "INTERNAL", message: "original" };
      const result: Result<number> = err(error);
      const mapped = mapErr(result, (e) => ({ ...e, message: "transformed" }));

      expect(isErr(mapped)).toBe(true);
      if (isErr(mapped)) {
        expect(mapped.error.message).toBe("transformed");
        expect(mapped.error.code).toBe("INTERNAL");
      }
    });
  });

  describe("flatMap", () => {
    it("should chain successful results", () => {
      const result = ok(42);
      const chained = flatMap(result, (n) => ok(n * 2));

      expect(isOk(chained)).toBe(true);
      if (isOk(chained)) {
        expect(chained.value).toBe(84);
      }
    });

    it("should short-circuit on first error", () => {
      const error: MystiraError = { code: "INTERNAL", message: "error" };
      const result: Result<number> = err(error);
      const chained = flatMap(result, (n) => ok(n * 2));

      expect(isErr(chained)).toBe(true);
      if (isErr(chained)) {
        expect(chained.error).toEqual(error);
      }
    });

    it("should propagate errors from mapper function", () => {
      const result = ok(42);
      const error: MystiraError = { code: "VALIDATION", message: "invalid" };
      const chained = flatMap(result, () => err(error));

      expect(isErr(chained)).toBe(true);
      if (isErr(chained)) {
        expect(chained.error).toEqual(error);
      }
    });

    it("should work with async operations pattern", () => {
      // Simulate async operation result
      const fetchUser = (id: number): Result<{ id: number; name: string }> => {
        if (id > 0) {
          return ok({ id, name: "John" });
        }
        return err({ code: "NOT_FOUND", message: "User not found" });
      };

      const result1 = flatMap(ok(1), fetchUser);
      expect(isOk(result1)).toBe(true);

      const result2 = flatMap(ok(-1), fetchUser);
      expect(isErr(result2)).toBe(true);
    });
  });

  describe("Result pattern composition", () => {
    it("should compose multiple operations", () => {
      const divide = (a: number, b: number): Result<number> => {
        if (b === 0) {
          return err({ code: "BAD_REQUEST", message: "Division by zero" });
        }
        return ok(a / b);
      };

      const result1 = divide(10, 2);
      const doubled = map(result1, (n) => n * 2);
      const final = map(doubled, (n) => Math.round(n));

      expect(isOk(final)).toBe(true);
      if (isOk(final)) {
        expect(final.value).toBe(10);
      }

      const result2 = divide(10, 0);
      const mapped = map(result2, (n) => n * 2);

      expect(isErr(mapped)).toBe(true);
    });
  });
});
