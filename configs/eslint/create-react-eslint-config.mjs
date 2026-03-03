import js from "@eslint/js";
import globals from "globals";
import reactHooks from "eslint-plugin-react-hooks";
import reactRefresh from "eslint-plugin-react-refresh";
import tseslint from "typescript-eslint";

/**
 * Shared React/TS ESLint preset for package-level wrappers.
 */
export function createReactEslintConfig({
  ignores = ["dist", "node_modules"],
  ecmaVersion = 2022,
  noUnusedVarsSeverity = "warn",
  noUnusedVarsOptions = { argsIgnorePattern: "^_" },
  includeNoExplicitAny = false,
  testFiles = ["**/*.test.{ts,tsx}", "**/tests/**/*.{ts,tsx}"],
  additionalRules = {},
  tsconfigRootDir = undefined,
} = {}) {
  const rules = {
    ...reactHooks.configs.recommended.rules,
    "react-refresh/only-export-components": [
      "warn",
      { allowConstantExport: true },
    ],
    "@typescript-eslint/no-unused-vars": [
      noUnusedVarsSeverity,
      noUnusedVarsOptions,
    ],
    ...additionalRules,
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
        ecmaVersion,
        globals: globals.browser,
        ...(tsconfigRootDir && {
          parserOptions: { tsconfigRootDir },
        }),
      },
      plugins: {
        "react-hooks": reactHooks,
        "react-refresh": reactRefresh,
      },
      rules,
    },
    {
      files: testFiles,
      rules: {
        "react-refresh/only-export-components": "off",
      },
    }
  );
}
