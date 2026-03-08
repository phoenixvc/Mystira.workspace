# Metrics Discussion: Rationale & Methodology

> Meta-analysis of the metrics chosen in `ui-metrics-evaluation.md`.
> Why these specific metrics? What trade-offs were made? What's missing?
> Last updated: 2026-03-07

---

## 1. Methodology: How Metrics Were Selected

The evaluation in `ui-metrics-evaluation.md` followed a three-stage selection process:

### Stage 1: Section Purpose Mapping
Each section was classified by its primary function:

| Function Type | Sections | Dominant Metric Category |
|--------------|----------|------------------------|
| **Conversion** | Hero, SignUp, SignIn | Funnel completion rates |
| **Discovery** | Adventures, Bundles, Filters | Engagement + search success |
| **Core Experience** | Game Session | Session quality + immersion signals |
| **Setup/Administration** | Character Assignment, Profiles, Parent Dashboard | Task completion time + error rates |
| **Reward/Retention** | Achievements, Awards | Emotional satisfaction + return triggers |
| **Infrastructure** | Header, Footer, Overlays, Auth Callbacks | Reliability + non-interference |

This classification determined which CATEGORY of metrics was primary for each section. A conversion section gets conversion metrics; an experience section gets engagement metrics. This sounds obvious, but many UI evaluations apply the same metrics everywhere — measuring "conversion rate" on the achievements page or "engagement time" on the sign-in page produces noise, not signal.

### Stage 2: Signal-Over-Noise Filtering
For each section, candidate metrics were evaluated against three criteria:

1. **Does this metric directly measure whether the UI succeeds at its intended purpose?** If the metric could improve while the UI gets worse (or vice versa), it's not a direct signal.

2. **Can it be measured with available tools or reasonable instrumentation?** The codebase has Application Insights via `ITelemetryService` but almost no events are wired. Metrics that require entirely new infrastructure (eye tracking, biometric sensors) were excluded from implementation priorities, though referenced in the research document.

3. **Does it reveal actionable usability gaps?** A metric that only confirms "things are fine" or "things are bad" without pointing to WHY is diagnostic dead weight.

### Stage 3: Priority Calibration
Metrics were ranked by:
- **Impact:** How much does this metric's improvement move a business or user outcome?
- **Feasibility:** How much instrumentation work is needed?
- **Urgency:** Is there evidence (from the codebase) that this area might already be underperforming?

---

## 2. Key Decisions and Trade-offs

### Decision 1: Behavioral Metrics Over Survey-Based Metrics

The evaluation heavily favors behavioral/quantitative metrics (click rates, completion times, funnel conversion) over attitudinal/survey metrics (satisfaction scores, NPS, VisAWI questionnaires).

**Rationale:**
- Mystira is in ALPHA. The user base is too small for statistically valid surveys.
- Behavioral metrics can be collected passively through the existing `ITelemetryService`.
- The platform's primary audience includes children ages 3-12, who cannot reliably complete survey instruments.
- Parents/facilitators who CAN complete surveys are a secondary audience for most sections.

**Trade-off acknowledged:** Behavioral metrics measure what users DO, not what they FEEL. A user who completes signup in 10 seconds might still feel the page was ugly. The aesthetic/experiential analysis (separate document) addresses this gap through expert evaluation.

**When to revisit:** Once the user base exceeds ~500 active users, implement UEQ (User Experience Questionnaire) for the parent/facilitator audience. For children, behavioral proxies (return rate, session duration, choice engagement) remain the best available signal.

### Decision 2: Section-Specific Metrics Over Cross-Platform Metrics

Rather than defining 5-10 universal KPIs applied everywhere, the evaluation defines 3-8 metrics PER SECTION. This produces a larger total metric count (~80) but ensures each measurement is contextually meaningful.

**Rationale:**
- Mystira's sections serve fundamentally different purposes. A universal "engagement score" that averages the hero section (marketing), game session (entertainment), and parent dashboard (compliance) is meaningless.
- Section-specific metrics enable section-specific improvement. "The adventures filter has a 40% empty-result rate" is actionable. "The platform has a 3.2 engagement score" is not.

**Trade-off acknowledged:** More metrics = more instrumentation work + more dashboards + more cognitive load on the team reviewing data. The Priority Matrix (P0-P3) in the evaluation document addresses this by staging implementation.

**Mitigation:** The cross-cutting metrics section provides 7 platform-level metrics (WASM load time, cache hit rate, PWA split, offline usage, accessibility score, cross-device continuity, auth-gate friction) that serve as health indicators.

### Decision 3: Acceptable Ranges Instead of Binary Pass/Fail

Each metric includes an "acceptable range" rather than a single target number.

**Rationale:**
- Mystira serves age groups 1-2 through 19+ with fundamentally different interaction patterns. A fixed target (e.g., "choice response time < 10 seconds") would be too tight for 3-year-olds and too loose for teenagers.
- Ranges communicate intent: "we expect this to fall between X and Y; outside that range warrants investigation."
- Early-stage products shouldn't over-optimize for specific numbers — the ranges will narrow as baseline data accumulates.

**Trade-off acknowledged:** Ranges can become an excuse to accept mediocrity ("we're within range!"). The evaluation document addresses this by framing ranges as investigation triggers, not success criteria.

### Decision 4: Excluding Vanity Metrics

Several commonly tracked metrics were deliberately excluded:

| Excluded Metric | Why It Was Excluded |
|----------------|-------------------|
| **Total pageviews** | Doesn't indicate quality; more views of the error page is bad. Already captured implicitly by section-specific views. |
| **Bounce rate (global)** | Meaningless without section context. "Bouncing" from the awards page after reading badges is success, not failure. |
| **Average session duration (global)** | A parent spending 45 minutes on the parent dashboard is concerning, not positive. Section-specific durations are tracked instead. |
| **Number of profiles created** | More profiles ≠ better UX. The metric that matters is whether profiles-per-account matches actual usage. |
| **Social media shares** | No social sharing features exist in the codebase. |
| **SEO metrics (domain authority, backlinks)** | Relevant for marketing, not UI suitability evaluation. |

### Decision 5: COPPA Compliance Metrics as P0

The parent dashboard's consent action completion rate is classified as P0 despite being a low-traffic page.

**Rationale:**
- COPPA non-compliance is a legal liability, not just a UX issue.
- The FTC has imposed fines exceeding $100M for COPPA violations. A 0.5% failure rate on consent revocation is not an acceptable UX bug — it's a regulatory risk.
- The parent dashboard has 13 ARIA references (highest of any page), suggesting the team already recognizes its compliance significance.

---

## 3. Metric Relationships and Dependencies

Some metrics in the evaluation are not independent — they form causal chains:

### Conversion Funnel Chain
```
Hero-to-signup CTR → Signup completion rate → First adventure started → Session completion
```
If signup completion is 90% but hero CTR is 2%, the bottleneck is the hero, not the signup flow. Improving signup UX won't move the needle.

### Engagement Quality Chain
```
Adventures load time → Filter usage → Bundle drill-down → Adventure start → Choice response time → Session duration → Completion rate
```
Each step depends on the previous. Slow adventure loading suppresses filter usage, which suppresses discovery, which suppresses starts.

### Trust Formation Chain
```
Hero first impression (LCP/CLS) → About page read-through → Signup with benefits visible → Parent dashboard access → Consent verification
```
For parents evaluating the platform, visual quality at the hero level sets the trust floor. If the first impression is unprofessional, they never reach the parent dashboard.

### Retention Loop
```
Session completion → Awards satisfaction → Achievement review → Return to adventures → New session start
```
The awards page is the hinge point in the retention loop. If post-adventure celebration falls flat, the motivation to return weakens.

---

## 4. What's Missing (Known Gaps)

### Gap 1: No Qualitative Feedback Channel
The evaluation is entirely quantitative. There's no mechanism for users to say "I hated the color scheme" or "my child found the dice roller confusing." A feedback widget or periodic survey would fill this gap.

### Gap 2: Accessibility Depth
The evaluation includes Lighthouse accessibility score as a cross-cutting metric, but this only catches automated violations. Manual testing with screen readers, keyboard-only navigation, and color blindness simulation is needed but not captured as a metric — it's a testing activity, not a continuous measurement.

### Gap 3: Content Quality Metrics
The evaluation measures whether users ENGAGE with adventures but not whether the CONTENT is good. Scene narrative quality, choice meaningfulness, and media appropriateness are content metrics that fall outside UI evaluation but heavily influence the metrics being measured.

### Gap 4: Facilitator-Specific Workflows
The codebase supports group play (facilitators leading multiple children), but the evaluation doesn't include facilitator-specific metrics: setup time for a group session, mid-session management overhead, or post-session review efficiency. These would require separate user research.

### Gap 5: Comparative Benchmarks
The evaluation defines acceptable ranges based on industry norms and platform context, but Mystira has no internal baseline data yet. The first round of measurement will establish baselines; the ranges become meaningful only after that.

### Gap 6: Emotional Response Measurement
The research document identifies Norman's three emotional levels (Visceral, Behavioral, Reflective), but the evaluation document only measures Behavioral responses (clicks, times, completions). Visceral and Reflective quality require either survey instruments or physiological measurement — neither is instrumented.

---

## 5. Implementation Recommendations

### Phase 1: Instrument the Telemetry Service
The `ITelemetryService` exists but only tracks one event. Expand it:

```
Recommended initial events (P0 metrics):
- hero_view, hero_cta_click
- signup_start, signup_complete, signup_method
- signin_start, signin_complete, signin_method, signin_error
- adventure_view, adventure_filter, adventure_start
- game_session_start, game_choice_made, game_session_end
- consent_revoke_start, consent_revoke_complete, consent_revoke_error
- wasm_load_time (via Performance API)
```

### Phase 2: Build Dashboards
Group metrics by audience:
- **Product dashboard:** Funnel metrics (conversion, completion, retention)
- **Engineering dashboard:** Performance metrics (load times, error rates, cache hits)
- **Compliance dashboard:** COPPA metrics (consent states, action success rates)

### Phase 3: Establish Baselines
Run all P0 + P1 metrics for 30 days before setting targets. The "acceptable ranges" in the evaluation document are informed estimates — replace them with data-driven ranges.

### Phase 4: Add Qualitative Layer
Once baseline data exists, layer in:
- UEQ survey for parent/facilitator audience (quarterly)
- Expert heuristic evaluation (bi-annually)
- VisAWI evaluation for visual design changes

---

## 6. Metric Lifecycle

Metrics should not be permanent. Each metric should be reviewed quarterly:

| Review Question | Action |
|----------------|--------|
| Is this metric still varying? | If it's been stable at "acceptable" for 3 months, reduce monitoring frequency |
| Has the section changed significantly? | Update or replace metrics that no longer match the UI |
| Are we acting on this data? | If a metric hasn't influenced a decision in 6 months, drop it |
| Is the acceptable range calibrated? | Narrow ranges as baseline data matures |
| Are there new features that need metrics? | Add metrics for new sections/flows |

The goal is a lean, high-signal metrics set — not a sprawling dashboard that nobody reads.
