# Admin UI Migration Strategy

This document outlines the technical strategy for migrating the Admin UI from ASP.NET Razor Pages to a modern React SPA.

## Background

### Original Implementation

The Admin UI was originally part of `Mystira.App` as ASP.NET Core Razor Pages:

- **Framework**: ASP.NET Core 8.0
- **UI**: Razor Pages with Bootstrap 5
- **Location**: `Mystira.App.Admin.Api/Views/Admin/`
- **Pages**: 22 Razor Pages
- **Styling**: Bootstrap 5 with custom admin.css

### Target Implementation

Modern React SPA in independent repository:

- **Framework**: React 18.3.1 + TypeScript 5.6
- **Build Tool**: Vite 5.4
- **Styling**: Bootstrap 5.3.3 (same as original)
- **State**: Zustand
- **Data Fetching**: TanStack React Query
- **Forms**: React Hook Form + Zod

## Migration Strategy

### Approach: Greenfield Rewrite

We chose a **greenfield rewrite** approach rather than incremental migration:

**Reasons**:
1. Razor Pages and React have fundamentally different paradigms
2. Modern React patterns (hooks, functional components) are cleaner
3. TypeScript provides better type safety than C#/Razor mix
4. Vite provides superior developer experience (hot reload)
5. Independent repository enables better deployment flexibility

**Trade-offs**:
- Higher initial effort than incremental migration
- Risk of missing features during rewrite
- Need to maintain two codebases temporarily

### Component Mapping

| Razor Pattern | React Pattern |
|---------------|---------------|
| Razor Page + PageModel | Page Component + React Query |
| Partial View | Reusable Component |
| ViewData | Props or Context |
| Form (asp-for) | React Hook Form |
| Validation | Zod Schema |
| Alert (success/error) | Toast Notifications |
| Table pagination | Pagination Component |

### API Integration

The Admin UI connects to the Admin API (already extracted):

```
Admin UI (React SPA)
    ↓ REST API calls
Admin API (ASP.NET Core)
    ↓ NuGet packages
Domain/Infrastructure (Mystira.App)
```

**Key Considerations**:
- CORS must be configured on Admin API
- Cookie-based authentication (same-site cookies)
- API base URL configured via environment variables

## Technical Decisions

### 1. React Instead of Blazor

**Decision**: Use React, not Blazor

**Rationale**:
- Larger ecosystem and community
- Better tooling (Vite, ESLint, Prettier)
- More frontend developers familiar with React
- Faster development iteration
- Lighter bundle size

### 2. TypeScript

**Decision**: Use TypeScript with strict mode

**Rationale**:
- Type safety catches errors at compile time
- Better IDE support (autocomplete, refactoring)
- Self-documenting code
- Easier maintenance

### 3. Bootstrap 5 (Not Tailwind)

**Decision**: Keep Bootstrap 5

**Rationale**:
- Minimal styling changes from original
- Admin already styled with Bootstrap
- Consistent look and feel
- Faster migration (reuse existing CSS)

### 4. Zustand for State

**Decision**: Use Zustand instead of Redux

**Rationale**:
- Simpler API
- Less boilerplate
- Sufficient for admin app state
- Easy testing

### 5. React Query for Data

**Decision**: Use TanStack React Query

**Rationale**:
- Handles caching, refetching, error states
- Reduces boilerplate
- Optimistic updates
- Background refetching

### 6. React Hook Form + Zod

**Decision**: Use React Hook Form with Zod validation

**Rationale**:
- Performant (uncontrolled inputs)
- Type-safe schemas with Zod
- Easy integration with UI components
- Consistent validation patterns

## File Structure

```
src/
├── api/          # API client modules
│   ├── client.ts # Base axios client
│   ├── auth.ts   # Auth endpoints
│   ├── scenarios.ts
│   └── ...
├── components/   # Reusable UI components
│   ├── LoadingSpinner.tsx
│   ├── ErrorAlert.tsx
│   ├── SearchBar.tsx
│   └── ...
├── pages/        # Page components
│   ├── DashboardPage.tsx
│   ├── LoginPage.tsx
│   ├── ScenariosPage.tsx
│   └── ...
├── state/        # Zustand stores
│   └── authStore.ts
├── utils/        # Utility functions
│   └── toast.ts
├── styles/       # CSS files
│   ├── admin.css # Ported from original
│   └── index.css
├── App.tsx       # Routes definition
├── Layout.tsx    # Main layout
└── main.tsx      # Entry point
```

## Migration Process

### Step 1: Setup Project
- Initialize Vite + React + TypeScript
- Configure dependencies
- Set up ESLint, Prettier

### Step 2: Core Infrastructure
- Create API client
- Set up authentication store
- Create Layout component
- Set up routing

### Step 3: Port Styles
- Copy admin.css
- Adjust for React (className, etc.)

### Step 4: Migrate Pages
For each Razor Page:
1. Analyze PageModel (data requirements)
2. Create API client function
3. Create React component
4. Add React Query hook
5. Build UI with same Bootstrap classes
6. Add form validation if applicable

### Step 5: Create Reusables
- Identify repeated patterns
- Extract to reusable components
- Update pages to use components

### Step 6: Testing & Polish
- Test with real API
- Fix styling issues
- Improve UX

## Lessons Learned

1. **Start with infrastructure**: Having API client and auth working first made page migration smoother

2. **Extract components early**: Identified patterns after 3-4 pages, then refactored

3. **Keep Bootstrap classes**: Direct port of Bootstrap classes reduced CSS work

4. **TypeScript for API types**: Matching API response types caught integration issues early

5. **React Query simplified state**: No need for complex state management with data-fetching library

## References

- [React Documentation](https://react.dev/)
- [Vite Documentation](https://vitejs.dev/)
- [TanStack Query](https://tanstack.com/query/)
- [React Hook Form](https://react-hook-form.com/)
- [Zod](https://zod.dev/)
