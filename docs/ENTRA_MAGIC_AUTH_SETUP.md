# Entra-Magic Auth Standardization - Setup Guide

## Overview

This document provides comprehensive setup instructions for the unified Entra-Magic authentication system implemented across the Mystira platform. The system provides dual-path authentication allowing users to choose between:

- **Entra External ID**: Enterprise SSO integration for organizational users
- **Magic Link**: Passwordless email authentication for all users

## Architecture

### Centralized Authentication Authority

The authentication system is centralized in the **Identity API** (`packages/identity/`) which serves as the single source of truth for all authentication operations across the platform.

### Supported Applications

- **App PWA** (`packages/app/`) - Progressive Web App with Blazor
- **StoryGen Web** (`packages/story-generator/`) - Blazor WebAssembly application
- **Publisher** (`packages/publisher/`) - React TypeScript application
- **DevHub CLI** (`packages/devhub/`) - .NET CLI tool with JWT authentication

## Prerequisites

### Azure Requirements

1. **Azure Active Directory Tenant**
2. **App Registration** for Entra External ID
3. **Email Service** for Magic Link authentication (SendGrid, SMTP, etc.)

### Development Environment

- .NET 10.0 SDK
- Node.js 18+ (for Publisher React app)
- Azure CLI (for local development)

## Configuration

> **🚨 SECURITY WARNING**: Files containing real secrets (e.g., `Authentication.Jwt.SecretKey`, `Entra.ClientId/ClientSecret`, `Email.SendGrid.ApiKey`) must **never** be committed to version control. Use:
>
> - `dotnet user-secrets` for local development
> - Environment variables for deployment
> - Azure Key Vault or similar secrets store for production
>   Replace literal secrets in examples with placeholders or environment variable references.

### 1. Identity API Configuration

Create `appsettings.json` in `packages/identity/src/Mystira.Identity.Api/`:

```json
{
  "Authentication": {
    "Jwt": {
      "Issuer": "https://your-domain.com",
      "Audience": "mystira-platform",
      "SecretKey": "your-super-secret-key-at-least-32-chars",
      "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----",
      "TokenExpirationMinutes": 60
    },
    "Entra": {
      "Authority": "https://login.microsoftonline.com/{tenant-id}",
      "ClientId": "your-app-registration-client-id",
      "RedirectUri": "https://localhost:5001/auth/callback",
      "Scopes": ["openid", "profile", "email"]
    },
    "MagicLink": {
      "TokenExpirationMinutes": 30,
      "SignupExpirationDays": 7
    }
  },
  "Email": {
    "Provider": "SendGrid",
    "SendGrid": {
      "ApiKey": "your-sendgrid-api-key",
      "FromEmail": "noreply@your-domain.com",
      "FromName": "Mystira Platform"
    }
  }
}
```

### 2. App PWA Configuration

Create `appsettings.json` in `packages/app/src/Mystira.App.PWA/`:

```json
{
  "Authentication": {
    "IdentityApiUrl": "https://localhost:7001",
    "Authority": "https://login.microsoftonline.com/{tenant-id}",
    "ClientId": "your-app-registration-client-id",
    "RedirectUri": "https://localhost:5001/auth/callback"
  }
}
```

### 3. StoryGen Web Configuration

Create `appsettings.json` in `packages/story-generator/src/Mystira.StoryGenerator.Web/`:

```json
{
  "Authentication": {
    "IdentityApiUrl": "https://localhost:7001",
    "Jwt": {
      "Issuer": "https://your-domain.com",
      "Audience": "mystira-platform",
      "SecretKey": "your-super-secret-key-at-least-32-chars"
    }
  }
}
```

### 4. Publisher React Configuration

Create `.env.local` in `packages/publisher/`:

```bash
VITE_IDENTITY_API_URL=https://localhost:7001
VITE_ENTRA_AUTHORITY=https://login.microsoftonline.com/{tenant-id}
VITE_ENTRA_CLIENT_ID=your-app-registration-client-id
VITE_ENTRA_REDIRECT_URI=http://localhost:3000/auth/callback
```

### 5. DevHub CLI Configuration

Create `appsettings.json` in `packages/devhub/Mystira.DevHub.CLI/`:

```json
{
  "Authentication": {
    "Jwt": {
      "Issuer": "https://your-domain.com",
      "Audience": "mystira-platform",
      "SecretKey": "your-super-secret-key-at-least-32-chars",
      "RsaPublicKey": "-----BEGIN PUBLIC KEY-----\n...\n-----END PUBLIC KEY-----"
    }
  }
}
```

> **🔑 JWT KEY CONSISTENCY**: The `Jwt.SecretKey` (for symmetric signing) or `Jwt.RsaPublicKey` (for RSA) must be **identical across all services** that validate tokens:
>
> - **Identity API**: Signs JWT tokens
> - **StoryGen Web**: Validates tokens
> - **DevHub CLI**: Validates tokens
> - **App PWA**: Validates tokens
>
> Mismatched keys will cause authentication to fail across services.

## Azure Setup

### 1. App Registration

1. Go to Azure Portal → Azure Active Directory → App registrations
2. Click "New registration"
3. Name: "Mystira Platform"
4. Supported account types: "Accounts in any organizational directory"
5. Redirect URI: Add web platform with `https://localhost:5001/auth/callback`
6. Click "Register"

### 2. API Permissions

1. Go to your app registration → API permissions
2. Add Microsoft Graph → Delegated permissions:
   - `openid`
   - `profile`
   - `email`
3. Grant admin consent

### 3. Client Secret

1. Go to Certificates & secrets
2. Click "New client secret"
3. Description: "Mystira Platform Secret"
4. Duration: 24 months
5. Copy the secret value immediately

### 4. Token Configuration

1. Go to Token configuration
2. Add optional claims:
   - Email address
   - Name
   - Upn (user principal name)

## Email Service Setup

### SendGrid Configuration

1. Create SendGrid account
2. Verify your sender domain
3. Generate API key
4. Update configuration with API key and sender information

### SMTP Alternative

```json
"Email": {
  "Provider": "Smtp",
  "Smtp": {
    "Host": "smtp.your-provider.com",
    "Port": 587,
    "Username": "your-smtp-username",
    "Password": "your-smtp-password",
    "UseSsl": true,
    "FromEmail": "noreply@your-domain.com",
    "FromName": "Mystira Platform"
  }
}
```

## Database Setup

### 1. Identity Database

The Identity API uses Entity Framework Core. Run migrations:

```bash
cd packages/identity/src/Mystira.Identity.Api
dotnet ef database update
```

### 2. Account Model Updates

The `Account` model now includes `EntraObjectId` for Entra user linking:

```csharp
public class Account
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public string? EntraObjectId { get; set; } // New field
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
```

## Running the Applications

### 1. Identity API

```bash
cd packages/identity/src/Mystira.Identity.Api
dotnet run
```

The API will be available at `https://localhost:7001`

### 2. App PWA

```bash
cd packages/app/src/Mystira.App.PWA
dotnet run
```

Available at `https://localhost:5001`

### 3. StoryGen Web

```bash
cd packages/story-generator/src/Mystira.StoryGenerator.Web
dotnet run
```

Available at `https://localhost:5002`

### 4. Publisher React

```bash
cd packages/publisher
npm install
npm run dev
```

Available at `http://localhost:3000`

### 5. DevHub CLI

```bash
cd packages/devhub/Mystira.DevHub.CLI
dotnet run -- --help
```

## Authentication Flows

### Entra External ID Flow

1. User clicks "Sign in with Entra"
2. Redirected to Microsoft login page
3. User authenticates with organizational credentials
4. Redirect back to application with authorization code
5. Application exchanges code for tokens
6. Identity API validates tokens and creates/updates account
7. JWT issued for application access

### Magic Link Flow

1. User enters email address
2. Identity API generates magic link token
3. Email sent with magic link
4. User clicks link (valid for 30 minutes)
5. Identity API validates token and creates account
6. JWT issued for application access

## Testing

### Unit Tests

Run all tests to verify functionality:

```bash
dotnet test
```

### Integration Tests

Test authentication flows manually:

1. **Entra Flow**: Use organizational account to test SSO
2. **Magic Link Flow**: Use personal email to test passwordless auth
3. **Token Validation**: Test JWT validation across applications
4. **Cross-App Auth**: Verify same JWT works across all apps

## Security Considerations

### JWT Security

- Use RSA keys for production environments
- Rotate keys regularly
- Set appropriate token expiration (60 minutes recommended)
- Implement refresh token strategy

### Email Security

- Verify sender domain (SPF, DKIM, DMARC)
- Use TLS for SMTP connections
- Rate limit magic link requests
- Implement email validation

### Entra Security

- Use conditional access policies
- Implement multi-factor authentication
- Monitor sign-in logs
- Use least-privilege access

## Troubleshooting

### Common Issues

1. **CORS Errors**: Ensure Identity API allows origins from all applications
2. **Token Validation Failures**: Verify JWT configuration matches across apps
3. **Email Not Received**: Check email service configuration and spam filters
4. **Entra Redirect Failures**: Verify redirect URI matches app registration

### Debug Logging

Enable debug logging in Identity API:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Mystira.Identity.Api": "Debug"
    }
  }
}
```

### Health Checks

Monitor application health:

- Identity API: `GET /healthz`
- App PWA: `GET /healthz`
- StoryGen Web: `GET /healthz`

## Migration Guide

### From Legacy Authentication

1. Update existing accounts to include `EntraObjectId` if using Entra
2. Migrate user sessions to JWT tokens
3. Update API clients to use JWT authentication
4. Deprecate old authentication endpoints

### Token Migration

For existing systems, implement a token migration endpoint that:

1. Validates legacy authentication
2. Creates JWT token
3. Returns new token to client
4. Logs migration for audit

## Production Deployment

### Environment Variables

Use environment variables for sensitive configuration:

```bash
export JWT_SECRET_KEY="your-production-secret"
export ENTRA_CLIENT_SECRET="your-production-client-secret"
export SENDGRID_API_KEY="your-production-sendgrid-key"
```

> **🔒 HTTPS REQUIREMENT**: Production **must use HTTPS** (not HTTP) for:
>
> - OAuth redirect URIs (Microsoft Entra requires HTTPS for non-localhost)
> - JWT token transmission
> - Magic link delivery URLs
>
> Configure TLS/SSL certificates (managed certs or reverse proxy) for production deployment.

### Load Balancing

- **Sticky sessions are unnecessary** for stateless JWT token validation
- **Distributed cache only needed** for:
  - Refresh token storage (if implemented)
  - Session state management (if using session cookies)
  - Token revocation/blacklist systems
- **Pure JWT validation** works without sticky sessions or distributed cache
- Use load balancer health checks for service availability

### Monitoring

Monitor authentication metrics:

- Login success/failure rates
- Token validation performance
- Email delivery rates
- Entra integration health

## Support

For issues with the authentication system:

1. Check application logs for detailed error messages
2. Verify configuration matches across all applications
3. Test individual components (Identity API, email service, Entra)
4. Review Azure AD sign-in logs for Entra issues

## Future Enhancements

Planned improvements to the authentication system:

- Social login providers (Google, GitHub)
- Biometric authentication support
- Advanced conditional access policies
- Audit logging and compliance features
- Multi-factor authentication enforcement
