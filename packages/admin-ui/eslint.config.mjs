import { createReactEslintConfig } from "../../configs/eslint/create-react-eslint-config.mjs";

export default createReactEslintConfig({
  ignores: ["dist", "node_modules", "coverage"],
  includeNoExplicitAny: true,
  noUnusedVarsOptions: {
    argsIgnorePattern: "^_",
    varsIgnorePattern: "^_",
    caughtErrorsIgnorePattern: "^_",
  },
});
