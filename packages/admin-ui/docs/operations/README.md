# Operations Documentation

This directory contains operational procedures and checklists for the Mystira Admin UI project.

## Contents

- [Testing Checklist](./TESTING_CHECKLIST.md) - Comprehensive testing procedures
- [Deployment Strategy](./DEPLOYMENT_STRATEGY.md) - Deployment workflows and procedures
- [Rollback Procedure](./ROLLBACK_PROCEDURE.md) - Recovery and rollback steps

## Quick Reference

### Deployment Commands

```bash
# Build for production
npm run build

# Preview production build
npm run preview

# Run linting
npm run lint

# Run tests
npm run test
```

### Environment Variables

```bash
VITE_API_BASE_URL=https://api.admin.mystira.app
VITE_APP_ENV=production
```

## Related Documentation

- [Implementation Roadmap](../planning/implementation-roadmap.md)
- [Migration Phases](../migration/phases.md)
