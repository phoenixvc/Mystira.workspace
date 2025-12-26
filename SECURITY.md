# Security Policy

## Supported Versions

We actively support security updates for the following versions:

| Version | Supported          |
| ------- | ------------------ |
| Latest  | :white_check_mark: |
| < Latest | :x:                |

## Reporting a Vulnerability

If you discover a security vulnerability, please follow these steps:

1. **Do NOT** create a public GitHub issue
2. Email security details to: `eben@phoenixvc.tech`
3. Include:
   - Description of the vulnerability
   - Steps to reproduce
   - Potential impact
   - Suggested fix (if any)

## Response Timeline

- **Initial Response**: Within 48 hours
- **Status Update**: Within 7 days
- **Fix Timeline**: Depends on severity

## Severity Levels

- **Critical**: Immediate response, fix within 24-48 hours
- **High**: Response within 7 days, fix within 2 weeks
- **Medium**: Response within 14 days, fix within 1 month
- **Low**: Response within 30 days, fix in next release

---

## Security Architecture

### Authentication & Authorization

For detailed authentication architecture, see:

- [ADR-0010: Authentication and Authorization Strategy](docs/architecture/adr/0010-authentication-and-authorization-strategy.md)
- [ADR-0011: Microsoft Entra ID Integration](docs/architecture/adr/0011-entra-id-authentication-integration.md)

#### Summary

| Component | Auth Method | Provider |
|-----------|-------------|----------|
| Admin UI | Cookie + OIDC | Microsoft Entra ID |
| Admin API | JWT Bearer | Microsoft Entra ID |
| Public API | JWT Bearer | Entra External ID / Internal |
| Service-to-Service | Managed Identity | Azure |

### Secret Management

#### Required Secrets

| Secret | Storage | Access |
|--------|---------|--------|
| JWT Signing Key | Azure Key Vault | App Service Managed Identity |
| Database Connection | Azure Key Vault | App Service Managed Identity |
| AI Provider API Keys | Azure Key Vault | App Service Managed Identity |
| Discord Bot Token | Azure Key Vault | App Service Managed Identity |

#### Local Development

- Use `dotnet user-secrets` for .NET projects
- Use `.env.local` files (never commit)
- Copy from `.env.example` templates

#### Production

- Azure Key Vault for all secrets
- Key Vault references in App Service configuration
- Managed Identity for access (no credentials in code)

### Network Security

#### Public Endpoints

| Endpoint | Protection |
|----------|------------|
| Public API | HTTPS, WAF, Rate Limiting |
| Admin API | HTTPS, VPN/Private Endpoint (recommended) |
| PWA | HTTPS, CDN |

#### Internal Services

| Service | Network |
|---------|---------|
| Kubernetes Services | ClusterIP (internal only) |
| Cosmos DB | Private Endpoint |
| Redis | Private Endpoint |

### Data Protection

#### Encryption

- **At Rest**: Azure Storage/Cosmos DB encryption (AES-256)
- **In Transit**: TLS 1.2+ everywhere
- **Key Management**: Azure Key Vault

#### PII Handling

- Minimize PII collection
- Use Azure AD claims for identity
- Pseudonymize where possible
- GDPR compliance for EU users

### OWASP Top 10 Mitigations

| Risk | Mitigation |
|------|------------|
| Injection | Parameterized queries, input validation |
| Broken Auth | Entra ID, MFA, session management |
| Sensitive Data | Encryption, Key Vault, no logging of secrets |
| XXE | Disable external entities in parsers |
| Broken Access | RBAC, policy-based authorization |
| Misconfig | IaC, security baselines, automated scanning |
| XSS | Content Security Policy, output encoding |
| Insecure Deserialization | Type-safe JSON handling |
| Vulnerable Components | Dependabot, regular updates |
| Logging/Monitoring | Application Insights, audit logs |

---

## Security Best Practices

### For Developers

#### Code Security

- Never commit secrets or API keys
- Use environment variables for sensitive data
- Validate all inputs at API boundaries
- Use parameterized queries (no string concatenation for SQL)
- Apply principle of least privilege

#### Dependencies

- Keep dependencies up to date
- Review Dependabot alerts promptly
- Use `npm audit` / `dotnet list package --vulnerable`
- Pin versions in production

#### Code Review

- All changes require review before merge
- Security-sensitive changes need security review
- No force pushes to main/production branches

### For Operations

#### Access Control

- Use Azure RBAC for resource access
- Separate development and production environments
- Audit access logs regularly
- Rotate secrets periodically (90 days recommended)

#### Monitoring

- Enable Azure Defender for cloud resources
- Set up alerts for suspicious activity
- Monitor failed authentication attempts
- Track API rate limit violations

#### Incident Response

1. **Detect**: Automated alerts, user reports
2. **Contain**: Isolate affected systems
3. **Eradicate**: Remove threat, patch vulnerabilities
4. **Recover**: Restore from backups if needed
5. **Lessons**: Post-incident review

---

## Compliance

### COPPA (Children's Online Privacy Protection Act)

As a platform potentially serving minors:

- Age verification at registration
- Parental consent for under-13 users
- Limited data collection for minors
- No behavioral advertising to children

### GDPR

For EU users:

- Right to access personal data
- Right to erasure ("right to be forgotten")
- Data portability
- Consent management
- Data Protection Officer contact: `eben@phoenixvc.tech`

---

## Bug Bounty

We offer a bug bounty program for critical security vulnerabilities. Details available upon request.

## Disclosure Policy

We follow responsible disclosure practices. Please allow us time to address vulnerabilities before public disclosure.

---

## Security Contacts

- **Security Team**: `eben@phoenixvc.tech`
- **Data Protection Officer**: `eben@phoenixvc.tech`
- **Backup Contact**: `jurie@phoenixvc.tech`
- **Emergency**: [On-call procedure for critical issues]

## Related Documentation

- [ADR-0010: Authentication Strategy](docs/architecture/adr/0010-authentication-and-authorization-strategy.md)
- [ADR-0011: Entra ID Integration](docs/architecture/adr/0011-entra-id-authentication-integration.md)
- [ADR-0005: Service Networking](docs/architecture/adr/0005-service-networking-and-communication.md)
- [Kubernetes Secrets Management](docs/infrastructure/kubernetes-secrets-management.md)
