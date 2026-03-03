import { createNodeEslintConfig } from "../../configs/eslint/create-node-eslint-config.mjs";
import { fileURLToPath } from "url";
import path from "path";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export default createNodeEslintConfig({
  ignores: ["dist", "node_modules"],
  tsconfigRootDir: __dirname,
});
