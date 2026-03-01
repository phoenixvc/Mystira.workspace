import { ErrorInfo } from "react";

/**
 * Error severity levels for categorizing errors
 */
export type ErrorSeverity = "info" | "warning" | "error" | "fatal";

/**
 * Context information about where/when the error occurred
 */
export interface ErrorContext {
  /** Component or function where error occurred */
  source?: string;
  /** User action that triggered the error */
  action?: string;
  /** Current route/page */
  route?: string;
  /** Any additional context data */
  extra?: Record<string, unknown>;
}

/**
 * Standardized error report structure
 */
export interface ErrorReport {
  /** Error message */
  message: string;
  /** Error stack trace */
  stack?: string;
  /** HTTP status code (for API errors) */
  status?: number;
  /** Error code */
  code?: string;
  /** Severity level */
  severity: ErrorSeverity;
  /** Timestamp when error occurred */
  timestamp: string;
  /** Context information */
  context?: ErrorContext;
  /** React component stack (for ErrorBoundary) */
  componentStack?: string;
  /** User agent string */
  userAgent: string;
  /** Current URL */
  url: string;
}

/**
 * Interface for error reporting services (Sentry, LogRocket, etc.)
 */
export interface IErrorReporter {
  /** Report an error to the service */
  report(error: ErrorReport): void;
  /** Set user context for error reports */
  setUser(userId: string, email?: string): void;
  /** Clear user context */
  clearUser(): void;
}

/**
 * Console-based error reporter for development
 */
class ConsoleErrorReporter implements IErrorReporter {
  report(error: ErrorReport): void {
    const style = this.getSeverityStyle(error.severity);
    console.groupCollapsed(`%c[${error.severity.toUpperCase()}] ${error.message}`, style);
    console.log("Timestamp:", error.timestamp);
    console.log("URL:", error.url);
    if (error.status) console.log("Status:", error.status);
    if (error.code) console.log("Code:", error.code);
    if (error.context) console.log("Context:", error.context);
    if (error.stack) console.log("Stack:", error.stack);
    if (error.componentStack) console.log("Component Stack:", error.componentStack);
    console.groupEnd();
  }

  setUser(userId: string, email?: string): void {
    console.log(`[ErrorReporter] User set: ${userId}${email ? ` (${email})` : ""}`);
  }

  clearUser(): void {
    console.log("[ErrorReporter] User cleared");
  }

  private getSeverityStyle(severity: ErrorSeverity): string {
    const styles: Record<ErrorSeverity, string> = {
      info: "color: #0dcaf0; font-weight: bold;",
      warning: "color: #ffc107; font-weight: bold;",
      error: "color: #dc3545; font-weight: bold;",
      fatal: "color: #fff; background: #dc3545; font-weight: bold; padding: 2px 6px;",
    };
    return styles[severity];
  }
}

/**
 * No-op error reporter for production when no service is configured
 */
class NoOpErrorReporter implements IErrorReporter {
  report(): void {
    // No-op
  }
  setUser(): void {
    // No-op
  }
  clearUser(): void {
    // No-op
  }
}

/**
 * Error Reporting Service
 * Provides a unified interface for reporting errors across the application
 */
class ErrorReportingService {
  private reporter: IErrorReporter;
  private defaultContext: ErrorContext = {};

  constructor() {
    // Use console reporter in development, no-op in production
    // Replace NoOpErrorReporter with actual service (Sentry, LogRocket, etc.) when configured
    this.reporter = import.meta.env.DEV ? new ConsoleErrorReporter() : new NoOpErrorReporter();
  }

  /**
   * Configure a custom error reporter (e.g., Sentry, LogRocket)
   */
  setReporter(reporter: IErrorReporter): void {
    this.reporter = reporter;
  }

  /**
   * Set default context that will be included in all error reports
   */
  setDefaultContext(context: ErrorContext): void {
    this.defaultContext = { ...this.defaultContext, ...context };
  }

  /**
   * Report an error from an Error object
   */
  reportError(error: Error, severity: ErrorSeverity = "error", context?: ErrorContext): void {
    const report = this.createReport(error.message, severity, context);
    report.stack = error.stack;
    this.reporter.report(report);
  }

  /**
   * Report an API error with status code
   */
  reportApiError(message: string, status?: number, code?: string, context?: ErrorContext): void {
    const severity: ErrorSeverity = status && status >= 500 ? "error" : "warning";
    const report = this.createReport(message, severity, context);
    report.status = status;
    report.code = code;
    this.reporter.report(report);
  }

  /**
   * Report a React ErrorBoundary error
   */
  reportBoundaryError(error: Error, errorInfo: ErrorInfo | null, context?: ErrorContext): void {
    const report = this.createReport(error.message, "fatal", context);
    report.stack = error.stack;
    report.componentStack = errorInfo?.componentStack ?? undefined;
    this.reporter.report(report);
  }

  /**
   * Report a warning (non-critical issue)
   */
  reportWarning(message: string, context?: ErrorContext): void {
    const report = this.createReport(message, "warning", context);
    this.reporter.report(report);
  }

  /**
   * Report an info message (for tracking)
   */
  reportInfo(message: string, context?: ErrorContext): void {
    const report = this.createReport(message, "info", context);
    this.reporter.report(report);
  }

  /**
   * Set user context for error reports
   */
  setUser(userId: string, email?: string): void {
    this.reporter.setUser(userId, email);
  }

  /**
   * Clear user context
   */
  clearUser(): void {
    this.reporter.clearUser();
  }

  /**
   * Create a standardized error report
   */
  private createReport(
    message: string,
    severity: ErrorSeverity,
    context?: ErrorContext
  ): ErrorReport {
    return {
      message,
      severity,
      timestamp: new Date().toISOString(),
      userAgent: navigator.userAgent,
      url: window.location.href,
      context: { ...this.defaultContext, ...context },
    };
  }
}

// Singleton instance
export const errorReportingService = new ErrorReportingService();
