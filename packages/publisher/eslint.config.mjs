import { createReactEslintConfig } from "../../configs/eslint/create-react-eslint-config.mjs";

export default createReactEslintConfig({
  ignores: ["dist", "coverage", "node_modules", "public/mockServiceWorker.js"],
  ecmaVersion: 2020,
  noUnusedVarsSeverity: "error",
  noUnusedVarsOptions: { argsIgnorePattern: "^_" },
});
