import { createReactEslintConfig } from "../../configs/eslint/create-react-eslint-config.mjs";
import { fileURLToPath } from "url";
import path from "path";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export default createReactEslintConfig({
  ignores: ["dist", "node_modules", "coverage"],
  includeNoExplicitAny: true,
  noUnusedVarsOptions: {
    argsIgnorePattern: "^_",
    varsIgnorePattern: "^_",
    caughtErrorsIgnorePattern: "^_",
  },
  tsconfigRootDir: __dirname,
});
