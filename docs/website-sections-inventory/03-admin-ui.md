# Mystira Admin UI — Website Sections Inventory

> Internal content management and moderation dashboard for platform administrators.
> Built with React 19 + TypeScript + Bootstrap 5.
> Last updated: 2026-03-07

## Application Overview

| Property | Value |
|----------|-------|
| Technology | React 19 + TypeScript |
| Location | `packages/admin-ui` |
| Build tool | Vite 7.x |
| Styling | Bootstrap 5.3.8 + Bootstrap Icons + Custom CSS variables |
| State management | Zustand (auth) + React Context (theme) + React Query (server state) |
| Authentication | Microsoft Entra (MSAL) + Username/Password fallback |
| Form validation | React Hook Form + Zod |
| HTTP client | Axios |

## Routes

`/login`, `/admin` (dashboard), `/admin/scenarios`, `/admin/scenarios/create`, `/admin/scenarios/edit/:id`, `/admin/scenarios/import`, `/admin/scenarios/validate`, `/admin/media`, `/admin/media/import`, `/admin/media/import-zip`, `/admin/badges`, `/admin/badges/create`, `/admin/badges/edit/:id`, `/admin/badges/import`, `/admin/bundles`, `/admin/bundles/create`, `/admin/bundles/edit/:id`, `/admin/bundles/import`, `/admin/character-maps`, `/admin/character-maps/create`, `/admin/character-maps/edit/:id`, `/admin/character-maps/import`, `/admin/master-data/:type`, `/admin/accounts`, `/admin/profiles`, `/admin/avatars`

---

## HEADER (Layout — Global Navigation)

- **Primary purpose:** Provide navigation across all admin sections and quick access to theme/auth controls
- **Target audience:** Authenticated administrators and content managers
- **Key message:** Full access to Mystira content management tools
- **Current layout/structure:**
  - Dark Bootstrap 5 navbar with brand "Mystira Admin" and dice icon
  - Main navigation links: Dashboard, Scenarios, Media, Badges, Bundles, Avatars, Character Maps
  - **Dropdown: "Master Data"** with 5 items:
    - Age Groups
    - Archetypes
    - Compass Axes
    - Echo Types
    - Fantasy Themes
  - **Dropdown: "Users"** with 2 items:
    - Accounts
    - Profiles
  - Right side: Theme selector dropdown (Light/Dark/System) + Logout button
  - Responsive: Collapses to hamburger menu on mobile
- **Call-to-action:** Navigation links; Logout button

---

## FOOTER (Layout — Global Footer)

- **Primary purpose:** Display copyright information
- **Target audience:** All users
- **Key message:** Mystira ownership and branding
- **Current layout/structure:**
  - Simple centered text: copyright notice
- **Call-to-action:** None

---

## PAGE: Login (`/login`)

### Section: Login Card

- **Primary purpose:** Authenticate administrators before granting access to the admin dashboard
- **Target audience:** Platform administrators and content managers
- **Key message:** Secure access to content management tools
- **Current layout/structure:**
  - Centered login card on full-page layout
  - Username/password form with validation
  - Error alert for failed login attempts
  - Submit button with loading spinner during authentication
  - Redirects to `/admin` on successful login
- **Call-to-action:** "Sign In" button

---

## PAGE: Dashboard (`/admin`)

### Section: Dashboard Header

- **Primary purpose:** Welcome administrators and provide high-level content overview
- **Target audience:** Authenticated admins landing after login
- **Key message:** Quick visibility into platform content status
- **Current layout/structure:**
  - "Dashboard" page title
  - Refresh button to reload statistics
- **Call-to-action:** Refresh button

### Section: Stats Cards

- **Primary purpose:** Provide at-a-glance metrics for all content types
- **Target audience:** Admins monitoring content health
- **Key message:** Real-time counts across all content categories
- **Current layout/structure:**
  - Four stat cards in responsive grid (4 columns on XL, 2 on MD):
    1. **Scenarios** — count with colored left border (primary/blue)
    2. **Media** — count with colored left border (success/green)
    3. **Badges** — count with colored left border (info/cyan)
    4. **Bundles** — count with colored left border (warning/yellow)
  - Each card: icon + label + count
  - Loading state: Spinner
  - Error state: ErrorAlert with retry
- **Call-to-action:** None (informational; links are in the navbar)

---

## PAGE: Scenarios (`/admin/scenarios`)

### Section: Scenarios List

- **Primary purpose:** Browse, search, and manage all story scenarios in the platform
- **Target audience:** Content managers maintaining the story catalog
- **Key message:** Full control over scenario lifecycle — create, edit, import, validate, delete
- **Current layout/structure:**
  - **Header:** "Scenarios" title + action buttons:
    - "Create" (links to create page)
    - "Import" (links to import page)
    - "Validate" (links to validation page)
  - **SearchBar:** Text search with clear button
  - **Table card:** Responsive Bootstrap table with columns:
    - Title
    - Description (truncated)
    - Tags (badge pills)
    - Age Rating
    - Actions (Edit, Delete)
  - **Pagination:** Previous/Current/Next page controls
  - **Empty state:** "No scenarios found" with "Create a Scenario" button
  - **Delete confirmation:** ConfirmationDialog modal with danger styling
  - Loading/Error states via React Query
- **Call-to-action:** Create, Import, Validate buttons; Edit/Delete per row

### Sub-page: Create Scenario (`/admin/scenarios/create`)

- **Purpose:** Add a new scenario to the platform
- **Layout:** Form card with fields: Title (required), Description (textarea), Age Rating (number 0–18), Tags (comma-separated input). Cancel + Submit buttons.

### Sub-page: Edit Scenario (`/admin/scenarios/edit/:id`)

- **Purpose:** Modify an existing scenario's metadata
- **Layout:** Same form as Create, pre-populated with existing data

### Sub-page: Import Scenario (`/admin/scenarios/import`)

- **Purpose:** Bulk import scenario data from a file
- **Layout:** File input + file info display + Upload button with loading state

### Sub-page: Validate Scenarios (`/admin/scenarios/validate`)

- **Purpose:** Validate scenario media references to ensure all linked media exists
- **Layout:** "Run Validation" button + accordion-based results display showing per-scenario validation status with pass/fail indicators

---

## PAGE: Media (`/admin/media`)

### Section: Media Library

- **Primary purpose:** Manage all media assets (images, audio, video) used across scenarios
- **Target audience:** Content managers maintaining media library
- **Key message:** Upload, search, download, and clean up media files
- **Current layout/structure:**
  - **Header:** "Media" title + "Import" and "Import ZIP" buttons
  - **SearchBar:** Text search for media files
  - **Table card:** Responsive table with columns:
    - Filename
    - Content Type (MIME type)
    - Size
    - Actions (Download, Delete)
  - **Pagination:** Page navigation controls
  - **Delete confirmation:** Modal dialog
- **Call-to-action:** Import, Import ZIP buttons; Download/Delete per row

### Sub-page: Import Media (`/admin/media/import`)

- **Purpose:** Upload individual media files
- **Layout:** File input with file info display (name, size) + Upload button

### Sub-page: Import Media ZIP (`/admin/media/import-zip`)

- **Purpose:** Bulk upload media via ZIP archive
- **Layout:** ZIP file input + Upload button with progress indication

---

## PAGE: Badges (`/admin/badges`)

### Section: Badges Management

- **Primary purpose:** Manage achievement badges that players earn through gameplay
- **Target audience:** Content designers creating the achievement system
- **Key message:** Design and maintain the badge/achievement catalog
- **Current layout/structure:**
  - **Header:** "Badges" title + "Create" and "Import" buttons
  - **SearchBar:** Badge search
  - **Table card:** Badge listing with name, description, tier, actions
  - **Pagination:** Page controls
  - **Delete confirmation:** Modal dialog
- **Call-to-action:** Create, Import buttons; Edit/Delete per badge

### Sub-page: Create Badge (`/admin/badges/create`)

- **Purpose:** Design a new achievement badge
- **Layout:** Form with badge metadata fields + Save/Cancel

### Sub-page: Edit Badge (`/admin/badges/edit/:id`)

- **Purpose:** Modify existing badge properties
- **Layout:** Pre-populated form matching Create layout

### Sub-page: Import Badge (`/admin/badges/import`)

- **Purpose:** Upload badge image asset
- **Layout:** Image file input with preview + Upload button

---

## PAGE: Bundles (`/admin/bundles`)

### Section: Bundles Management

- **Primary purpose:** Group scenarios into content bundles for organized discovery
- **Target audience:** Content strategists organizing the adventure catalog
- **Key message:** Curate adventure collections by theme, age group, or series
- **Current layout/structure:**
  - **Header:** "Bundles" title + "Create" and "Import" buttons
  - **SearchBar:** Bundle search
  - **Table card:** Bundle listing with title, description, scenario count, actions
  - **Pagination:** Page controls
  - **Delete confirmation:** Modal dialog
- **Call-to-action:** Create, Import buttons; Edit/Delete per bundle

### Sub-pages: Create/Edit/Import

- Same CRUD pattern as Scenarios and Badges

---

## PAGE: Character Maps (`/admin/character-maps`)

### Section: Character Maps Management

- **Primary purpose:** Define character role mappings for scenarios
- **Target audience:** Content designers setting up scenario character configurations
- **Key message:** Map character definitions to scenario requirements
- **Current layout/structure:**
  - **Header:** "Character Maps" title + "Create" and "Import" buttons
  - **SearchBar:** Search functionality
  - **Table card:** Character map listing with name, description, character count, actions
  - **Pagination:** Page controls
- **Call-to-action:** Create, Import buttons; Edit/Delete per entry

### Sub-pages: Create/Edit/Import

- Same CRUD pattern as other content types

---

## PAGE: Master Data (`/admin/master-data/:type`)

### Section: Master Data Management

- **Primary purpose:** Manage reference data used across the platform (age groups, archetypes, etc.)
- **Target audience:** Platform administrators configuring system-level data
- **Key message:** Central management for all configurable reference data
- **Current layout/structure:**
  - Dynamic page that adapts to 5 master data types:
    1. **Age Groups** — Age range definitions for content rating
    2. **Archetypes** — Character archetype definitions
    3. **Compass Axes** — Developmental compass axis definitions (e.g., empathy, courage)
    4. **Echo Types** — Echo type classifications
    5. **Fantasy Themes** — Theme categories for scenarios
  - Each type uses the same generic CRUD interface:
    - Header with type-specific title and icon
    - SearchBar
    - Table with type-specific columns
    - Create/Edit/Delete operations
    - Pagination
- **Call-to-action:** Create button; Edit/Delete per entry

---

## PAGE: Accounts (`/admin/accounts`)

### Section: User Accounts Table (Read-Only)

- **Primary purpose:** View all registered user accounts for monitoring and support
- **Target audience:** Administrators reviewing user registrations
- **Key message:** Visibility into platform user base — read-only for security
- **Current layout/structure:**
  - **Header:** "Accounts" title
  - **Table card:** Read-only table with columns:
    - Account ID
    - Email address
    - Display Name
    - Account Status
    - Created Date
    - Last Login Date
  - **Pagination:** Page navigation
  - No edit/delete actions (read-only by design)
- **Call-to-action:** None (observation only)

---

## PAGE: Profiles (`/admin/profiles`)

### Section: User Profiles Table (Read-Only)

- **Primary purpose:** View all player profiles across the platform
- **Target audience:** Administrators reviewing profile data for support
- **Key message:** Visibility into player profiles — read-only for privacy
- **Current layout/structure:**
  - **Header:** "Profiles" title
  - **Table card:** Read-only table with columns:
    - Profile ID
    - Display Name
    - Age Group
    - Avatar
    - Created Date
    - Updated Date
  - **Pagination:** Page navigation
  - No edit/delete actions (read-only by design)
- **Call-to-action:** None (observation only)

---

## PAGE: Avatars (`/admin/avatars`)

### Section: Avatar Configuration

- **Primary purpose:** Configure available avatars per age group
- **Target audience:** Content managers curating the avatar selection
- **Key message:** Manage which avatars are available to players in each age group
- **Current layout/structure:**
  - **Header:** "Avatars" title
  - **Accordion interface:** One expandable section per age group
    - Each section shows:
      - Age group name and avatar count
      - List of avatar media IDs
      - "Add Avatar" input with media ID field
      - "Remove" button per avatar
  - Loading/Error states
- **Call-to-action:** Add/Remove avatar buttons per age group

---

## PAGE: Not Found (404)

### Section: 404 Error State

- **Primary purpose:** Handle navigation to non-existent admin pages
- **Target audience:** Users who hit invalid URLs
- **Key message:** "Page Not Found"
- **Current layout/structure:**
  - NotFoundPage component with message and "Go to Dashboard" button
- **Call-to-action:** "Go to Dashboard" button

---

## STYLING ARCHITECTURE

### Theme System

| Property | Light | Dark |
|----------|-------|------|
| Background | `#f8f9fa` | `#1a1d21` |
| Card background | `#ffffff` | `#212529` |
| Text | `#212529` | `#e9ecef` |
| Primary | `#4e73df` | `#4e73df` |
| Success | `#1cc88a` | `#1cc88a` |
| Info | `#36b9cc` | `#36b9cc` |
| Warning | `#f6c23e` | `#f6c23e` |

Theme switching: JavaScript-based via `data-bs-theme` attribute with system preference detection and localStorage persistence.

### CSS Architecture

- **Framework:** Bootstrap 5.3.8 with responsive grid and utility classes
- **Custom:** `admin.css` with CSS custom properties for admin-specific theming
- **Icons:** Bootstrap Icons 1.13.1 (`bi-*` classes)
- **Transitions:** Smooth 0.3s transitions for theme switching
- **Card hover:** Transform + shadow elevation on hover

---

## COMPONENT INVENTORY

### Layout Components

`Layout` (navbar + footer), `ThemeSelector`, `ProtectedRoute`, `ErrorBoundary`

### Form Components

`TextInput`, `NumberInput`, `Textarea`, `Checkbox`, `FileInput`, `FormField`

### Data Display Components

`Pagination`, `SearchBar`, `Card`, `ValidationResults`

### Feedback Components

`LoadingSpinner` (sm/md/lg), `ConfirmationDialog` (danger/warning/info), `Alert`, `ErrorAlert`, `ErrorDisplay`

### Page Components (24)

Dashboard, Login, Scenarios (list/create/edit/import/validate), Media (list/import/import-zip), Badges (list/create/edit/import), Bundles (list/create/edit/import), Character Maps (list/create/edit/import), Master Data, Accounts, Profiles, Avatars, NotFound, Error

---

## STATE MANAGEMENT

| Layer | Technology | Purpose |
|-------|-----------|---------|
| Auth | Zustand + localStorage | Token and authentication state |
| Theme | React Context | Light/Dark/System theme preference |
| Server | React Query v5 | API data fetching, caching, invalidation |
| Forms | React Hook Form + Zod | Form state and validation |

---

## API MODULES

| Module | Endpoints |
|--------|-----------|
| `admin.ts` | Dashboard statistics |
| `scenarios.ts` | Scenario CRUD + validation |
| `media.ts` | Media upload/download/delete |
| `badges.ts` | Badge CRUD |
| `bundles.ts` | Bundle CRUD |
| `avatars.ts` | Avatar config per age group |
| `characterMaps.ts` | Character map CRUD |
| `masterData.ts` | Generic master data CRUD |
| `accounts.ts` | User accounts + profiles (read-only) |
| `client.ts` | Axios client with interceptors |
