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

**Status**: 🚧 Ready to begin - Repository setup complete

**Tasks**:

- [ ] Identify Admin UI code in `Mystira.App` (Blazor components, Razor Pages, etc.)
- [ ] Extract UI components and pages
- [ ] Convert from Blazor/Razor to modern SPA framework (if needed)
- [ ] Set up frontend build tooling (Vite/Webpack/etc)
- [ ] Configure API client for `Mystira.Admin.Api`
- [ ] Migrate authentication/authorization logic
- [ ] Port styling and assets
- [ ] Update routing and navigation

**Source Location** (to be confirmed):

- Likely in `Mystira.App` under Admin-related Razor Pages or Blazor components

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
