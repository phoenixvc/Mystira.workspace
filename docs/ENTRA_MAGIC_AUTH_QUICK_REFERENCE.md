# Entra-Magic Auth - Quick Reference

## Authentication Flow Summary

```
User → Application → Identity API → (Entra/Magic Link) → JWT → Application
```

## Key Endpoints

### Identity API (`https://localhost:7001`)

| Method | Endpoint                   | Purpose               |
| ------ | -------------------------- | --------------------- |
| POST   | `/auth/entra/login`        | Initiate Entra login  |
| POST   | `/auth/entra/callback`     | Handle Entra callback |
| POST   | `/auth/magic-link/request` | Request magic link    |
| POST   | `/auth/magic-link/verify`  | Verify magic link     |
| POST   | `/auth/magic-link/resend`  | Resend magic link     |
| GET    | `/auth/me`                 | Get current user info |

## Configuration Keys

### JWT Settings

- `Authentication:Jwt:Issuer`
- `Authentication:Jwt:Audience`
- `Authentication:Jwt:SecretKey`
- `Authentication:Jwt:RsaPublicKey`

### Entra Settings

- `Authentication:Entra:Authority`
- `Authentication:Entra:ClientId`
- `Authentication:Entra:RedirectUri`

### Magic Link Settings

- `Authentication:MagicLink:TokenExpirationMinutes`
- `Authentication:MagicLink:SignupExpirationDays`

## Frontend Integration

### React (Publisher)

```typescript
import { useAuth } from "./hooks/useAuth";

const { signInWithEntra, requestMagicLink, user } = useAuth();

// Entra login
await signInWithEntra();

// Magic link
await requestMagicLink("user@example.com", "Display Name");
```

### Blazor (App PWA, StoryGen)

```csharp
@inject IAuthService AuthService

// Entra login
var entraResult = await AuthService.SignInWithEntraAsync();

// Magic link
var magicLinkResult = await AuthService.SignInWithMagicLinkAsync(email, displayName);
```

### CLI (DevHub)

```bash
# Get JWT token
mystira auth login --entra

# Use token in commands
mystira migration run --auth-token <jwt-token>
```

## Common Error Codes

| Code     | Description                 | Resolution                          |
| -------- | --------------------------- | ----------------------------------- |
| AUTH_001 | Invalid JWT token           | Check token format and expiration   |
| AUTH_002 | Entra configuration missing | Verify Entra settings               |
| AUTH_003 | Magic link expired          | Request new magic link              |
| AUTH_004 | Email not verified          | Check email service                 |
| AUTH_005 | User not found              | Create account or check credentials |

## Testing Commands

```bash
# Run all tests
dotnet test

# Run specific auth tests
dotnet test --filter "TestCategory=Authentication"

# Test Identity API
curl -X POST https://localhost:7001/auth/magic-link/request \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","displayName":"Test User"}'
```

## Debug Tips

1. **Enable debug logging**: Set `LogLevel:Default` to `Debug`
2. **Check JWT payload**: Use jwt.ms to decode tokens
3. **Verify Entra flow**: Check Azure AD sign-in logs
4. **Test email service**: Send test email via API
5. **Cross-app testing**: Use same JWT in different applications

## Security Checklist

- [ ] JWT secrets are in environment variables
- [ ] RSA keys used in production
- [ ] Entra app registration configured
- [ ] Email domain verified
- [ ] CORS settings correct
- [ ] Token expiration appropriate
- [ ] Rate limiting enabled
- [ ] Audit logging enabled
