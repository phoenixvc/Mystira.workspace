import { createNodeEslintConfig } from "../../configs/eslint/create-node-eslint-config.mjs";

export default createNodeEslintConfig({
  ignores: ["dist", "node_modules", "dotnet"],
});
