# SignalR WebSocket Implementation Files

This directory contains the implementation files for SignalR WebSocket support in the Mystira platform.

## Backend Implementation (Admin API)

The following files have been created for the Admin API (`packages/admin-api`):

### 1. EventsHub (`backend/Hubs/EventsHub.cs`)

SignalR hub that handles WebSocket connections from clients.

**Features:**
- Authentication required via `[Authorize]` attribute
- Connection lifecycle management (connect/disconnect logging)
- Group management for targeted broadcasts
- Ping/pong for connection testing

**Target Location:** `packages/admin-api/src/Mystira.App.Admin.Api/Hubs/EventsHub.cs`

### 2. Event Notification Service (`backend/Services/EventNotificationService.cs`)

Service for broadcasting events to connected SignalR clients.

**Methods:**
- `NotifyScenarioUpdatedAsync` - Broadcast scenario updates
- `NotifyContentPublishedAsync` - Broadcast published content
- `NotifyUserActivityAsync` - Broadcast user activities
- `NotifyAllAsync` - Send generic events to all clients
- `NotifyGroupAsync` - Send events to specific groups
- `NotifyUserAsync` - Send events to specific users

**Target Location:** `packages/admin-api/src/Mystira.App.Admin.Api/Services/EventNotificationService.cs`

### 3. SignalR Configuration (`backend/Configuration/SignalRConfiguration.cs`)

Extension methods for configuring SignalR in the application.

**Methods:**
- `AddMystiraSignalR` - Configures SignalR with Redis backplane
- `MapMystiraSignalRHubs` - Maps SignalR hub endpoints
- `AddSignalRCors` - Configures CORS for SignalR

**Target Location:** `packages/admin-api/src/Mystira.App.Admin.Api/Configuration/SignalRConfiguration.cs`

## Frontend Implementation (Shared Utils)

The following files have been added to the shared utilities package (`packages/shared-utils`):

### 1. SignalR Types (`src/signalr.types.ts`)

TypeScript interfaces and types for SignalR connections.

**Exports:**
- `SignalROptions` - Connection configuration
- `SignalRConnectionState` - Connection state enum
- `ISignalRConnection` - Connection interface
- Event types: `ScenarioUpdatedEvent`, `ContentPublishedEvent`, `UserActivityEvent`

**Status:** ✅ Already integrated into `packages/shared-utils`

### 2. SignalR Client (`src/signalr.client.ts`)

SignalR client implementation for TypeScript/React applications.

**Exports:**
- `SignalRConnection` - Main connection class
- `createSignalRConnection` - Factory function

**Features:**
- Automatic reconnection with exponential backoff
- Event handler management
- Group management
- TypeScript support
- Lazy loading of @microsoft/signalr

**Status:** ✅ Already integrated into `packages/shared-utils`

## Integration Steps

### Step 1: Add NuGet Packages to Admin API

```bash
cd packages/admin-api
dotnet add src/Mystira.App.Admin.Api/Mystira.App.Admin.Api.csproj package Microsoft.AspNetCore.SignalR
dotnet add src/Mystira.App.Admin.Api/Mystira.App.Admin.Api.csproj package Microsoft.AspNetCore.SignalR.StackExchangeRedis
```

### Step 2: Copy Backend Files to Admin API

The backend C# files are in this directory. Copy them to the admin-api submodule:

```bash
# From workspace root
cd packages/admin-api

# Initialize submodule if not already done
git submodule update --init

# Create a new branch for SignalR implementation
git checkout -b feature/signalr-websocket-support

# Copy the files
cp ../../docs/implementation/signalr/backend/Hubs/EventsHub.cs src/Mystira.App.Admin.Api/Hubs/
cp ../../docs/implementation/signalr/backend/Services/EventNotificationService.cs src/Mystira.App.Admin.Api/Services/
cp ../../docs/implementation/signalr/backend/Configuration/SignalRConfiguration.cs src/Mystira.App.Admin.Api/Configuration/

# Commit and push to admin-api repository
git add .
git commit -m "feat: add SignalR WebSocket support"
git push origin feature/signalr-websocket-support
```

### Step 3: Update Program.cs

Add SignalR configuration to `packages/admin-api/src/Mystira.App.Admin.Api/Program.cs`:

```csharp
using Mystira.App.Admin.Api.Configuration;
using Mystira.App.Admin.Api.Hubs;

// After existing service registrations (after builder.Services.AddControllers()), add:
builder.Services.AddMystiraSignalR(builder.Configuration, builder.Environment);

// Update CORS if needed (or use existing CORS policy)
builder.Services.AddSignalRCors(builder.Configuration);

var app = builder.Build();

// Configure CORS before other middleware
app.UseCors("SignalRPolicy"); // Or your existing CORS policy name

// After app.UseAuthorization(), add:
app.MapMystiraSignalRHubs();
```

### Step 4: Update appsettings.json

Add SignalR configuration to `packages/admin-api/src/Mystira.App.Admin.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"  // Update with your Redis connection string
  },
  "SignalR": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5173",
      "https://dev.admin.mystira.app",
      "https://dev.admin-api.mystira.app"
    ]
  }
}
```

For production (`appsettings.Production.json`):

```json
{
  "ConnectionStrings": {
    "Redis": "your-production-redis-connection-string"
  },
  "SignalR": {
    "AllowedOrigins": [
      "https://admin.mystira.app",
      "https://admin-api.mystira.app"
    ]
  }
}
```

### Step 5: Install Frontend Package

The SignalR client utilities are already in `@mystira/shared-utils`. 

For admin-ui or other projects using it:

```bash
cd packages/admin-ui  # or your React app
npm install @microsoft/signalr
```

Update `package.json` dependencies:

```json
{
  "dependencies": {
    "@microsoft/signalr": "^8.0.0",
    "@mystira/shared-utils": "workspace:*"
  }
}
```

### Step 6: Use SignalR in React Components

Example usage in a React component:

```typescript
import { createSignalRConnection, ScenarioUpdatedEvent } from '@mystira/shared-utils';
import { useEffect, useState } from 'react';

function Dashboard() {
  const [connection, setConnection] = useState<ISignalRConnection | null>(null);
  const [scenarios, setScenarios] = useState([]);

  useEffect(() => {
    // Create connection
    const conn = createSignalRConnection({
      hubUrl: `${import.meta.env.VITE_API_URL}/hubs/events`,
      accessTokenFactory: () => localStorage.getItem('access_token') || '',
      debug: import.meta.env.DEV,
    });

    // Listen for scenario updates
    conn.on<ScenarioUpdatedEvent>('ScenarioUpdated', (event) => {
      console.log('Scenario updated:', event);
      // Update local state
      setScenarios(prev => 
        prev.map(s => s.id === event.scenarioId ? { ...s, ...event.data } : s)
      );
    });

    setConnection(conn);

    return () => {
      conn.disconnect();
    };
  }, []);

  return (
    <div>
      <h1>Dashboard</h1>
      <div>
        Connection: {connection?.isConnected ? '🟢 Connected' : '🔴 Disconnected'}
      </div>
      {/* Rest of your UI */}
    </div>
  );
}
```

## Testing

### Backend Testing

```bash
cd packages/admin-api
dotnet build
dotnet test
dotnet run
```

### Manual Testing with wscat

```bash
# Install wscat
npm install -g wscat

# Test WebSocket connection
wscat -c ws://localhost:5000/hubs/events

# You should see SignalR handshake
```

### Frontend Testing

```bash
cd packages/shared-utils
npm test
```

## Example Usage in Services

Here's how to use the Event Notification Service in your existing services:

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

    public async Task UpdateScenarioAsync(Scenario scenario, CancellationToken cancellationToken)
    {
        await _repository.UpdateAsync(scenario, cancellationToken);
        
        // Broadcast update to all connected clients
        await _eventService.NotifyScenarioUpdatedAsync(
            scenario.Id,
            new
            {
                scenario.Title,
                scenario.Status,
                scenario.LastModified
            },
            cancellationToken);
    }
}
```

## Deployment Checklist

Follow the complete deployment guide:

1. ✅ Backend implementation files created
2. ✅ Frontend utilities implemented in shared-utils
3. ⏳ Add NuGet packages to Admin API
4. ⏳ Copy C# files to admin-api submodule
5. ⏳ Update Program.cs configuration
6. ⏳ Update appsettings.json
7. ⏳ Configure Front Door for WebSocket support
8. ⏳ Update DNS records
9. ⏳ Test WebSocket connections
10. ⏳ Deploy to staging
11. ⏳ Deploy to production

See [SignalR Deployment Checklist](../../guides/signalr-deployment-checklist.md) for detailed steps.

## Documentation

- [Full Implementation Guide](../../guides/signalr-websocket-implementation.md)
- [Deployment Checklist](../../guides/signalr-deployment-checklist.md)
- [ADR-0015: Event-Driven Architecture](../../planning/adr-0015-implementation-roadmap.md)

## Troubleshooting

### Connection Fails

- Check CORS configuration includes client origin
- Verify authentication token is valid
- Check Redis connection if using backplane

### Events Not Received

- Verify event name matches on client and server
- Check SignalR hub is mapped in Program.cs
- Review Application Insights logs

### Performance Issues

- Enable Redis backplane for scaling
- Check WebSocket transport is being used
- Monitor connection counts in Azure Monitor

## Notes

- Backend files are ready to be integrated into admin-api submodule
- Frontend files are already in shared-utils package
- Follow integration steps to complete implementation
- Test thoroughly in development before deploying to production
