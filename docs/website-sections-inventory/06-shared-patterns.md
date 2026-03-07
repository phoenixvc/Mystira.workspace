# Cross-Application Shared Patterns

> Common UI patterns, states, and conventions used across all Mystira applications.
> Last updated: 2026-03-07

## Overview

Despite different technology stacks (Blazor WASM, React, Tauri), Mystira applications share recurring UI patterns for consistency and user familiarity.

---

## REPEATED PATTERN: Auth Gate

- **Appears in:** App PWA (Profiles, Achievements, Awards, Parent Dashboard), Publisher (all authenticated routes), Admin UI (all routes except Login), Story Generator (all routes except Login/Home), DevHub (all routes)
- **Purpose:** Redirect or display sign-in prompt when accessing authenticated content without a session
- **Layout variants:**
  - **Redirect:** Automatic navigation to login page (Admin UI, Publisher)
  - **Inline prompt:** Lock icon + "Authentication Required" heading + "Please sign in" message + "Sign In" button (App PWA)
  - **Token entry:** JWT token paste form (Story Generator)

---

## REPEATED PATTERN: Loading State

- **Appears in:** Every data-driven page across all applications
- **Purpose:** Indicate data is being fetched from the API
- **Layout variants:**
  - **App PWA:** Spinner component with contextual message (e.g., "Loading adventures...", "Loading profiles...")
  - **Publisher:** Skeleton loaders (card-shaped placeholders)
  - **Admin UI:** LoadingSpinner component (sm/md/lg sizes) with optional message
  - **Story Generator:** Spinner with loading text
  - **DevHub:** Loading indicators per panel

---

## REPEATED PATTERN: Empty State

- **Appears in:** Adventures (no bundles), Profiles (no profiles), Achievements (no badges), Stories (no results), Dashboard (no stories), Media Library, Badge Library, Story Library
- **Purpose:** Guide users toward the next action when no content exists
- **Layout:**
  - Large icon (contextual — book, trophy, compass, etc.)
  - Title describing the empty state
  - Description with encouraging next-step message
  - Primary action button (e.g., "Create Your First Profile", "Register Your First Story")
- **Design principle:** Never leave users on a dead-end screen — always provide a path forward

---

## REPEATED PATTERN: Error State

- **Appears in:** Most data-driven pages across all applications
- **Purpose:** Communicate failures and offer recovery options
- **Layout variants:**
  - **Alert banner:** Warning icon + error message + retry button (Admin UI: ErrorAlert)
  - **Inline message:** Error text below the failing section
  - **Error boundary:** Full-page error with error details and recovery navigation (App PWA: ErrorBoundaryWrapper, Admin UI: ErrorBoundary)
  - **Toast notification:** Transient error message (App PWA: ToastContainer, Publisher: react-hot-toast)

---

## REPEATED PATTERN: Confirmation Dialog

- **Appears in:** Delete operations across all applications
- **Purpose:** Prevent accidental destructive actions
- **Layout:**
  - Modal overlay with centered card
  - Warning icon or danger styling
  - Description of the action and its consequences
  - Cancel + Confirm buttons (confirm button styled as danger/red)
- **Variants:**
  - **Admin UI:** ConfirmationDialog component (danger/warning/info variants)
  - **App PWA:** WarningModal, ReplayWarningModal, GuestWarningModal
  - **Publisher:** Confirmation prompts for story actions

---

## REPEATED PATTERN: Search + Filter

- **Appears in:** Adventures (App PWA), Stories (Publisher), Scenarios/Media/Badges (Admin UI), Story Library (Story Generator)
- **Purpose:** Help users find specific content in lists
- **Layout variants:**
  - **Search bar:** Text input with clear/reset button
  - **Filter pills:** Clickable category chips (age groups in App PWA)
  - **Dropdown filters:** Status/type selectors (Publisher, Admin UI)
  - **Combined:** Search + filter chips + result count (App PWA FilterSection)

---

## REPEATED PATTERN: Card Grid

- **Appears in:** Adventures, Profiles, Achievements (App PWA), Stories (Publisher), Dashboard stats (Admin UI), Story Library (Story Generator)
- **Purpose:** Display collections of items in a scannable, responsive grid
- **Layout:**
  - Responsive grid (CSS Grid or Bootstrap columns)
  - Cards with consistent structure: image/icon → title → metadata → action
  - Hover effects (lift, shadow, glow)
  - Click targets for navigation or selection

---

## REPEATED PATTERN: CRUD Table

- **Appears in:** All Admin UI content pages (Scenarios, Media, Badges, Bundles, Character Maps, Master Data, Accounts, Profiles)
- **Purpose:** Tabular data display with management actions
- **Layout:**
  - Header with title + action buttons (Create, Import)
  - Search bar
  - Responsive table with sortable columns
  - Row actions (Edit, Delete, Download)
  - Pagination controls (Previous / Page N / Next)
  - Empty state when no results

---

## REPEATED PATTERN: Form Card

- **Appears in:** Create/Edit pages (Admin UI), Registration Wizard (Publisher), Sign In/Sign Up (App PWA, Publisher), Login (Story Generator)
- **Purpose:** Collect user input with validation feedback
- **Layout:**
  - Card container with title and subtitle
  - Form fields with labels, validation, and help text
  - Required field indicators
  - Submit button with loading spinner during async operations
  - Cancel button or back navigation
  - Success/error feedback after submission

---

## REPEATED PATTERN: Theme Toggle

- **Appears in:** App PWA, Publisher, Admin UI
- **Purpose:** Allow users to switch between light and dark color modes
- **Implementation variants:**
  - **App PWA:** ThemeToggle component (sun/moon icon button) using CSS variables + `data-theme` attribute + localStorage
  - **Publisher:** Theme toggle with CSS variables + localStorage
  - **Admin UI:** ThemeSelector dropdown (Light/Dark/System) using Bootstrap's `data-bs-theme` + localStorage + system preference detection
- **Common:** All persist preference in localStorage and support system preference detection

---

## REPEATED PATTERN: Pagination

- **Appears in:** Admin UI (all list pages), App PWA (adventure grids), Publisher (stories list)
- **Purpose:** Navigate through large data sets
- **Layout:**
  - Previous / Current Page / Next controls
  - Page size typically 20 items (Admin UI) or variable
  - Disabled state for first/last page boundaries

---

## REPEATED PATTERN: Status Badge

- **Appears in:** Consent status (App PWA), Story status (Publisher), Age group/rarity (App PWA), Achievement tiers (App PWA)
- **Purpose:** Visually communicate item state at a glance
- **Layout:**
  - Small pill/badge with color-coded background
  - Text label describing the status
  - Color mapping: green (success/verified), yellow (pending/warning), red (error/denied), blue (info), gray (neutral)

---

## TECHNOLOGY COMPARISON

| Pattern | App PWA | Publisher | Admin UI | Story Generator | DevHub |
|---------|---------|-----------|----------|-----------------|--------|
| Framework | Blazor WASM | React 19 | React 19 | Blazor WASM | Tauri + React |
| CSS | Bootstrap 5 + Custom | Custom CSS vars | Bootstrap 5 + Custom | Tailwind CSS | Tailwind CSS |
| State | Services (DI) | Zustand | Zustand + React Query | Services (DI) | Zustand |
| Forms | DataAnnotations | Zod + RHF | Zod + RHF | Manual binding | Manual/Zod |
| HTTP | HttpClient + Polly | Axios | Axios | HttpClient | Tauri IPC |
| Auth | Entra + Magic Link | Entra + Magic Link | MSAL + Password | JWT token | JWT token |
| Theme | CSS vars + data-theme | CSS vars | Bootstrap data-bs-theme | Component CSS | Tailwind dark: |
| Icons | Bootstrap Icons | Custom SVG | Bootstrap Icons | Emoji icons | Lucide/Custom |

---

## DESIGN SYSTEM ALIGNMENT STATUS

| Application | Token System | Brand Alignment | Consistency |
|-------------|-------------|-----------------|-------------|
| App PWA | Partial (app.css vars) | Strong (primary purple) | Good |
| Publisher | Comprehensive (variables.css) | Strong (purple palette) | Excellent |
| Admin UI | Bootstrap defaults + custom | Moderate (blue primary) | Good |
| Story Generator | Tailwind defaults | Weak (blue/purple gradient) | Fair |
| DevHub | Tailwind defaults | Weak (sky blue) | Fair |

### Recommended Alignment

All applications should migrate to the unified `@mystira/design-tokens` package to ensure consistent brand expression. See [Phase 1A Design Tokens](../design/06-phase-1a-design-tokens.md) for the canonical token specification.
