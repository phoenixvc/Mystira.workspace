# Setup Documentation

This directory contains comprehensive setup and configuration guides for the Mystira Application Suite.

> 📘 **Azure Naming Conventions**: All Azure resources follow the standardized naming pattern `[org]-[env]-[project]-[type]-[region]`. See [Azure Naming Conventions](../AZURE-NAMING-CONVENTIONS.md) for complete details on resource naming standards.

## Quick Navigation

### 🔐 Secrets & Security

| Document                                                      | Purpose                                                    | Audience                    |
| ------------------------------------------------------------- | ---------------------------------------------------------- | --------------------------- |
| **[Quick Secrets Reference](QUICK_SECRETS_REFERENCE.md)**     | Quick lookup for GitHub secrets by environment             | DevOps, Contributors        |
| **[GitHub Secrets & Variables](GITHUB_SECRETS_VARIABLES.md)** | Complete CI/CD secrets configuration guide                 | DevOps, Infrastructure Team |
| **[Secrets Management](SECRETS_MANAGEMENT.md)**               | General secrets management (Azure Key Vault, User Secrets) | Developers, DevOps          |

### 📧 Email & Communication

| Document                          | Purpose                                    | Audience                   |
| --------------------------------- | ------------------------------------------ | -------------------------- |
| **[Email Setup](EMAIL_SETUP.md)** | Azure Communication Services configuration | DevOps, Backend Developers |

### 💾 Database & Data

| Document                                | Purpose                                         | Audience           |
| --------------------------------------- | ----------------------------------------------- | ------------------ |
| **[Database Setup](database-setup.md)** | Database initialization and master data seeding | Developers, DevOps |

### 🏗️ Infrastructure & Naming

| Document                                                       | Purpose                                              | Audience                                |
| -------------------------------------------------------------- | ---------------------------------------------------- | --------------------------------------- |
| **[Azure Naming Conventions](../AZURE-NAMING-CONVENTIONS.md)** | Standardized naming patterns for all Azure resources | DevOps, Infrastructure Team, Developers |

## Getting Started

### For Developers (Local Development)

1. Start with **[Secrets Management](SECRETS_MANAGEMENT.md)**
2. Set up User Secrets for local development
3. Configure your IDE for the project

### For DevOps/Infrastructure

1. Review **[GitHub Secrets & Variables](GITHUB_SECRETS_VARIABLES.md)** for CI/CD setup
2. Use **[Quick Secrets Reference](QUICK_SECRETS_REFERENCE.md)** for fast lookups
3. Implement secrets rotation according to security best practices

### For Contributors

1. Read **[Secrets Management](SECRETS_MANAGEMENT.md)** to understand how NOT to commit secrets
2. Check **[Quick Secrets Reference](QUICK_SECRETS_REFERENCE.md)** if working on CI/CD workflows
3. See **[Email Setup](EMAIL_SETUP.md)** if working on email features

## Environment Overview

The Mystira application uses **three environments**:

| Environment     | Branch    | Azure Region       | Current Naming          | New Standard Naming         | Purpose                        |
| --------------- | --------- | ------------------ | ----------------------- | --------------------------- | ------------------------------ |
| **Development** | `dev`     | South Africa North | `dev-san-*`             | `mys-dev-mystira-*-san`     | Active development and testing |
| **Staging**     | `staging` | West US            | `mystira-app-staging-*` | `mys-staging-mystira-*-wus` | Pre-production validation      |
| **Production**  | `main`    | West US            | `prod-wus-*`            | `mys-prod-mystira-*-wus`    | Live production environment    |

Each environment requires its own set of secrets and configuration. See [Azure Naming Conventions](../AZURE-NAMING-CONVENTIONS.md) for the standardized resource naming pattern.

## Document Hierarchy

```
Setup Documentation
├── Quick Secrets Reference (Quick lookup tables)
├── GitHub Secrets & Variables (Complete CI/CD guide)
│   ├── Environment-specific requirements
│   ├── Secret generation instructions
│   ├── Workflow-to-secret mapping
│   └── Troubleshooting guide
├── Secrets Management (Development & runtime)
│   ├── Azure Key Vault setup
│   ├── User Secrets for local dev
│   ├── Secret rotation
│   └── Security best practices
└── Email Setup (Communication services)
    ├── Azure Communication Services
    └── Email configuration per environment
```

## Common Tasks

### Setup New Environment

1. Configure Azure infrastructure
2. Generate environment-specific JWT keys
3. Set up GitHub secrets (see [GitHub Secrets Guide](GITHUB_SECRETS_VARIABLES.md))
4. Configure Azure App Service settings
5. Test deployments

### Rotate Secrets

1. Generate new keys/credentials
2. Update Azure Key Vault (production)
3. Update GitHub Secrets (CI/CD)
4. Update User Secrets (local dev)
5. Test applications
6. Archive old secrets (for rollback)

### Troubleshoot Secret Issues

1. Check [Quick Secrets Reference](QUICK_SECRETS_REFERENCE.md) for correct secret names
2. Review workflow logs for specific errors
3. Verify secret format (JSON, XML, base64, etc.)
4. Consult troubleshooting sections in relevant guides

## Security Principles

All documentation follows these core principles:

- ✅ **Never commit secrets to version control**
- ✅ **Use separate keys per environment**
- ✅ **Rotate secrets regularly (90 days)**
- ✅ **Use Azure Key Vault for production**
- ✅ **Use User Secrets for local development**
- ✅ **Enable secret scanning in GitHub**
- ✅ **Audit secret access regularly**
- ✅ **Document all secret rotation procedures**

## Related Documentation

### Main Repository Docs

- [Main README](../../README.md) - Project overview
- [Contributing Guide](../../CONTRIBUTING.md) - How to contribute
- [Deploy Now Script](../../DEPLOY-NOW.md) - Infrastructure deployment

### Architecture Docs

- [Architecture Documentation](../architecture/) - System design and patterns
- [CQRS Guide](../architecture/CQRS_MIGRATION_GUIDE.md) - Application architecture
- [Hexagonal Architecture](../architecture/HEXAGONAL_ARCHITECTURE_REFACTORING_SUMMARY.md) - Clean architecture patterns

### DevOps Resources

- [Azure Naming Conventions](../AZURE-NAMING-CONVENTIONS.md) - Resource naming standards
- [Production Review](../PRODUCTION_REVIEW_REPORT_UPDATED.md) - Production readiness
- [Testing Strategy](../TESTING_STRATEGY.md) - QA and testing

## Need Help?

1. **Check the documentation** - Start with the relevant guide above
2. **Review workflow logs** - GitHub Actions logs show specific errors
3. **Verify secret format** - Use generation commands from the guides
4. **Contact DevOps team** - For Azure access or infrastructure issues

## Contributing to Documentation

When updating setup documentation:

1. Keep instructions clear and step-by-step
2. Include command examples that can be copy-pasted
3. Add troubleshooting sections for common issues
4. Update this README if adding new documents
5. Test all commands before documenting

---

**Last Updated**: 2025-12-08  
**Maintainers**: DevOps Team  
**For Updates**: Submit PR with documentation changes
