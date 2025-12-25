# Changelog

All notable changes to the Mystira workspace will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **CI Workflows**: Added component CI workflows for Admin API, App, and Devhub
- **Repository Metadata Tooling**: Created `scripts/sync-repo-metadata.sh` for syncing GitHub repository metadata
- **Repository Metadata Config**: Added `scripts/repo-metadata.json` for centralized repository configuration
- **ADR-0012**: Created Architecture Decision Record documenting GitHub Workflow Naming Convention
- **CI Status Badges**: Added status badges for all 7 component CI workflows in README.md
- **Comprehensive Documentation**:
  - Complete rewrite of `infra/README.md` with current infrastructure setup
  - Enhanced `README.md` with component inventory, CI/CD pipeline details, and architecture
  - Improved `docs/README.md` with all 12 ADRs and better navigation

### Changed

- **Workflow Naming**: Standardized all 14 GitHub workflow names using hierarchical "Category: Name" pattern:
  - Components: Admin API - CI, Admin UI - CI, App - CI, Chain - CI, Devhub - CI, Publisher - CI, Story Generator - CI
  - Infrastructure: Deploy, Validate
  - Deployment: Production, Staging
  - Workspace: CI, Release
  - Utilities: Check Submodules
- **ADR-0004**: Updated with complete list of current CI/CD workflows and cross-reference to ADR-0012
- **Documentation Organization**: Consolidated and improved documentation structure across workspace
- **Script Permissions**: Fixed executable permissions on all utility scripts (755)

### Removed

- **Temporary Documentation**: Removed 24 outdated status and summary files:
  - Root-level: 14 files (ADMIN_UI_*.md, CI_ISSUES_RESOLVED.md, COMPLETION_SUMMARY.md, etc.)
  - Infra: 7 files (FRONT_DOOR_*.md, ENVIRONMENT_URLS_*.md, etc.)
  - Docs: 2 files (DOCUMENTATION_*.md)
  - Scripts: 1 file (README-CERTIFICATES.md)
- **Net Documentation Reduction**: ~6,400 lines of outdated/redundant documentation

## [0.1.0] - Initial Release

### Added

- Initial workspace setup with monorepo structure
- pnpm workspaces for package management
- Turborepo for build orchestration
- GitHub Actions CI/CD pipeline
- Git submodule integration for all components
- Infrastructure as Code (Terraform + Kubernetes)
- Docker containerization for all services
- Azure deployment configuration
