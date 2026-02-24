# Mystira.Publisher Technical Design Document

## Product Overview

Mystira.Publisher is a modern, single-page web application (SPA) designed to provide a seamless and transparent user experience for collaborative, auditable registration of creative stories and intellectual property (IP) on-chain. It empowers authors, illustrators, publishers, and legal administrators with intuitive UI tools for submitting stories, managing attribution, tracking approvals, and viewing immutable on-chain records.

Mystira.Publisher is a frontend-only React application. All business logic—such as orchestration, workflows, attribution calculations, validation, approval consensus, and audit trails—is handled exclusively by backend APIs and gRPC services. The frontend acts purely as a UI adapter, managing user state, presentation, client-side data orchestration, and error reporting.

---

## Purpose

The frontend's primary function is to interface with robust backend services, enabling:

- **Clear Attribution and Collaboration:** Providing users with tools to submit projects, assign roles, and participate in consensus-driven approvals—while delegating all workflows and rule enforcement to the Mystira APIs.

- **Transparency and Audit:** Displaying up-to-date, immutable registration and contribution histories as received from the APIs or Mystira.Chain via gRPC.

- **User Experience:** Ensuring responsive, accessible, and error-robust interfaces throughout the collaborative IP workflow.

The frontend does not implement or duplicate domain/business rules; instead, it strictly consumes backend contracts, presents data, handles user-triggered events, and delivers API errors intelligibly.

### Example Scenarios

- A creative team initiates a project and assigns contributor splits using the React UI, with each update or approval routed via API/gRPC to backend workflow engines. The frontend reflects the state and feedback provided by these core services.

- Users can browse project histories and prior attributions, with all record immutability and audit integrity guaranteed and managed by backend and blockchain integrations—not the frontend.

---

## Target Audience

### Primary Personas

| Persona | Needs | Pain Points Addressed |
|---------|-------|----------------------|
| Authors/Illustrators | User-friendly, modern interface for collaboration, recognition | Difficult interfaces, confusion |
| Publishers | Visibility into contributor history, streamlined registration | Manual input, fragmented tools |
| Legal Administrators | Audit-friendly UI, quick search and record export | Tedious review, lack of transparency |
| Platform Operators | Efficient user onboarding, low support overhead | Onboarding friction, UX inconsistencies |

The UI is designed to serve these groups by presenting and orchestrating backend-driven flows and data.

---

## Expected Outcomes

### Benefits (Frontend-Focused)

- Intuitive and accessible UI empowering creative contributors, publishers, and administrators
- Reliable representation of backend-defined workflows and state
- Clear, actionable error messaging from backend responses
- Responsive SPA for seamless navigation across collaborative scenarios

### Key Metrics (UI/UX & API Interaction)

- User registration and project submission rates
- API error rates and front-end crash-free sessions
- User journey drop-off and funnel analytics
- Accessibility compliance scores
- End-to-end scenario completion without backend failures

---

## Design Details

Mystira.Publisher is a React SPA directly interfacing with existing backend APIs:

- **Admin API (REST/gRPC):** Project, attribution, user, and audit management
- **Public API:** Story browsing, viewing, and public attribution lookups
- **Mystira.Chain (gRPC):** Blockchain registration, immutable audit

### Key Design Principles

- **No Business Logic Duplication:** All orchestration, workflow management, consensus, and validation are backend-only.
- **Frontend as UI Adapter:** Handles client state, UX, input validation (where possible), error display, optimistic updates, and API orchestration.
- **Modern SPA:** Fast, responsive navigation with real-time updates where supported (WebSockets or gRPC streams).

### High-Level Workflow

1. **Data Fetch/Mutation:** React components call API endpoints for reads and writes.
2. **Backend Orchestration:** API/gRPC backends perform role assignment, attribution splits, approval workflows, and blockchain registrations.
3. **Frontend Presentation:** UI reflects current state, errors, and status exactly as returned by API/gRPC. No logic inferred or duplicated in React.
4. **User Experience Adjustment:** State, loading, error, and notifications are managed client-side for smooth journeys.

There is no hexagonal/domain layering within the frontend; all contracts and business flows are owned by the backend.

---

## Architectural Overview

### Core Architecture

| Component | Responsibility | Technology |
|-----------|---------------|------------|
| React SPA | UX/UI, state, API data-fetch/presentation, error handling | React, React Router, Hooks |
| API Clients | Fetch/mutate via REST/gRPC, handle auth, parse errors | Axios/grpc-web, JWT OAuth |
| State | Represents user and workflow state as mirrored from backend | React Context/Redux/Zustand |
| Backend APIs | Full business logic, validation, consensus | .NET REST APIs, gRPC |

- React is concerned solely with user interaction and API consumption.
- All workflows, data validation, and state transitions occur server-side.
- No domain-layer or "hexagonal" abstraction is implemented client-side.

### Data Flow

```
User → React UI → API Client → (.NET Admin API | Public API | Mystira.Chain gRPC) → Backend → [Blockchain / DB]
```

All responses (successes, failures, updated resource states) are rendered in the frontend as read from the API/gRPC.

### API Contract

- **Strongly typed interfaces:** Frontend models mirror backend API schemas (via Swagger/OpenAPI, or gRPC proto files).
- **Strict versioning:** UI expects backend to evolve contracts via additive changes with proper deprecation signals.

---

## Data Structures

The frontend uses only lightweight data models that exactly reflect backend contracts, for display and input purposes. No local domain logic.

| Model | Description (from Backend) | Fields (as consumed by frontend) |
|-------|---------------------------|----------------------------------|
| Story | Project metadata and attribution/state | id, title, summary, contributors[], status, timestamps |
| User | Authenticated user info, project role | id, name, email, roles |
| Attribution | Contributor's split, role, approval state | storyId, userId, role, split, approvalStatus |
| AuditLog | Immutable event log entry, from backend | eventType, actor, timestamp, details |

Frontend models are strictly for typing, rendering, and form handling—not enforcing or duplicating business logic.

---

## System Interfaces

- **Admin API:** Secure, authenticated endpoints for project creation, role management, attributions, approvals, and querying audit/event logs.
- **Public API:** Exposes stories, attribution history, project lookups, and public-facing datasets.
- **Mystira.Chain (gRPC):** Blockchain-based event/registration services for surfacing immutable state.

Frontend interacts strictly via these APIs' public contracts.

---

## User Interface

### SPA Flows

- **Authentication:** User logs in using OAuth/JWT; session tokens delegated to API clients.
- **Project Management:** List, create, update, and view projects and stories—fetch and display from backend.
- **Attribution/Split:** UI forms for attribution input, with results posted to the API; confirmation/status surfaced.
- **Approval/Consensus:** Users submit approvals or rejections; consensus state reflected as returned from backend.
- **Audit & History:** Render logs/changes exactly as streamed from backend/Mystira.Chain.
- **Notifications:** Display API-driven feedback for status, errors, and next actions.

### UX Details

- Loading, error, and empty states handled gracefully.
- Inline validation for required inputs (well before submit to API).
- Accessibility via ARIA roles, keyboard shortcut support, and WCAG compliance.
- Responsive layout for all device sizes.

---

## Hardware/Integration Interfaces

- **Browser-Only:** Universal, responsive design—no native/hardware dependencies.
- **Wallet Integration:** (If needed) Managed via standard browser wallet JS libraries; backend manages all on-chain interactions.
- **File Uploads:** User files are sent directly to API endpoints; no on-device processing beyond standard pre-validation.

---

## Testing Plan

Frontend testing prioritizes API contract validation, UI/UX flows, accessibility, and error state presentation.

### Testing Scope

- **API Contract Testing:** Mocks/stubs for REST/gRPC endpoints to ensure frontend aligns with backend expectations.
- **End-to-End (E2E) Flows:** Automated user journey simulation (login, create, approve, audit) using Cypress or Playwright.
- **Mocked Interaction:** Local development/test environments use API mocks based on recorded contracts.
- **Accessibility & UX:** Automated a11y tests (axe), manual keyboard/screen reader reviews.

### Testing Strategy

- **Unit Tests:** Components, hooks, and logic isolated from live APIs (Jest, React Testing Library).
- **Integration Tests:** Simulate API responses to verify all UI edge cases, error handling, and state transitions.
- **E2E Scenarios:** Cypress-driven tests mimic real user behavior, covering full workflow paths.

| Tool/Framework | Use Case | Justification |
|----------------|----------|---------------|
| Jest/React Testing Library | Unit/component tests, UI logic | Leading React test solutions |
| Cypress | End-to-end, cross-browser test flows | Widely adopted, robust selector support |
| msw.js/Mock Service Worker | API mocking/stubbing for development and CI | Accurate contract mocking, ease of use |
| axe-core/jest-axe | Accessibility audits and linting | Automated, comprehensive a11y coverage |

### Testing Environments

- **Local Development:** React dev server, API mocks, and hot-reload tools for rapid feedback.
- **CI Pipeline:** PR-triggered tests (unit, integration, contract, E2E) with mocked and live staging backends.
- **Staging:** Full stack with test API endpoints against realistic databases/blockchain test networks.
- **Production:** Read-only synthetic user journeys with browser automation and real API monitoring.

### Test Cases

| Test Case | Coverage | Expected Outcome |
|-----------|----------|------------------|
| Project Creation from UI | Auth → form input → POST API | Project appears in user dashboard |
| Attribution Form Submission | Valid/invalid input, error display | Accurate error messages, valid post |
| Consensus Approval Flow | Approve/reject paths, state updates | Workflow reflects backend consensus |
| API Error Handling | Empty, 4xx, 5xx, offline, slow API | User sees actionable UI errors |
| Audit Log Browsing | Fetch paged logs, filter, edge cases | Correct ordering, empty states shown |
| Accessibility (a11y) Navigation | Tab, screen reader, mobile input | WCAG compliance, no accessibility regressions |

### Reporting & Metrics

- **Frontend Analytics:** Track user interactions, funnels, drop-offs (Google Analytics, Segment).
- **API Monitoring:** Capture and alert on client-side errors (Sentry, LogRocket).
- **Test Coverage:** CI pipeline tracks unit/integration/E2E coverage for all major workflows.
- **Usability Feedback:** In-app reporting funnels user suggestions/crashes to triage dashboards.

---

## Deployment Plan

Deployment focuses on robust, repeatable React app delivery and CI/CD hygiene.

### CI/CD Pipelines

- PR-based lint/testing gates
- Automated Docker build and push for the React SPA
- Deployment to cloud/static host (Netlify, Vercel, S3+CloudFront, Azure Static Web Apps)

### Versioning

- Semantic versioning for UI builds, surfacing backend API compatibility in release notes

### Monitoring

- Rollout health checks and canary deploys
- Real-time error and usage monitoring

| Tool/Framework | Purpose | Rationale |
|----------------|---------|-----------|
| GitHub Actions | Automated CI/CD | Fast setup, wide integration |
| Netlify/Vercel | Static hosting | Seamless SPA/CDN deploys, SSL |
| Docker | Containerization | Build-time reproducibility |
| Sentry/LogRocket | Error/usage tracking | Deep frontend telemetry, actionable |

### Deployment Steps

#### Preflight

- Automated tests (unit, integration, E2E)
- Lint, accessibility, bundle-size checks

#### Build & Distribute

- Production React build
- Docker image (if needed) or static asset packaging
- Deploy to cloud hosting/CDN

#### Post-Deploy Validation

- Synthetic user journeys (login, list, create, approve) with mocked and live APIs
- Error/404 catch routing validated
- A11y and performance checked in production

#### Rollback

- Previous builds available for fast reversion
- Route-based rollbacks via hosting/CDN configs

### Continuous Deployment

- **Automated PR/Ticket Workflow:** All pushes run through defined test, build, and deploy steps.
- **Integration Branch Environments:** Feature previews for rapid cross-team feedback.
- **Rapid Rollback:** Clear audit/history at each deploy step; API contract version checks.
- **Monitoring Integration:** Frontend alerts surfaced for API downtimes, contract mismatches, or client crashes.

---

## Summary: Frontend as UI Adapter

- No implementation of orchestration, consensus, or business rules exists in the React app.
- All domain, data validation, attribution logic, and workflow state are managed strictly by backend APIs/gRPC.
- The React SPA acts as a consumer and orchestrator only—focusing on presentation, state, API contract adherence, and world-class user experience.
- Changes in domain logic, workflow, or integration are reflected automatically in the UI by updating API contracts—minimizing long-term frontend maintenance, maximizing adaptability, testability, and user impact.
