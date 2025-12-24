/**
 * Simple structured logger utilities
 */

export type LogLevel = 'debug' | 'info' | 'warn' | 'error';

export interface LogContext {
  [key: string]: unknown;
}

export interface LogEntry {
  level: LogLevel;
  message: string;
  timestamp: string;
  context?: LogContext;
}

export interface Logger {
  debug(message: string, context?: LogContext): void;
  info(message: string, context?: LogContext): void;
  warn(message: string, context?: LogContext): void;
  error(message: string, context?: LogContext): void;
  child(context: LogContext): Logger;
}

const LOG_LEVELS: Record<LogLevel, number> = {
  debug: 0,
  info: 1,
  warn: 2,
  error: 3,
};

export interface LoggerOptions {
  /** Minimum log level to output */
  level?: LogLevel;
  /** Base context to include in all logs */
  context?: LogContext;
  /** Custom output function */
  output?: (entry: LogEntry) => void;
}

/**
 * Create a structured logger
 *
 * @example
 * ```typescript
 * const logger = createLogger({ level: 'info' });
 * logger.info('Request received', { requestId: '123' });
 *
 * const childLogger = logger.child({ service: 'api' });
 * childLogger.error('Failed to process', { error: 'timeout' });
 * ```
 */
export function createLogger(options: LoggerOptions = {}): Logger {
  const { level = 'info', context = {}, output = defaultOutput } = options;

  const minLevel = LOG_LEVELS[level];

  function log(logLevel: LogLevel, message: string, logContext?: LogContext): void {
    if (LOG_LEVELS[logLevel] < minLevel) return;

    const entry: LogEntry = {
      level: logLevel,
      message,
      timestamp: new Date().toISOString(),
      context: { ...context, ...logContext },
    };

    output(entry);
  }

  return {
    debug: (message, ctx) => log('debug', message, ctx),
    info: (message, ctx) => log('info', message, ctx),
    warn: (message, ctx) => log('warn', message, ctx),
    error: (message, ctx) => log('error', message, ctx),
    child: (childContext) =>
      createLogger({
        level,
        context: { ...context, ...childContext },
        output,
      }),
  };
}

function safeStringify(value: unknown): string {
  try {
    return JSON.stringify(value);
  } catch {
    // Handle circular references or non-serializable values
    return '[unserializable context]';
  }
}

function defaultOutput(entry: LogEntry): void {
  const { level, message, timestamp, context } = entry;
  const contextStr = context && Object.keys(context).length > 0 ? ` ${safeStringify(context)}` : '';

  // Use appropriate console method based on level
  const consoleFn =
    level === 'error'
      ? console.error
      : level === 'warn'
        ? console.warn
        : level === 'debug'
          ? console.debug
          : console.log;

  consoleFn(`[${timestamp}] ${level.toUpperCase()}: ${message}${contextStr}`);
}
