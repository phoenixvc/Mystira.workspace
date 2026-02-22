# Mystira Admin UI

> Modern admin interface for the Mystira platform - A comprehensive content management system for scenarios, media, badges, and more.

[![TypeScript](https://img.shields.io/badge/TypeScript-5.6-blue)](https://www.typescriptlang.org/)
[![React](https://img.shields.io/badge/React-18.3-61dafb)](https://reactjs.org/)
[![Vite](https://img.shields.io/badge/Vite-5.4-646cff)](https://vitejs.dev/)
[![License](https://img.shields.io/badge/license-Private-red)]()

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Development](#development)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Testing](#testing)
- [Deployment](#deployment)
- [Contributing](#contributing)
- [Troubleshooting](#troubleshooting)

## 🌟 Overview

Mystira Admin UI is a modern single-page application (SPA) built with React, TypeScript, and Vite. It provides a comprehensive interface for managing content, users, and platform configuration for the Mystira platform.

### Key Capabilities

- **Content Management**: Full CRUD operations for scenarios, media, badges, bundles, and avatars
- **Validation**: Client-side schema validation for scenario uploads with detailed error reporting
- **Media Management**: Bulk media uploads via ZIP files with metadata support
- **Error Handling**: Comprehensive error boundaries with stack traces and recovery options
- **Responsive Design**: Mobile-friendly interface built with Bootstrap 5
- **Type Safety**: Full TypeScript coverage with strict type checking

## ✨ Features

### Content Management

- **Scenarios**: Create, edit, import, and validate story scenarios with full schema validation
- **Media**: Upload single files or bulk import via ZIP with metadata
- **Badges**: Manage achievement badges with image assets
- **Bundles**: Create and manage content bundles
- **Avatars**: Configure avatars by age group
- **Character Maps**: Define character relationships and properties
- **Master Data**: Manage age groups, archetypes, compass axes, echo types, and fantasy themes

### Special Features

- **Schema Validation**: Client-side validation against JSON Schema before upload
- **Media ZIP Upload**: Bulk media upload with `media-metadata.json` support
- **Scenario Validation**: Check all media references in scenarios against database
- **Error Boundaries**: Graceful error handling with detailed stack traces
- **404 Handling**: User-friendly not found pages with quick navigation

### Developer Experience

- **Hot Module Replacement**: Instant feedback during development
- **TypeScript**: Full type safety and IntelliSense support
- **ESLint & Prettier**: Consistent code style and quality
- **React Query**: Efficient data fetching and caching
- **Form Validation**: React Hook Form with Zod schemas
- **Component Library**: Reusable UI components (Alert, Card, FileInput, etc.)

## 🏗️ Architecture

### Technology Stack

- **Frontend Framework**: React 18.3
- **Language**: TypeScript 5.6
- **Build Tool**: Vite 5.4
- **Styling**: Bootstrap 5.3 + Custom CSS
- **State Management**: Zustand 5.0
- **Data Fetching**: TanStack React Query 5.60
- **Form Handling**: React Hook Form 7.53 + Zod 3.23
- **Routing**: React Router DOM 6.28
- **HTTP Client**: Axios 1.7
- **Authentication**: MSAL (Microsoft Authentication Library) for Azure AD / Entra ID
- **Validation**: Ajv 8.17 (JSON Schema validator)
- **YAML Parsing**: js-yaml 4.1

### System Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Mystira Admin UI                      │
│                   (React + TypeScript)                   │
├─────────────────────────────────────────────────────────┤
│  Components Layer                                        │
│  ├── Pages (Scenarios, Media, Badges, etc.)            │
│  ├── Reusable Components (Alert, Card, FileInput)      │
│  └── Error Boundaries & Error Pages                     │
├─────────────────────────────────────────────────────────┤
│  Business Logic Layer                                    │
│  ├── Custom Hooks (useFileValidation, useFileUpload)   │
│  ├── API Clients (scenarios, media, badges, etc.)      │
│  └── Utilities (errorHandler, schemaValidator)         │
├─────────────────────────────────────────────────────────┤
│  State Management                                        │
│  ├── Zustand Stores (auth, UI state)                   │
│  └── React Query Cache                                  │
└─────────────────────────────────────────────────────────┘
                          │ REST API
                          ▼
┌─────────────────────────────────────────────────────────┐
│               Mystira Admin API                          │
│              (ASP.NET Core Backend)                      │
└─────────────────────────────────────────────────────────┘
```

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- **Node.js**: Version 18.x or higher ([Download](https://nodejs.org/))
- **pnpm**: Version 9.x (specified in `package.json` as `packageManager`) ([Install](https://pnpm.io/installation))
- **Git**: For version control ([Download](https://git-scm.com/))
- **Mystira Admin API**: Backend service must be running

### Installation

1. **Clone the repository**

```bash
git clone https://github.com/phoenixvc/Mystira.Admin.UI.git
cd Mystira.Admin.UI
```

2. **Install dependencies**

```bash
pnpm install
```

3. **Configure environment variables**

Create a `.env.local` file in the root directory:

```env
# API Configuration
VITE_API_BASE_URL=http://localhost:5000

# Azure AD / Entra ID Authentication (Required)
VITE_AZURE_CLIENT_ID=your-client-id
VITE_AZURE_TENANT_ID=your-tenant-id
VITE_AZURE_REDIRECT_URI=http://localhost:7001
VITE_AZURE_API_SCOPE=api://your-backend-app-id/access_as_user

# Optional: Environment
VITE_ENV=development
```

4. **Start the development server**

```bash
pnpm dev
```

The application will be available at `http://localhost:7001`

### Quick Start Checklist

- [ ] Node.js 18+ installed
- [ ] pnpm installed
- [ ] Repository cloned
- [ ] Dependencies installed (`pnpm install`)
- [ ] Azure AD app registration configured (see [Authentication Setup](#authentication-setup))
- [ ] Environment variables configured (`.env.local`)
- [ ] Admin API running on configured URL
- [ ] Development server started (`pnpm dev`)
- [ ] Browser opened to `http://localhost:7001`

### Authentication Setup

This application uses **Microsoft Entra ID (Azure AD)** for authentication via MSAL. To set up authentication:

1. **Register an application** in Azure Portal > App Registrations
2. **Configure the application**:
   - Set the application type to **Single-Page Application (SPA)**
   - Add redirect URI: `http://localhost:7001` (for development)
   - Enable **ID tokens** and **Access tokens** in Authentication settings
3. **Set up API permissions** for your backend API
4. **Copy the values** to your `.env.local`:
   - `VITE_AZURE_CLIENT_ID`: Application (client) ID
   - `VITE_AZURE_TENANT_ID`: Directory (tenant) ID
   - `VITE_AZURE_REDIRECT_URI`: Your redirect URI
   - `VITE_AZURE_API_SCOPE`: Your backend API scope

## 💻 Development

### Available Scripts

| Script | Description |
|--------|-------------|
| `pnpm dev` | Start development server with HMR |
| `pnpm build` | Build for production |
| `pnpm preview` | Preview production build locally |
| `pnpm lint` | Run ESLint to check code quality |
| `pnpm lint:fix` | Fix ESLint errors automatically |
| `pnpm format` | Format code with Prettier |
| `pnpm test` | Run tests once |
| `pnpm test:watch` | Run tests in watch mode |
| `pnpm test:coverage` | Generate test coverage report |
| `pnpm typecheck` | Type check without building |

### Development Workflow

1. **Create a feature branch**

```bash
git checkout -b feature/your-feature-name
```

2. **Make your changes**

Follow the project structure and coding conventions.

3. **Run tests and linting**

```bash
pnpm typecheck
pnpm lint
pnpm test
```

4. **Commit your changes**

```bash
git add .
git commit -m "feat: add your feature description"
```

5. **Push and create a pull request**

```bash
git push origin feature/your-feature-name
```

### Coding Standards

- **TypeScript**: Use strict mode, no `any` types
- **Components**: Functional components with hooks
- **Naming**: PascalCase for components, camelCase for functions/variables
- **Files**: One component per file, named exports preferred
- **Styling**: Bootstrap classes + custom CSS modules
- **State**: Use Zustand for global state, React Query for server state
- **Forms**: React Hook Form + Zod validation
- **Error Handling**: Use error boundaries and proper error types

### Component Development

When creating new components:

1. **Use TypeScript interfaces for props**

```tsx
interface MyComponentProps {
  title: string;
  onAction: () => void;
  optional?: boolean;
}

function MyComponent({ title, onAction, optional = false }: MyComponentProps) {
  // Component logic
}
```

2. **Use reusable components from the library**

```tsx
import Alert from "../components/Alert";
import Card from "../components/Card";
import FileInput from "../components/FileInput";
```

3. **Handle errors gracefully**

```tsx
import { handleApiError } from "../utils/errorHandler";

try {
  await someOperation();
} catch (error) {
  handleApiError(error, "Custom error message");
}
```

4. **Add loading states**

```tsx
import LoadingSpinner from "../components/LoadingSpinner";

if (isLoading) return <LoadingSpinner text="Loading data..." />;
```

## 📁 Project Structure

```
mystira-admin-ui/
├── public/                      # Static assets
├── src/
│   ├── api/                     # API client modules
│   │   ├── client.ts           # Axios client with MSAL token injection
│   │   ├── scenarios.ts        # Scenarios API
│   │   ├── media.ts            # Media API
│   │   ├── badges.ts           # Badges API
│   │   ├── bundles.ts          # Bundles API
│   │   ├── avatars.ts          # Avatars API
│   │   └── ...                 # Other API modules
│   ├── auth/                    # MSAL Authentication
│   │   ├── AuthProvider.tsx    # React context provider for MSAL
│   │   ├── msalConfig.ts       # MSAL configuration
│   │   ├── msalInstance.ts     # PublicClientApplication instance
│   │   ├── useAuth.ts          # Custom hook for auth operations
│   │   └── index.ts            # Barrel export
│   ├── components/              # Reusable UI components
│   │   ├── Alert.tsx           # Alert component
│   │   ├── Card.tsx            # Card wrapper
│   │   ├── Checkbox.tsx        # Checkbox input
│   │   ├── ErrorBoundary.tsx   # Error boundary
│   │   ├── ErrorDisplay.tsx    # Error display
│   │   ├── FileInput.tsx       # File input
│   │   ├── LoadingSpinner.tsx  # Loading indicator
│   │   ├── Pagination.tsx      # Pagination controls
│   │   ├── SearchBar.tsx       # Search input
│   │   ├── ValidationResults.tsx # Validation display
│   │   └── ...                 # Other components
│   ├── hooks/                   # Custom React hooks
│   │   ├── useFileValidation.ts # File validation hook
│   │   ├── useFileUpload.ts    # File upload hook
│   │   └── ...                 # Other hooks
│   ├── pages/                   # Page components
│   │   ├── DashboardPage.tsx   # Dashboard
│   │   ├── ScenariosPage.tsx   # Scenarios list
│   │   ├── CreateScenarioPage.tsx # Create scenario
│   │   ├── EditScenarioPage.tsx # Edit scenario
│   │   ├── ImportScenarioPage.tsx # Import scenario
│   │   ├── ValidateScenariosPage.tsx # Validate scenarios
│   │   ├── MediaPage.tsx       # Media list
│   │   ├── ImportMediaZipPage.tsx # Bulk media import
│   │   ├── BadgesPage.tsx      # Badges list
│   │   ├── BundlesPage.tsx     # Bundles list
│   │   ├── AvatarsPage.tsx     # Avatars management
│   │   ├── NotFoundPage.tsx    # 404 page
│   │   ├── ErrorPage.tsx       # Error page
│   │   └── ...                 # Other pages
│   ├── schemas/                 # JSON schemas
│   │   └── story-schema.json   # Scenario validation schema
│   ├── state/                   # State management
│   │   └── authStore.ts        # Authentication store
│   ├── styles/                  # CSS files
│   │   └── index.css           # Global styles
│   ├── utils/                   # Utility functions
│   │   ├── errorHandler.ts     # Error handling utilities
│   │   ├── schemaValidator.ts  # Schema validation
│   │   └── toast.ts            # Toast notifications
│   ├── App.tsx                  # Main app component
│   ├── Layout.tsx               # Layout wrapper
│   ├── main.tsx                 # Entry point
│   └── ProtectedRoute.tsx       # Route guard
├── .env.local                   # Local environment variables (not in git)
├── .eslintrc.cjs               # ESLint configuration
├── .gitignore                   # Git ignore rules
├── .prettierrc                  # Prettier configuration
├── index.html                   # HTML template
├── package.json                 # Dependencies and scripts
├── pnpm-lock.yaml              # Lock file
├── tsconfig.json               # TypeScript configuration
├── tsconfig.node.json          # TypeScript config for Node
├── vite.config.ts              # Vite configuration
└── README.md                    # This file
```

### Key Directories

- **`src/api/`**: API client modules using Axios for HTTP requests
- **`src/auth/`**: MSAL authentication (AuthProvider, useAuth hook, config)
- **`src/components/`**: Reusable UI components (Alert, Card, FileInput, etc.)
- **`src/hooks/`**: Custom React hooks for business logic
- **`src/pages/`**: Page-level components for routing
- **`src/schemas/`**: JSON schemas for validation
- **`src/state/`**: Zustand stores for global state
- **`src/utils/`**: Utility functions and helpers

## ⚙️ Configuration

### Environment Variables

Create a `.env.local` file for local development:

```env
# Required: API base URL
VITE_API_BASE_URL=http://localhost:5000

# Required: Azure AD / Entra ID Authentication
VITE_AZURE_CLIENT_ID=your-client-id
VITE_AZURE_TENANT_ID=your-tenant-id
VITE_AZURE_REDIRECT_URI=http://localhost:7001
VITE_AZURE_API_SCOPE=api://your-backend-app-id/access_as_user

# Optional: Environment identifier
VITE_ENV=development

# Optional: Enable debug logging
VITE_DEBUG=true
```

**Available Variables:**

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `VITE_API_BASE_URL` | Backend API URL | - | Yes |
| `VITE_AZURE_CLIENT_ID` | Azure AD Application (client) ID | - | Yes |
| `VITE_AZURE_TENANT_ID` | Azure AD Directory (tenant) ID | `common` | Yes |
| `VITE_AZURE_REDIRECT_URI` | OAuth redirect URI | `window.location.origin` | No |
| `VITE_AZURE_API_SCOPE` | API scope for backend access | `User.Read` | Yes |
| `VITE_ENV` | Environment name | `development` | No |
| `VITE_DEBUG` | Enable debug logs | `false` | No |

### Vite Configuration

The `vite.config.ts` file configures:

- **Port**: Development server runs on port 7001
- **Proxy**: API requests are proxied to avoid CORS issues
- **Build**: Output directory and optimization settings
- **Plugins**: React plugin with Fast Refresh

### TypeScript Configuration

The `tsconfig.json` file enables:

- **Strict Mode**: Full type checking
- **Path Aliases**: Import shortcuts (e.g., `@/components`)
- **JSON Imports**: Import JSON files as modules
- **JSX**: React JSX support

### ESLint Configuration

Code quality rules include:

- TypeScript ESLint recommended rules
- React Hooks rules
- React Refresh rules
- Custom rules for consistency

### Prettier Configuration

Code formatting settings:

- 2 spaces indentation
- Single quotes
- Trailing commas
- 100 character line width

## 🧪 Testing

### Running Tests

```bash
# Run all tests once
pnpm test

# Run tests in watch mode
pnpm test:watch

# Generate coverage report
pnpm test:coverage
```

### Test Structure

Tests are located next to the files they test:

```
src/
├── components/
│   ├── Alert.tsx
│   └── Alert.test.tsx
├── utils/
│   ├── errorHandler.ts
│   └── errorHandler.test.ts
```

### Writing Tests

Use React Testing Library and Vitest:

```tsx
import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import Alert from './Alert';

describe('Alert', () => {
  it('renders success alert', () => {
    render(<Alert variant="success">Success message</Alert>);
    expect(screen.getByText('Success message')).toBeInTheDocument();
  });
});
```

## 🚢 Deployment

### Building for Production

```bash
# Build the application
pnpm build

# Preview the build locally
pnpm preview
```

The build output will be in the `dist/` directory.

### Deployment Checklist

- [ ] Update environment variables for production
- [ ] Run `pnpm typecheck` to ensure no type errors
- [ ] Run `pnpm lint` to ensure code quality
- [ ] Run `pnpm test` to ensure all tests pass
- [ ] Run `pnpm build` to create production build
- [ ] Test the production build with `pnpm preview`
- [ ] Deploy `dist/` directory to hosting service
- [ ] Verify API connectivity in production
- [ ] Test critical user flows

### Deployment Targets

The application can be deployed to:

- **Vercel**: Automatic deployments from Git
- **Netlify**: Continuous deployment
- **AWS S3 + CloudFront**: Static hosting with CDN
- **Azure Static Web Apps**: Integrated with Azure services
- **Docker**: Containerized deployment

### Environment-Specific Builds

Create environment-specific `.env` files:

- `.env.development` - Development settings
- `.env.staging` - Staging settings
- `.env.production` - Production settings

## 🤝 Contributing

### Contribution Guidelines

1. **Fork the repository** and create a feature branch
2. **Follow coding standards** and conventions
3. **Write tests** for new features
4. **Update documentation** as needed
5. **Submit a pull request** to the `dev` branch

### Pull Request Process

1. Ensure all tests pass
2. Update README if needed
3. Add description of changes
4. Request review from maintainers
5. Address review feedback
6. Merge after approval

### Code Review Checklist

- [ ] Code follows project conventions
- [ ] TypeScript types are properly defined
- [ ] Components are properly tested
- [ ] No console errors or warnings
- [ ] Accessibility considerations addressed
- [ ] Performance implications considered
- [ ] Documentation updated

## 🐛 Troubleshooting

### Common Issues

#### Port Already in Use

```bash
# Error: Port 7001 is already in use
# Solution: Kill the process or use a different port
lsof -ti:7001 | xargs kill -9
# Or change the port in vite.config.ts
```

#### API Connection Failed

```bash
# Error: Network Error or CORS issues
# Solution: Verify API is running and VITE_API_BASE_URL is correct
curl http://localhost:5000/api/health
```

#### TypeScript Errors

```bash
# Error: Type errors during build
# Solution: Run type check and fix errors
pnpm typecheck
```

#### Module Not Found

```bash
# Error: Cannot find module
# Solution: Clear cache and reinstall
rm -rf node_modules pnpm-lock.yaml
pnpm install
```

### Debug Mode

Enable debug logging:

```env
VITE_DEBUG=true
```

Check browser console for detailed logs.

### Getting Help

- **Issues**: [GitHub Issues](https://github.com/phoenixvc/Mystira.Admin.UI/issues)
- **Discussions**: [GitHub Discussions](https://github.com/phoenixvc/Mystira.Admin.UI/discussions)
- **Documentation**: Check the `/docs` directory

## 📄 License

This project is private and proprietary. All rights reserved.

## 🔗 Related Repositories

- **[Mystira.Admin.Api](https://github.com/phoenixvc/Mystira.Admin.Api)**: Backend API service
- **[Mystira.App](https://github.com/phoenixvc/Mystira.App)**: Main application

## 📝 Changelog

See [CHANGELOG.md](./CHANGELOG.md) for a list of changes.

## 👥 Team

Maintained by the Mystira development team.

---

**Built with ❤️ using React, TypeScript, and Vite**
