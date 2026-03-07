# UI Metrics Evaluation: Mystira Platform

> Section-by-section assessment of applicable metrics for evaluating UI suitability.
> Based on codebase analysis of Blazor WebAssembly PWA + React Publisher apps.
> Last updated: 2026-03-07

## Measurement Infrastructure Context

Before diving into section-specific metrics, note what's currently available:

- **Application Insights** via `TelemetryService` (JSInterop bridge) — supports custom events, exceptions, and metrics. Currently only tracks `EndpointChanged` events; largely untapped for UX measurement.
- **Service Worker** with offline caching and update notification — enables PWA-specific metrics.
- **No client-side analytics SDK** (no Amplitude, GA, Mixpanel) beyond App Insights.
- **ARIA attributes** present across components (~83 occurrences) but coverage is uneven — concentrated in media/assignment components, sparse in navigation and filtering.
- **Loading states** are well-implemented (LoadingIndicator, SkeletonLoader, LoadingExperience components, 136+ references) — latency perception can be measured.

**Implication:** Most metrics below require instrumenting the existing `ITelemetryService` with new custom events. The infrastructure exists; the event catalog does not.

---

## APPLICATION 1: MYSTIRA APP (PWA)

---

### HEADER (MainLayout — Global Navigation)

The header serves dual roles: wayfinding for all users and account management for authenticated users. Its hamburger collapse on mobile is the critical design decision to evaluate.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Mobile hamburger open rate** | If users never open the mobile nav, they're either not finding features or the visible items suffice. Low rate + high bounce = discoverability problem. | 15-30% of mobile sessions should interact with the hamburger | Custom event on toggle click, segmented by auth state |
| **Navigation item click distribution** | Reveals whether the information architecture matches user mental models. If "Adventures" gets 80% of clicks and "About" gets 0.2%, the nav may be over-weighted. | No single nav item below 2% click share (otherwise remove it) | Click events per nav item |
| **Time-to-first-nav-interaction** | Measures whether the header is perceived as interactive or decorative. In a children's platform, visual clutter can cause nav blindness. | < 8 seconds for authenticated users | Timestamp delta from page load to first nav click |
| **Profile dropdown engagement depth** | The dropdown contains 5 items (Achievements, Profiles, Install, Settings, Sign Out). If users only ever use Sign Out, the dropdown is a poor container for those features. | At least 2 distinct items used per session on average | Track each dropdown item click |
| **"Get Started" CTA click-through rate** | The primary conversion lever for unauthenticated visitors. This is the header's most important metric. | > 5% of unauthenticated pageviews | Click event / unauthenticated session count |

**What to skip:** Don't measure header render time separately — it's part of the Blazor WASM initial load and is better captured at the page level.

---

### FOOTER (MainLayout — Global Footer)

The footer is purely informational (environment badge, version, connection status). It has no user-facing actions.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Visibility scroll-reach rate** | Whether users ever see the footer. If it's never scrolled to, the environment/version info is invisible. | N/A — this is a diagnostic data point, not a target | Intersection Observer event |
| **Offline status display accuracy** | The footer shows Online/Offline. If this disagrees with actual connectivity, it erodes trust. | 100% accuracy (tested via service worker) | Compare `navigator.onLine` events with displayed state |

**What to skip:** Engagement metrics, click rates, time-on-footer — none apply. The footer's success criterion is "doesn't confuse anyone."

---

### GLOBAL OVERLAY COMPONENTS

These components share a critical constraint: they must communicate without blocking the user's primary task.

#### Offline Indicator

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Display latency after connectivity loss** | If the banner appears 10+ seconds after going offline, users have already encountered errors. | < 2 seconds after `navigator.onLine` fires `false` | Timestamp comparison |
| **False positive rate** | Banner showing when connectivity exists → trust erosion. | 0% | QA testing + user reports |

#### PWA Install Button

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Install prompt acceptance rate** | Direct measure of whether the floating button is effective without being annoying. | 3-8% of eligible sessions | `beforeinstallprompt` → `appinstalled` event conversion |
| **Dismiss-to-re-show ratio** | If users dismiss and it reappears too aggressively, this becomes hostile UX. | Not shown again within same session after dismissal | Event tracking on dismiss |

#### Toast Container

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Toast display duration vs. content length** | Short toasts with long messages = users can't read them. Long toasts with short messages = annoying. | 3-5 seconds for < 50 chars; 5-8 seconds for longer | Audit toast configuration |
| **Toast overlap/stack depth** | More than 3 simultaneous toasts = noise. | Max 3 visible simultaneously | Count concurrent toast instances |
| **Error toast → user action correlation** | When an error toast appears, does the user retry or abandon? | > 60% retry after recoverable error toasts | Sequence analysis: error toast → next action |

#### Update Notification

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Time-to-accept update** | How long users delay refreshing. Long delays = disruptive timing or unclear messaging. | < 30 seconds median | Timestamp from notification display to acceptance |
| **Session loss on update** | Whether updating causes users to lose game state or position. | 0% data loss events | Track active session state pre/post refresh |

---

### PAGE: Home (`/`, `/home`)

#### Hero Section

This is the most expensive section to render (particle canvas, SVG animations, auto-playing video with theme variants, pulse overlay). Its job: emotional hook + conversion.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **First Contentful Paint (FCP)** | Blazor WASM has an inherently slow cold start. The hero must show something meaningful before the full runtime loads. | < 2.5 seconds on 4G | Web Vitals via App Insights |
| **Largest Contentful Paint (LCP)** | The hero video/logo is likely the LCP element. If it loads after users have scrolled past, the emotional hook fails. | < 4 seconds | Performance Observer API |
| **Video engagement: play rate** | The auto-playing intro video is a significant investment. If it's skipped immediately or never plays (autoplay blocked), the effort is wasted. | > 40% of sessions see at least 3 seconds of video | Track video `play`, `pause`, `ended` events + Skip button clicks |
| **"Watch Full Intro" click rate** | Measures whether the teaser creates curiosity. | 5-15% of unauthenticated sessions | Click event |
| **Hero-to-signup conversion rate** | The section's core KPI. "Get Started" button clicks / total unauthenticated hero views. | > 8% | Funnel: hero view → signup click |
| **Cumulative Layout Shift (CLS)** | Video → static logo → pulse animation transitions risk layout shifts. | < 0.1 | Web Vitals |
| **Scroll depth past hero** | If users scroll but don't click CTA, the hero informed but didn't convert. Useful for A/B testing hero copy. | > 60% scroll past hero | Intersection Observer on feature cards section |

**What to skip:** Particle canvas frame rate — unless users report jank, this is over-measurement.

#### Feature Cards (Unauthenticated Only)

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Viewability rate** | Cards only matter if users scroll to them. If the hero is too tall, these are invisible. | > 50% of unauthenticated sessions | Intersection Observer |
| **Read-through rate (time in viewport)** | Are users actually reading the three value propositions, or scrolling past? | > 3 seconds average viewport time | Intersection Observer + timer |
| **Post-card-view conversion lift** | Do users who see the feature cards convert at a higher rate than those who don't? | Statistically significant positive lift | Cohort comparison: saw cards vs. didn't → signup rate |

**What to skip:** Individual card click tracking — the cards have no CTAs, so clicks are irrelevant.

#### Adventures Section

This is the primary engagement surface for authenticated users. It has the most complex interaction pattern: filtering, browsing bundles, selecting scenarios, managing active sessions.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Adventures load time** | API call to render complete grid. Spinner shows "Loading adventures..." — if this takes too long, users see an empty page. | < 2 seconds from auth to rendered grid | Performance mark from component init to `StateHasChanged` after data load |
| **Filter usage rate** | If nobody uses age group filters, they're either unnecessary or undiscoverable. | > 20% of adventure browsing sessions use at least one filter | Click events on filter pills |
| **Filter-to-result satisfaction** | After filtering, do users click an adventure (success) or clear filters (dissatisfaction)? | > 50% of filter actions lead to an adventure click within 30 seconds | Event sequence analysis |
| **Bundle drill-down rate** | Do users click into bundles, or do they bounce from the grid view? | > 40% of sessions with adventure views | Bundle card click events |
| **Adventure start rate** | Sessions that load adventures section → sessions that click "Start Adventure" | > 25% per session | Funnel analysis |
| **Active session continuation rate** | When users have in-progress adventures, how often do they click "Continue" vs. starting a new one? | > 60% continue if an active session exists | Event comparison |
| **Empty state encounter rate** | How often filters produce zero results. Frequent empty states = content gap or poor filter UX. | < 5% of filter interactions | Track when result count = 0 |
| **Completed adventure visibility toggle usage** | "Show/hide completed" — measures whether users want to revisit or avoid replays. | Track to inform, no target | Toggle event |

---

### PAGE: About (`/about`)

A content page with no interactive elements beyond a "Back to Home" button. Its success criterion is trust-building for evaluating decision-makers.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Referral source → About page visit rate** | Who visits About? If it's all organic search, the page needs SEO attention. If it's internal nav, users are vetting the platform. | Track to understand audience composition | UTM + referrer analysis |
| **Read completion rate** | Scroll depth through the three content paragraphs + feature cards. If users bounce at paragraph 1, the copy isn't compelling. | > 40% reach the feature cards | Intersection Observer at card section |
| **About → Signup conversion** | Decision-makers who read About and then sign up = high-intent conversions. | > 3% of About visitors navigate to signup within session | Page sequence analysis |
| **"Back to Home" click rate** | If > 80% use this button, users aren't finding other navigation paths from this page. | 30-60% (rest should use header nav) | Click event |
| **Time on page** | Distinguishes skimmers from readers. | > 45 seconds median for meaningful engagement | Page visibility timer |

---

### PAGE: Sign In (`/signin`)

Authentication is a gate — every second of friction here costs returning users.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Auth method selection split** | Entra SSO vs. magic link usage. Reveals user preference and helps decide where to invest. | Track to inform — no predetermined target | Click events per method |
| **Sign-in completion rate** | Started sign-in → successfully authenticated. Drop-off here = broken auth flow or confusion. | > 85% | Funnel: page load → authenticated redirect |
| **Sign-in error rate** | How often the error alert box appears. | < 5% of attempts | Error event tracking |
| **Magic link email-to-click conversion** | Users who request a magic link → users who actually click it and authenticate. | > 70% within 10 minutes | Server-side: link sent → link verified events |
| **Time-to-authenticate** | From page load to successful auth redirect. | < 15 seconds for Entra SSO; < 120 seconds for magic link (includes email wait) | Timestamp deltas |
| **"Don't have an account?" click-through** | Users who landed on Sign In but actually need Sign Up. High rate = confused entry points. | < 15% | Click event |

---

### PAGE: Sign Up (`/signup`)

The conversion endpoint. Every element exists to reduce friction.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Signup completion rate** | Page load → account created. The single most important metric for this page. | > 60% | Funnel analysis |
| **Benefits list scroll visibility** | The three benefit bullets ("Free to get started", "Interactive story adventures", "Safe for the whole family") appear before the signup buttons. If users don't see them, they're not doing their job. | > 90% viewability (they should be above the fold) | Layout audit + Intersection Observer |
| **Auth method selection (Entra vs. magic link)** | Same as Sign In — tracks preference. | Track to inform | Click events |
| **Form abandonment point** | Where in the email input + send flow do users drop off? Empty field? After typing? After clicking Send? | Identify highest drop-off step | Input focus → blur → click sequence |
| **Time from page load to signup action** | Measures decision friction. Very long times = copy isn't convincing or users are hesitating. | < 30 seconds median to first signup button click | Timestamp delta |
| **"Already have an account?" redirect rate** | Wrong-page arrivals. | < 10% | Click event |

---

### PAGE: Character Assignment (`/character-assignment/{scenarioId}`)

A multi-step setup flow that must feel quick and playful, not administrative. This is the last gate before the core experience.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Assignment completion rate** | Loaded page → clicked "Start Adventure." Drop-offs here represent lost engagement after the user already committed to an adventure. | > 75% | Funnel analysis |
| **Time-to-complete assignment** | Long times indicate confusion with the character/profile matching UX. | < 90 seconds for returning users; < 3 minutes for first-time | Timestamp delta |
| **Tab usage distribution** | Profile Selection / New Profile / AI Player / Guest tabs. If AI Player or Guest dominate, users may not be creating persistent profiles — which affects achievement tracking. | Track to inform profile strategy | Tab switch events |
| **Age group mismatch modal trigger rate** | How often profiles don't match scenario age groups. High rate = poor scenario recommendation or missing profiles. | < 15% of assignment sessions | Modal display events |
| **Guest warning modal conversion** | After seeing the guest warning, do users proceed anyway or go create a profile? | > 50% should create a profile (desired behavior) | Post-modal navigation tracking |
| **Avatar selection engagement** | Do users browse the carousel or accept defaults? High engagement = the feature adds value. | > 30% interact with carousel | Carousel swipe/click events |
| **"Back to Adventures" abandonment from error state** | When scenario isn't found, do users navigate back or leave entirely? | > 80% click "Back to Adventures" (not browser back) | Click event vs. page unload |

---

### PAGE: Game Session (`/game`)

The core product experience. Metrics here measure whether the storytelling UI actually works as an immersive, engaging narrative medium.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Session duration** | How long players stay in a single game session. Too short = disengagement. Too long without choices = passive consumption, not interaction. | 10-30 minutes per session (age-dependent) | Session start/end timestamps |
| **Choice response time** | Time from choice buttons appearing to user selecting one. Fast = engaged and decisive. Very slow = confused or disengaged. Very fast = not reading/considering. | 5-30 seconds (age-dependent; younger skews faster) | Timestamp: choices rendered → choice clicked |
| **Choice distribution entropy** | If all players always pick the same choice, the branching narrative isn't creating meaningful decisions. | No single choice should capture > 70% of selections for a given branch point | Aggregate choice analytics per scene |
| **Scene media load time** | Images/video within FantasyMediaFrame. Slow media breaks immersion. | < 1.5 seconds per scene transition | Performance marks around media load |
| **Scenes-per-session completion rate** | How many scenes users progress through vs. total available in the path. Early exits = engagement failure. | > 70% of started paths reach a natural ending | Scene count at exit / total path length |
| **Dice roller usage rate** | Optional mechanic — is it adding value? | Track to decide if worth maintaining | Click events |
| **Session exit method** | Did users complete the adventure, use the exit button, navigate away, or close the browser? Ungraceful exits = friction or boredom. | > 50% should reach completion or use exit button | Event tracking per exit type |
| **Content attribution visibility** | Are credits being seen? Required for proper content licensing. | > 80% of sessions have attribution visible for ≥ 2 seconds | Intersection Observer |
| **Error/loading interruption rate** | How often the game experience is interrupted by loading spinners or errors mid-session. | < 2% of scene transitions show errors | Error event tracking during active sessions |
| **Return-to-session rate** | When users leave mid-adventure, do they come back via "Active Adventures"? | > 40% of interrupted sessions resume within 48 hours | Session state tracking |

---

### PAGE: Profiles (`/profiles`)

Administrative page — success means users can manage profiles quickly and return to adventures.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Task completion time: create profile** | From clicking "New Profile" to saving. Long times = form confusion or avatar selection friction. | < 60 seconds | Timestamp delta |
| **Task completion time: edit profile** | Same as create, but should be faster. | < 45 seconds | Timestamp delta |
| **Delete confirmation rate** | Users who open delete modal → users who confirm. Very high rate = they're sure. Very low rate = accidental clicks. | 60-90% (below 60% suggests accidental triggers) | Modal open → confirm/cancel events |
| **Empty state → first profile creation** | When new users see "No Profiles Yet," how quickly do they create one? | > 70% create within 60 seconds | Funnel: empty state view → profile saved |
| **Profiles-per-account distribution** | Helps understand if the multi-profile model matches actual usage. If 90% of accounts have 1 profile, the grid/management UI is over-engineered. | Track to inform design | Account-level profile count |
| **Avatar carousel engagement** | Same as character assignment — carousel interaction rate. | > 25% browse beyond first page | Carousel events |
| **Error rate on save** | Profile creation/edit failures. | < 2% of save attempts | Error event tracking |

---

### PAGE: Achievements (`/achievements`)

A reward/progress tracking page. Its job: make players feel accomplished and motivate continued play.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **View mode preference** | Simplified (badge grid) vs. Advanced (developmental axis breakdown). Reveals whether the audience wants detail or visual gratification. | Track to inform — if > 80% stay on simplified, consider making it the only view | Toggle events |
| **Badge detail modal open rate** | Do users click badges to learn more, or just visually scan? | > 20% of badges viewed are clicked for details | Click events per badge |
| **Time on page by view mode** | Simplified view time vs. advanced view time. Longer time on advanced = users finding the developmental axis information valuable. | > 30 seconds on simplified; > 60 seconds on advanced | Page timer segmented by mode |
| **"Continue Adventures" click-through** | The page's only CTA. High rate = achievements motivate further play. | > 15% of page visits | Click event |
| **Profile switcher usage** | When multiple profiles exist, do users check achievements for each? | Track to understand cross-profile engagement | Profile selection events |
| **Empty state encounter rate** | Users arriving with no badges. High rate for established accounts = badge earning is too hard or unclear. | < 30% of page visits for accounts > 7 days old | Conditional tracking |
| **Tier progression visibility** | In advanced view, are users scrolling to see all tiers (Bronze through Diamond)? | > 50% scroll to see all earned tiers | Intersection Observer per tier card |

---

### PAGE: Awards (`/game/awards`)

Post-adventure celebration. This page has 5-15 seconds to create a positive emotional peak before the user navigates away.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Time on awards page** | Very short = users don't care about badges. Very long = they're savoring the moment. Optimal is a brief, positive interaction. | 8-30 seconds | Page timer |
| **Badge earned rate per adventure** | What percentage of completed adventures award at least one badge? Zero-badge completions with the "no badges" message deflate the experience. | > 60% of completions award at least one badge | Server-side completion events |
| **Navigation choice: "Go Home" vs. "View Achievements"** | Reveals post-completion intent. Achievements link = investment in progress tracking. Home = ready for next adventure. | Track to inform — both are valid exits | Click events |
| **Age mismatch warning encounter rate** | How often the contextual alert about age group mismatch appears. High rate = character assignment isn't preventing mismatches well enough. | < 10% | Alert display events |
| **Repeat play info display rate** | How often users see the "already played" message. High rate = users are replaying content, which may indicate content scarcity. | Track to inform content strategy | Alert display events |

---

### PAGE: Parent Dashboard (`/parent-dashboard`)

COPPA compliance management. This page has regulatory implications — its primary metric is whether parents can actually exercise their consent rights.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Consent action completion rate** | Can parents successfully revoke consent when they want to? Failure here is a compliance risk. | 100% of attempted revocations complete successfully | Success/failure events on revoke action |
| **Task completion time: revoke consent** | From clicking "Revoke Consent" to confirmation complete (includes email verification step). | < 120 seconds | Timestamp delta |
| **Dashboard access frequency** | How often parents visit. Very infrequent = either everything is fine, or they've forgotten it exists. | Track to understand engagement pattern | Page view frequency per account |
| **Consent status distribution** | Breakdown of Verified/Pending/Expired/Revoked/Denied across all child profiles. High Pending = onboarding friction. High Expired = renewal UX is failing. | > 80% Verified for active accounts | Database query / dashboard analytics |
| **"Manage Profiles" click-through from empty state** | When no child profiles exist, do parents follow the link? | > 60% | Click event |
| **Error rate on consent operations** | Any failure in consent management is a compliance concern. | 0% for consent state changes | Error tracking with alerts |
| **Accessibility compliance (WCAG 2.1 AA)** | Parent dashboard handles sensitive legal actions. It must be fully accessible. This page has 13 ARIA references — the highest of any page — but needs audit. | Full WCAG 2.1 AA compliance | Automated + manual accessibility audit |

---

### PAGE: Brand (`/brand`)

Internal reference page for design system consistency. Used by designers and developers, not end users.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Component coverage completeness** | Does the brand page document all components actually used in the app? Missing components = design drift. | 100% of reusable components represented | Manual audit against component inventory |
| **Page visit frequency by role** | Are designers/developers actually using this reference? | At least monthly visits from the team | Page view tracking (internal) |
| **Design token consistency** | Do the showcased styles (typography, buttons, status, motion) match what's deployed in production? | 100% match | Visual regression testing |

**What to skip:** User engagement metrics — this isn't a user-facing page.

---

### AUTHENTICATION CALLBACK PAGES

These are transient redirect handlers. Users should spend minimal time here.

| Metric | Why It Matters | Acceptable Range | How to Measure |
|--------|---------------|-----------------|----------------|
| **Redirect completion rate** | Callbacks that successfully redirect to the intended destination. Failures strand users. | > 99% | Success/error events |
| **Time on callback page** | Should be near-zero. Long times = something is stuck. | < 3 seconds | Page timer |
| **Magic link expiry encounter rate** | Users clicking expired magic links. High rate = links expire too quickly or email delivery is slow. | < 5% of magic link clicks | Expired link error events |
| **Error display quality** | When callbacks fail, is the error message helpful or cryptic? | Qualitative audit — no broken/technical error messages shown to users | Manual testing of failure scenarios |

---

## CROSS-CUTTING METRICS (All Sections)

These apply across the entire application and shouldn't be measured per-section but holistically:

| Metric | Why It Matters | Acceptable Range |
|--------|---------------|-----------------|
| **Blazor WASM initial load time** | The cold-start penalty affects every first-visit experience. This is the single biggest technical UX constraint. | < 5 seconds on broadband; < 8 seconds on 4G |
| **Service worker cache hit rate** | Subsequent visits should load near-instantly from cache. | > 90% cache hit on return visits |
| **PWA vs. browser usage split** | Installed PWA users likely have higher engagement. Validates PWA investment. | Track to inform |
| **Offline feature usage** | When offline, what do users attempt? Reveals which features need offline support. | Track to inform |
| **Accessibility score (Lighthouse)** | Automated baseline across all pages. | > 90 on all pages |
| **Cross-device session continuity** | Users starting on one device and continuing on another. Important for family use cases. | Track to inform |
| **Auth-gate friction** | Pages that require auth (Adventures, Profiles, Achievements, Parent Dashboard) — how many users hit the auth gate and abandon? | < 20% abandonment at auth gates |

---

## MEASUREMENT PRIORITY MATRIX

Not all metrics are equally urgent to implement. Prioritized by impact and feasibility:

### P0 — Implement Immediately
1. **Hero-to-signup conversion rate** (direct revenue/growth impact)
2. **Signup completion rate** (conversion funnel)
3. **Sign-in completion rate** (returning user retention)
4. **Adventure start rate** (core engagement)
5. **Game session duration + completion rate** (product-market fit signal)
6. **Consent action completion rate** (compliance requirement)
7. **Blazor WASM initial load time** (foundational UX constraint)

### P1 — Implement Within 30 Days
8. **Choice response time** (engagement quality)
9. **Assignment completion rate** (funnel step)
10. **Adventures load time** (perceived performance)
11. **Filter usage + satisfaction** (content discovery)
12. **LCP/CLS Web Vitals** (Core Web Vitals for SEO + UX)
13. **Auth method selection split** (infrastructure investment decision)
14. **Session exit method tracking** (engagement diagnosis)

### P2 — Implement Within 90 Days
15. **Achievement view mode preference** (feature investment)
16. **Profile management task times** (usability)
17. **Toast/notification effectiveness** (communication quality)
18. **PWA install rate** (distribution channel)
19. **Video engagement metrics** (hero section ROI)
20. **Scroll depth + viewability metrics** (content effectiveness)

### P3 — Nice to Have
21. **Badge detail modal opens** (micro-engagement)
22. **Dice roller usage** (feature retention decision)
23. **Brand page visit frequency** (internal tooling)
24. **Footer visibility** (diagnostic curiosity)
