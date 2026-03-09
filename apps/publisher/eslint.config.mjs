import { createReactEslintConfig } from "../../configs/eslint/create-react-eslint-config.mjs";
import { fileURLToPath } from "url";
import path from "path";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export default createReactEslintConfig({
  ignores: ["dist", "coverage", "node_modules", "public/mockServiceWorker.js"],
  ecmaVersion: 2020,
  noUnusedVarsSeverity: "error",
  noUnusedVarsOptions: { argsIgnorePattern: "^_" },
  tsconfigRootDir: __dirname,
});
