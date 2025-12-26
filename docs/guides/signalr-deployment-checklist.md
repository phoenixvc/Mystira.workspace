# SignalR WebSocket Deployment Checklist

Quick reference guide for deploying SignalR WebSocket infrastructure to Mystira platform.

## Prerequisites

- [ ] Backend services deployed and healthy
- [ ] DNS access configured
- [ ] Azure subscription with appropriate permissions
- [ ] Terraform installed and configured

## Phase 1: Backend Implementation (Admin API)

### 1. Add NuGet Packages

```bash
cd packages/admin-api
dotnet add package Microsoft.AspNetCore.SignalR --version 8.0.*
dotnet add package Microsoft.AspNetCore.SignalR.StackExchangeRedis --version 8.0.*
```

### 2. Create SignalR Hub

- [ ] Create `Hubs/EventsHub.cs` with `[Authorize]` attribute
- [ ] Implement `OnConnectedAsync` and `OnDisconnectedAsync`
- [ ] Add group management methods (`JoinGroup`, `LeaveGroup`)

### 3. Create Event Notification Service

- [ ] Create `Services/IEventNotificationService.cs` interface
- [ ] Implement `EventNotificationService.cs` with `IHubContext<EventsHub>`
- [ ] Add methods for broadcasting events (ScenarioUpdated, ContentPublished, etc.)

### 4. Configure in Program.cs

```csharp
// Add SignalR with Redis backplane
builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration.GetConnectionString("Redis"));

// Register notification service
builder.Services.AddSingleton<IEventNotificationService, EventNotificationService>();

// Configure CORS with AllowCredentials
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://dev.admin.mystira.app")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();  // Required for SignalR
    });
});

// Map hub
app.MapHub<EventsHub>("/hubs/events");
```

### 5. Test Locally

```bash
# Run Admin API
cd packages/admin-api
dotnet run

# Test with wscat
npm install -g wscat
wscat -c ws://localhost:5000/hubs/events
```

## Phase 2: Frontend Implementation (Admin UI)

### 1. Install SignalR Client

```bash
cd packages/admin-ui
npm install @microsoft/signalr
```

### 2. Create React Hooks

- [ ] Create `src/hooks/useSignalR.ts` with connection management
- [ ] Create `src/hooks/useEventListener.ts` for event subscriptions
- [ ] Implement automatic reconnection logic
- [ ] Add connection status indicators

### 3. Replace Polling with SignalR

Before:
```typescript
// ❌ Remove polling
useEffect(() => {
  const interval = setInterval(fetchData, 5000);
  return () => clearInterval(interval);
}, []);
```

After:
```typescript
// ✅ Use SignalR
useEventListener({
  eventName: 'DataUpdated',
  onEvent: (data) => setData(data),
});
```

### 4. Test in Browser

- [ ] Open DevTools → Network tab
- [ ] Filter by `WS` (WebSocket)
- [ ] Verify SignalR connection established
- [ ] Check for automatic reconnection on disconnect

## Phase 3: Azure Front Door Configuration

### 1. Enable Admin Services in Terraform

Edit `infra/terraform/environments/dev/front-door.tf`:

```hcl
module "front_door" {
  # ... other config ...
  
  enable_admin_services     = true  # ✅ Enable this
  admin_api_backend_address = "dev.admin-api.mystira.app"
  admin_ui_backend_address  = "dev.admin.mystira.app"
  custom_domain_admin_api   = "dev.admin-api.mystira.app"
  custom_domain_admin_ui    = "dev.admin.mystira.app"
}
```

### 2. Deploy Front Door

```bash
cd infra/terraform/environments/dev
terraform init
terraform plan -out=tfplan
terraform apply tfplan

# Note the outputs
terraform output front_door_admin_api_endpoint
terraform output front_door_admin_ui_endpoint
terraform output front_door_custom_domain_verification
```

### 3. Update DNS CNAME Records

**Using Azure CLI**:

```bash
# Admin API CNAME
az network dns record-set cname set-record \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --record-set-name dev.admin-api \
  --cname <front_door_admin_api_endpoint>

# Admin UI CNAME
az network dns record-set cname set-record \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --record-set-name dev.admin \
  --cname <front_door_admin_ui_endpoint>
```

**Using Terraform** (`infra/terraform/modules/dns/front-door.tf`):

```hcl
resource "azurerm_dns_cname_record" "admin_api" {
  name                = "dev.admin-api"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  record              = module.front_door.admin_api_endpoint_hostname
}

resource "azurerm_dns_cname_record" "admin_ui" {
  name                = "dev.admin"
  zone_name           = azurerm_dns_zone.main.name
  resource_group_name = var.resource_group_name
  ttl                 = 300
  record              = module.front_door.admin_ui_endpoint_hostname
}
```

### 4. Add Domain Validation TXT Records

```bash
# Get validation tokens
terraform output front_door_custom_domain_verification

# Add TXT records
az network dns record-set txt add-record \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --record-set-name _dnsauth.dev.admin-api \
  --value "<validation_token>"

az network dns record-set txt add-record \
  --resource-group mys-shared-dns-rg \
  --zone-name mystira.app \
  --record-set-name _dnsauth.dev.admin \
  --value "<validation_token>"
```

### 5. Wait for Certificate Provisioning

**Check status**:

```bash
az afd custom-domain show \
  --resource-group mys-dev-core-rg-san \
  --profile-name mystira-dev-fd \
  --custom-domain-name mystira-dev-admin-api-domain \
  --query "validationProperties.validationState"
```

**Expected timeline**: 45 minutes to 3 hours

**Status progression**: `Pending` → `Validating` → `Approved`

### 6. Test WebSocket/SignalR Through Front Door

**Using wscat**:

```bash
wscat -c wss://dev.admin-api.mystira.app/hubs/events
```

**Using browser console**:

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://dev.admin-api.mystira.app/hubs/events", {
    transport: signalR.HttpTransportType.WebSockets
  })
  .build();

await connection.start();
console.log("Connected:", connection.connectionId);
```

**Verify Front Door headers**:

```bash
curl -I https://dev.admin-api.mystira.app/health | grep -i "x-azure"
# Should see: X-Azure-Ref, X-FD-HealthProbe, etc.
```

## Phase 4: Backend Updates for Front Door

### Update Program.cs for Forwarded Headers

```csharp
// Configure forwarded headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Enable BEFORE other middleware
app.UseForwardedHeaders();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
```

### Update CORS Origins

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "https://dev.admin.mystira.app",        // Front Door domain
            "https://dev.admin-api.mystira.app",    // API domain
            "http://localhost:3000",                // Local dev
            "http://localhost:5173"
        )
        .AllowCredentials();
    });
});
```

## Phase 5: Verification and Testing

### Connection Tests

- [ ] WebSocket connection establishes successfully
- [ ] SignalR handshake completes
- [ ] Events are received in real-time
- [ ] Automatic reconnection works after disconnect
- [ ] Multiple tabs/windows work independently

### Load Tests

```bash
# Test with multiple concurrent connections
npm install -g artillery

# artillery.yml
config:
  target: 'wss://dev.admin-api.mystira.app'
  phases:
    - duration: 60
      arrivalRate: 10
  ws:
    connect:
      url: '/hubs/events'

# Run test
artillery run artillery.yml
```

### Monitoring

- [ ] Check Azure Monitor for Front Door metrics
- [ ] Verify SignalR connection counts in Application Insights
- [ ] Monitor message send/receive rates
- [ ] Check for connection errors or failures

### Performance Validation

- [ ] Message latency < 100ms (p95)
- [ ] Connection establishment < 1 second
- [ ] Zero message loss
- [ ] Graceful handling of temporary disconnects

## Rollback Plan

If issues occur:

1. **Immediate**: Revert DNS CNAME to direct AKS ingress
   ```bash
   az network dns record-set cname set-record \
     --resource-group mys-shared-dns-rg \
     --zone-name mystira.app \
     --record-set-name dev.admin-api \
     --cname dev.admin-api.mystira.app.original
   ```

2. **Backend**: Disable SignalR, revert to polling
   ```typescript
   // Temporarily disable SignalR
   const useSignalR = false;  // Feature flag
   ```

3. **Terraform**: Set `enable_admin_services = false`
   ```bash
   cd infra/terraform/environments/dev
   terraform apply
   ```

## Troubleshooting

### WebSocket Connection Fails (502)

**Check**: Backend health

```bash
kubectl get pods -n mys-dev | grep admin-api
kubectl logs -n mys-dev deploy/mys-admin-api --tail=50
```

### Certificate Not Provisioning

**Check**: DNS records

```bash
dig _dnsauth.dev.admin-api.mystira.app TXT
dig dev.admin-api.mystira.app CNAME
```

### SignalR Connects Then Disconnects

**Check**: CORS configuration includes Front Door domain

### High Latency

**Check**: Session affinity is disabled (not needed for SignalR)

## Success Criteria

- [ ] ✅ All WebSocket connections establish through Front Door
- [ ] ✅ TLS certificates automatically renewed
- [ ] ✅ WAF protection enabled
- [ ] ✅ Real-time events working in all environments
- [ ] ✅ No polling code remaining (replaced with SignalR)
- [ ] ✅ Monitoring and alerts configured
- [ ] ✅ Documentation complete

## Next Steps

After successful deployment:

1. **Monitor** for 48 hours in dev environment
2. **Deploy to staging** following same process
3. **User acceptance testing** with power users
4. **Deploy to production** during maintenance window
5. **Remove polling code** after validation period
6. **Update runbooks** with new troubleshooting steps

## References

- [Full Implementation Guide](./signalr-websocket-implementation.md)
- [ADR-0015: Event-Driven Architecture](../planning/adr-0015-implementation-roadmap.md)
- [Azure Front Door Documentation](https://learn.microsoft.com/en-us/azure/frontdoor/)
- [ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
