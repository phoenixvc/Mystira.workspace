# Admin Tooling Migration Phases

This document tracks the migration of Admin tooling from `Mystira.App` monorepo into separate repositories (`Mystira.Admin.Api` and `Mystira.Admin.UI`).

## Migration Overview

**Goal**: Extract Admin API and Admin UI from `Mystira.App` into independent repositories to enable:

- Independent deployment and versioning
- Separate development workflows
- Modern frontend stack without .NET/Blazor dependencies
- Better separation of concerns

## Current Status: Phase 3 (Admin UI Code Migration)

### ✅ Phase 1: Admin API Extraction - **COMPLETED**

**Status**: ✅ Complete and operational

**Completed Tasks**:

- ✅ Repository created: `Mystira.Admin.Api`
- ✅ Admin API code extracted from `Mystira.App`
- ✅ Pure REST/gRPC API (no Razor Pages UI)
- ✅ NuGet package dependencies configured
- ✅ CORS configured for Admin UI integration
- ✅ Repository registered as git submodule in workspace
- ✅ Deployed to production and development environments
- ✅ Documentation created

**Evidence**:

- Repository exists at `packages/admin-api/` with full codebase
- Git commit: `9d80ed6 feat: initial Admin API extraction from Mystira.App`
- Active on `dev` branch
- Production URL: `prod-wus-app-mystira-api-admin.azurewebsites.net`
- Development URL: `dev-san-app-mystira-admin-api.azurewebsites.net/swagger`

**What Remains in Mystira.App**:

- `src/Mystira.App.Admin.Api` - **Should be removed** after Admin UI migration is complete and verified

---

### ✅ Phase 2: Admin UI Repository Setup - **COMPLETED**

**Status**: ✅ Complete - Repository set up and registered as submodule

**Completed Tasks**:

- ✅ Repository created: `Mystira.Admin.UI`
- ✅ Initial README.md created and pushed to remote repository
- ✅ Repository registered in `.gitmodules` with `dev` branch
- ✅ Successfully registered as git submodule in workspace
- ✅ Migration plan documented in README

**Evidence**:

- Repository exists at `packages/admin-ui/` as proper git submodule
- Git commit: `6b20eca docs: add initial README with migration status`
- Active on `dev` branch
- Submodule status shows: `6b20eca568d9248f0d78230e688e398398ae26d4 packages/admin-ui (heads/dev)`

**Next Steps** (Phase 3):

1. Extract Admin UI code from `Mystira.App` (likely Blazor/Razor Pages)
2. Set up modern frontend stack (React/Vue/Next.js/etc)
3. Configure API integration with `Mystira.Admin.Api`
4. Set up CI/CD pipeline
5. Deploy and verify functionality

---

### 🚧 Phase 3: Admin UI Code Migration - **IN PROGRESS**

**Status**: 🚧 Phase 3.1 Complete - Project structure initialized

**Completed Tasks (Phase 3.1)**:

- ✅ Identified Admin UI code in `Mystira.App.Admin.Api/Views` (Razor Pages)
- ✅ Created migration analysis document
- ✅ Set up React 18 + TypeScript + Vite project structure
- ✅ Configured Bootstrap 5 and Bootstrap Icons
- ✅ Created basic Layout, Login, and Dashboard pages
- ✅ Set up API client with axios
- ✅ Implemented authentication store with Zustand
- ✅ Added React Router for navigation
- ✅ Ported admin.css styles

**Source Location**:

- `Mystira.App.Admin.Api/Views/Admin/` - 22 Razor Pages
- `Mystira.App.Admin.Api/Views/Shared/` - Layout files
- `Mystira.App.Admin.Api/wwwroot/css/admin.css` - Styles

**Completed Tasks (Phase 3.2-3.3)**:

- ✅ Complete API client implementation (scenarios, media, badges, bundles)
- ✅ Updated auth to use cookie-based authentication
- ✅ Created Scenarios management page with list, search, delete
- ✅ Created Media management page with list, search, upload, delete
- ✅ Added routing for new pages
- ✅ Implemented pagination and error handling
- ✅ Set up React Query for data fetching

**Completed Tasks (Phase 3.4)**:

- ✅ Migrated Badges page with list, search, delete
- ✅ Migrated Bundles page with list and search
- ✅ Created Import Scenario page with file upload
- ✅ Created Import Media page with file upload
- ✅ Created Import Bundle page with validation options
- ✅ Added navigation links for all pages
- ✅ Set up ESLint and Prettier configuration

**Completed Tasks (Phase 3.5)**:

- ✅ Migrated Character Maps page with list, search, delete
- ✅ Created Import Character Map page
- ✅ Created reusable Pagination and SearchBar components
- ✅ Migrated all Master Data pages (Age Groups, Archetypes, Compass Axes, Echo Types, Fantasy Themes)
- ✅ Created unified MasterDataPage component for efficient code reuse
- ✅ Added masterData API client with full CRUD operations

**Completed Tasks (Phase 3.6)**:

- ✅ Created Import Badge page with image preview
- ✅ Added reusable LoadingSpinner component
- ✅ Added reusable ErrorAlert component with retry functionality
- ✅ Updated ScenariosPage to use new components
- ✅ All import pages now complete (Scenario, Media, Bundle, Badge, Character Map)

**Completed Tasks (Phase 3.7-3.8)**:

- ✅ Updated all major pages (Scenarios, Media, Badges, Bundles, Dashboard) to use reusable components
- ✅ Replaced all inline loading states with LoadingSpinner component
- ✅ Replaced all inline error states with ErrorAlert component
- ✅ Replaced all inline search bars with SearchBar component
- ✅ Replaced all inline pagination with Pagination component
- ✅ Improved Dashboard refresh to use React Query refetch instead of page reload
- ✅ Improved code consistency and maintainability across all pages
- ✅ MasterDataPage already uses reusable components (created with them)

**Completed Tasks (Phase 3.9)**:

- ✅ Added react-hot-toast library for toast notifications
- ✅ Created toast utility with success, error, info, loading, and promise helpers
- ✅ Replaced all alert() calls with toast notifications across entire application
- ✅ Updated all pages: Scenarios, Media, Badges, Bundles, Character Maps, Master Data
- ✅ Updated all import pages: Scenario, Media, Bundle, Badge, Character Map
- ✅ Improved UX with non-blocking, consistent notifications

**Completed Tasks (Phase 3.10-3.12)**:

- ✅ Created Edit Scenario page with React Hook Form + Zod validation
- ✅ Implemented form validation with proper error messages
- ✅ Created reusable form components: FormField, TextInput, Textarea, NumberInput
- ✅ Refactored EditScenarioPage to use new form components
- ✅ Created Edit Badge page with React Hook Form + Zod validation
- ✅ Created Edit Character Map page with React Hook Form + Zod validation
- ✅ Created unified Edit Master Data page for all master data types (Age Groups, Archetypes, Compass Axes, Echo Types, Fantasy Themes)
- ✅ Added routes for all edit pages
- ✅ Updated list pages (Badges, Character Maps, Master Data) to link to edit pages
- ✅ Updated CharacterMapsPage to use LoadingSpinner and ErrorAlert
- ✅ Updated LoginPage to use toast notifications
- ✅ Fixed missing imports in BundlesPage

**Completed Tasks (Phase 3.13)**:

- ✅ Created Create Scenario page with React Hook Form + Zod validation
- ✅ Created Create Badge page with React Hook Form + Zod validation
- ✅ Created Create Character Map page with React Hook Form + Zod validation
- ✅ Created unified Create Master Data page for all master data types
- ✅ Added routes for all create pages
- ✅ Updated list pages to link to create pages instead of placeholder buttons

**Remaining Tasks**:

- [x] ~~Create edit/create forms for Media~~ - Not applicable (file-based, upload/delete only)
- [x] ~~Create edit/create forms for Bundles~~ - Not applicable (file-based, upload/delete only)
- [ ] Test authentication flow end-to-end
- [ ] Verify API integration with real backend
- [ ] Complete styling migration and UI polish (minor refinements)
- [ ] Set up CI/CD pipeline
- [ ] Remove Admin UI from `Mystira.App` monorepo (after verification)

**Note**: Media and Bundles are file-based entities that use upload/import pages. Traditional create/edit forms are not applicable for these entities.

- [ ] Complete styling migration and polish UI
- [ ] Set up CI/CD pipeline

---

### ⏳ Phase 4: Integration & Testing - **NOT STARTED**

**Status**: ⏳ Pending Phase 3 completion

**Tasks**:

- [ ] Verify Admin UI connects to Admin API correctly
- [ ] Test all admin workflows end-to-end
- [ ] Verify authentication/authorization
- [ ] Test CORS configuration
- [ ] Performance testing
- [ ] Security audit
- [ ] User acceptance testing

---

### ⏳ Phase 5: Deployment & Verification - **NOT STARTED**

**Status**: ⏳ Pending Phase 4 completion

**Tasks**:

- [ ] Set up CI/CD pipeline for Admin UI
- [ ] Configure deployment to staging environment
- [ ] Deploy to staging and verify
- [ ] Deploy to production
- [ ] Monitor for issues
- [ ] Update documentation

---

### ⏳ Phase 6: Cleanup - **NOT STARTED**

**Status**: ⏳ Pending Phase 5 completion

**Tasks**:

- [ ] Remove Admin API code from `Mystira.App` (`src/Mystira.App.Admin.Api`)
- [ ] Remove Admin UI code from `Mystira.App` (Blazor/Razor components)
- [ ] Update `Mystira.App` documentation
- [ ] Update workspace documentation
- [ ] Archive or remove old admin-related code paths
- [ ] Update any references in other repositories

---

## Architecture

### Before Migration (Current in Mystira.App)

```
Mystira.App/
├── src/Mystira.App.Admin.Api/     ← Admin API (to be removed)
├── [Admin UI Razor/Blazor]        ← Admin UI (to be removed)
└── [Shared libraries]              ← Will remain
```

### After Migration (Target State)

```
Mystira.Admin.Api/                  ← Pure REST/gRPC API ✅
Mystira.Admin.UI/                   ← Modern SPA (React/Vue/etc) 🚧
Mystira.App/                        ← Main app (Admin code removed)
```

### Integration Flow

```
Admin UI (SPA) → REST/gRPC → Admin API → NuGet packages → Mystira.App (Domain/Infra)
```

---

## Repository Status

| Repository          | Status      | Branch | Submodule     | Notes                    |
| ------------------- | ----------- | ------ | ------------- | ------------------------ |
| `Mystira.Admin.Api` | ✅ Complete | `dev`  | ✅ Registered | Fully operational        |
| `Mystira.Admin.UI`  | ✅ Setup    | `dev`  | ✅ Registered | Ready for code migration |
| `Mystira.App`       | 📦 Source   | `main` | ✅ Registered | Contains code to migrate |

---

## Key Decisions

1. **Admin API**: Already extracted and using pure REST/gRPC (no UI dependencies)
2. **Admin UI**: Will be modern SPA (not Blazor) to enable better frontend tooling
3. **Dependencies**: Admin API depends on NuGet packages from `Mystira.App`
4. **CORS**: Admin API configured to accept requests from Admin UI

---

## Next Actions

### Immediate (Phase 3 - Code Migration)

1. **Extract Admin UI code** from `Mystira.App`
2. **Set up frontend framework** (choose: React/Vue/Next.js/etc)
3. **Configure API integration** with `Mystira.Admin.Api`
4. **Set up build tooling** and development environment

### Short-term (Phase 3)

1. Complete code migration
2. Set up build tooling
3. Configure API integration

### Medium-term (Phases 4-5)

1. Testing and verification
2. CI/CD setup
3. Deployment

### Long-term (Phase 6)

1. Cleanup `Mystira.App`
2. Documentation updates
3. Archive old code

---

## Notes

- Admin API is already deployed and operational in production
- Admin UI repository exists but is empty (blocking submodule registration)
- Need to identify exact location of Admin UI code in `Mystira.App` before extraction
- Consider whether to convert from Blazor to modern SPA or keep Blazor (decision pending)
