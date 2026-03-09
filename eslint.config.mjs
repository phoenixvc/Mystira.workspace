import js from "@eslint/js";
import globals from "globals";
import tseslint from "typescript-eslint";
import eslintConfigPrettier from "eslint-config-prettier";
import { fileURLToPath } from "url";
import path from "path";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export default tseslint.config(
  {
    ignores: [
      "dist",
      "node_modules",
      "packages/*/dist",
      "packages/*/node_modules",
      "apps/*/dist",
      "apps/*/node_modules",
      "apps/*/*/dist",
      "apps/*/*/node_modules",
      "**/wwwroot/**",
      "**/service-worker*.js",
      "**/*.min.js",
    ],
  },
  {
    extends: [js.configs.recommended, ...tseslint.configs.recommended],
    files: ["**/*.{ts,tsx}"],
    languageOptions: {
      ecmaVersion: 2022,
      globals: {
        ...globals.node,
        ...globals.es2022,
      },
      parserOptions: {
        tsconfigRootDir: __dirname,
      },
    },
    rules: {
      "@typescript-eslint/no-unused-vars": [
        "error",
        { argsIgnorePattern: "^_" },
      ],
      "@typescript-eslint/explicit-module-boundary-types": "off",
      "@typescript-eslint/no-explicit-any": "warn",
      "@typescript-eslint/no-empty-object-type": "warn",
    },
  },
  {
    files: ["**/apps/devhub/**/*.{ts,tsx}"],
    rules: {
      "@typescript-eslint/no-explicit-any": "warn",
      "@typescript-eslint/no-empty-object-type": "off",
      "@typescript-eslint/no-unused-vars": [
        "warn",
        {
          argsIgnorePattern: "^_",
          varsIgnorePattern: "^_",
          caughtErrorsIgnorePattern: ".*",
        },
      ],
      "no-case-declarations": "warn",
      "no-useless-escape": "warn",
      "no-useless-catch": "warn",
      "prefer-const": "warn",
    },
  },
  eslintConfigPrettier,
);
