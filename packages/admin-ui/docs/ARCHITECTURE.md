# Architecture Documentation

This document describes the architecture and design decisions of the Mystira Admin UI.

## Overview

Mystira Admin UI is a modern single-page application (SPA) built with React and TypeScript. It follows a component-based architecture with clear separation of concerns between presentation, business logic, and data management.

## Technology Stack

### Core Technologies

**Frontend Framework**: React 18.3 provides the foundation for building interactive user interfaces with a component-based approach. React's virtual DOM and efficient rendering make it ideal for complex admin interfaces with frequent updates.

**Language**: TypeScript 5.6 adds static typing to JavaScript, enabling better developer experience with IntelliSense, compile-time error checking, and improved code maintainability. Strict mode is enabled to enforce type safety throughout the codebase.

**Build Tool**: Vite 5.4 offers lightning-fast development server with Hot Module Replacement (HMR) and optimized production builds. Vite's native ES modules support and efficient bundling significantly improve development experience and build performance.

### UI and Styling

**CSS Framework**: Bootstrap 5.3 provides a comprehensive set of responsive components and utilities. Bootstrap's grid system and pre-built components accelerate development while maintaining consistency across the application.

**Icons**: Bootstrap Icons 1.11 offers a wide range of SVG icons that integrate seamlessly with Bootstrap components. Icons are used throughout the interface for visual communication and improved user experience.

**Custom Styles**: Additional CSS is used sparingly for application-specific styling that cannot be achieved with Bootstrap alone. Custom styles are organized by component or feature to maintain modularity.

### State Management

**Global State**: Zustand 5.0 manages application-wide state such as UI preferences. Zustand's simple API and minimal boilerplate make it easy to create and consume stores without complex setup.

**Authentication State**: MSAL (Microsoft Authentication Library) manages authentication state including user sessions, tokens, and login/logout flows. MSAL handles token caching, refresh, and silent authentication automatically.

**Server State**: TanStack React Query 5.60 handles all server-side data fetching, caching, and synchronization. React Query automatically manages loading states, error handling, and cache invalidation, reducing boilerplate and improving data consistency.

**Form State**: React Hook Form 7.53 manages form state and validation with minimal re-renders. Combined with Zod 3.23 for schema validation, it provides a robust solution for complex forms with type-safe validation.

### Data Fetching and Validation

**HTTP Client**: Axios 1.7 provides a promise-based HTTP client with interceptors for request/response transformation, automatic JSON parsing, and error handling. Axios is used in all API client modules for consistent HTTP communication.

**Schema Validation**: Ajv 8.17 validates JSON data against JSON Schema specifications. Used primarily for validating scenario uploads against the story schema before submission to the backend.

**YAML Parsing**: js-yaml 4.1 enables parsing and stringifying YAML files. Scenarios can be uploaded in both YAML and JSON formats, providing flexibility for content creators.

## Architecture Layers

### Presentation Layer

The presentation layer consists of React components organized into pages and reusable UI components. Components are responsible for rendering UI and handling user interactions but contain minimal business logic.

**Pages** represent complete views in the application (e.g., ScenariosPage, MediaPage). Each page component orchestrates multiple smaller components and hooks to create a cohesive user experience. Pages handle routing parameters and coordinate data fetching.

**Components** are reusable UI elements (e.g., Alert, Card, FileInput) that can be composed to build pages. Components accept props for configuration and emit events for user interactions. They are designed to be generic and reusable across different contexts.

**Error Boundaries** wrap component trees to catch and handle React errors gracefully. When an error occurs, the error boundary displays a fallback UI instead of crashing the entire application. Stack traces and error details are shown in development mode for debugging.

### Business Logic Layer

The business logic layer contains custom hooks and utility functions that encapsulate application logic separate from UI concerns.

**Custom Hooks** extract reusable stateful logic from components. For example, useFileValidation handles file parsing and schema validation, while useFileUpload manages upload state and error handling. Hooks promote code reuse and testability.

**API Clients** are modules that encapsulate HTTP requests to the backend API. Each domain (scenarios, media, badges, etc.) has its own API client module with typed functions for CRUD operations. API clients use Axios and return typed promises.

**Utilities** are pure functions that perform specific tasks such as error parsing, schema validation, and toast notifications. Utilities are stateless and can be easily tested in isolation.

### Data Management Layer

The data management layer handles application state and data persistence.

**Zustand Stores** manage global application state such as authentication tokens and user preferences. Stores expose actions for updating state and selectors for accessing state. State changes trigger component re-renders automatically.

**React Query Cache** stores server-side data with automatic caching, background refetching, and stale-while-revalidate behavior. React Query reduces network requests and improves perceived performance by serving cached data while fetching fresh data in the background.

**Session Storage** is used by MSAL for token caching. Tokens are stored securely in the browser's session storage and automatically refreshed when they expire. This provides a balance between security (tokens don't persist after browser close) and user experience (no re-authentication during a session).

## Component Architecture

### Component Hierarchy

```
App (ErrorBoundary)
├── BrowserRouter
│   └── Routes
│       ├── LoginPage
│       └── Layout (ProtectedRoute)
│           ├── Navigation
│           ├── Sidebar
│           └── Outlet
│               ├── DashboardPage
│               ├── ScenariosPage
│               │   ├── SearchBar
│               │   ├── Pagination
│               │   └── ScenarioList
│               ├── MediaPage
│               └── ...
```

### Component Patterns

**Container/Presentational Pattern**: Complex pages are split into container components (handle logic and data) and presentational components (render UI). This separation makes components easier to test and reuse.

**Compound Components**: Related components are grouped together (e.g., Card with Card.Header, Card.Body). This pattern provides flexibility while maintaining consistency.

**Render Props**: Some components accept render functions as props to customize rendering behavior. This pattern enables component composition without prop drilling.

**Higher-Order Components**: ProtectedRoute wraps routes to enforce authentication. HOCs add behavior to components without modifying their implementation.

## Data Flow

### Request Flow

User interactions trigger events that flow through the application layers:

1. **User Action**: User clicks a button or submits a form
2. **Event Handler**: Component event handler is called
3. **Business Logic**: Custom hook or utility function processes the action
4. **API Request**: API client sends HTTP request to backend
5. **Response Handling**: Response is parsed and cached by React Query
6. **State Update**: Component state or global state is updated
7. **Re-render**: React re-renders affected components
8. **UI Update**: User sees the result of their action

### Error Flow

Errors are handled at multiple levels:

1. **API Errors**: Caught by API client, parsed by error handler, displayed via toast
2. **Validation Errors**: Caught by form validation, displayed inline near form fields
3. **React Errors**: Caught by error boundaries, displayed in ErrorDisplay component
4. **Network Errors**: Detected by Axios interceptors, retried automatically

## Security Considerations

### Authentication

Authentication is handled via **Microsoft Entra ID (Azure AD)** using the **MSAL (Microsoft Authentication Library)** for React. The authentication flow works as follows:

1. **Login**: Users authenticate via Microsoft's identity platform using popup or redirect flows
2. **Token Management**: MSAL automatically handles token acquisition, caching (sessionStorage), and refresh
3. **API Authorization**: Access tokens are silently acquired and attached to API requests via Axios interceptors
4. **Session State**: The `AuthProvider` component initializes MSAL and manages the authentication state globally

Key authentication files:
- `src/auth/AuthProvider.tsx` - MSAL provider wrapper with initialization logic
- `src/auth/useAuth.ts` - Custom hook exposing `login()`, `logout()`, `getAccessToken()`, `isAuthenticated`
- `src/auth/msalConfig.ts` - MSAL configuration with environment variables
- `src/api/client.ts` - Axios interceptor that automatically attaches bearer tokens

### Authorization

Route-level authorization is enforced by ProtectedRoute component. Unauthenticated users are redirected to the login page. API-level authorization is handled by the backend, with the frontend respecting 401/403 responses.

### Input Validation

All user input is validated on both client and server sides. Client-side validation provides immediate feedback, while server-side validation ensures security. Zod schemas define validation rules that are enforced in forms.

### XSS Prevention

React automatically escapes content to prevent XSS attacks. Dangerous HTML is never rendered directly. User-generated content is sanitized before display.

### CSRF Protection

CSRF tokens are included in API requests via Axios interceptors. The backend validates tokens to prevent cross-site request forgery attacks.

## Performance Optimization

### Code Splitting

Large components and routes are lazy-loaded using React.lazy() and Suspense. This reduces initial bundle size and improves time-to-interactive. Code splitting is applied at the route level for optimal loading performance.

### Memoization

Expensive computations are memoized using useMemo() to avoid recalculation on every render. Callback functions are wrapped in useCallback() to prevent unnecessary re-renders of child components.

### Virtual Scrolling

Large lists use virtual scrolling to render only visible items. This dramatically improves performance when displaying thousands of items.

### Image Optimization

Images are lazy-loaded and served in appropriate sizes. Responsive images use srcset to serve different sizes based on device capabilities.

### Caching Strategy

React Query implements a stale-while-revalidate caching strategy. Cached data is served immediately while fresh data is fetched in the background. This provides instant UI updates while ensuring data freshness.

## Testing Strategy

### Unit Tests

Individual functions and components are tested in isolation using Vitest and React Testing Library. Unit tests verify that components render correctly and respond to user interactions as expected.

### Integration Tests

Integration tests verify that multiple components work together correctly. These tests simulate user workflows and verify that data flows correctly through the application layers.

### End-to-End Tests

E2E tests (planned) will verify complete user journeys from login to task completion. E2E tests run against a real backend and database to ensure the entire system works as expected.

## Deployment Architecture

### Build Process

The production build process compiles TypeScript to JavaScript, bundles modules, minifies code, and optimizes assets. The output is a set of static files that can be served from any web server or CDN.

### Hosting Options

The application can be deployed to various hosting platforms:

**Static Hosting**: Vercel, Netlify, AWS S3 + CloudFront, Azure Static Web Apps
**Container**: Docker container with Nginx serving static files
**Traditional Server**: Any web server capable of serving static files

### Environment Configuration

Environment-specific configuration is injected at build time via environment variables. Different builds are created for development, staging, and production environments with appropriate API URLs and feature flags.

## Future Considerations

### Progressive Web App

Adding PWA capabilities would enable offline functionality and installability. Service workers could cache API responses and assets for offline access.

### Real-Time Updates

WebSocket connections could provide real-time updates for collaborative editing and notifications. This would improve the user experience for multi-user scenarios.

### Internationalization

i18n support would enable the application to be translated into multiple languages. This would expand the potential user base and improve accessibility.

### Accessibility

Enhanced accessibility features would make the application usable for users with disabilities. This includes keyboard navigation, screen reader support, and ARIA attributes.

### Micro-Frontends

As the application grows, it could be split into micro-frontends for better team autonomy and independent deployment. Each feature area could be developed and deployed separately.

## Conclusion

The architecture of Mystira Admin UI is designed for maintainability, scalability, and developer experience. Clear separation of concerns, strong typing, and modern tooling enable rapid development while maintaining code quality. The component-based architecture promotes reusability and testability, making it easy to extend and modify the application as requirements evolve.
