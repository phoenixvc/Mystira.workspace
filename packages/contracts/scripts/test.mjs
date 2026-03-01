import { execSync } from "child_process";
execSync("tsc --noEmit", { stdio: "inherit" });
