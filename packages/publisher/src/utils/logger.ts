// Logging utility that respects environment

type LogLevel = 'debug' | 'info' | 'warn' | 'error';

const isProduction = import.meta.env.PROD;

class Logger {
  private shouldLog(level: LogLevel): boolean {
    if (isProduction) {
      // In production, only log errors and warnings
      return level === 'error' || level === 'warn';
    }
    // In development, log everything
    return true;
  }

  debug(...args: unknown[]): void {
    if (this.shouldLog('debug')) {
      console.debug('[DEBUG]', ...args);
    }
  }

  info(...args: unknown[]): void {
    if (this.shouldLog('info')) {
      console.info('[INFO]', ...args);
    }
  }

  warn(...args: unknown[]): void {
    if (this.shouldLog('warn')) {
      console.warn('[WARN]', ...args);
    }
  }

  error(...args: unknown[]): void {
    if (this.shouldLog('error')) {
      console.error('[ERROR]', ...args);
      // In production, send to error tracking service
      if (isProduction) {
        // TODO: Integrate with error tracking service (Sentry, etc.)
        // errorTrackingService.captureException(args[0]);
      }
    }
  }
}

export const logger = new Logger();

