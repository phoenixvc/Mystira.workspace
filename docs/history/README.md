# Documentation History

This directory contains historical documentation for significant work completed in the Mystira workspace.

Archived snapshot artifacts are intentionally not kept in the working tree.
If needed for investigation, retrieve them from git history.

## Structure

```
docs/history/
├── implementations/          # Major implementations, ADR implementations, refactoring
├── bug-fixes/              # Complex bug resolutions
├── features/               # New feature launches
├── migrations/             # Major migrations/upgrades
├── .index.json            # Sequential numbering tracking
└── README.md              # This file
```

## Sequential Numbering

All documentation files use a sequential numbering system: `XXXX-YYYY-MM-DD-[title]-[type].md`

- **XXXX**: Sequential 4-digit number (starting from 0001)
- **YYYY-MM-DD**: Completion date
- **[title]**: Sanitized title (kebab-case)
- **[type]**: Document type (implementation, bugfix, feature, migration)

## Creating Documentation

Use the provided script to create standardized documentation templates:

```bash
# Create implementation documentation
./scripts/create-doc.ps1 implementation "Feature Name" 1234

# Create bug fix documentation
./scripts/create-doc.ps1 bugfix "Bug Description"

# Create feature documentation
./scripts/create-doc.ps1 feature "Feature Name"
```

## Documentation Types

### Implementations

- Major refactoring affecting >5 projects or >10k lines
- ADR implementations
- Performance improvements
- Infrastructure changes
- Breaking changes

### Bug Fixes

- Complex or critical bug resolutions
- Security fixes
- Multi-component bug fixes

### Features

- New feature launches
- Significant enhancements
- User-facing functionality

### Migrations

- Major version upgrades
- Database migrations
- Platform migrations

## Related Documentation

- **PR Documentation Strategy**: `docs/process/pr-documentation-strategy.md`
- **Quality Assurance Guide**: `docs/guides/quality-assurance-guide.md`
- **Architecture Decisions**: `docs/adr/`

## Index Management

The `.index.json` file tracks:

- Sequential numbering for each document type
- Document metadata and descriptions
- Last updated timestamp

This file is automatically managed by the `create-doc.ps1` script.
