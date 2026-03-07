# Mystira Publisher — Website Sections Inventory

> Creator-facing story registration and attribution platform.
> Built with React 19 + TypeScript.
> Last updated: 2026-03-07

## Application Overview

| Property | Value |
|----------|-------|
| Technology | React 19 + TypeScript |
| Location | `packages/publisher` |
| Build tool | Vite |
| Styling | Custom CSS with comprehensive design tokens (`variables.css`) |
| State management | Zustand (auth + UI) + React Query (server state) |
| Authentication | Email/Password + Microsoft Entra JWT + Magic Link |
| Form validation | Zod + React Hook Form |
| HTTP client | Axios |

## Routes

**Public:** `/` (home), `/login`

**Protected:** `/dashboard`, `/stories`, `/stories/:id`, `/register`, `/audit`, `/open-roles`, `/role-requests`

---

## HEADER (HomePage — Public Header)

- **Primary purpose:** Provide navigation and conversion for the Publisher landing page
- **Target audience:** Unauthenticated visitors and returning users
- **Key message:** Mystira Publisher — professional tool for story registration
- **Current layout/structure:**
  - Left: "Mystira Publisher" logo text (links to `/`)
  - Center nav: "Home" link
  - Right:
    - Theme Toggle
    - **Authenticated:** "Dashboard" link + "Dashboard" button
    - **Unauthenticated:** "Sign In" text link + "Get Started" button
- **Call-to-action:** "Get Started" button for new visitors

---

## HEADER (Layout — Authenticated Header)

- **Primary purpose:** Provide navigation across all authenticated Publisher pages
- **Target audience:** Authenticated creators and contributors
- **Key message:** Full access to story management, registration, and audit tools
- **Current layout/structure:**
  - Left: "Mystira Publisher" logo (links to `/dashboard`)
  - Center nav: Dashboard, Stories, Open Roles, Role Requests, Register, Audit Trail (active state highlighted with `.header__link--active`)
  - Right: Theme Toggle + Notification Bell + User Avatar + User Name + "Sign Out" button (ghost variant)
  - **Sidebar nav** (`.app__sidebar`): Mirrors header nav links for responsive/mobile
  - Skip link for accessibility (`#main-content`)
- **Call-to-action:** Navigation links; Sign Out

---

## FOOTER

- **Note:** No dedicated footer component exists in the Publisher application.

---

## PAGE: Publisher Home (`/`)

### Section: Hero

- **Primary purpose:** Communicate the Publisher's core value proposition and drive sign-up
- **Target audience:** Story creators, authors, illustrators evaluating the platform
- **Key message:** "Register your creative stories on-chain with transparent attribution and royalty splits."
- **Current layout/structure:**
  - "Mystira Publisher" heading
  - Subtitle text describing on-chain registration with attribution
  - **Authenticated:** "Go to Dashboard" button
  - **Unauthenticated:** "Get Started" + "Learn More" buttons (both link to `/login`)
- **Call-to-action:** "Get Started" (primary, links to `/login`)

### Section: Features Grid

- **Primary purpose:** Explain why creators should use Mystira Publisher
- **Target audience:** Creators considering the platform
- **Key message:** Four key differentiators for transparent story registration
- **Current layout/structure:**
  - "Why Mystira Publisher?" heading
  - Four elevated cards in `.features__grid`:
    1. **Transparent Attribution** — "Every contributor is recognized with clear role assignments and royalty splits."
    2. **Consensus-Based** — "All contributors must approve before registration, ensuring fairness."
    3. **Immutable Records** — "On-chain registration creates permanent, verifiable proof of ownership."
    4. **Full Audit Trail** — "Every action is logged for complete transparency and legal compliance."
- **Call-to-action:** None (informational; hero CTA drives conversion)

---

## PAGE: Login (`/login`)

### Section: Login Card

- **Primary purpose:** Authenticate users with multiple sign-in methods
- **Target audience:** Existing users and new registrants
- **Key message:** "Welcome back to Mystira Publisher" — flexible authentication options
- **Current layout/structure:**
  - Centered login card
  - "Sign In" title + "Welcome back to Mystira Publisher" subtitle
  - **Auth method selector** (three toggle buttons):
    1. **Email & Password:** Email input (placeholder: "you@example.com") + Password input (placeholder: "Enter your password") + "Sign In" button
    2. **Microsoft Entra:** JWT Token textarea (placeholder: "Paste your JWT token from Entra", 4 rows) + Helper text with link to Mystira Identity Service + "Sign In with Entra" button
    3. **Magic Link:** Email input + "Send Magic Link" button + Helper text: "We'll send you a magic link that will instantly sign you in."
      - After sending: Success alert with email confirmation + "Send to different email" button + "I've clicked the link" button
  - Error alerts for login and magic link failures
  - Footer: "Don't have an account? Contact us" (mailto link)
- **Call-to-action:** "Sign In" / "Sign In with Entra" / "Send Magic Link"

---

## PAGE: Dashboard (`/dashboard`)

### Section: Dashboard Header

- **Primary purpose:** Welcome returning users and provide quick access to story registration
- **Target audience:** Authenticated creators
- **Key message:** "Here's what's happening with your stories"
- **Current layout/structure:**
  - "Welcome back, {user.name}" greeting
  - Subtitle: "Here's what's happening with your stories"
  - "+ Register New Story" button (size lg, links to `/register`)
- **Call-to-action:** "+ Register New Story"

### Section: Stats Cards

- **Primary purpose:** Provide at-a-glance metrics on story portfolio
- **Target audience:** Active creators tracking their registrations
- **Key message:** Quick visibility into registered, pending, and total story counts
- **Current layout/structure:**
  - Three stat cards in horizontal row (shown when total > 0):
    1. **Registered** — count with checkmark/document icon (primary color)
    2. **Pending** — count with question mark icon (warning color)
    3. **Total Stories** — count with book icon (info color)
- **Call-to-action:** None (informational)

### Section: Recent Stories

- **Primary purpose:** Quick access to recent story activity
- **Target audience:** Creators monitoring their stories
- **Key message:** Stay on top of your latest work
- **Current layout/structure:**
  - Card with "Recent Stories" header + "View All →" link (links to `/stories`)
  - List of up to 5 stories with:
    - Story title
    - Story summary
    - Status badge (color-coded: success=registered, warning=pending_approval, danger=rejected)
  - **Empty state:** "No stories yet" + "Get started by registering your first creative story on-chain." + "Register Your First Story" button (links to `/register`)
  - **Loading state:** Skeleton loader with 5 placeholders
- **Call-to-action:** Story links; "Register Your First Story" (empty state); "View All →"

### Section: Quick Actions

- **Primary purpose:** Provide shortcuts to common workflows
- **Target audience:** Active users looking for fast navigation
- **Key message:** Common tasks are always one click away
- **Current layout/structure:**
  - Card with "Quick Actions" header
  - Three action links (outline buttons, full width):
    1. "Start Registration" (plus icon, links to `/register`)
    2. "View All Stories" (book icon, links to `/stories`)
    3. "Audit Trail" (document icon, links to `/audit`)
- **Call-to-action:** Each action navigates to its respective page

---

## PAGE: Stories (`/stories`)

### Section: Stories List with Filters

- **Primary purpose:** Browse, search, and filter all stories
- **Target audience:** Creators managing their story portfolio
- **Key message:** Full control and visibility over all your stories
- **Current layout/structure:**
  - **Header:** "Stories" title + "Register New Story" button (links to `/register`)
  - **Filters row** (`.stories-filters`):
    - Search input (placeholder: "Search stories...", 300ms debounce)
    - Status dropdown: All Statuses / Draft / Pending Approval / Approved / Registered / Rejected
  - **Stories grid** (`.stories-grid`): Cards showing:
    - Story title + status badge (color-coded variant)
    - Summary text
    - Contributor count + last updated date
  - **Empty state:** "No stories found" + "Create your first story registration or adjust your search filters." + "Register a Story" button
  - **Loading state:** Skeleton loader with 6 card placeholders
- **Call-to-action:** "Register New Story"; individual story cards link to detail view

---

## PAGE: Story Detail (`/stories/:id`)

### Section: Story Header

- **Primary purpose:** Identify the story and its current status
- **Target audience:** Story owners and contributors
- **Current layout/structure:**
  - "Back to Stories" link
  - Story title + status badge (color-coded)
  - "Continue Registration" button (shown only for draft stories, links to `/register?story={id}`)

### Section: Story Details Card

- **Primary purpose:** Display story metadata
- **Target audience:** Story stakeholders
- **Current layout/structure:**
  - "Details" card title
  - Summary paragraph
  - Metadata definition list:
    - Created (formatted datetime)
    - Last Updated (formatted datetime)
    - Registered (formatted datetime, if applicable)
    - Transaction ID (code block, if applicable)

### Section: Contributors List

- **Primary purpose:** Show all contributors with their roles and royalty splits
- **Target audience:** Story owners reviewing attribution
- **Current layout/structure:**
  - "Contributors" card title
  - ContributorList component showing: user, role, split percentage, approval status

### Section: Open Roles Manager

- **Primary purpose:** Create and manage open contributor positions
- **Target audience:** Story owners seeking collaborators
- **Current layout/structure:**
  - "Open Roles" section title
  - OpenRoleManager component with error boundary

### Section: Role Requests

- **Primary purpose:** Review applications from potential contributors
- **Target audience:** Story owners evaluating candidates
- **Current layout/structure:**
  - "Role Requests" card title
  - RoleRequestList component with error boundary

### Section: Approval Panel

- **Primary purpose:** Facilitate consensus-based approval for registration
- **Target audience:** Contributors who need to approve before on-chain registration
- **Current layout/structure:**
  - ApprovalPanel component (shown only when authenticated AND story status === "pending_approval")

### Section: Activity Sidebar

- **Primary purpose:** Show recent audit trail activity for this story
- **Target audience:** All story stakeholders
- **Current layout/structure:**
  - "Activity" card title in right sidebar column
  - AuditLogList component showing latest 10 audit log entries
  - Loading spinner during data fetch

---

## PAGE: Register (`/register`)

### Section: Registration Wizard

- **Primary purpose:** Guide users through multi-step story registration on blockchain
- **Target audience:** Creators ready to register a new story
- **Key message:** "Follow the steps below to register your story on-chain with transparent attribution"
- **Current layout/structure:**
  - **Header:** "Register Story" title + subtitle
  - RegistrationWizard component (multi-step form):
    - Story creation/selection (StoryForm / StoryPicker)
    - Contributor and role assignment
    - Royalty split negotiation
    - Review and submission
  - RegistrationStatus component for progress tracking
- **Call-to-action:** Wizard step navigation; final submit for on-chain registration

---

## PAGE: Audit Trail (`/audit`)

### Section: Audit Trail Interface

- **Primary purpose:** Provide complete, immutable activity history for compliance and transparency
- **Target audience:** Story owners, contributors, legal teams needing audit records
- **Key message:** "Complete history of all actions across registered stories"
- **Current layout/structure:**
  - **Header:** "Audit Trail" title + "Complete history of all actions across registered stories" subtitle
  - **Action buttons:**
    - "Show Filters" / "Hide Filters" toggle (outline, badge indicator when filters active)
    - "Export" button (outline, download icon) — exports audit logs to file
  - **Filters card** (collapsible): AuditLogFilters component with event type and date range controls
  - **Activity Log card:**
    - Header: "Activity Log" + "{count} event(s)" or "Loading..."
    - AuditLogList component (scrollable log entries)
  - **Detail overlay:** AuditLogDetail component (opens on log entry selection, closeable)
- **Call-to-action:** Export button; filter controls; log entry selection

---

## PAGE: Open Roles (`/open-roles`)

### Section: Open Roles Browser

- **Primary purpose:** Browse available contributor positions across all stories
- **Target audience:** Creators looking to contribute (illustrators, editors, co-authors)
- **Key message:** Find stories that need your skills
- **Current layout/structure:**
  - OpenRolesBrowser component showing available positions with story context, role type, and apply action
- **Call-to-action:** Apply to open roles

---

## PAGE: Role Requests (`/role-requests`)

### Section: Role Requests Management

- **Primary purpose:** Review and respond to contributor applications
- **Target audience:** Story owners evaluating incoming role requests
- **Key message:** "Review and respond to contributor applications"
- **Current layout/structure:**
  - Card with "Role Requests" header + "Review and respond to contributor applications" subtitle
  - RoleRequestList component with optional storyId filter (from query params)
  - Approve/reject actions per request
- **Call-to-action:** Approve/reject role request actions

---

## PAGE: Not Found (404)

### Section: 404 Error State

- **Primary purpose:** Handle navigation to non-existent pages
- **Target audience:** Users who hit broken/invalid URLs
- **Key message:** Page not found
- **Current layout/structure:**
  - EmptyState component with title, description, and "Go to Home" button
- **Call-to-action:** "Go to Home" button

---

## STYLING ARCHITECTURE

### Design Token System (variables.css)

The Publisher has the most comprehensive design token system in the Mystira platform.

#### Color Palette

| Token | Value | Purpose |
|-------|-------|---------|
| `--color-primary-500` | `#9333ea` | Primary brand purple |
| `--color-primary-600` | `#7e22ce` | Primary hover/active |
| `--color-neutral-50`–`900` | Full grayscale | UI elements |
| `--color-success` | `#10b981` | Success states |
| `--color-warning` | `#f59e0b` | Warning states |
| `--color-danger` | `#ef4444` | Error/danger states |
| `--color-info` | `#3b82f6` | Info states |

#### Theme Variables

| Property | Light | Dark |
|----------|-------|------|
| `--color-bg` | `#ffffff` | `#1a0b2e` |
| `--color-bg-secondary` | `#fafafa` | `#241538` |
| `--color-text` | `#18181b` | `#f4f4f5` |
| `--color-text-secondary` | `#52525b` | `#c9a8e0` |
| `--color-border` | `#e4e4e7` | `#3d2a54` |

#### Typography

- **Sans-serif:** `'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif`
- **Monospace:** `'Fira Code', 'Consolas', monospace`
- **Sizes:** xs (0.75rem) through 4xl (2.25rem) — 8-step scale
- **Weights:** normal (400), medium (500), semibold (600), bold (700)

#### Spacing

11-point scale from `0` to `4rem` (0, 0.25, 0.5, 0.75, 1, 1.25, 1.5, 2, 2.5, 3, 4rem)

#### Border Radius

sm (0.25rem), md (0.375rem), lg (0.5rem), xl (0.75rem), full (9999px)

#### Shadows

4 levels: sm, md, lg, xl — dark mode shadows use higher opacity for contrast

#### Transitions

fast (150ms), base (200ms), slow (300ms) — all with ease timing

#### Z-Index Scale

dropdown (100), sticky (200), modal (300), tooltip (400), toast (500)

#### Layout Constants

container-max: 1200px, sidebar-width: 260px, header-height: 64px

---

## STATE MANAGEMENT

### Auth Store (Zustand)

- `user`, `isAuthenticated`
- Actions: `setUser()`, `clearUser()`
- Persisted to localStorage ("auth-storage")

### UI Store (Zustand)

- `sidebarOpen`, `notifications[]`, `theme` (light/dark/system), `effectiveTheme`
- Actions: `toggleSidebar()`, `setTheme()`, `toggleTheme()`, `addNotification()`, `removeNotification()`
- System theme detection via `prefers-color-scheme`
- Persisted to localStorage ("ui-storage", theme only)

---

## COMPONENT INVENTORY

### Core UI Components

`Button` (primary/outline/ghost), `Input`, `Select`, `Card` (CardBody/CardHeader), `Badge`, `Alert`, `Avatar`, `Modal`, `Spinner`, `SkeletonLoader`, `EmptyState`, `Toast`/`ToastContainer`, `ThemeToggle`, `SkipLink`, `ErrorBoundary`, `FeatureErrorBoundary`

### Feature Modules

| Feature | Components |
|---------|-----------|
| Registration | `RegistrationWizard`, `StoryForm`, `StoryPicker`, `RegistrationStatus` |
| Contributors | `ContributorList`, `OpenRoleManager`, `OpenRolesBrowser`, `RoleRequestList`, `ApprovalPanel` |
| Audit Trail | `AuditLogList`, `AuditLogFilters`, `AuditLogDetail` |
| Notifications | `NotificationBell` |

---

## CSS FILES

`variables.css`, `index.css`, `layout.css`, `pages.css`, `components.css`, `typography.css`, `registration.css`, `open-roles.css`, `audit.css`, `notifications.css`, `toast.css`, `skeleton.css`, `theme-toggle.css`, `utilities.css`, `reset.css`, `skip-link.css`
