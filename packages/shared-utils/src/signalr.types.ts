/**
 * SignalR connection configuration options
 */
export interface SignalROptions {
  /**
   * The URL of the SignalR hub
   */
  hubUrl: string;
  
  /**
   * Optional access token for authentication
   */
  accessToken?: string;
  
  /**
   * Function to retrieve access token (called on each connection attempt)
   */
  accessTokenFactory?: () => string | Promise<string>;
  
  /**
   * Whether to automatically connect on initialization
   * @default true
   */
  autoConnect?: boolean;
  
  /**
   * Reconnection delay in milliseconds
   * @default 5000
   */
  reconnectDelay?: number;
  
  /**
   * Maximum number of reconnection attempts
   * @default Infinity
   */
  maxReconnectAttempts?: number;
  
  /**
   * Whether to enable debug logging
   * @default false
   */
  debug?: boolean;
  
  /**
   * Additional query parameters to send with the connection
   */
  queryParams?: Record<string, string>;
}

/**
 * SignalR connection state
 */
export enum SignalRConnectionState {
  Disconnected = 'Disconnected',
  Connecting = 'Connecting',
  Connected = 'Connected',
  Reconnecting = 'Reconnecting',
}

/**
 * Event handler type for SignalR events
 */
export type SignalREventHandler<T = any> = (data: T) => void | Promise<void>;

/**
 * SignalR connection interface
 */
export interface ISignalRConnection {
  /**
   * Current connection state
   */
  readonly state: SignalRConnectionState;
  
  /**
   * Connection ID (null when disconnected)
   */
  readonly connectionId: string | null;
  
  /**
   * Whether the connection is currently connected
   */
  readonly isConnected: boolean;
  
  /**
   * Last error that occurred (if any)
   */
  readonly error: Error | null;
  
  /**
   * Connect to the SignalR hub
   */
  connect(): Promise<void>;
  
  /**
   * Disconnect from the SignalR hub
   */
  disconnect(): Promise<void>;
  
  /**
   * Register an event handler
   * @param eventName Name of the event to listen for
   * @param handler Function to call when the event is received
   */
  on<T = any>(eventName: string, handler: SignalREventHandler<T>): void;
  
  /**
   * Unregister an event handler
   * @param eventName Name of the event to stop listening for
   * @param handler Optional specific handler to remove
   */
  off(eventName: string, handler?: SignalREventHandler): void;
  
  /**
   * Invoke a hub method and wait for the response
   * @param methodName Name of the hub method to invoke
   * @param args Arguments to pass to the method
   */
  invoke<T = any>(methodName: string, ...args: any[]): Promise<T>;
  
  /**
   * Send a message to the hub without waiting for a response
   * @param methodName Name of the hub method to invoke
   * @param args Arguments to pass to the method
   */
  send(methodName: string, ...args: any[]): Promise<void>;
  
  /**
   * Join a SignalR group
   * @param groupName Name of the group to join
   */
  joinGroup(groupName: string): Promise<void>;
  
  /**
   * Leave a SignalR group
   * @param groupName Name of the group to leave
   */
  leaveGroup(groupName: string): Promise<void>;
}

/**
 * Event payload with timestamp
 */
export interface SignalREvent<T = any> {
  data: T;
  timestamp: string;
}

/**
 * Scenario update event
 */
export interface ScenarioUpdatedEvent {
  scenarioId: string;
  data: any;
  timestamp: string;
}

/**
 * Content published event
 */
export interface ContentPublishedEvent {
  contentId: string;
  data: any;
  timestamp: string;
}

/**
 * User activity event
 */
export interface UserActivityEvent {
  userId: string;
  data: any;
  timestamp: string;
}
