# Mystira DevHub — Website Sections Inventory

> Desktop developer portal for infrastructure management, service orchestration, and data operations.
> Built with Tauri (Rust backend) + React + TypeScript.
> Last updated: 2026-03-07

## Application Overview

| Property | Value |
|----------|-------|
| Technology | Tauri 2.x (Rust) + React + TypeScript |
| Location | `packages/devhub/Mystira.DevHub` |
| Runtime | Desktop application (native via Tauri) |
| Styling | Tailwind CSS |
| State management | Zustand (connections, deployments, resources, settings, smart deployment) |
| Communication | Tauri IPC (invoke commands to Rust backend) |
| Layout | VS Code-inspired layout (activity bar, sidebar, bottom panel, status bar) |

## Views (Activity Bar Navigation)

| Icon | View | Purpose |
|------|------|---------|
| ⚡ | Services | Manage local and deployed services |
| 📊 | Dashboard | Overview and quick actions |
| 🔮 | Cosmos Explorer | Explore Azure Cosmos DB |
| 🔄 | Migration Manager | Database migration tools |
| ☁️ | Infrastructure | Deploy and manage Azure resources |
| 🧪 | Test | Run and view test results |

---

## APPLICATION SHELL (VSCodeLayout)

- **Primary purpose:** Provide an IDE-like workspace layout familiar to developers
- **Target audience:** Developers and DevOps engineers
- **Key message:** Professional, dense tooling interface modeled after VS Code
- **Current layout/structure:**
  - **Activity Bar** (left edge): Vertical icon strip for view switching (Services, Dashboard, Cosmos, Migration, Infrastructure, Test)
  - **Primary Sidebar** (left, resizable, collapsible): Context-sensitive panel with view description and quick actions
  - **Main Content Area** (center): Active view content
  - **Bottom Panel** (resizable, collapsible): Tabbed output panel with tabs:
    - Output (global logs)
    - Deployment Logs
    - Problems
  - **Status Bar** (bottom): Blue bar with:
    - Left: Green dot + "MYSTIRA DEVHUB" + environment summary
    - Right: Dark/light mode toggle (☀️/🌙) + version (v1.0.0)
  - All panel sizes persist to localStorage

---

## SIDEBAR (AppSidebar — Context-Sensitive)

- **Primary purpose:** Provide view-specific quick actions and contextual information
- **Target audience:** Developers navigating between DevHub tools
- **Key message:** Relevant actions always one click away
- **Current layout/structure:**
  - **Header:** View icon + view title (uppercase) + description text
  - **Quick Actions section** (varies by view):
    - **Services:** "Start All Services", "Stop All Services", "Refresh Status"
    - **Infrastructure:** "Deploy to Azure", "View Resources"
    - **Cosmos:** "Connect Database", "New Query"
    - **Migration:** "New Migration", "Run Pending"
    - **Dashboard:** "Refresh Data"
  - **Services list** (Services view only): Lists all service configs with environment badges (LOCAL/DEV/PROD, color-coded)
- **Call-to-action:** Quick action buttons per view

---

## BOTTOM PANEL (Output / Logs)

- **Primary purpose:** Display real-time logs, deployment output, and problem diagnostics
- **Target audience:** Developers monitoring operations
- **Key message:** Full visibility into system output and errors
- **Current layout/structure:**
  - Tabbed interface:
    - **Output tab:** Global application logs with filter controls (search, type, source, severity) + auto-scroll toggle + clear button
    - **Deployment Logs tab:** Deployment-specific log output
    - **Problems tab:** Issue/error list with count badge
  - Log entries with severity coloring, timestamps, source labels
  - Resizable panel height

---

## VIEW: Services (ServiceManager)

### Section: Service Manager

- **Primary purpose:** Manage all Mystira microservices — start, stop, monitor, and configure
- **Target audience:** Developers running the platform locally or managing deployments
- **Key message:** Central control panel for all Mystira services
- **Current layout/structure:**
  - **Header** (ServiceManagerHeader):
    - Title + service count
    - Repository configuration
    - Environment switcher (Local/Dev/Prod presets)
    - View mode selector (card/table/grouped)
  - **Environment Banner:** Visual indicator of current environment with warnings for production
  - **Service Cards/List** (ServiceList):
    - Each service card shows:
      - Service name + status indicator (running/stopped/error)
      - Build status indicator
      - Environment badge
      - Deployment info (last deployed, version)
      - Controls: Start/Stop/Restart/Build buttons
      - Expandable: logs viewer, webview, deployment info
    - Collapsed view: Compact single-row per service
    - Table view: Tabular layout with sortable columns
    - Grouped view: Organized by category
  - **Logs Viewer** (per-service):
    - Real-time log streaming
    - Filter bar (search, severity)
    - Log grouping
    - Auto-scroll
  - **WebView Panel:** Embedded browser for service web UIs
- **Call-to-action:** Start/Stop/Restart/Build buttons per service; "Start All" / "Stop All"

---

## VIEW: Dashboard

### Section: Dashboard Header

- **Primary purpose:** Welcome developers and provide operational overview
- **Target audience:** Developers checking system health
- **Key message:** "Welcome to Mystira DevHub — Your central hub for development operations and data management"
- **Current layout/structure:**
  - "Welcome to Mystira DevHub" heading (text-4xl)
  - Subtitle: "Your central hub for development operations and data management"

### Section: Connection Status

- **Primary purpose:** Display connectivity status for all backend services
- **Target audience:** Developers verifying service availability
- **Key message:** At-a-glance health of all connections
- **Current layout/structure:**
  - "Connection Status" heading
  - 4-column responsive grid of connection cards:
    - Each card: Service icon + name + status icon (✓ connected / ✗ disconnected / ⏳ checking)
    - Color-coded borders (green=connected, red=disconnected, yellow=checking)
    - Connection details and error messages
  - Auto-tests connections on mount
- **Call-to-action:** None (auto-refreshing)

### Section: Quick Actions

- **Primary purpose:** Provide shortcuts to common development operations
- **Target audience:** Developers performing routine tasks
- **Key message:** Common tasks always one click away
- **Current layout/structure:**
  - "Quick Actions" heading
  - 3-column responsive grid of gradient action cards:
    1. **Export Sessions** (📤, blue) — "Export game sessions to CSV" → navigates to Cosmos
    2. **View Statistics** (📊, green) — "Scenario completion analytics" → navigates to Cosmos
    3. **Run Migration** (🔄, purple) — "Migrate data between environments" → navigates to Migration
    4. **Validate Infrastructure** (🔍, yellow) — "Check Bicep templates" → navigates to Infrastructure
    5. **Deploy Infrastructure** (🚀, red) — "Deploy to Azure via GitHub Actions" → navigates to Infrastructure
    6. **View Bicep Files** (📄, indigo) — "Browse infrastructure templates" → navigates to Infrastructure
  - Hover: Scale up (105%) + shadow
- **Call-to-action:** Each card navigates to the relevant view

### Section: Recent Operations

- **Primary purpose:** Show history of recent DevHub operations
- **Target audience:** Developers tracking their recent activity
- **Key message:** Audit trail of development operations
- **Current layout/structure:**
  - "Recent Operations" heading + "View All →" link
  - List of operation entries:
    - Each entry: Operation icon + title + status badge (OperationStatusBadge) + details + relative timestamp
  - Empty state: "No recent operations"
- **Call-to-action:** "View All →" link

### Section: System Info Cards

- **Primary purpose:** Provide helpful context about DevHub capabilities
- **Target audience:** New or infrequent DevHub users
- **Key message:** Tips, documentation, and performance info
- **Current layout/structure:**
  - 3-column grid of info cards:
    1. **Tips & Tricks** (💡, blue gradient) — "Use Quick Actions above for common operations, or navigate via the sidebar for advanced features"
    2. **Documentation** (📚, green gradient) — "Check the README in tools/Mystira.DevHub for complete setup and usage guides"
    3. **Performance** (⚡, purple gradient) — "DevHub is built with Tauri for native performance and minimal resource usage"
- **Call-to-action:** None (informational)

---

## VIEW: Cosmos Explorer (CosmosExplorer)

### Section: Cosmos DB Explorer

- **Primary purpose:** Browse, query, and manage Azure Cosmos DB data
- **Target audience:** Developers working with Mystira's database
- **Key message:** Direct database access for development and debugging
- **Current layout/structure:**
  - **Preview Warning** (CosmosDbPreviewWarning): Feature preview notice
  - **Database browser:** Container/collection tree navigation
  - **Query interface:** Query input and execution
  - **Results display:** JSON viewer for query results
  - **Export panel:** Export data functionality
- **Call-to-action:** Connect, query, export data

---

## VIEW: Migration Manager (MigrationManager)

### Section: Migration Interface

- **Primary purpose:** Migrate data between Cosmos DB environments (local → dev → prod)
- **Target audience:** Developers and DevOps managing data across environments
- **Key message:** Safe, controlled data migration between environments
- **Current layout/structure:**
  - **Environment Selector** (EnvironmentSelector): Source and destination environment pickers
  - **Migration Config Form** (MigrationConfigForm): Configuration options for migration
  - **Resource Selection** (ResourceSelectionForm): Select which resources/collections to migrate
  - **Migration Progress** (MigrationProgress): Real-time progress indicators
  - **Step Indicator** (MigrationStepIndicator): Multi-step workflow visualization
  - **Migration Results** (MigrationResults): Post-migration summary and validation
- **Call-to-action:** "New Migration", "Run Pending", step navigation buttons

---

## VIEW: Infrastructure (InfrastructurePanel)

### Section: Infrastructure Management

- **Primary purpose:** Deploy, validate, and manage Azure infrastructure using Bicep templates
- **Target audience:** DevOps engineers managing Mystira's cloud infrastructure
- **Key message:** Full infrastructure lifecycle management from the desktop
- **Current layout/structure:**
  - **Panel Header** (InfrastructurePanelHeader): Title + action controls
  - **Resource Group Config** (ResourceGroupConfig): Azure resource group selection
  - **Tabbed Interface** (InfrastructureTabs):
    - **Actions Tab:** Deployment actions (validate, what-if, deploy)
    - **Resources Tab:** Live Azure resource listing with status
    - **Templates Tab:** Bicep template browser and editor
    - **History Tab:** Deployment history
    - **Recommended Fixes Tab:** Suggested infrastructure improvements
  - **Smart Deployment Panel** (SmartDeploymentPanel):
    - Prerequisites check
    - Configuration panel
    - Git operations
    - Resource discovery
    - Deployment decision logic
    - What-If preview
    - Deployment progress stepper
  - **Bicep Viewer** (BicepViewer): Template preview and validation
  - **What-If Viewer** (WhatIfViewer): Preview deployment changes before applying
    - Change items with summary
    - Warnings display
  - **Resource Grid** (ResourceGrid): Azure resource visualization
    - Grid/Table/Grouped views
    - Resource cards with status
    - Summary statistics
  - **Deploy Now workflow:**
    - Prerequisites check
    - Configuration
    - Git operations
    - Resource discovery
    - Smart deployment decision
  - **Output panels:** Response display, build logs, workflow status
  - **Confirm dialogs:** Destroy resource confirmation
  - **Ready to Deploy banner:** Visual indicator when deployment is ready
- **Call-to-action:** "Deploy to Azure", "Validate", "What-If", "Destroy", action buttons per tab

---

## VIEW: Test (Placeholder)

### Section: Test Panel

- **Primary purpose:** Future test runner and results viewer
- **Target audience:** Developers running tests
- **Key message:** "Test runner and test results will be displayed here."
- **Current layout/structure:**
  - Simple placeholder with heading "Test Panel" and description text
- **Call-to-action:** None (not yet implemented)

---

## STYLING ARCHITECTURE

### CSS Framework

- **Tailwind CSS** as primary utility framework
- Dark mode via `dark:` prefix classes
- `useDarkMode` hook for toggle and localStorage persistence

### Color Scheme

| Context | Color |
|---------|-------|
| Activity bar | `bg-gray-900` |
| Sidebar | `bg-gray-800` with `border-gray-700` |
| Main content | `bg-gray-900` (dark) / white (light) |
| Status bar | `bg-blue-600` |
| Success | `text-green-500` / `bg-green-100` |
| Error | `text-red-500` / `bg-red-100` |
| Warning | `text-yellow-500` / `bg-yellow-100` |
| Prod env badge | `bg-red-900/50 text-red-300` |
| Dev env badge | `bg-blue-900/50 text-blue-300` |
| Local env badge | `bg-gray-700 text-gray-400` |

### Layout Constants

- Activity bar width: 48px
- Primary sidebar: Resizable (persisted)
- Bottom panel: Resizable (persisted)
- All panel states persist to localStorage

---

## STATE MANAGEMENT (Zustand Stores)

| Store | Purpose |
|-------|---------|
| `connectionStore` | Service connection status tracking and testing |
| `deploymentsStore` | Deployment state, history, and progress |
| `resourcesStore` | Azure resource discovery and management |
| `settingsStore` | App settings and configuration |
| `smartDeploymentStore` | Smart deployment workflow state machine |

---

## COMPONENT INVENTORY

### App Shell (4)

`VSCodeLayout`, `AppContent`, `AppSidebar`, `AppBottomPanel`

### VS Code Layout Components (6)

`ActivityBar`, `Sidebar`, `SidebarPanel`, `BottomPanel`, `OutputPanel`, `TreeItem`

### Dashboard (2)

`Dashboard`, `StatisticsPanel`

### Service Manager (15+)

`ServiceManager`, `ServiceManagerHeader`, `ServiceList`, `ServiceCard`, `ServiceControls`, `ServiceCardHeader`, `ServiceCardControls`, `ServiceCardStatusRow`, `ServiceCardDeploymentInfo`, `LogsViewer`, `LogDisplay`, `LogFilterBar`, `LogGroup`, `LogLine`, `EnvironmentSwitcher`, `EnvironmentBanner`, `EnvironmentPresetSelector`, `ViewModeSelector`, `WebviewView`, `RepositoryConfig`, `BuildStatusIndicator`, `DeploymentInfo`

### Cosmos Explorer (2)

`CosmosExplorer`, `CosmosDbPreviewWarning`

### Migration Manager (6)

`MigrationManager`, `EnvironmentSelector`, `MigrationConfigForm`, `MigrationProgress`, `MigrationResults`, `MigrationStepIndicator`, `ResourceSelectionForm`

### Infrastructure (20+)

`InfrastructurePanel`, `InfrastructureStatus`, `ResourceGroupConfig`, `InfrastructurePanelHeader`, `InfrastructureTabs`, `InfrastructureActionsTab`, `InfrastructureResourcesTab`, `InfrastructureTemplatesTab`, `InfrastructureHistoryTab`, `InfrastructureRecommendedFixesTab`, `SmartDeploymentPanel`, `DeploymentProgress`, `InfrastructureProgressStepper`, `InfrastructureActionButtons`, `InfrastructureConfirmDialogs`, `InfrastructureOutputPanel`, `InfrastructureResponseDisplay`, `ReadyToDeployBanner`, `WorkflowStatusDisplay`, `ConfigurationPanel`, `GitOperationsPanel`, `PrerequisitesCheckPanel`, `ResourceDiscoveryPanel`, `SmartDeployDecision`, `CliBuildLogsViewer`

### Resource Grid (6)

`ResourceGrid`, `ResourceGridHeader`, `ResourceGridSummary`, `ResourceGridView`, `ResourceGroupedView`, `ResourceTableView`, `ResourceCard`

### What-If Viewer (4)

`WhatIfViewer`, `WhatIfChangeItem`, `WhatIfSummary`, `WhatIfWarnings`

### Templates (3)

`TemplateEditor`, `TemplateInspector`, `TemplateSelector`

### Project Deployment (7)

`ProjectDeployment`, `ProjectDeploymentPlanner`, `ProjectCard`, `ProjectDeploymentCard`, `ProjectDeploymentHeader`, `ProjectDeploymentPlannerHeader`, `ProjectDeploymentSummary`, `DeploymentHistory`, `ResourceCheckbox`, `WorkflowLogsViewer`

### UI Primitives (12)

`Button`, `TabbedPanel`, `Loading`, `Feedback`, `Alert`, `ErrorDisplay`, `OperationStatusBadge`, `ProgressBar`, `StatusBadge`, `StatusIndicator`, `SuccessDisplay`, `ToastContainer`, `ToastItem`

### Utility Components (6)

`BicepViewer`, `JsonViewer`, `ExportPanel`, `WebViewPanel`, `ConfirmDialog`, `DestroyButton`, `TruncatedId`, `ErrorBoundary`, `LiveRegion`, `VisuallyHidden`

---

## TAURI INTEGRATION

DevHub uses Tauri IPC to invoke Rust backend commands for:

- Starting/stopping local services
- Azure CLI operations (resource management, deployment)
- Cosmos DB connectivity and queries
- File system operations (Bicep template reading)
- Git operations (branch detection, commit info)
- GitHub Actions workflow triggers
- Environment configuration management

All heavy operations run on the Rust backend for native performance, with the React frontend providing the UI layer.
