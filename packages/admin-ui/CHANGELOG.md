# Changelog

All notable changes to the Mystira Admin UI project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Comprehensive README documentation with setup, configuration, and deployment guides
- CONTRIBUTING.md with detailed contribution guidelines
- Error boundaries for graceful error handling
- ErrorDisplay component with stack trace support
- NotFoundPage (404) with quick navigation links
- ErrorPage for routing and general errors
- Error handler utility for API error parsing
- Client-side schema validation for scenario uploads
- ValidationResults component for displaying validation errors
- Media ZIP upload functionality with metadata support
- Scenario media reference validation feature
- Reusable UI components:
  - Alert component with multiple variants
  - Card component for consistent layouts
  - Checkbox component with labels
  - FileInput component with file info display
  - ValidationResults for error display
- Custom hooks:
  - useFileValidation for file validation logic
  - useFileUpload for upload handling
- Avatars management page with full CRUD operations
- Bundle Create and Edit pages
- Schema validator utility using Ajv
- Support for JSON file uploads in addition to YAML

### Changed
- Refactored ImportScenarioPage to use reusable components and hooks
- Improved error handling throughout the application
- Enhanced validation error messages with detailed context
- Updated App.tsx with error boundaries and 404 handling
- Improved React Query configuration with error handlers

### Fixed
- TypeScript configuration to support JSON module imports
- Error boundary error catching and recovery
- API error parsing for various error types

## [0.1.0] - 2024-12-21

### Added
- Initial project setup with React, TypeScript, and Vite
- Authentication flow with cookie-based auth
- Core pages:
  - Dashboard
  - Scenarios (list, create, edit, import)
  - Media (list, import)
  - Badges (list, create, edit, import)
  - Bundles (list, import)
  - Character Maps (list, create, edit, import)
- Master Data pages:
  - Age Groups
  - Archetypes
  - Compass Axes
  - Echo Types
  - Fantasy Themes
- API client infrastructure with Axios
- State management with Zustand
- Form handling with React Hook Form and Zod validation
- Toast notifications with react-hot-toast
- Reusable components:
  - Pagination
  - SearchBar
  - LoadingSpinner
  - ErrorAlert
  - FormField
  - TextInput
  - Textarea
  - NumberInput
- Bootstrap 5 styling with custom admin theme
- ESLint and Prettier configuration
- Vite configuration for development and production builds

### Changed
- Migrated from Blazor WebAssembly to React SPA
- Separated frontend from backend API
- Improved development workflow with HMR

### Removed
- Blazor WebAssembly dependencies
- .NET frontend dependencies

## Release Types

### Major Version (X.0.0)
- Breaking changes
- Major feature additions
- Architecture changes

### Minor Version (0.X.0)
- New features
- Non-breaking changes
- Enhancements

### Patch Version (0.0.X)
- Bug fixes
- Documentation updates
- Minor improvements

## Categories

### Added
New features and capabilities

### Changed
Changes to existing functionality

### Deprecated
Features that will be removed in future versions

### Removed
Features that have been removed

### Fixed
Bug fixes

### Security
Security fixes and improvements

---

[Unreleased]: https://github.com/phoenixvc/Mystira.Admin.UI/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/phoenixvc/Mystira.Admin.UI/releases/tag/v0.1.0
