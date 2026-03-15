import { useRef } from "react";
import { ServiceLog } from "../types";
import { LogLine } from "./LogLine";
import { isErrorMessage, isStackTraceLine } from "./logUtils";

interface LogDisplayProps {
  logs: ServiceLog[];
  filter: { search: string };
  showLineNumbers: boolean;
  isMaximized: boolean;
  logLineRefs: React.MutableRefObject<Map<number, HTMLDivElement>>;
  highlightErrorIndex?: number;
  onCopyLog: (log: ServiceLog) => void;
  wordWrap: boolean;
  timestampFormat: "time" | "full" | "relative";
}

export function LogDisplay({
  logs,
  filter,
  showLineNumbers,
  isMaximized,
  logLineRefs,
  highlightErrorIndex,
  onCopyLog,
  wordWrap,
  timestampFormat,
}: LogDisplayProps) {
  const logEndRef = useRef<HTMLDivElement>(null);

  return (
    <div
      className={`bg-black text-green-400 font-mono text-xs overflow-y-auto flex-1 ${isMaximized ? "h-full" : ""}`}
    >
      {logs.length === 0 ? (
        <div className="text-gray-500">No logs to display</div>
      ) : (
        <>
          {logs.map((log, index) => {
            const isBuildLog = log.source === "build";
            const messageLower = log.message.toLowerCase();

            // Check if this is a stack trace continuation line
            const isStackTrace = isStackTraceLine(log.message);

            // Check if previous line was an error (for stack trace detection)
            const prevLog = index > 0 ? logs[index - 1] : null;
            const prevWasError = !!(
              prevLog &&
              (prevLog.type === "stderr" ||
                isErrorMessage(prevLog.message) ||
                isStackTraceLine(prevLog.message))
            );

            // Stack trace lines should be treated as error lines (red, no timestamp)
            const isErrorMsg =
              log.type === "stderr" ||
              isErrorMessage(log.message) ||
              (isStackTrace && prevWasError);

            const isWarning =
              !isErrorMsg &&
              (messageLower.includes("warning") ||
                messageLower.includes("warn") ||
                messageLower.includes("deprecated"));

            return (
              <div
                key={index}
                ref={(el) => {
                  if (el) logLineRefs.current.set(index, el);
                }}
              >
                <LogLine
                  log={log}
                  index={index}
                  showLineNumbers={showLineNumbers}
                  wordWrap={wordWrap}
                  timestampFormat={timestampFormat}
                  filterSearch={filter.search}
                  isError={isErrorMsg}
                  isWarning={isWarning}
                  isBuildLog={isBuildLog}
                  isHighlighted={index === highlightErrorIndex}
                  onCopy={onCopyLog}
                  isStackTrace={isStackTrace}
                />
              </div>
            );
          })}
          <div ref={logEndRef} />
        </>
      )}
    </div>
  );
}
