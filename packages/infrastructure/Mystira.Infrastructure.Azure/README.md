# Mystira.Infrastructure.Azure

Azure infrastructure package for the Mystira platform.

## Features

- Azure Blob Storage integration for media assets
- Azure Cosmos DB integration
- Azure Communication Services for email
- FFmpeg audio transcoding services

## Installation

```bash
dotnet add package Mystira.Infrastructure.Azure
```

## Usage

```csharp
services.AddMystiraAzureInfrastructure(configuration);
```

## Dependencies

- `Mystira.Application` - Application layer contracts

## License

MIT
