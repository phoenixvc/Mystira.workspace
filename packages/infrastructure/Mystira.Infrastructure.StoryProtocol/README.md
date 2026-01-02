# Mystira.Infrastructure.StoryProtocol

.NET gRPC client for communicating with the Mystira.Chain Python service for Story Protocol blockchain operations.

## Overview

This package provides the infrastructure layer implementation for Story Protocol blockchain integration. It implements `IStoryProtocolService` port interface from the Application layer.

## Architecture

```
┌─────────────────────────────────────┐
│  Mystira.Application                │
│  ├── Ports/IStoryProtocolService    │  ◄── Interface
│  └── Configuration/ChainServiceOpts │  ◄── Configuration
└─────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────┐
│  Mystira.Infrastructure.StoryProtocol │
│  ├── GrpcStoryProtocolService       │  ◄── gRPC Client
│  ├── MockStoryProtocolService       │  ◄── Dev/Test Mock
│  └── ChainServiceHealthCheck        │  ◄── Health Check
└─────────────────────────────────────┘
                 │
                 ▼ gRPC
┌─────────────────────────────────────┐
│  Mystira.Chain (Python)             │
│  └── ChainService                   │  ◄── gRPC Server
└─────────────────────────────────────┘
```

## Features

- **IP Asset Registration**: Register content as IP Assets on Story Protocol
- **Royalty Management**: Configure, pay, and claim royalties
- **Retry with Exponential Backoff**: Configurable retry policy for resilience
- **TLS Support**: Secure gRPC communication
- **API Key Authentication**: Service-to-service authentication
- **Health Checks**: ASP.NET Core health check integration
- **Mock Implementation**: For development and testing without blockchain

## Configuration

Add to `appsettings.json`:

```json
{
  "ChainService": {
    "GrpcEndpoint": "https://localhost:50051",
    "UseGrpc": true,
    "UseTls": true,
    "ApiKey": "your-api-key",
    "TimeoutSeconds": 120,
    "EnableRetry": true,
    "MaxRetryAttempts": 3,
    "RetryBaseDelayMs": 1000,
    "EnableHealthChecks": true
  }
}
```

## Registration

```csharp
// In Program.cs or Startup.cs
services.AddStoryProtocolServices(configuration);

// With health checks
services.AddHealthChecks()
    .AddChainServiceHealthCheck();
```

## Usage

```csharp
public class MyService
{
    private readonly IStoryProtocolService _storyProtocol;

    public MyService(IStoryProtocolService storyProtocol)
    {
        _storyProtocol = storyProtocol;
    }

    public async Task RegisterContentAsync(string contentId, string title, List<Contributor> contributors)
    {
        var result = await _storyProtocol.RegisterIpAssetAsync(
            contentId,
            title,
            contributors,
            metadataUri: "ipfs://...",
            licenseTermsId: "PIL_TERMS_ID");

        Console.WriteLine($"Registered: {result.IpAssetId}");
    }
}
```

## Development

Set `UseGrpc: false` in configuration to use the mock implementation during development.

## Proto Files

gRPC code is generated from proto files in `/protos/mystira/chain/v1/`:
- `chain_service.proto` - Main service definition
- `ip_assets.proto` - IP Asset registration messages
- `royalties.proto` - Royalty management messages
- `common.proto` - Shared types

## Related Documentation

- [ADR-0013: gRPC Adoption](../../docs/adr/0013-grpc-adoption.md)
- [Story Protocol Documentation](https://docs.story.foundation)
