import js from "@eslint/js";
import globals from "globals";
import tseslint from "typescript-eslint";

/**
 * Shared Node/TS ESLint preset for package-level wrappers.
 */
export function createNodeEslintConfig({
  ignores = ["dist", "node_modules"],
  noUnusedVarsSeverity = "warn",
  includeNoExplicitAny = true,
  noUnusedVarsOptions = { argsIgnorePattern: "^_" },
  tsconfigRootDir = undefined,
} = {}) {
  const rules = {
    "@typescript-eslint/no-unused-vars": [
      noUnusedVarsSeverity,
      noUnusedVarsOptions,
    ],
  };

  if (includeNoExplicitAny) {
    rules["@typescript-eslint/no-explicit-any"] = "warn";
  }

  return tseslint.config(
    { ignores },
    {
      extends: [js.configs.recommended, ...tseslint.configs.recommended],
      files: ["**/*.{ts,tsx}"],
      languageOptions: {
        ecmaVersion: 2022,
        globals: {
          ...globals.node,
          ...globals.es2022,
        },
        ...(tsconfigRootDir && {
          parserOptions: { tsconfigRootDir },
        }),
      },
      rules,
    },
    {
      files: ["*.config.ts", "*.config.js", "*.config.mjs"],
      languageOptions: {
        globals: {
          ...globals.node,
        },
      },
    }
  );
}
