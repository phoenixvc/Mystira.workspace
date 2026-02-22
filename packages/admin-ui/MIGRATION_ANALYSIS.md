# Admin UI Migration Analysis

## Current Implementation (Mystira.App.Admin.Api)

### Technology Stack
- **Framework**: ASP.NET Core with Razor Pages
- **UI Library**: Bootstrap 5.3.0
- **Icons**: Bootstrap Icons 1.11.0
- **Authentication**: Cookie-based authentication
- **Styling**: Custom `admin.css` + Bootstrap

### Structure
```
Mystira.App.Admin.Api/
├── Views/
│   ├── Admin/
│   │   ├── Dashboard.cshtml
│   │   ├── Login.cshtml
│   │   ├── Scenarios.cshtml
│   │   ├── EditScenario.cshtml
│   │   ├── Media.cshtml
│   │   ├── MediaMetadata.cshtml
│   │   ├── Badges.cshtml
│   │   ├── BadgeImages.cshtml
│   │   ├── ImportBadges.cshtml
│   │   ├── Bundles.cshtml
│   │   ├── ImportBundle.cshtml
│   │   ├── CharacterMaps.cshtml
│   │   ├── ImportCharacterMap.cshtml
│   │   ├── CharacterMediaMetadata.cshtml
│   │   ├── AvatarManagement.cshtml
│   │   ├── AppStatus.cshtml
│   │   ├── AgeGroups.cshtml
│   │   ├── Archetypes.cshtml
│   │   ├── CompassAxes.cshtml
│   │   ├── EchoTypes.cshtml
│   │   ├── FantasyThemes.cshtml
│   │   └── ImportScenario.cshtml
│   └── Shared/
│       ├── _AdminLayout.cshtml
│       └── _Layout.cshtml
├── Controllers/
│   └── [Various Admin Controllers]
└── wwwroot/
    └── css/
        └── admin.css
```

### Key Features Identified
1. **Dashboard**: Content statistics and overview
2. **Scenarios Management**: CRUD operations for scenarios
3. **Media Management**: Upload, view, and manage media files
4. **Badges Management**: Badge configuration and import
5. **Bundles Management**: Content bundle management
6. **Character Maps**: Character mapping management
7. **Master Data**: Age Groups, Archetypes, Compass Axes, Echo Types, Fantasy Themes
8. **Authentication**: Login/logout functionality
9. **Import Features**: Bulk import for various content types

### API Endpoints (from Controllers)
- `/admin` - Dashboard
- `/admin/login` - Login page
- `/admin/scenarios` - Scenarios list
- `/admin/media` - Media management
- `/admin/badges` - Badges management
- `/admin/bundles` - Bundles management
- `/admin/charactermaps` - Character maps
- Various API endpoints for CRUD operations

## Target Implementation (Mystira.Admin.UI)

### Recommended Technology Stack
- **Framework**: React 18+ with TypeScript
- **Build Tool**: Vite
- **UI Library**: 
  - Option A: React + Bootstrap 5 (easiest migration, keep existing styling)
  - Option B: React + Tailwind CSS (modern, better DX)
  - Option C: Next.js 14 (if SSR needed, but likely not for admin)
- **State Management**: Zustand or React Query
- **API Client**: Axios or Fetch API
- **Routing**: React Router v6
- **Forms**: React Hook Form + Zod validation
- **Icons**: Bootstrap Icons (keep existing) or React Icons

### Migration Strategy

#### Phase 3.1: Project Setup
1. Initialize React + TypeScript + Vite project
2. Set up project structure
3. Configure build tools and dev server
4. Set up routing structure

#### Phase 3.2: Core Infrastructure
1. Create API client for Admin API
2. Set up authentication service (JWT/cookie handling)
3. Create layout components (navbar, sidebar)
4. Set up protected routes

#### Phase 3.3: Page Migration
1. Login page
2. Dashboard
3. Scenarios management
4. Media management
5. Badges management
6. Bundles management
7. Character maps
8. Master data pages
9. Import pages

#### Phase 3.4: Styling & Assets
1. Port admin.css styles
2. Set up Bootstrap or Tailwind
3. Migrate icons
4. Ensure responsive design

## Next Steps

1. **Choose Framework**: Recommend React + Vite + TypeScript for fastest migration
2. **Set up Project**: Initialize in `packages/admin-ui`
3. **Create API Client**: Generate TypeScript client from Admin API OpenAPI spec
4. **Start Migration**: Begin with Login and Dashboard pages
