# SignalR WebSocket Implementation Guide

## Overview

This guide provides comprehensive instructions for implementing SignalR WebSocket connections in the Mystira platform. SignalR enables real-time, bidirectional communication between the server and clients, replacing polling mechanisms with efficient WebSocket connections.

## Table of Contents

- [When to Implement Where](#when-to-implement-where)
- [Backend Implementation (ASP.NET Core)](#backend-implementation-aspnet-core)
- [Frontend Implementation (React/TypeScript)](#frontend-implementation-reacttypescript)
- [Deployment Configuration](#deployment-configuration)
- [Testing and Monitoring](#testing-and-monitoring)
- [Migration from Polling](#migration-from-polling)

---

## When to Implement Where

### Decision Matrix

| Component | Implement SignalR Hub? | Reason |
|-----------|----------------------|--------|
| **Admin API** (`packages/admin-api`) | ✅ **YES** | Primary backend for admin operations, content management, and real-time event broadcasting |
| **Story Generator** (`packages/story-generator`) | ✅ **YES** | Long-running story generation needs real-time progress updates |
| **Publisher** (`packages/publisher`) | ⚠️ **OPTIONAL** | Consider if content publishing needs real-time updates |
| **Admin UI** (`packages/admin-ui`) | ✅ **CLIENT** | React client that consumes SignalR connections from Admin API |
| **App** (`packages/app`) | ⚠️ **EVALUATE** | Depends on whether end-users need real-time features |

### Recommended Approach

**Start with Admin API + Admin UI** - This is the most valuable implementation:

1. **Admin API** (C#/ASP.NET Core) - Implement SignalR Hub at `/hubs/events`
2. **Admin UI** (React/TypeScript) - Replace polling with SignalR client
3. **Story Generator** - Add SignalR Hub for story generation progress updates

---

## Backend Implementation (ASP.NET Core)

### 1. Add SignalR Package

**Admin API (`packages/admin-api/Mystira.Admin.Api.csproj`)**:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="8.0.*" />
  <PackageReference Include="Microsoft.AspNetCore.SignalR.StackExchangeRedis" Version="8.0.*" />
</ItemGroup>
```

### 2. Create SignalR Hub

**`packages/admin-api/Hubs/EventsHub.cs`**:

```csharp
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace Mystira.Admin.Api.Hubs;

/// <summary>
/// SignalR hub for real-time event broadcasting to admin UI clients.
/// Replaces polling mechanisms for better performance and user experience.
/// </summary>
[Authorize] // Require authentication
public class EventsHub : Hub
{
    private readonly ILogger<EventsHub> _logger;

    public EventsHub(ILogger<EventsHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        _logger.LogInformation(
            "Client connected to EventsHub: ConnectionId={ConnectionId}, UserId={UserId}",
            Context.ConnectionId,
            userId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        
        if (exception != null)
        {
            _logger.LogWarning(
                exception,
                "Client disconnected with error: ConnectionId={ConnectionId}, UserId={UserId}",
                Context.ConnectionId,
                userId);
        }
        else
        {
            _logger.LogInformation(
                "Client disconnected: ConnectionId={ConnectionId}, UserId={UserId}",
                Context.ConnectionId,
                userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client can join a specific group (e.g., for scenario-specific updates).
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation(
            "Client joined group: ConnectionId={ConnectionId}, Group={GroupName}",
            Context.ConnectionId,
            groupName);
    }

    /// <summary>
    /// Client can leave a specific group.
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogInformation(
            "Client left group: ConnectionId={ConnectionId}, Group={GroupName}",
            Context.ConnectionId,
            groupName);
    }
}
```

### 3. Create Event Notification Service

**`packages/admin-api/Services/EventNotificationService.cs`**:

```csharp
using Microsoft.AspNetCore.SignalR;
using Mystira.Admin.Api.Hubs;

namespace Mystira.Admin.Api.Services;

/// <summary>
/// Service for broadcasting events to SignalR clients.
/// Use this instead of polling endpoints.
/// </summary>
public interface IEventNotificationService
{
    Task NotifyScenarioUpdatedAsync(string scenarioId, object data);
    Task NotifyContentPublishedAsync(string contentId, object data);
    Task NotifyUserActivityAsync(string userId, object data);
    Task NotifyAllAsync(string eventType, object data);
    Task NotifyGroupAsync(string groupName, string eventType, object data);
}

public class EventNotificationService : IEventNotificationService
{
    private readonly IHubContext<EventsHub> _hubContext;
    private readonly ILogger<EventNotificationService> _logger;

    public EventNotificationService(
        IHubContext<EventsHub> hubContext,
        ILogger<EventNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyScenarioUpdatedAsync(string scenarioId, object data)
    {
        _logger.LogDebug("Broadcasting scenario update: {ScenarioId}", scenarioId);
        
        await _hubContext.Clients.All.SendAsync(
            "ScenarioUpdated",
            new { scenarioId, data, timestamp = DateTimeOffset.UtcNow });
    }

    public async Task NotifyContentPublishedAsync(string contentId, object data)
    {
        _logger.LogDebug("Broadcasting content published: {ContentId}", contentId);
        
        await _hubContext.Clients.All.SendAsync(
            "ContentPublished",
            new { contentId, data, timestamp = DateTimeOffset.UtcNow });
    }

    public async Task NotifyUserActivityAsync(string userId, object data)
    {
        _logger.LogDebug("Broadcasting user activity: {UserId}", userId);
        
        await _hubContext.Clients.All.SendAsync(
            "UserActivity",
            new { userId, data, timestamp = DateTimeOffset.UtcNow });
    }

    public async Task NotifyAllAsync(string eventType, object data)
    {
        _logger.LogDebug("Broadcasting event to all clients: {EventType}", eventType);
        
        await _hubContext.Clients.All.SendAsync(
            eventType,
            new { data, timestamp = DateTimeOffset.UtcNow });
    }

    public async Task NotifyGroupAsync(string groupName, string eventType, object data)
    {
        _logger.LogDebug(
            "Broadcasting event to group: Group={GroupName}, EventType={EventType}",
            groupName,
            eventType);
        
        await _hubContext.Clients.Group(groupName).SendAsync(
            eventType,
            new { data, timestamp = DateTimeOffset.UtcNow });
    }
}
```

### 4. Configure SignalR in Program.cs

**`packages/admin-api/Program.cs`**:

```csharp
using Mystira.Admin.Api.Hubs;
using Mystira.Admin.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR with Redis backplane for scaling
builder.Services.AddSignalR(options =>
{
    // Configure timeouts
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    
    // Enable detailed errors in development
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
})
.AddStackExchangeRedis(
    builder.Configuration.GetConnectionString("Redis"),
    options =>
    {
        options.Configuration.ChannelPrefix = "mystira:signalr:";
    });

// Register event notification service
builder.Services.AddSingleton<IEventNotificationService, EventNotificationService>();

// Add CORS for SignalR (adjust origins as needed)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://admin.mystira.app",
            "http://localhost:3000",
            "http://localhost:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // Required for SignalR
    });
});

var app = builder.Build();

// Enable CORS
app.UseCors();

// Map SignalR hub
app.MapHub<EventsHub>("/hubs/events");

app.Run();
```

### 5. Integrate with Domain Events

**Example: Publishing events when scenarios are updated**

```csharp
public class ScenarioService
{
    private readonly IScenarioRepository _repository;
    private readonly IEventNotificationService _eventService;

    public ScenarioService(
        IScenarioRepository repository,
        IEventNotificationService eventService)
    {
        _repository = repository;
        _eventService = eventService;
    }

    public async Task UpdateScenarioAsync(Scenario scenario)
    {
        await _repository.UpdateAsync(scenario);
        
        // Broadcast update to all connected clients
        await _eventService.NotifyScenarioUpdatedAsync(
            scenario.Id,
            new
            {
                scenario.Title,
                scenario.Status,
                scenario.LastModified
            });
    }
}
```

---

## Frontend Implementation (React/TypeScript)

### 1. Install SignalR Client

**`packages/admin-ui/package.json`**:

```json
{
  "dependencies": {
    "@microsoft/signalr": "^8.0.0"
  }
}
```

### 2. Create SignalR Service Hook

**`packages/admin-ui/src/hooks/useSignalR.ts`**:

```typescript
import { useEffect, useRef, useState, useCallback } from 'react';
import * as signalR from '@microsoft/signalr';

interface UseSignalROptions {
  hubUrl: string;
  accessToken?: string;
  autoConnect?: boolean;
  reconnectDelay?: number;
}

interface SignalRConnection {
  connection: signalR.HubConnection | null;
  isConnected: boolean;
  error: Error | null;
  connect: () => Promise<void>;
  disconnect: () => Promise<void>;
  on: (eventName: string, callback: (...args: any[]) => void) => void;
  off: (eventName: string) => void;
  invoke: <T = any>(methodName: string, ...args: any[]) => Promise<T>;
}

export function useSignalR({
  hubUrl,
  accessToken,
  autoConnect = true,
  reconnectDelay = 5000,
}: UseSignalROptions): SignalRConnection {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [error, setError] = useState<Error | null>(null);
  const eventHandlersRef = useRef<Map<string, Set<(...args: any[]) => void>>>(
    new Map()
  );

  const connect = useCallback(async () => {
    if (connectionRef.current?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    try {
      // Build connection
      const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
          accessTokenFactory: () => accessToken || '',
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff with max delay
            return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), reconnectDelay);
          },
        })
        .configureLogging(
          process.env.NODE_ENV === 'development'
            ? signalR.LogLevel.Debug
            : signalR.LogLevel.Warning
        )
        .build();

      // Setup event handlers
      connection.onreconnecting((error) => {
        console.warn('SignalR reconnecting...', error);
        setIsConnected(false);
        setError(error || null);
      });

      connection.onreconnected((connectionId) => {
        console.info('SignalR reconnected:', connectionId);
        setIsConnected(true);
        setError(null);
      });

      connection.onclose((error) => {
        console.error('SignalR connection closed', error);
        setIsConnected(false);
        setError(error || null);
      });

      // Re-register event handlers
      eventHandlersRef.current.forEach((handlers, eventName) => {
        handlers.forEach((handler) => {
          connection.on(eventName, handler);
        });
      });

      await connection.start();
      connectionRef.current = connection;
      setIsConnected(true);
      setError(null);

      console.info('SignalR connected:', connection.connectionId);
    } catch (err) {
      const error = err instanceof Error ? err : new Error(String(err));
      console.error('SignalR connection failed:', error);
      setError(error);
      setIsConnected(false);
    }
  }, [hubUrl, accessToken, reconnectDelay]);

  const disconnect = useCallback(async () => {
    if (connectionRef.current) {
      try {
        await connectionRef.current.stop();
        connectionRef.current = null;
        setIsConnected(false);
        setError(null);
      } catch (err) {
        console.error('Error disconnecting SignalR:', err);
      }
    }
  }, []);

  const on = useCallback((eventName: string, callback: (...args: any[]) => void) => {
    // Store handler for reconnection
    if (!eventHandlersRef.current.has(eventName)) {
      eventHandlersRef.current.set(eventName, new Set());
    }
    eventHandlersRef.current.get(eventName)!.add(callback);

    // Register with connection if connected
    if (connectionRef.current) {
      connectionRef.current.on(eventName, callback);
    }
  }, []);

  const off = useCallback((eventName: string) => {
    // Remove from stored handlers
    eventHandlersRef.current.delete(eventName);

    // Unregister from connection
    if (connectionRef.current) {
      connectionRef.current.off(eventName);
    }
  }, []);

  const invoke = useCallback(
    async <T = any>(methodName: string, ...args: any[]): Promise<T> => {
      if (!connectionRef.current) {
        throw new Error('SignalR connection not established');
      }
      return connectionRef.current.invoke<T>(methodName, ...args);
    },
    []
  );

  useEffect(() => {
    if (autoConnect) {
      connect();
    }

    return () => {
      disconnect();
    };
  }, [autoConnect, connect, disconnect]);

  return {
    connection: connectionRef.current,
    isConnected,
    error,
    connect,
    disconnect,
    on,
    off,
    invoke,
  };
}
```

### 3. Create Event Listener Hook

**`packages/admin-ui/src/hooks/useEventListener.ts`**:

```typescript
import { useEffect } from 'react';
import { useSignalR } from './useSignalR';

interface UseEventListenerOptions<T> {
  eventName: string;
  onEvent: (data: T) => void;
  enabled?: boolean;
}

export function useEventListener<T = any>({
  eventName,
  onEvent,
  enabled = true,
}: UseEventListenerOptions<T>) {
  const signalR = useSignalR({
    hubUrl: `${import.meta.env.VITE_API_URL}/hubs/events`,
    accessToken: localStorage.getItem('access_token') || undefined,
  });

  useEffect(() => {
    if (!enabled || !signalR.isConnected) {
      return;
    }

    signalR.on(eventName, onEvent);

    return () => {
      signalR.off(eventName);
    };
  }, [signalR, eventName, onEvent, enabled]);

  return signalR;
}
```

### 4. Use in Components - Replace Polling

**Before (Polling)**:

```typescript
function ScenarioList() {
  const [scenarios, setScenarios] = useState<Scenario[]>([]);

  // ❌ OLD: Polling every 5 seconds
  useEffect(() => {
    const interval = setInterval(async () => {
      const response = await fetch('/api/scenarios');
      const data = await response.json();
      setScenarios(data);
    }, 5000);

    return () => clearInterval(interval);
  }, []);

  return <div>{/* Render scenarios */}</div>;
}
```

**After (SignalR)**:

```typescript
function ScenarioList() {
  const [scenarios, setScenarios] = useState<Scenario[]>([]);

  // Initial load
  useEffect(() => {
    async function loadScenarios() {
      const response = await fetch('/api/scenarios');
      const data = await response.json();
      setScenarios(data);
    }
    loadScenarios();
  }, []);

  // ✅ NEW: Real-time updates via SignalR
  useEventListener<{ scenarioId: string; data: any }>({
    eventName: 'ScenarioUpdated',
    onEvent: ({ scenarioId, data }) => {
      setScenarios((prev) =>
        prev.map((s) => (s.id === scenarioId ? { ...s, ...data } : s))
      );
    },
  });

  return <div>{/* Render scenarios */}</div>;
}
```

### 5. Complete Example with Connection Status

```typescript
import { useSignalR } from '@/hooks/useSignalR';
import { useEventListener } from '@/hooks/useEventListener';

function Dashboard() {
  const [updates, setUpdates] = useState<any[]>([]);

  const signalR = useSignalR({
    hubUrl: `${import.meta.env.VITE_API_URL}/hubs/events`,
    accessToken: localStorage.getItem('access_token') || undefined,
  });

  // Listen to multiple event types
  useEffect(() => {
    if (!signalR.isConnected) return;

    const handleScenarioUpdate = (data: any) => {
      setUpdates((prev) => [...prev, { type: 'scenario', data }]);
    };

    const handleContentPublished = (data: any) => {
      setUpdates((prev) => [...prev, { type: 'content', data }]);
    };

    signalR.on('ScenarioUpdated', handleScenarioUpdate);
    signalR.on('ContentPublished', handleContentPublished);

    return () => {
      signalR.off('ScenarioUpdated');
      signalR.off('ContentPublished');
    };
  }, [signalR]);

  return (
    <div>
      {/* Connection status indicator */}
      <div className="connection-status">
        {signalR.isConnected ? (
          <span className="text-green-600">● Connected</span>
        ) : signalR.error ? (
          <span className="text-red-600">● Error: {signalR.error.message}</span>
        ) : (
          <span className="text-yellow-600">● Connecting...</span>
        )}
      </div>

      {/* Real-time updates */}
      <div className="updates">
        {updates.map((update, i) => (
          <div key={i}>{JSON.stringify(update)}</div>
        ))}
      </div>
    </div>
  );
}
```

---

## Deployment Configuration

### 0. Azure Front Door Configuration for WebSocket/SignalR

**Azure Front Door requires special configuration to support WebSocket/SignalR connections.** Follow these steps to enable and configure Front Door with WebSocket support.

#### Prerequisites

- Backend services (Admin API, Publisher, Story Generator) deployed and healthy
- DNS access to configure CNAME records
- Azure subscription with appropriate permissions

#### Step 1: Enable Front Door in Terraform

**For Admin API with SignalR (`infra/terraform/environments/dev/front-door.tf`)**:

Ensure `enable_admin_services` is set to `true` in the Front Door configuration:

```hcl
module "front_door" {
  source = "../../modules/front-door"

  environment         = "dev"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  project_name        = "mystira"

  # Backend addresses
  publisher_backend_address = "dev.publisher.mystira.app"
  chain_backend_address     = "dev.chain.mystira.app"

  # Custom domains
  custom_domain_publisher = "dev.publisher.mystira.app"
  custom_domain_chain     = "dev.chain.mystira.app"

  # ✅ CRITICAL: Enable admin services for SignalR support
  enable_admin_services     = true
  admin_api_backend_address = "dev.admin-api.mystira.app"
  admin_ui_backend_address  = "dev.admin.mystira.app"
  custom_domain_admin_api   = "dev.admin-api.mystira.app"
  custom_domain_admin_ui    = "dev.admin.mystira.app"

  # Story Generator (if using SignalR for progress updates)
  enable_story_generator              = true
  story_generator_api_backend_address = "dev.story-api.mystira.app"
  story_generator_swa_backend_address = module.story_generator.static_web_app_default_hostname
  custom_domain_story_generator_api   = "dev.story-api.mystira.app"
  custom_domain_story_generator_swa   = "dev.story.mystira.app"

  # WAF and other settings
  enable_waf           = true
  waf_mode             = "Detection" # Use Detection for dev
  enable_caching       = true
  session_affinity_enabled = false

  tags = local.common_tags
}
```

**For Mystira.App service (if needed)**:

If your Mystira.App service requires SignalR, you would need to add Front Door support. Currently, the Front Door module doesn't have a dedicated `enable_mystira_app` variable. To add Mystira.App endpoints to Front Door, you have two options:

**Option A: Add variables to Front Door module**

Add to `infra/terraform/modules/front-door/variables.tf`:

```hcl
variable "enable_mystira_app" {
  description = "Enable Mystira.App API and web endpoints in Front Door"
  type        = bool
  default     = false
}

variable "mystira_app_api_backend_address" {
  description = "Backend address for Mystira.App API"
  type        = string
  default     = ""
}

variable "mystira_app_web_backend_address" {
  description = "Backend address for Mystira.App web frontend"
  type        = string
  default     = ""
}

variable "custom_domain_mystira_app_api" {
  description = "Custom domain for Mystira.App API"
  type        = string
  default     = ""
}

variable "custom_domain_mystira_app_web" {
  description = "Custom domain for Mystira.App web"
  type        = string
  default     = ""
}
```

Then add similar endpoint/origin/route resources in `main.tf` following the admin services pattern.

**Option B: Use existing admin services configuration**

If Mystira.App is your primary admin interface, you can map it to the admin endpoints:

```hcl
  enable_admin_services     = true
  admin_api_backend_address = "dev.mystira-api.mystira.app"  # Mystira.App API
  admin_ui_backend_address  = "dev.mystira.mystira.app"      # Mystira.App web
```

#### Step 2: Deploy Front Door Infrastructure

```bash
cd infra/terraform/environments/dev
terraform init
terraform plan -out=tfplan
terraform apply tfplan
```

**Note the outputs** - you'll need these for DNS configuration:

```bash
terraform output front_door_admin_api_endpoint
terraform output front_door_admin_ui_endpoint
terraform output front_door_custom_domain_verification
```

Example output:
```
front_door_admin_api_endpoint = "mystira-dev-admin-api-abc123.z01.azurefd.net"
front_door_admin_ui_endpoint = "mystira-dev-admin-ui-abc123.z01.azurefd.net"
```

#### Step 3: Update DNS CNAME Records

Update your DNS provider (e.g., Cloudflare, Azure DNS, Route53) to point to Front Door:

**Before (direct to AKS ingress)**:
```
dev.admin-api.mystira.app  A     20.123.45.67  (NGINX Ingress IP)
dev.admin.mystira.app      A     20.123.45.67  (NGINX Ingress IP)
```

**After (through Front Door)**:
```
dev.admin-api.mystira.app  CNAME  mystira-dev-admin-api-abc123.z01.azurefd.net
dev.admin.mystira.app      CNAME  mystira-dev-admin-ui-abc123.z01.azurefd.net
```

**Using Azure CLI (for Azure DNS)**:

```bash
# Remove old A records
az network dns record-set a delete \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --name dev.admin-api \
  --yes

az network dns record-set a delete \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --name dev.admin \
  --yes

# Add new CNAME records
az network dns record-set cname create \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --name dev.admin-api

az network dns record-set cname set-record \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --record-set-name dev.admin-api \
  --cname mystira-dev-admin-api-abc123.z01.azurefd.net

az network dns record-set cname create \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --name dev.admin

az network dns record-set cname set-record \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --record-set-name dev.admin \
  --cname mystira-dev-admin-ui-abc123.z01.azurefd.net
```

#### Step 4: Add Domain Validation TXT Records

Azure Front Door requires domain ownership validation via `_dnsauth` TXT records.

**Get validation token**:

```bash
terraform output front_door_custom_domain_verification
```

Example output:
```json
{
  "admin-api": {
    "validation_token": "abc123def456...",
    "validation_record_name": "_dnsauth.dev.admin-api"
  },
  "admin-ui": {
    "validation_token": "xyz789ghi012...",
    "validation_record_name": "_dnsauth.dev.admin"
  }
}
```

**Add TXT records**:

```bash
# Admin API validation
az network dns record-set txt add-record \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --record-set-name _dnsauth.dev.admin-api \
  --value "abc123def456..."

# Admin UI validation
az network dns record-set txt add-record \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --record-set-name _dnsauth.dev.admin \
  --value "xyz789ghi012..."
```

**Alternative: Add via Terraform** (`infra/terraform/modules/dns/front-door-validation.tf`):

```hcl
resource "azurerm_dns_txt_record" "admin_api_validation" {
  name                = "_dnsauth.dev.admin-api"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = azurerm_dns_zone.main.resource_group_name
  ttl                 = 300

  record {
    value = module.front_door.admin_api_validation_token
  }
}

resource "azurerm_dns_txt_record" "admin_ui_validation" {
  name                = "_dnsauth.dev.admin"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = azurerm_dns_zone.main.resource_group_name
  ttl                 = 300

  record {
    value = module.front_door.admin_ui_validation_token
  }
}
```

#### Step 5: Wait for Certificate Provisioning

Azure Front Door automatically provisions managed TLS certificates after domain validation.

**Check certificate status**:

```bash
# Via Azure CLI
az afd custom-domain show \
  --resource-group mys-dev-core-rg-san \
  --profile-name mystira-dev-fd \
  --custom-domain-name mystira-dev-admin-api-domain \
  --query "tlsSettings.certificateType"

# Expected: "ManagedCertificate"
```

**Typical timeline**:
- DNS propagation: 5-30 minutes
- Domain validation: 10-60 minutes
- Certificate issuance: 30-120 minutes

**Total time**: 45 minutes to 3 hours

**Monitor via Azure Portal**:
1. Navigate to Front Door profile
2. Go to "Domains"
3. Check "Certificate Status" column
4. Status should progress: `Pending` → `Validating` → `Approved`

#### Step 6: Verify WebSocket/SignalR Through Front Door

**Test WebSocket connection**:

```bash
# Install wscat if not already installed
npm install -g wscat

# Test WebSocket endpoint (replace with your domain)
wscat -c wss://dev.admin-api.mystira.app/hubs/events

# Expected output:
# Connected (press CTRL+C to quit)
# < {"protocol":"json","version":1}
```

**Test SignalR connection (browser console)**:

```javascript
// Open browser console on https://dev.admin.mystira.app
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://dev.admin-api.mystira.app/hubs/events", {
    accessTokenFactory: () => "YOUR_AUTH_TOKEN",
    transport: signalR.HttpTransportType.WebSockets
  })
  .configureLogging(signalR.LogLevel.Debug)
  .build();

await connection.start();
console.log("Connected:", connection.connectionId);
```

**Verify routing through Front Door**:

```bash
# Check response headers to confirm Front Door is handling request
curl -I https://dev.admin-api.mystira.app/health

# Look for Front Door headers:
# X-Azure-Ref: ...
# X-Cache: ...
# X-FD-...
```

**Test from Admin UI**:

1. Open Admin UI: `https://dev.admin.mystira.app`
2. Open browser DevTools → Network tab
3. Filter for `WS` (WebSocket)
4. Verify SignalR connection established
5. Check connection uses `wss://` (secure WebSocket)

#### Step 7: Configure Backend for Front Door Headers

Update your Admin API to handle Front Door headers:

**`packages/admin-api/Program.cs`**:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers for Front Door
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    
    // Trust Front Door forwarded headers
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    
    // Front Door sends client IP in X-Azure-ClientIP
    options.ForwardedForHeaderName = "X-Forwarded-For";
    options.OriginalHostHeaderName = "X-Original-Host";
});

var app = builder.Build();

// Enable forwarded headers BEFORE other middleware
app.UseForwardedHeaders();

// Rest of middleware...
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<EventsHub>("/hubs/events");
app.Run();
```

#### Troubleshooting

**Issue: WebSocket connection fails with 502 Bad Gateway**

**Solution**: Check backend health probe

```bash
# Verify backend is healthy
kubectl get pods -n mys-dev
kubectl logs -n mys-dev deploy/mys-admin-api --tail=50

# Check Front Door origin health
az afd origin show \
  --resource-group mys-dev-core-rg-san \
  --profile-name mystira-dev-fd \
  --origin-group-name admin-api-origin-group \
  --origin-name admin-api-origin \
  --query "healthProbeSettings"
```

**Issue: Certificate not provisioning**

**Solution**: Verify DNS records

```bash
# Check DNS propagation
dig _dnsauth.dev.admin-api.mystira.app TXT
dig dev.admin-api.mystira.app CNAME

# Verify validation token matches
terraform output front_door_custom_domain_verification
```

**Issue: SignalR connects but immediately disconnects**

**Solution**: Check CORS configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // Must include Front Door domain
        policy.WithOrigins(
            "https://dev.admin.mystira.app",           // Front Door domain
            "https://dev.admin-api.mystira.app",       // API domain
            "http://localhost:3000",                   // Local dev
            "http://localhost:5173"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();  // Required for SignalR
    });
});
```

**Issue: High latency on WebSocket connections**

**Solution**: Verify session affinity is disabled (WebSocket doesn't need it)

```hcl
# In front-door.tf
module "front_door" {
  # ...
  session_affinity_enabled = false  # ✅ Correct for stateless SignalR
}
```

#### Environment-Specific Configuration

**Development**:
```hcl
enable_admin_services = true
waf_mode              = "Detection"  # Don't block during testing
cache_duration_seconds = 1800        # 30 minutes
```

**Staging**:
```hcl
enable_admin_services = true
waf_mode              = "Prevention"
cache_duration_seconds = 3600        # 1 hour
```

**Production**:
```hcl
enable_admin_services = true
waf_mode              = "Prevention"
cache_duration_seconds = 3600
rate_limit_threshold  = 300          # Higher for production
```

#### Summary Checklist

- [ ] Set `enable_admin_services = true` in `front-door.tf`
- [ ] Run `terraform apply` to create Front Door endpoints
- [ ] Update DNS CNAME records to point to Front Door hostnames
- [ ] Add `_dnsauth` TXT records for domain validation
- [ ] Wait for certificate provisioning (check Azure Portal)
- [ ] Test WebSocket/SignalR connections through Front Door
- [ ] Configure backend to handle Front Door headers
- [ ] Verify health probes passing
- [ ] Monitor connection metrics in Azure Monitor

---

### 1. Azure SignalR Service (Recommended for Production)

For production, use **Azure SignalR Service** for better scalability:

**Terraform (`infra/terraform/modules/signalr/main.tf`)**:

```hcl
resource "azurerm_signalr_service" "mystira" {
  name                = "mys-${var.environment}-signalr-san"
  location            = var.location
  resource_group_name = var.resource_group_name

  sku {
    name     = "Standard_S1"
    capacity = 1
  }

  service_mode = "Default"

  cors {
    allowed_origins = [
      "https://admin.mystira.app",
      "https://${var.environment}.admin.mystira.app"
    ]
  }

  tags = var.tags
}

output "connection_string" {
  value     = azurerm_signalr_service.mystira.primary_connection_string
  sensitive = true
}
```

**Update Admin API configuration**:

```csharp
builder.Services.AddSignalR()
    .AddAzureSignalR(builder.Configuration["Azure:SignalR:ConnectionString"]);
```

### 2. Kubernetes Configuration

**`infra/kubernetes/base/admin-api/deployment.yaml`**:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mys-admin-api
spec:
  template:
    spec:
      containers:
        - name: admin-api
          env:
            - name: Azure__SignalR__ConnectionString
              valueFrom:
                secretKeyRef:
                  name: admin-api-secrets
                  key: signalr-connection-string
            # Enable sticky sessions for SignalR
            - name: ASPNETCORE_FORWARDEDHEADERS_ENABLED
              value: "true"
```

**`infra/kubernetes/base/admin-api/service.yaml`**:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: mys-admin-api
  annotations:
    # Enable session affinity for SignalR
    service.beta.kubernetes.io/azure-load-balancer-session-affinity: "ClientIP"
spec:
  sessionAffinity: ClientIP
  sessionAffinityConfig:
    clientIP:
      timeoutSeconds: 10800
```

### 3. Environment Variables

**`.env.admin-api`**:

```bash
# Development: Use Redis backplane
ConnectionStrings__Redis=localhost:6379

# Production: Use Azure SignalR Service
Azure__SignalR__ConnectionString=Endpoint=https://mys-prod-signalr-san.service.signalr.net;AccessKey=...;Version=1.0;
```

**`.env.admin-ui`** (for frontend):

```bash
VITE_API_URL=https://api.admin.mystira.app
VITE_SIGNALR_HUB_URL=https://api.admin.mystira.app/hubs/events
```

---

## Testing and Monitoring

### 1. Integration Tests

**`packages/admin-api/Tests/SignalRTests.cs`**:

```csharp
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

public class SignalRIntegrationTests : IAsyncLifetime
{
    private HubConnection _connection;

    public async Task InitializeAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/hubs/events")
            .Build();

        await _connection.StartAsync();
    }

    [Fact]
    public async Task Should_Receive_ScenarioUpdated_Event()
    {
        var tcs = new TaskCompletionSource<object>();

        _connection.On<object>("ScenarioUpdated", data =>
        {
            tcs.SetResult(data);
        });

        // Trigger scenario update through API
        // ...

        var result = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.NotNull(result);
    }

    public async Task DisposeAsync()
    {
        await _connection.StopAsync();
        await _connection.DisposeAsync();
    }
}
```

### 2. Frontend Tests

**`packages/admin-ui/src/hooks/__tests__/useSignalR.test.ts`**:

```typescript
import { renderHook, waitFor } from '@testing-library/react';
import { useSignalR } from '../useSignalR';

describe('useSignalR', () => {
  it('should connect to SignalR hub', async () => {
    const { result } = renderHook(() =>
      useSignalR({
        hubUrl: 'http://localhost:5000/hubs/events',
        autoConnect: true,
      })
    );

    await waitFor(() => {
      expect(result.current.isConnected).toBe(true);
    });
  });

  it('should receive events', async () => {
    const { result } = renderHook(() =>
      useSignalR({
        hubUrl: 'http://localhost:5000/hubs/events',
        autoConnect: true,
      })
    );

    const events: any[] = [];
    result.current.on('TestEvent', (data) => {
      events.push(data);
    });

    // Trigger event from server...

    await waitFor(() => {
      expect(events.length).toBeGreaterThan(0);
    });
  });
});
```

### 3. Monitoring

**Application Insights Query**:

```kusto
// SignalR connection metrics
customMetrics
| where name startswith "signalr"
| summarize 
    AvgConnections = avg(value),
    MaxConnections = max(value)
    by bin(timestamp, 5m), name

// SignalR errors
exceptions
| where outerMessage contains "SignalR"
| summarize count() by bin(timestamp, 1h), outerMessage
```

**Azure Monitor Alerts**:

- SignalR connection count > threshold
- SignalR message send failures
- WebSocket connection errors

---

## Migration from Polling

### Migration Strategy

1. **Phase 1: Implement SignalR alongside polling**
   - Add SignalR infrastructure
   - Keep existing polling as fallback
   - Test with feature flag

2. **Phase 2: Switch primary mechanism**
   - Default to SignalR
   - Use polling as fallback for unsupported browsers

3. **Phase 3: Remove polling code**
   - Monitor SignalR reliability
   - Remove polling endpoints and client code

### Feature Flag Implementation

```typescript
// Feature flag for gradual rollout
const useSignalR = import.meta.env.VITE_FEATURE_SIGNALR === 'true';

function DataComponent() {
  const [data, setData] = useState([]);

  // Real-time via SignalR
  useEventListener({
    eventName: 'DataUpdated',
    onEvent: setData,
    enabled: useSignalR,
  });

  // Fallback polling
  useEffect(() => {
    if (useSignalR) return;

    const interval = setInterval(async () => {
      const response = await fetch('/api/data');
      setData(await response.json());
    }, 5000);

    return () => clearInterval(interval);
  }, [useSignalR]);

  return <div>{/* Render data */}</div>;
}
```

### Graceful Degradation

```typescript
function useRealTimeData() {
  const signalR = useSignalR({
    hubUrl: `${import.meta.env.VITE_API_URL}/hubs/events`,
    accessToken: getAccessToken(),
  });

  const [data, setData] = useState([]);
  const [usingFallback, setUsingFallback] = useState(false);

  // Primary: SignalR
  useEffect(() => {
    if (signalR.isConnected) {
      setUsingFallback(false);
      signalR.on('DataUpdated', setData);
    }
  }, [signalR]);

  // Fallback: Polling (if SignalR fails)
  useEffect(() => {
    if (signalR.error && !signalR.isConnected) {
      setUsingFallback(true);
      const interval = setInterval(async () => {
        const response = await fetch('/api/data');
        setData(await response.json());
      }, 10000); // Slower polling as fallback

      return () => clearInterval(interval);
    }
  }, [signalR]);

  return { data, usingFallback };
}
```

---

## Best Practices

### Security

1. **Always require authentication**:
   ```csharp
   [Authorize] // On hub class
   ```

2. **Validate user access in hub methods**:
   ```csharp
   public async Task JoinGroup(string groupName)
   {
       var userId = Context.User?.FindFirst("sub")?.Value;
       if (!await _authService.CanAccessGroup(userId, groupName))
       {
           throw new HubException("Unauthorized");
       }
       await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
   }
   ```

3. **Use CORS properly**:
   ```csharp
   builder.Services.AddCors(options =>
   {
       options.AddDefaultPolicy(policy =>
       {
           policy.WithOrigins("https://admin.mystira.app")
               .AllowCredentials(); // Required for SignalR
       });
   });
   ```

### Performance

1. **Use Redis backplane for scaling**
2. **Implement connection throttling**
3. **Use groups for targeted broadcasting**
4. **Monitor connection count and message throughput**

### Reliability

1. **Implement automatic reconnection** (handled by SignalR client)
2. **Handle connection state changes gracefully**
3. **Provide fallback mechanisms**
4. **Log connection events for debugging**

---

## Troubleshooting

### Common Issues

1. **CORS errors**:
   - Ensure `AllowCredentials()` is set
   - Check allowed origins

2. **Connection failures**:
   - Verify WebSocket support
   - Check firewall/proxy settings
   - Ensure sticky sessions in load balancer

3. **Reconnection loops**:
   - Check Redis backplane connectivity
   - Verify Azure SignalR Service health
   - Review timeout configurations

---

## Related Documentation

- [ADR-0015: Event-Driven Architecture](../planning/adr-0015-implementation-roadmap.md)
- [Caching Strategies Guide](./caching-strategies.md)
- [Infrastructure: Shared Resources](../infrastructure/shared-resources.md)

---

## References

- [ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
- [Azure SignalR Service](https://learn.microsoft.com/en-us/azure/azure-signalr/)
- [@microsoft/signalr NPM Package](https://www.npmjs.com/package/@microsoft/signalr)
- [SignalR Redis Backplane](https://learn.microsoft.com/en-us/aspnet/core/signalr/redis-backplane)
