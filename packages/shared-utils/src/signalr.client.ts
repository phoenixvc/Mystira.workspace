import type {
  SignalROptions,
  SignalREventHandler,
  ISignalRConnection,
} from "./signalr.types";
import { SignalRConnectionState } from "./signalr.types";

/**
 * SignalR connection implementation that works with @microsoft/signalr
 * This is a lightweight wrapper that provides a consistent interface
 * and handles connection lifecycle, reconnection, and event management.
 */
export class SignalRConnection implements ISignalRConnection {
  private _connection: any = null; // Will be HubConnection from @microsoft/signalr
  private _state: SignalRConnectionState = SignalRConnectionState.Disconnected;
  private _connectionId: string | null = null;
  private _error: Error | null = null;
  private _eventHandlers = new Map<string, Set<SignalREventHandler>>();
  private _reconnectAttempts = 0;
  private _reconnectTimer: ReturnType<typeof setTimeout> | null = null;

  constructor(private options: SignalROptions) {
    // Ensure we have either accessToken or accessTokenFactory
    if (!options.accessToken && !options.accessTokenFactory) {
      console.warn(
        "SignalR: No authentication configured. Connection may fail if server requires auth."
      );
    }
  }

  get state(): SignalRConnectionState {
    return this._state;
  }

  get connectionId(): string | null {
    return this._connectionId;
  }

  get isConnected(): boolean {
    return this._state === SignalRConnectionState.Connected;
  }

  get error(): Error | null {
    return this._error;
  }

  /**
   * Initialize the SignalR connection object
   * This is separated from connect() to support lazy loading of @microsoft/signalr
   */
  private async initializeConnection(): Promise<void> {
    if (this._connection) {
      return;
    }

    try {
      // Dynamic import to support lazy loading
      const signalR = await import("@microsoft/signalr");

      const builder = new signalR.HubConnectionBuilder()
        .withUrl(this.options.hubUrl, {
          accessTokenFactory:
            this.options.accessTokenFactory ||
            (() => this.options.accessToken || ""),
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff with max delay
            const delay = Math.min(
              1000 * Math.pow(2, retryContext.previousRetryCount),
              this.options.reconnectDelay || 5000
            );
            return delay;
          },
        });

      // Configure logging
      if (this.options.debug) {
        builder.configureLogging(signalR.LogLevel.Debug);
      } else if (process.env.NODE_ENV === "development") {
        builder.configureLogging(signalR.LogLevel.Information);
      } else {
        builder.configureLogging(signalR.LogLevel.Warning);
      }

      this._connection = builder.build();

      // Setup connection event handlers
      this._connection.onreconnecting((error?: Error) => {
        this.log("Reconnecting...", error);
        this._state = SignalRConnectionState.Reconnecting;
        this._error = error || null;
        this._reconnectAttempts++;
      });

      this._connection.onreconnected((connectionId?: string) => {
        this.log("Reconnected:", connectionId);
        this._state = SignalRConnectionState.Connected;
        this._connectionId = connectionId || null;
        this._error = null;
        this._reconnectAttempts = 0;

        // Re-register all event handlers
        this.reregisterEventHandlers();
      });

      this._connection.onclose((error?: Error) => {
        this.log("Connection closed", error);
        this._state = SignalRConnectionState.Disconnected;
        this._connectionId = null;
        this._error = error || null;

        // Attempt manual reconnection if within retry limit
        if (
          this.options.autoConnect !== false &&
          this._reconnectAttempts <
            (this.options.maxReconnectAttempts || Infinity)
        ) {
          this.scheduleReconnect();
        }
      });
    } catch (error) {
      const err = error instanceof Error ? error : new Error(String(error));
      this._error = err;
      throw new Error(`Failed to initialize SignalR: ${err.message}`);
    }
  }

  private scheduleReconnect(): void {
    if (this._reconnectTimer) {
      clearTimeout(this._reconnectTimer);
    }

    const delay = this.options.reconnectDelay || 5000;
    this.log(
      `Scheduling reconnect in ${delay}ms (attempt ${this._reconnectAttempts + 1})`
    );

    this._reconnectTimer = setTimeout(() => {
      this._reconnectTimer = null;
      this.connect().catch((error) => {
        this.log("Reconnect attempt failed:", error);
      });
    }, delay);
  }

  private reregisterEventHandlers(): void {
    if (!this._connection) return;

    this._eventHandlers.forEach((handlers, eventName) => {
      handlers.forEach((handler) => {
        this._connection.on(eventName, handler);
      });
    });
  }

  private log(message: string, ...args: any[]): void {
    if (this.options.debug || process.env.NODE_ENV === "development") {
      console.log(`[SignalR] ${message}`, ...args);
    }
  }

  async connect(): Promise<void> {
    if (this._state === SignalRConnectionState.Connected) {
      this.log("Already connected");
      return;
    }

    if (this._state === SignalRConnectionState.Connecting) {
      this.log("Connection already in progress");
      return;
    }

    try {
      this._state = SignalRConnectionState.Connecting;
      this._error = null;

      await this.initializeConnection();

      if (!this._connection) {
        throw new Error("Failed to initialize connection");
      }

      await this._connection.start();
      this._state = SignalRConnectionState.Connected;
      this._connectionId = this._connection.connectionId || null;
      this._reconnectAttempts = 0;

      this.log("Connected:", this._connectionId);
    } catch (error) {
      const err = error instanceof Error ? error : new Error(String(error));
      this._error = err;
      this._state = SignalRConnectionState.Disconnected;
      this.log("Connection failed:", err);
      throw err;
    }
  }

  async disconnect(): Promise<void> {
    if (this._reconnectTimer) {
      clearTimeout(this._reconnectTimer);
      this._reconnectTimer = null;
    }

    if (!this._connection) {
      return;
    }

    try {
      await this._connection.stop();
      this._state = SignalRConnectionState.Disconnected;
      this._connectionId = null;
      this.log("Disconnected");
    } catch (error) {
      this.log("Error during disconnect:", error);
    }
  }

  on<T = any>(eventName: string, handler: SignalREventHandler<T>): void {
    // Store handler for reconnection
    if (!this._eventHandlers.has(eventName)) {
      this._eventHandlers.set(eventName, new Set());
    }
    this._eventHandlers.get(eventName)!.add(handler);

    // Register with connection if connected
    if (this._connection) {
      this._connection.on(eventName, handler);
    }

    this.log(`Registered handler for event: ${eventName}`);
  }

  off(eventName: string, handler?: SignalREventHandler): void {
    if (handler) {
      // Remove specific handler
      const handlers = this._eventHandlers.get(eventName);
      if (handlers) {
        handlers.delete(handler);
        if (handlers.size === 0) {
          this._eventHandlers.delete(eventName);
        }
      }
      if (this._connection) {
        this._connection.off(eventName, handler);
      }
    } else {
      // Remove all handlers for this event
      this._eventHandlers.delete(eventName);
      if (this._connection) {
        this._connection.off(eventName);
      }
    }

    this.log(`Unregistered handler(s) for event: ${eventName}`);
  }

  async invoke<T = any>(methodName: string, ...args: any[]): Promise<T> {
    if (!this._connection) {
      throw new Error("SignalR connection not initialized");
    }

    if (this._state !== SignalRConnectionState.Connected) {
      throw new Error("SignalR connection not connected");
    }

    try {
      return (await this._connection.invoke(methodName, ...args)) as T;
    } catch (error) {
      this.log(`Error invoking ${methodName}:`, error);
      throw error;
    }
  }

  async send(methodName: string, ...args: any[]): Promise<void> {
    if (!this._connection) {
      throw new Error("SignalR connection not initialized");
    }

    if (this._state !== SignalRConnectionState.Connected) {
      throw new Error("SignalR connection not connected");
    }

    try {
      await this._connection.send(methodName, ...args);
    } catch (error) {
      this.log(`Error sending ${methodName}:`, error);
      throw error;
    }
  }

  async joinGroup(groupName: string): Promise<void> {
    return this.invoke("JoinGroup", groupName);
  }

  async leaveGroup(groupName: string): Promise<void> {
    return this.invoke("LeaveGroup", groupName);
  }
}

/**
 * Create a new SignalR connection
 * @param options Connection options
 * @returns SignalR connection instance
 */
export function createSignalRConnection(
  options: SignalROptions
): ISignalRConnection {
  const connection = new SignalRConnection(options);

  if (options.autoConnect !== false) {
    connection.connect().catch((error) => {
      console.error("Failed to auto-connect SignalR:", error);
    });
  }

  return connection;
}
