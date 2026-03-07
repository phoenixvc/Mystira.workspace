# Mystira Story Generator — Website Sections Inventory

> AI-powered story generation and analysis tool for story designers and operators.
> Built with Blazor WebAssembly + Tailwind CSS.
> Last updated: 2026-03-07

## Application Overview

| Property | Value |
|----------|-------|
| Technology | Blazor WebAssembly (C# / .NET) |
| Location | `packages/story-generator/src/Mystira.StoryGenerator.Web` |
| Hosting | Azure Static Web App |
| Styling | Tailwind CSS 3.x + Component-scoped CSS |
| State management | Service-based (DI) + SSE streaming |
| Authentication | JWT token-based |
| Key features | Dual-mode generation (Classic chat + Agent mode), YAML story structure, version tracking |

## Routes

`/`, `/home`, `/login`, `/story-generator`, `/story/agent`, `/story-library`, `/story-continuity`, `/scenario-dominator-path-analysis`

---

## SIDEBAR NAVIGATION (NavMenu)

- **Primary purpose:** Provide persistent navigation across all Story Generator pages
- **Target audience:** Authenticated story designers and operators
- **Key message:** Quick access to all generation and analysis tools
- **Current layout/structure:**
  - Vertical sidebar with gradient background
  - Brand: "Mystira.StoryGenerator.Web"
  - Hamburger toggle button (mobile responsive)
  - Navigation items with icons:
    - Home
    - Story Continuity
    - Path Analysis
    - AI Story Generator
    - Story Library
  - Active state styling on current page
- **Call-to-action:** Navigation links

---

## HEADER (MainLayout — Top Bar)

- **Primary purpose:** Display auth status and global controls
- **Target audience:** All users
- **Key message:** Quick access to login/logout and system info
- **Current layout/structure:**
  - Top-right section with:
    - Login/Logout buttons (depending on auth state)
    - About link
- **Call-to-action:** Login/Logout buttons

---

## THREE PANEL LAYOUT (ThreePanelLayout — Story Generator Shell)

- **Primary purpose:** Provide the main chat-based story generation workspace
- **Target audience:** Active story creators using the classic mode
- **Key message:** Full-featured AI-assisted storytelling workspace
- **Current layout/structure:**
  - **Header bar:**
    - Sidebar toggle button
    - "Mystira Story Generator" title with animated ALPHA badge (purple pulse)
    - Settings icon (SVG)
    - Help icon (SVG)
    - "Agentic" / "Standard" toggle button (green)
    - "AI Model Settings" button (blue)
  - **Three-panel grid:**
    - **Left Panel:** ChatHistoryPanel (collapsible sidebar)
    - **Center Panel:** ActiveChatPanel (main chat interface)
    - **Right Panel:** YamlPreviewPanel (story structure preview)
  - All panels collapsible for responsive usage
- **Call-to-action:** Toggle Agentic mode; Open AI Model Settings

---

## PAGE: Home (`/`, `/home`)

### Section: Hero

- **Primary purpose:** Welcome users and present the two story generation modes
- **Target audience:** New and returning users choosing a workflow
- **Key message:** "Create engaging, interactive stories powered by AI"
- **Current layout/structure:**
  - Full-width hero section with gradient background (#667eea to #764ba2)
  - Main heading: "Welcome to Mystira Story Generator"
  - Subheading: "Create engaging, interactive stories powered by AI"
  - Two mode selection cards:
    - **Classic Mode** (💬): Chat-based story generation with real-time AI conversation
    - **Agent Mode** (🤖): Advanced AI-driven generation with evaluation and refinement
  - Each card has icon, title, description, and CTA button
- **Call-to-action:** "Start Classic Mode" (white button) and "Start Agent Mode" (purple gradient button)

---

## PAGE: Login (`/login`)

### Section: Login Card

- **Primary purpose:** Authenticate users via JWT token
- **Target audience:** Unauthenticated users needing access
- **Key message:** "Sign in to access story generation tools"
- **Current layout/structure:**
  - Centered card form (500px max-width)
  - Header with logo and tagline
  - Form with:
    - "Authentication Token" label
    - Textarea for JWT token input (4 rows)
    - Helper text with link to Mystira Identity Service
  - Error message alert (if login fails)
  - "Sign In" button (disabled when no token entered)
  - Loading state: Spinner with "Signing in..."
  - Auto-redirects if already authenticated
- **Call-to-action:** "Sign In" button

---

## PAGE: Story Generator (`/story-generator`)

### Section: Active Chat Panel (Center)

- **Primary purpose:** Main conversational interface for AI-assisted story creation
- **Target audience:** Story creators in classic mode
- **Key message:** Interactive, real-time story development through conversation
- **Current layout/structure:**
  - **Panel header:** Chat title + message count + "Clear Chat" button
  - **Messages area** (scrollable):
    - User messages (right-aligned, purple gradient bubble, 👤 avatar)
    - AI messages (left-aligned, gray bubble, 🧙‍♂️ avatar, markdown rendered)
    - System messages (centered, amber background, ℹ️ icon)
    - AI typing indicator (bouncing dots animation)
    - Empty state: "Generate Your Story" prompt with icon
  - **Input area:**
    - Textarea (3 rows) with placeholder
    - Send button (arrow icon)
    - "Generate Random Prompt" button
- **Call-to-action:** Send Message; Generate Random Prompt; Clear Chat

### Section: Chat History Panel (Left)

- **Primary purpose:** Browse and manage previous conversations
- **Target audience:** Returning users revisiting past work
- **Key message:** All your chat sessions are saved and accessible
- **Current layout/structure:**
  - "Chat History" header
  - Scrollable list of chat sessions:
    - Each item: title, creation date, last message preview
    - Delete button (hover-triggered)
  - Empty state with tips
  - "New Chat" button (full-width, primary style)
- **Call-to-action:** Chat item click (load session); Delete; "New Chat"

### Section: YAML Preview Panel (Right)

- **Primary purpose:** Display and edit the generated story's YAML structure
- **Target audience:** Story designers reviewing/editing structured output
- **Key message:** Real-time story structure with version tracking
- **Current layout/structure:**
  - **Header:**
    - "Story Preview (YAML)" title with version selector (e.g., "v1/3")
    - "Show Diff" / "Hide Diff" toggle
    - Diff comparison version selector
  - **Action buttons** (when YAML available):
    - Edit / Save / Cancel toggle
    - Copy to Clipboard
    - Validation indicator
  - **Content area:** YAML code block (read mode) or textarea (edit mode)
  - **Validation results:** Error/warning categories with details
- **Call-to-action:** Edit; Save; Copy to Clipboard; Show Diff

---

## PAGE: Agent Mode Story (`/story/agent`)

### Section: Agent Mode Prompt Form (initial state)

- **Primary purpose:** Collect story generation parameters for the advanced AI agent workflow
- **Target audience:** Users wanting sophisticated, multi-iteration story creation
- **Key message:** "Create a Story with AI" — configure prompt, age group, and knowledge mode
- **Current layout/structure:**
  - "Create a Story with AI" section title
  - Story Prompt textarea (placeholder: "Describe the story you want to create...")
    - Example: "Write a whimsical 800-word bedtime tale about a brave squirrel who learns to share, aimed at ages 6-9, using gentle humor and simple language."
  - Age Group dropdown (Ages 1-2, 3-5, 6-9, 10-12, 13-18)
  - Knowledge Mode radio buttons:
    - File Search (Vector Store)
    - AI Search
  - Submit button: "✨ Start Story" (disabled when prompt is empty)
- **Call-to-action:** "✨ Start Story" button

### Section: Agent Mode Progress Panel (active session)

- **Primary purpose:** Display real-time AI generation progress and evaluation results
- **Target audience:** Users monitoring story generation pipeline
- **Key message:** Live progress tracking through writing → validating → evaluating → complete
- **Current layout/structure:**
  - Iteration badge (shows current iteration count)
  - **Timeline visualization:** Phase markers with status icons:
    - ○ pending, ⏳ in progress, ✅ complete, ❌ failed
    - Phases: Writing → Validating → Evaluating → Evaluated → Complete
  - **Evaluation Scorecard** (when available):
    - Safety Gate: Pass/Fail with ✅/⚠️ indicator
    - Axes Alignment: Progress bar (0–1.0)
    - Dev Principles: Progress bar (0–1.0)
    - Narrative Logic: Progress bar (0–1.0)
  - Key Findings section (categorized list)
  - Rubric Summary (concerns, suggested focus areas)
- **Call-to-action:** "✨ Refine from Rubric"; "✅ Complete"; "📊 Proceed to Rubric"

### Section: Agent Mode Refinement Panel

- **Primary purpose:** Collect refinement instructions for story iteration
- **Target audience:** Users iterating on generated stories
- **Key message:** Fine-tune your story with targeted or full rewrite options
- **Current layout/structure:**
  - "Refine Story" heading
  - Full Rewrite checkbox (toggles between modes):
    - **Targeted Mode:** Scene checkboxes + Aspect checkboxes (dynamically loaded)
    - **Full Rewrite Mode:** Simple text input
  - Additional Instructions textarea
  - Button group:
    - "✨ Refine & Re-evaluate" (primary)
    - "📊 Proceed to Rubric" (secondary)
    - "✅ Complete" (secondary)
- **Call-to-action:** Three refinement action buttons

### Section: Story Display Panel

- **Primary purpose:** Render the generated story in readable format
- **Target audience:** Users reviewing generated content
- **Key message:** Clean, readable story presentation
- **Current layout/structure:**
  - "✨ Generated Story" heading
  - Story title (h1)
  - Story metadata (age group)
  - Scene-by-scene display:
    - Scene title, setting (italics), narrative text, characters list
  - Error alert if JSON parse fails
- **Call-to-action:** "✨ Complete & Continue in Chat" button

### Section: Story Version Diff Viewer

- **Primary purpose:** Compare two story versions side-by-side
- **Target audience:** Users reviewing changes between iterations
- **Key message:** See exactly what changed between story versions
- **Current layout/structure:**
  - "Compare Story Versions" heading
  - Version A / Version B selector dropdowns
  - Two-column preview display
  - Changes summary section
- **Call-to-action:** Version selection dropdowns

---

## PAGE: Story Library (`/story-library`)

### Section: Story Library Browser

- **Primary purpose:** Browse, search, and manage previously generated stories
- **Target audience:** Users revisiting or managing old stories
- **Key message:** "Browse and revisit your previously generated narratives"
- **Current layout/structure:**
  - **Header:** Title + control buttons
  - Safety notice banner: "Child safety filters are active on all historical content" (amber)
  - Search input (real-time filtering by title)
  - **Story cards grid** (responsive, min 280px per card):
    - Each card shows:
      - Story icon (📖)
      - Badges: "Story" label + message count
      - Story title
      - Creation date
      - Last updated date (if different from creation)
      - "Open Story →" footer link
  - **Empty state:** Book icon + prompt to create first story
  - Loading spinner state
  - Display limit: 10 stories per view
- **Call-to-action:** "Clear History" button; "+ New Story" button (primary gradient); Story card click

---

## PAGE: Story Continuity Analyzer (`/story-continuity`)

### Section: Continuity Analysis Interface

- **Primary purpose:** Analyze stories for narrative consistency issues (character, location, item references)
- **Target audience:** Advanced users QA-ing their stories for continuity errors
- **Key message:** AI-powered detection of narrative inconsistencies
- **Current layout/structure:**
  - Page header with back link
  - **Controls section** (light gray background):
    - Active story info display
    - Error message (if no active scenario)
    - **Provider Configuration** (2-column grid):
      - Prefix Summary Provider/Model selectors
      - Semantic Role Labelling Provider/Model selectors
    - **Filters** (3-column grid on desktop):
      - Issue Types checkboxes (5 types)
      - Confidence Levels: High / Medium / Low
      - Entity Types: Character / Location / Item / Concept
      - "Proper Nouns Only" checkbox
    - "Analyze Story" button (disabled while analyzing)
  - **Results section** (if analysis complete):
    - "Continuity Issues (count)" heading
    - Issues grid (responsive, min 400px per card):
      - Entity name with issue type badge
      - Scene, entity type, confidence level (color-coded)
      - Detailed description
      - Evidence span (quoted text from story)
    - Success message if no issues found
- **Call-to-action:** "Analyze Story" button

---

## PAGE: Scenario Dominator Path Analysis (`/scenario-dominator-path-analysis`)

### Section: Path Analysis Interface

- **Primary purpose:** Analyze branching narrative paths and story dominance patterns
- **Target audience:** Interactive fiction designers optimizing story structure
- **Key message:** Understand path dominance and narrative branching quality
- **Current layout/structure:**
  - Page header with back link
  - **Controls section:**
    - Active scenario info
    - Error messages
    - Provider/Model selection dropdowns
    - "Analyze Story Paths" button
  - **Results section** (if analysis complete):
    - "Path Analysis Results (count)" heading with export button
    - Grid of path cards:
      - Path title, assessment, dominance scores
- **Call-to-action:** "Analyze Story Paths" button; "📥 Export to CSV" button

---

## UTILITY COMPONENTS

### Provider Settings Modal

- **Purpose:** Configure AI model selection and generation parameters
- **Layout:**
  - "AI Provider Settings" header with refresh button
  - Provider dropdown (required)
  - Model dropdown (dependent on provider selection)
  - Model info text
  - Temperature slider (0–2, labeled "Focused" to "Creative")
  - Max Tokens input (1–25,000)
  - Error alert
  - Provider status indicator (available/unavailable)

### YAML Generator Modal

- **Purpose:** Generate YAML story structure from chat history
- **Layout:**
  - "Generate YAML Story Structure" header with close button
  - States: Generating (spinner) → Generated (YAML preview + validation) → Empty (prompt)
  - Footer: Generate / Regenerate / Save buttons + validation status

### Markdown Renderer

- **Purpose:** Convert markdown to styled HTML for AI message display
- **Features:** Headings, lists, code blocks, tables, blockquotes, links

---

## STYLING ARCHITECTURE

### Color Scheme

| Color | Hex | Usage |
|-------|-----|-------|
| Primary gradient start | `#667eea` | Hero, user messages, buttons |
| Primary gradient end | `#764ba2` | Hero, user messages, buttons |
| AI message bg | `#f9fafb` | AI response bubbles |
| System message bg | `#fef3c7` | System notifications |
| Agentic toggle | `#10b981` | Green toggle button |
| Settings button | `#2563eb` | Blue settings button |
| Danger | `#dc3545` | Delete/clear actions |

### CSS Framework

- **Tailwind CSS** 3.4.18 as primary utility framework
- **Component-scoped CSS** in each `.razor` component
- **System fonts:** Arial, system-ui, -apple-system, sans-serif
- **Responsive:** Mobile-first with media queries

---

## SERVICE LAYER

| Service | Purpose |
|---------|---------|
| `IChatService` | Chat message handling and AI communication |
| `IStoryGenerationService` | Story generation orchestration |
| `IYamlService` | YAML generation and validation |
| `IProviderService` | AI provider configuration |
| `IAuthService` | JWT token authentication |
| `IContinuityService` | Story continuity analysis |
| `IPathAnalysisService` | Scenario path analysis |

### Real-time Communication

- **SSE (Server-Sent Events)** for streaming agent mode progress
- Real-time phase updates, evaluation scores, and generation events
