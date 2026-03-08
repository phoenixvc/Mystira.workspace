# Mystira.app Website Sections Inventory

> Comprehensive documentation of every section across the Mystira platform.
> Last updated: 2026-03-07

Mystira.app comprises two primary web applications:

1. **Mystira App (PWA)** — The consumer-facing interactive storytelling platform for children, parents, and group leaders. Built with Blazor WebAssembly.
2. **Mystira Publisher** — The creator-facing story registration and attribution platform. Built with React + TypeScript.

---

## APPLICATION 1: MYSTIRA APP (PWA)

Routes: `/`, `/home`, `/about`, `/adventures`, `/brand`, `/signin`, `/signup`, `/game`, `/character-assignment/{scenarioId}`, `/profiles`, `/achievements`, `/game/awards`, `/parent-dashboard`

---

### HEADER (MainLayout — Global Navigation)

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

### FOOTER (MainLayout — Global Footer)

- **Primary purpose:** Display environment/connection status and app version information
- **Target audience:** All users; primarily useful for developers and support
- **Key message:** Transparency about app state and version
- **Current layout/structure:**
  - Minimal single-row footer with flex layout
  - Left: Environment badge (Development/Staging/Local — hidden in Production)
  - Right: Connection status ("Online"/"Offline") + version indicator with tooltip showing build date and commit SHA
- **Call-to-action:** None (informational only)

---

### GLOBAL OVERLAY COMPONENTS

These components appear on all pages via the MainLayout:

#### Offline Indicator
- **Purpose:** Alert users when internet connectivity is lost
- **Target audience:** All users
- **Layout:** Top banner with WiFi icon and message: "You are currently offline. Some features may be limited."

#### PWA Install Button
- **Purpose:** Encourage app installation for native-like experience
- **Target audience:** Users on supported browsers who haven't installed the PWA
- **Layout:** Floating button, visibility depends on authentication state and browser support

#### Discord Widget
- **Purpose:** Provide community access and support channel
- **Target audience:** All users seeking community engagement
- **Layout:** Floating widget overlay

#### Toast Container
- **Purpose:** Display transient success/error/info notifications
- **Target audience:** All users performing actions
- **Layout:** Stacked toast notifications

#### Update Notification
- **Purpose:** Prompt users to refresh when a new version is available
- **Target audience:** All users with cached service worker
- **Layout:** Notification banner/prompt

#### Environment Indicator
- **Purpose:** Visually distinguish non-production environments
- **Target audience:** Developers and testers
- **Layout:** Persistent badge for dev/staging environments

#### Account Settings Modal
- **Purpose:** Allow users to manage account settings without leaving current page
- **Target audience:** Authenticated users
- **Layout:** Modal dialog triggered from profile dropdown

---

### PAGE: Home (`/`, `/home`)

#### Section: Hero Section (HeroSection component)

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

#### Section: Feature Cards (Home page — unauthenticated only)

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

#### Section: Adventures Section (AdventuresSection component)

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
  - **In-Progress Session Modal**: Confirmation dialog asking "Continue Adventure?" with Continue/Cancel buttons
- **Call-to-action:** "Start Adventure" on each card (navigates to character assignment); "Continue" on active sessions

**Note:** This section also appears on the standalone Adventures page (`/adventures`).

---

### PAGE: About (`/about`)

#### Section: About Header

- **Primary purpose:** Establish what Mystira is and why it exists
- **Target audience:** Parents, educators, and facilitators evaluating the platform
- **Key message:** "Where imagination meets growth" — Mystira transforms shared playtime into developmental adventures
- **Current layout/structure:**
  - Centered layout constrained to `col-lg-8`
  - Dragon icon (4x), "About Mystira" heading, "Where imagination meets growth" subheading
- **Call-to-action:** None

#### Section: About Content Card

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

#### Section: About Feature Cards

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

### PAGE: Adventures (`/adventures`)

- **Primary purpose:** Dedicated page for adventure browsing and discovery
- **Target audience:** Authenticated users looking to start or continue adventures
- **Key message:** Full catalog access with filtering and sorting
- **Current layout/structure:** Renders the same `AdventuresSection` component documented under the Home page
- **Call-to-action:** Same as Home Adventures Section

---

### PAGE: Sign In (`/signin`)

#### Section: Sign In Card

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

### PAGE: Sign Up (`/signup`)

#### Section: Sign Up Card

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

### PAGE: Character Assignment (`/character-assignment/{scenarioId}`)

#### Section: Character Assignment Interface

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

### PAGE: Game Session (`/game`)

#### Section: Game Session Interface

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

### PAGE: Profiles (`/profiles`)

#### Section: Profiles Management Interface

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

### PAGE: Achievements (`/achievements`)

#### Section: Achievements Display

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

### PAGE: Awards (`/game/awards`)

#### Section: Post-Adventure Awards

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

### PAGE: Parent Dashboard (`/parent-dashboard`)

#### Section: Parental Consent Management

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

### PAGE: Brand (`/brand`)

#### Section: Brand Style Guide

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

### PAGE: Authentication Callbacks

#### Magic Link Sent (`/authentication/magic-link-sent`)
- **Purpose:** Confirmation that magic link email was sent
- **Target audience:** Users who requested magic link sign-in
- **Message:** Check your email for the sign-in link

#### Magic Link Verify (`/authentication/magic-verify`)
- **Purpose:** Handle magic link verification when user clicks the email link
- **Target audience:** Users clicking the magic link from their email

#### Login Callback (`/authentication/logincallback`)
- **Purpose:** Handle OAuth redirect callback after Entra authentication
- **Target audience:** Users returning from Microsoft Entra sign-in flow

---

## APPLICATION 2: MYSTIRA PUBLISHER (React SPA)

Routes: `/` (home), `/login`, `/dashboard`, `/stories`, `/stories/:id`, `/register`, `/audit`, `/open-roles`, `/role-requests`

---

### HEADER (HomePage — Public Header)

- **Primary purpose:** Provide navigation and conversion for the Publisher landing page
- **Target audience:** Unauthenticated visitors and returning users
- **Key message:** Mystira Publisher — professional tool for story registration
- **Current layout/structure:**
  - Left: "Mystira Publisher" logo text (links to `/`)
  - Center nav: "Dashboard" link (authenticated only)
  - Right:
    - Theme Toggle
    - **Authenticated:** "Dashboard" ghost button
    - **Unauthenticated:** "Sign In" text link + "Get Started" button
- **Call-to-action:** "Get Started" button for new visitors

---

### HEADER (Layout — Authenticated Header)

- **Primary purpose:** Provide navigation across all authenticated Publisher pages
- **Target audience:** Authenticated creators and contributors
- **Key message:** Full access to story management, registration, and audit tools
- **Current layout/structure:**
  - Left: "Mystira Publisher" logo (links to `/dashboard`)
  - Center nav: Dashboard, Stories, Open Roles, Role Requests, Register, Audit Trail (active state highlighted)
  - Right: Theme Toggle + Notification Bell + User Avatar + User Name + "Sign Out" button
  - **Sidebar nav** (aside): Same links as header nav (for responsive/mobile)
  - Skip link for accessibility
- **Call-to-action:** Navigation links; Sign Out

---

### FOOTER

- **Note:** No dedicated footer component exists in the Publisher application.

---

### PAGE: Publisher Home (`/`)

#### Section: Hero

- **Primary purpose:** Communicate the Publisher's core value proposition and drive sign-up
- **Target audience:** Story creators, authors, illustrators evaluating the platform
- **Key message:** "Register your creative stories on-chain with transparent attribution and royalty splits."
- **Current layout/structure:**
  - "Mystira Publisher" heading
  - Subtitle text describing on-chain registration with attribution
  - **Authenticated:** "Go to Dashboard" button
  - **Unauthenticated:** "Get Started" + "Learn More" buttons
- **Call-to-action:** "Get Started" (links to `/login`)

#### Section: Features Grid

- **Primary purpose:** Explain why creators should use Mystira Publisher
- **Target audience:** Creators considering the platform
- **Key message:** Four key differentiators
- **Current layout/structure:**
  - "Why Mystira Publisher?" heading
  - Four elevated cards in grid:
    1. **Transparent Attribution** — Every contributor is recognized with clear role assignments and royalty splits.
    2. **Consensus-Based** — All contributors must approve before registration, ensuring fairness.
    3. **Immutable Records** — On-chain registration creates permanent, verifiable proof of ownership.
    4. **Full Audit Trail** — Every action is logged for complete transparency and legal compliance.
- **Call-to-action:** None (informational; hero CTA drives conversion)

---

### PAGE: Login (`/login`)

#### Section: Login Card

- **Primary purpose:** Authenticate users with multiple sign-in methods
- **Target audience:** Existing users and new registrants
- **Key message:** "Welcome back to Mystira Publisher" — flexible authentication options
- **Current layout/structure:**
  - Centered login card
  - "Sign In" title + welcome message
  - **Auth method selector** (three toggle buttons):
    1. **Email & Password:** Standard email/password form
    2. **Microsoft Entra:** JWT token paste field with link to Identity Service
    3. **Magic Link:** Email input + "Send Magic Link" button; after sending shows confirmation + "I've clicked the link" button
  - Error alerts for login and magic link failures
  - Footer: "Don't have an account? Contact us" (mailto link)
- **Call-to-action:** "Sign In" / "Sign In with Entra" / "Send Magic Link"

---

### PAGE: Dashboard (`/dashboard`)

#### Section: Dashboard Header

- **Primary purpose:** Welcome returning users and provide quick access to story registration
- **Target audience:** Authenticated creators
- **Key message:** "Here's what's happening with your stories"
- **Current layout/structure:**
  - "Welcome back, [Name]" greeting
  - Subtitle: "Here's what's happening with your stories"
  - "+ Register New Story" button
- **Call-to-action:** "Register New Story"

#### Section: Stats Cards

- **Primary purpose:** Provide at-a-glance metrics on story portfolio
- **Target audience:** Active creators tracking their registrations
- **Key message:** Quick visibility into registered, pending, and total story counts
- **Current layout/structure:**
  - Three stat cards in horizontal row:
    1. **Registered** — count with checkmark icon (primary color)
    2. **Pending** — count with alert icon (warning color)
    3. **Total Stories** — count with book icon (info color)
  - Only shown when total count > 0
- **Call-to-action:** None (informational)

#### Section: Recent Stories

- **Primary purpose:** Quick access to recent story activity
- **Target audience:** Creators monitoring their stories
- **Key message:** Stay on top of your latest work
- **Current layout/structure:**
  - Card with "Recent Stories" header + "View All" link
  - List of up to 5 stories with title, summary, and status badge
  - Empty state: "No stories yet — Get started by registering your first creative story on-chain." with "Register Your First Story" button
  - Loading state: Skeleton loader
- **Call-to-action:** Story links; "Register Your First Story" (empty state)

#### Section: Quick Actions

- **Primary purpose:** Provide shortcuts to common workflows
- **Target audience:** Active users looking for fast navigation
- **Key message:** Common tasks are always one click away
- **Current layout/structure:**
  - Card with "Quick Actions" header
  - Three action links with icons:
    1. "Start Registration" (plus icon)
    2. "View All Stories" (book icon)
    3. "Audit Trail" (document icon)
- **Call-to-action:** Each action navigates to its respective page

---

### PAGE: Stories (`/stories`)

#### Section: Stories List with Filters

- **Primary purpose:** Browse, search, and filter all stories
- **Target audience:** Creators managing their story portfolio
- **Key message:** Full control and visibility over all your stories
- **Current layout/structure:**
  - Header: "Stories" title + "Register New Story" button
  - **Filters row:** Search input + Status dropdown (All/Draft/Pending Approval/Approved/Registered/Rejected)
  - **Stories grid:** Cards showing title, status badge, summary, contributor count, last updated date
  - Empty state: "No stories found" with "Register a Story" button
  - Loading state: Card skeleton loaders
- **Call-to-action:** "Register New Story"; individual story cards link to detail view

---

### PAGE: Story Detail (`/stories/:id`)

#### Section: Story Header

- **Primary purpose:** Identify the story and its current status
- **Target audience:** Story owners and contributors
- **Layout:** Back link, story title, status badge, "Continue Registration" button (for drafts)

#### Section: Story Details Card

- **Primary purpose:** Display story metadata
- **Target audience:** Story stakeholders
- **Layout:** Summary text + metadata (created date, last updated, registered date, transaction ID)

#### Section: Contributors List

- **Primary purpose:** Show all contributors with their roles and royalty splits
- **Target audience:** Story owners reviewing attribution
- **Layout:** ContributorList component showing user, role, split percentage, approval status

#### Section: Open Roles Manager

- **Primary purpose:** Create and manage open contributor positions
- **Target audience:** Story owners seeking collaborators
- **Layout:** OpenRoleManager component with error boundary

#### Section: Role Requests

- **Primary purpose:** Review applications from potential contributors
- **Target audience:** Story owners evaluating candidates
- **Layout:** RoleRequestList component with error boundary

#### Section: Approval Panel

- **Primary purpose:** Facilitate consensus-based approval for registration
- **Target audience:** Contributors who need to approve before on-chain registration
- **Layout:** ApprovalPanel component (shown only for pending_approval stories)

#### Section: Activity Sidebar

- **Primary purpose:** Show recent audit trail activity for this story
- **Target audience:** All story stakeholders
- **Layout:** Sidebar card with latest 10 audit log entries

---

### PAGE: Register (`/register`)

#### Section: Registration Wizard

- **Primary purpose:** Guide users through multi-step story registration on blockchain
- **Target audience:** Creators ready to register a new story
- **Key message:** "Follow the steps below to register your story on-chain with transparent attribution"
- **Current layout/structure:**
  - Header: "Register Story" title + subtitle
  - RegistrationWizard component (multi-step form):
    - Story selection/creation
    - Contributor and role assignment
    - Royalty split negotiation
    - Review and submission
- **Call-to-action:** Wizard step navigation; final submit for on-chain registration

---

### PAGE: Audit Trail (`/audit`)

#### Section: Audit Trail Interface

- **Primary purpose:** Provide complete, immutable activity history for compliance and transparency
- **Target audience:** Story owners, contributors, legal teams needing audit records
- **Key message:** "Complete history of all actions across registered stories"
- **Current layout/structure:**
  - Header: "Audit Trail" title + subtitle + "Show/Hide Filters" button + "Export" button
  - **Filters card** (collapsible): Event type, date range filters
  - **Activity Log card:** Event count + AuditLogList component (scrollable log entries)
  - **Detail overlay:** AuditLogDetail component (opens when selecting a log entry)
- **Call-to-action:** Export button; filter controls; log entry selection

---

### PAGE: Open Roles (`/open-roles`)

#### Section: Open Roles Browser

- **Primary purpose:** Browse available contributor positions across all stories
- **Target audience:** Creators looking to contribute to stories (illustrators, editors, co-authors)
- **Key message:** Find stories that need your skills
- **Current layout/structure:**
  - OpenRolesBrowser component showing available positions with story context, role type, and apply action
- **Call-to-action:** Apply to open roles

---

### PAGE: Role Requests (`/role-requests`)

#### Section: Role Requests Management

- **Primary purpose:** Review and respond to contributor applications
- **Target audience:** Story owners evaluating incoming role requests
- **Key message:** Manage who joins your creative team
- **Current layout/structure:**
  - Card with "Role Requests" header + "Review and respond to contributor applications" subtitle
  - RoleRequestList component with optional storyId filter
- **Call-to-action:** Approve/reject role request actions

---

### PAGE: Not Found (404)

#### Section: 404 Error State

- **Primary purpose:** Gracefully handle navigation to non-existent pages
- **Target audience:** Users who hit broken/invalid URLs
- **Key message:** "Page Not Found — The page you're looking for doesn't exist or has been moved."
- **Current layout/structure:**
  - EmptyState component with title, description, and "Go to Home" button
- **Call-to-action:** "Go to Home" button

---

## CROSS-APPLICATION SHARED PATTERNS

### Repeated Section: Auth Gate

- **Appears on:** Profiles, Achievements, Awards, Parent Dashboard, and all protected Publisher routes
- **Purpose:** Redirect or display sign-in prompt when accessing authenticated content without a session
- **Layout:** Lock icon + "Authentication Required" heading + "Please sign in" message + "Sign In" button

### Repeated Section: Loading State

- **Appears on:** Every data-driven page
- **Purpose:** Indicate data is being fetched
- **Layout:** Spinner with contextual message (e.g., "Loading adventures...", "Loading profiles...")

### Repeated Section: Empty State

- **Appears on:** Adventures (no bundles), Profiles (no profiles), Achievements (no badges), Stories (no results), Dashboard (no stories)
- **Purpose:** Guide users toward the next action when no content exists
- **Layout:** Large icon + title + description + action button (EmptyState component)

### Repeated Section: Error State

- **Appears on:** Most data-driven pages
- **Purpose:** Communicate failures and offer recovery options
- **Layout:** Warning icon or alert banner + error message + retry/navigation button
