# Mystira App (PWA) — Website Sections Inventory

> Consumer-facing interactive storytelling platform for children, parents, and group leaders.
> Built with Blazor WebAssembly.
> Last updated: 2026-03-07

## Application Overview

| Property | Value |
|----------|-------|
| Technology | Blazor WebAssembly (C# / .NET) |
| Location | `packages/app/src/Mystira.App.PWA` |
| Hosting | Azure Static Web App |
| Styling | Bootstrap 5 + Custom CSS variables + Scoped component CSS |
| State management | Service-based (DI) + EventCallback + localStorage |
| Authentication | Microsoft Entra SSO + Magic Link (passwordless) |
| PWA support | Full (manifest, service worker, install prompts, offline indicator) |

## Routes

`/`, `/home`, `/about`, `/adventures`, `/brand`, `/signin`, `/signup`, `/game`, `/character-assignment/{scenarioId}`, `/profiles`, `/achievements`, `/game/awards`, `/parent-dashboard`

**Authentication callbacks:** `/authentication/login-callback`, `/authentication/magic-verify`, `/authentication/magic-link-sent`

---

## HEADER (MainLayout — Global Navigation)

- **Primary purpose:** Provide consistent site-wide navigation and account management across all pages
- **Target audience:** All visitors (unauthenticated and authenticated)
- **Key message:** Mystira is an accessible, welcoming platform — easy to navigate and quick to get started
- **Current layout/structure:**
  - Fixed-top navbar with transparent background
  - Left: Dragon icon + "Mystira" brand link (links to `/`)
  - Right (collapsed on mobile via hamburger toggle):
    - Endpoint Switcher (dev tool)
    - Theme Toggle (dark/light mode)
    - "Home" link (always visible)
    - "Adventures" link (authenticated users only)
    - "About" link (always visible)
    - Active adventure indicator with scenario name (when a game session is active, links to `/game`)
    - **Authenticated state:** Profile dropdown menu with user display name containing:
      - Achievements link (`/achievements`)
      - Profiles link (`/profiles`)
      - PWA Install option
      - Settings button (opens modal)
      - Sign Out button
    - **Unauthenticated state:**
      - "Sign In" text link
      - "Get Started" CTA button (links to `/signup`)
- **Call-to-action:** "Get Started" button for unauthenticated users; profile menu for authenticated users

---

## FOOTER (MainLayout — Global Footer)

- **Primary purpose:** Display environment/connection status and app version information
- **Target audience:** All users; primarily useful for developers and support
- **Key message:** Transparency about app state and version
- **Current layout/structure:**
  - Minimal single-row footer with flex layout
  - Left: Environment badge (Development/Staging/Local — hidden in Production)
  - Right: Connection status ("Online"/"Offline") + version indicator with tooltip showing build date and commit SHA
- **Call-to-action:** None (informational only)

---

## GLOBAL OVERLAY COMPONENTS

These components appear on all pages via the MainLayout:

### Offline Indicator

- **Purpose:** Alert users when internet connectivity is lost
- **Target audience:** All users
- **Layout:** Top banner with WiFi icon and message: "You are currently offline. Some features may be limited."

### PWA Install Button

- **Purpose:** Encourage app installation for native-like experience
- **Target audience:** Users on supported browsers who haven't installed the PWA
- **Layout:** Floating button, visibility depends on authentication state and browser support

### Discord Widget

- **Purpose:** Provide community access and support channel
- **Target audience:** All users seeking community engagement
- **Layout:** Floating widget overlay

### Toast Container

- **Purpose:** Display transient success/error/info notifications
- **Target audience:** All users performing actions
- **Layout:** Stacked toast notifications

### Update Notification

- **Purpose:** Prompt users to refresh when a new version is available
- **Target audience:** All users with cached service worker
- **Layout:** Notification banner/prompt

### Environment Indicator

- **Purpose:** Visually distinguish non-production environments
- **Target audience:** Developers and testers
- **Layout:** Persistent badge for dev/staging environments

### Account Settings Modal

- **Purpose:** Allow users to manage account settings without leaving current page
- **Target audience:** Authenticated users
- **Layout:** Modal dialog triggered from profile dropdown

---

## PAGE: Home (`/`, `/home`)

### Section: Hero Section (HeroSection component)

- **Primary purpose:** Create an emotionally compelling first impression; establish brand identity and value proposition
- **Target audience:** New visitors (unauthenticated) and returning users (authenticated)
- **Key message:** "Where imagination meets growth" — Mystira transforms playtime into immersive, interactive adventures
- **Current layout/structure:**
  - Full-width centered section with particle canvas background (animated)
  - Golden light tube SVG animations (vine-like decorative elements radiating from center)
  - "Mystira" title with ALPHA badge
  - Tagline: "Where imagination meets growth"
  - Logo area with:
    - Auto-playing intro video (theme-aware: light/dark variants, loads in background)
    - Static logo image (shown before/after video)
    - SVG pulse animation overlay (plays after video ends)
    - Replay button, Skip button, "Watch Full Intro" button
  - Full intro video modal (expanded view with controls)
  - **Unauthenticated only:**
    - Description paragraph: "Bring stories to life with Mystira — an interactive adventure platform that helps children, parents, and group leaders explore imagination, teamwork, and growth through guided storytelling and play."
    - CTA buttons: "Get Started" (primary, links to `/signup`) + "Watch Demo (2 min)" (outline, triggers full intro video)
    - Trust indicators (commented out): "Safe for Kids", "No Ads", COPPA Compliance
- **Call-to-action:** "Get Started" (signup) and "Watch Demo" for unauthenticated users

### Section: Feature Cards (unauthenticated only)

- **Primary purpose:** Communicate the three core value pillars of Mystira at a glance
- **Target audience:** New visitors evaluating whether to sign up
- **Key message:** Mystira offers interactive adventures, collaborative play, and guided storytelling
- **Current layout/structure:**
  - Three-column card grid (responsive: stacked on mobile)
  - Each card has: icon (3x size), title, description
  - Card 1: **Interactive Adventures** — "Dive into guided quests that adapt to every group, helping facilitators lead unforgettable storytelling sessions with ease." (Compass icon, primary color)
  - Card 2: **Built for Teamwork** — "Foster empathy, cooperation, and problem-solving with collaborative prompts that keep every child engaged." (Users icon, green)
  - Card 3: **Guided Storytelling** — "Use curated visuals, music, and facilitator tips to spark imagination anywhere—from living rooms to after-school clubs." (Book icon, info/blue)
- **Call-to-action:** None (these cards inform; the hero CTA drives conversion)

### Section: Adventures Section (AdventuresSection component)

- **Primary purpose:** Browse, filter, and launch interactive story adventures
- **Target audience:** Authenticated users ready to play; also visible in preview mode for unauthenticated users
- **Key message:** A rich catalog of adventures organized by bundles and age groups
- **Current layout/structure:**
  - Loading indicator (spinner + "Loading adventures..." text)
  - **Filter Section** (FilterSection component):
    - Age group filter pills (1-2, 3-5, 6-9, 10-12, 13-18, 19+) with counts
    - Show/hide completed toggle
    - Result count label
    - Clear filters button
  - **Active Adventures** (visible when user has in-progress sessions):
    - Header: "Active Adventures" with count badge and show/hide toggle
    - Grid of ActiveSessionCard components showing scenario title, start time, "In Progress" status, and "Continue" button
  - **Content Bundles Grid** (when no bundle selected):
    - "Available Bundles" header
    - Featured bundle card (FeaturedBundleCard — first bundle, larger display with completion progress)
    - Regular bundle cards in grid (BundleCard — showing bundle title, description, age group, completion status)
  - **Scenarios Grid** (when a bundle is selected):
    - "Selected Bundle: [name]" header with "Back to Bundles" button
    - 4-column responsive grid of AdventureCard components (scenario title, themes/tags, completion indicator, difficulty)
  - **In-Progress Session Modal:** Confirmation dialog asking "Continue Adventure?" with Continue/Cancel buttons
- **Call-to-action:** "Start Adventure" on each card (navigates to character assignment); "Continue" on active sessions

---

## PAGE: About (`/about`)

### Section: About Header

- **Primary purpose:** Establish what Mystira is and why it exists
- **Target audience:** Parents, educators, and facilitators evaluating the platform
- **Key message:** "Where imagination meets growth" — Mystira transforms shared playtime into developmental adventures
- **Current layout/structure:**
  - Centered layout constrained to `col-lg-8`
  - Dragon icon (4x), "About Mystira" heading, "Where imagination meets growth" subheading
- **Call-to-action:** None

### Section: About Content Card

- **Primary purpose:** Communicate Mystira's philosophy, approach, and differentiators
- **Target audience:** Decision-makers (parents, teachers, group leaders) researching the platform
- **Key message:** Stories are research-grounded, collaborative, and beautifully crafted
- **Current layout/structure:**
  - Single elevated card with three content paragraphs:
    1. What Mystira does (storytelling for children/parents/leaders)
    2. Developmental foundation (child development research, empathy, problem-solving)
    3. How it works (curated visuals, music, guided prompts)
  - Closing tagline: "Mystira — Where imagination meets growth." (bold, primary color, border-top separator)
- **Call-to-action:** None

### Section: About Feature Cards

- **Primary purpose:** Reinforce key differentiators with concise visual summaries
- **Target audience:** Visitors scanning the page quickly
- **Key message:** Research-based, collaborative, beautifully crafted
- **Current layout/structure:**
  - Three-column card grid (same pattern as Home feature cards)
  - Card 1: **Research-Based** — "Grounded in child development research to support growth through play." (Microscope icon)
  - Card 2: **Collaborative** — "Designed to foster teamwork, empathy, and social-emotional learning." (Users icon)
  - Card 3: **Beautifully Crafted** — "Curated visuals, music, and prompts for immersive storytelling." (Palette icon)
- **Call-to-action:** "Back to Home" button below cards

---

## PAGE: Adventures (`/adventures`)

- **Primary purpose:** Dedicated page for adventure browsing and discovery
- **Target audience:** Authenticated users looking to start or continue adventures
- **Key message:** Full catalog access with filtering and sorting
- **Current layout/structure:** Renders the same `AdventuresSection` component documented under the Home page
- **Call-to-action:** Same as Home Adventures Section

---

## PAGE: Sign In (`/signin`)

### Section: Sign In Card

- **Primary purpose:** Authenticate returning users
- **Target audience:** Existing users with accounts
- **Key message:** "Welcome Back — Sign in to continue your adventure"
- **Current layout/structure:**
  - Centered auth card on full-page layout
  - **Auth Header:** Mystira logo image, "Welcome Back" title, "Sign in to continue your adventure" subtitle
  - **Error display:** Alert box for authentication errors
  - **Loading state:** Spinner with "Signing you in..." and "You'll be redirected shortly"
  - **Sign-in methods:**
    1. "Continue with Entra" button (Microsoft Entra SSO, primary method)
    2. "or" divider
    3. Magic link section: email input + "Send Link" button with helper text "We'll email you a magic link for instant sign-in"
  - **Security info:** Shield icon + "Your data is protected with enterprise-grade security"
  - **Divider text:** "Secure sign-in powered by Microsoft"
  - **Footer:** "Don't have an account? Get Started" link to `/signup`
  - **Back link:** "Back to Home" with arrow icon
- **Call-to-action:** "Continue with Entra" (primary) or "Send Link" (magic link)

---

## PAGE: Sign Up (`/signup`)

### Section: Sign Up Card

- **Primary purpose:** Convert new visitors into registered users
- **Target audience:** New visitors ready to create an account
- **Key message:** "Join Mystira — Start your magical adventure today"
- **Current layout/structure:**
  - Same centered auth card layout as Sign In
  - **Auth Header:** Logo, "Join Mystira" title, "Start your magical adventure today" subtitle
  - **Benefits list** (before sign-up buttons):
    - "Free to get started" (check icon)
    - "Interactive story adventures" (check icon)
    - "Safe for the whole family" (check icon)
  - **Sign-up methods:**
    1. "Continue with Entra" button
    2. "Or get a magic sign-in link" — email input + "Send Link"
  - **Security info:** Same as Sign In
  - **Footer:** "Already have an account? Sign In" link to `/signin`
  - **Back link:** "Back to Home"
- **Call-to-action:** "Continue with Entra" or "Send Link"

---

## PAGE: Character Assignment (`/character-assignment/{scenarioId}`)

### Section: Character Assignment Interface

- **Primary purpose:** Assign players (profiles) to character roles before starting an adventure
- **Target audience:** Authenticated users (facilitators/parents) setting up a game session
- **Key message:** Personalize the adventure experience by matching players to characters
- **Current layout/structure:**
  - Loading state: Spinner + "Preparing character assignment..."
  - Error state: Warning icon + "Scenario Not Found" with "Back to Adventures" button
  - **Main interface:**
    - Header with scenario title and back button
    - Character cards grid showing available characters from the scenario
    - Player assignment modal with tabs:
      - Profile Selection Tab (existing profiles)
      - New Profile Tab (create on the fly)
      - AI Player Tab (AI-controlled character option)
      - Guest Player Tab (temporary guest option)
    - Avatar carousel for avatar selection
    - Age group mismatch modal (warns when profile age doesn't match scenario age group)
    - Guest warning modal
    - Replay warning modal (for previously completed adventures)
    - "Start Adventure" button (navigates to game page)
- **Call-to-action:** "Start Adventure" button

---

## PAGE: Game Session (`/game`)

### Section: Game Session Interface

- **Primary purpose:** Deliver the core interactive storytelling experience
- **Target audience:** Players actively engaged in an adventure
- **Key message:** Immersive, guided storytelling with meaningful choices
- **Current layout/structure:**
  - **No active session state:** Warning icon, "No Active Adventure" message, "Go to Home" button
  - **Active session:**
    - **Game Header:** Scenario title + session controls (pause/resume, exit, settings)
    - **Scene Display Area:**
      - Scene media display (SceneMediaDisplay — images/video within FantasyMediaFrame)
      - Scene narrative text (rendered via MarkdownRenderer)
      - Character dialogue/narration
    - **Choice Section:**
      - ChoiceButtons component — interactive decision buttons representing branching story paths
      - Each choice shows text and may indicate consequences
    - **Dice Roller:** Optional dice-rolling utility for chance-based outcomes
    - **Session Info:** Progress tracking (scene count, choice count)
    - **Content Attribution:** Credits for media/story content
    - **Completion handling:** Navigates to awards page on adventure completion
- **Call-to-action:** Choice buttons that advance the story narrative

---

## PAGE: Profiles (`/profiles`)

### Section: Profiles Management Interface

- **Primary purpose:** Create, edit, and manage player profiles for adventures
- **Target audience:** Authenticated users (parents/facilitators) managing family/group profiles
- **Key message:** Easy profile management with age-appropriate avatar selection
- **Current layout/structure:**
  - Auth gate: Redirects to sign-in if not authenticated
  - **Header:** Back button + "Manage Profiles" title + "Create, edit, and manage player profiles" subtitle + "New Profile" button
  - **Success/Error messages:** Dismissible alert banners
  - **Empty state:** Icon + "No Profiles Yet" + "Create Your First Profile" button
  - **Profiles grid:** Responsive card layout (3 columns on large screens)
    - Each profile card shows:
      - Avatar image (or placeholder icon)
      - Guest badge (if guest profile)
      - Profile name
      - Age range badge
      - Created date
      - "Ready to play" indicator (if onboarding complete)
      - Edit and Delete action buttons
  - **Create/Edit Modal:**
    - Profile name input (required)
    - Age range select dropdown
    - Avatar selection via AvatarCarousel component
    - Save/Cancel buttons
  - **Delete Confirmation Modal:** Warning with irreversibility note
- **Call-to-action:** "New Profile" and "Create Your First Profile" buttons

---

## PAGE: Achievements (`/achievements`)

### Section: Achievements Display

- **Primary purpose:** Track and celebrate player progress through earned badges and tiers
- **Target audience:** Players (children) and their parents/facilitators reviewing growth
- **Key message:** Adventures lead to tangible achievement milestones across developmental axes
- **Current layout/structure:**
  - Auth gate with redirect to sign-in
  - **Profile selection grid** (when multiple profiles exist):
    - Profile cards with avatar, name, age range, and latest earned badges preview
  - **Achievements header:** Age-appropriate title ("Your Adventure Badges" for young children, "Achievements" for older), profile name, age group, and back/view toggle buttons
  - **View modes:**
    - **Simplified view:** Badge grid showing earned badges as clickable thumbnails; click opens modal with badge image, title, tier, description, earned date
    - **Advanced view:** Grouped by developmental axis (e.g., empathy, courage), each showing:
      - Axis name and tier count
      - Axis copy (positive/negative direction descriptions)
      - Tier cards (Bronze/Silver/Gold/Platinum/Diamond) with:
        - Badge image or medal icon
        - Tier label and name
        - Earned star indicator
        - Expandable description
        - Earned date or progress bar
  - **Empty state:** "No Badges Yet" + "Complete adventures to start earning achievements!" + "Continue Adventures" link
- **Call-to-action:** "Continue Adventures" link to discover more

---

## PAGE: Awards (`/game/awards`)

### Section: Post-Adventure Awards

- **Primary purpose:** Celebrate completion and present newly earned badges immediately after an adventure
- **Target audience:** Players who just finished an adventure
- **Key message:** Congratulations on your accomplishment; here are your rewards
- **Current layout/structure:**
  - Award trophy icon (4x)
  - **Badges earned:** "Congratulations!" heading + "You've earned new badges" message
  - **No badges earned:** "No badges earned this time" heading with encouraging message
  - Badge display: Centered flex grid of badge cards (image + name + profile name)
  - **Contextual alerts:**
    - Age group mismatch warning (explains why certain players didn't earn badges)
    - Already-played info (explains repeat play doesn't award badges)
    - General no-badge info (encouraging message about trying other adventures)
  - Link to Achievements page for full details
  - "Go Home" button
- **Call-to-action:** "Go Home" button; link to Achievements page

---

## PAGE: Parent Dashboard (`/parent-dashboard`)

### Section: Parental Consent Management

- **Primary purpose:** Manage COPPA parental consent for child profiles
- **Target audience:** Parents/guardians of child users
- **Key message:** Full control over your children's data and consent status
- **Current layout/structure:**
  - Auth gate with redirect to sign-in
  - **Header:** Back button + "Parent Dashboard" title + "Manage parental consent and review child profile status" subtitle
  - **Success/Error messages:** Dismissible alerts
  - **Empty state:** "No Child Profiles Found" with link to "Manage Profiles"
  - **Child profiles grid:** Cards for each child profile showing:
    - Profile header (avatar, name, age range)
    - Consent status badge (Verified/Pending/Expired/Revoked/Denied, color-coded)
    - Consent details message
    - Created date
    - Actions: "View Profile" and "Revoke Consent" (when verified)
  - **Revoke Consent Modal:** Confirmation dialog requiring email verification
- **Call-to-action:** "Revoke Consent" / "View Profile" per child; "Manage Profiles" when empty

---

## PAGE: Brand (`/brand`)

### Section: Brand Style Guide

- **Primary purpose:** Showcase Mystira's design system for internal reference and consistency
- **Target audience:** Designers, developers, and stakeholders reviewing brand guidelines
- **Key message:** "Magical choice-based storytelling for ages 3-12" — a narrative-driven gaming platform
- **Current layout/structure:**
  - Uses BrandLayout (minimal shell without standard header/footer)
  - **Brand Hero:**
    - "Mystira" title with Theme Toggle
    - Tagline: "Magical choice-based storytelling for ages 3-12."
    - Blurb: "A narrative-driven gaming platform with branching stories, moral compass growth, age-appropriate achievements, and character identity."
  - **Typography section** (TypographyShowcase component)
  - **Buttons section** (ButtonShowcase component)
  - **Status section** (StatusDemo component)
  - **Motion section** (MotionDemo component)
- **Call-to-action:** None (internal reference page)

---

## PAGE: Authentication Callbacks

### Magic Link Sent (`/authentication/magic-link-sent`)

- **Purpose:** Confirmation that magic link email was sent
- **Target audience:** Users who requested magic link sign-in
- **Message:** Check your email for the sign-in link

### Magic Link Verify (`/authentication/magic-verify`)

- **Purpose:** Handle magic link verification when user clicks the email link
- **Target audience:** Users clicking the magic link from their email

### Login Callback (`/authentication/logincallback`)

- **Purpose:** Handle OAuth redirect callback after Entra authentication
- **Target audience:** Users returning from Microsoft Entra sign-in flow

---

## STYLING ARCHITECTURE

### Theme System

| Property | Light | Dark |
|----------|-------|------|
| Background | `#f8fafc` | `#0f172a` |
| Foreground | `#111827` | `#f1f5f9` |
| Card | `rgba(255,255,255,0.85)` | `rgba(30,41,59,0.85)` |
| Navbar | `rgba(255,255,255,0.85)` | `rgba(15,23,42,0.95)` |
| Primary | `#7c3aed` | `#7c3aed` |

### Color Palette

| Token | Value | Usage |
|-------|-------|-------|
| `--primary-color` | `#7c3aed` | Primary brand purple |
| `--primary-hover` | `#6d28d9` | Hover state |
| `--success-color` | `#10B981` | Success states |
| `--danger-color` | `#EF4444` | Error/danger states |
| `--warning-color` | `#F59E0B` | Warning states |
| `--info-color` | `#3B82F6` | Info states |

### CSS Architecture

- **Global:** `wwwroot/css/app.css` (1,570 lines) — CSS variables, Bootstrap overrides, dark mode, animations
- **Brand:** `wwwroot/css/brand.css` — Brand-specific styles
- **Scoped:** 20+ `.razor.css` files for component isolation
- **Framework:** Bootstrap 5 utility classes as foundation

### Responsive Breakpoints

| Breakpoint | Width |
|------------|-------|
| Mobile | < 576px |
| Tablet | 576px–768px |
| Desktop | > 768px |

---

## COMPONENT INVENTORY

### Layout Components (9)

`MainLayout`, `BrandLayout`, `NavMenu`, `Footer`, `EnvironmentIndicator`, `EndpointSwitcher`, `ThemeToggle`, `PwaInstallButton`, `DiscordWidget`

### Page Components (15)

`Home`, `About`, `Adventures`, `SignIn`, `SignUp`, `CharacterAssignmentPage`, `GameSessionPage`, `ProfilesPage`, `AchievementsPage`, `AwardsPage`, `ParentDashboard`, `Brand`, `LoginCallback`, `MagicVerify`, `MagicLinkSent`

### Content Components (10+)

`AdventuresSection`, `AdventureCard`, `ActiveSessionCard`, `BundleCard`, `FeaturedBundleCard`, `FilterSection`, `HeroSection`, `FantasyMediaFrame`, `SceneMediaDisplay`, `MarkdownRenderer`

### Player/Character Components (5)

`CharacterCard`, `AvatarCarousel`, `PlayerAssignmentModal`, `AgeGroupMismatchModal`, `CoppaCompliancePill`

### Game Components (3)

`DiceRoller`, `ChoiceButtons`, `ContentAttribution`

### UI Components (10+)

`LoadingIndicator`, `LoadingExperience`, `SkeletonLoader`, `EmptyState`, `ToastContainer`, `ErrorBoundaryWrapper`, `ErrorNavigator`, `UpdateNotification`, `WarningModal`, `AccountSettingsModal`

### Brand Components (4)

`ButtonShowcase`, `TypographyShowcase`, `StatusDemo`, `MotionDemo`

---

## SERVICE LAYER

### Application Services

| Service | Purpose |
|---------|---------|
| `IAuthService` | Authentication (Entra + Magic Link) |
| `IGameSessionService` | Game session management |
| `IProfileService` | User profiles |
| `IAchievementsService` | Badge/achievement tracking |
| `ICharacterAssignmentService` | Character assignment |
| `IPlayerContextService` | Current player context |
| `ISettingsService` | App settings (localStorage) |
| `ToastService` | Toast notifications |
| `IImageCacheService` | Image caching |
| `ITelemetryService` | Application Insights tracking |

### API Clients (11, all with Polly v8 resilience)

`IScenarioApiClient`, `IGameSessionApiClient`, `IUserProfileApiClient`, `IMediaApiClient`, `IAvatarApiClient`, `IContentBundleApiClient`, `ICharacterApiClient`, `IDiscordApiClient`, `IBadgesApiClient`, `ICoppaApiClient`, `IMagicAuthApiClient`
