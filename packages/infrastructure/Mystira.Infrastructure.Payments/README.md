# Mystira.Infrastructure.Payments

Payment processing infrastructure package for the Mystira platform.

## Features

- Payment gateway abstractions
- Peach Payments integration
- Mock payment service for testing
- Health checks for payment service connectivity

## Installation

```bash
dotnet add package Mystira.Infrastructure.Payments
```

## Usage

```csharp
services.AddMystiraPayments(configuration);
```

## Dependencies

- `Mystira.Application` - Application layer contracts
- `Mystira.Contracts` - API contracts

## License

MIT
