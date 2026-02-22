# Mystira.Publisher

A modern, collaborative frontend application for transparent on-chain story and intellectual property (IP) registration via the Story Protocol. Mystira.Publisher enables creative teams—authors, illustrators, publishers, and legal representatives—to register their stories with clear attribution, consensus-based approvals, and immutable audit trails.

## 🎯 Overview

Mystira.Publisher provides a streamlined interface for registering stories created in StoryGenerator on-chain. The platform ensures transparent multi-user attribution and royalty splitting, building trust and auditability for collaborative creative teams.

### Key Features

- **Collaborative Registration**: Multi-user attribution with explicit role assignments
- **Royalty Split Management**: Intuitive controls for managing and negotiating royalty shares
- **Consensus Workflow**: Registration requires unanimous contributor approval with override mechanisms
- **On-Chain Publishing**: Direct integration with Mystira.Chain via gRPC for blockchain registration
- **Audit Trail**: Immutable record of all contributor actions, approvals, and registrations
- **Accessible Design**: WCAG-compliant UI with responsive layouts for all device sizes

## 🚀 Technology Stack

- **Frontend Framework**: React 18.3+ with TypeScript
- **Build Tool**: Vite 5.4+
- **Routing**: React Router 6.28+
- **State Management**: Zustand 5.0+
- **API Client**: Axios, gRPC-web
- **Data Fetching**: TanStack React Query 5.60+
- **Form Handling**: React Hook Form 7.53+ with Zod validation
- **Testing**: Vitest, React Testing Library, Cypress
- **Styling**: Modern CSS with accessibility focus
- **Code Quality**: ESLint, Prettier, TypeScript

## 📋 Prerequisites

- Node.js 18+ and npm/yarn/pnpm
- Git

## 🛠️ Getting Started

### Installation

1. Clone the repository:
```bash
git clone https://github.com/phoenixvc/Mystira.Publisher.git
cd Mystira.Publisher
```

2. Install dependencies:
```bash
npm install
```

3. Set up environment variables:
```bash
cp .env.example .env.development
# Edit .env.development with your configuration
```

### Development

Start the development server:
```bash
npm run dev
```

The application will be available at `http://localhost:5173` (or the port specified by Vite).

### Building for Production

Create an optimized production build:
```bash
npm run build
```

Preview the production build locally:
```bash
npm run preview
```

## 🧪 Testing

Run unit and integration tests:
```bash
npm run test
```

Run tests in watch mode:
```bash
npm run test:watch
```

Generate coverage report:
```bash
npm run test:coverage
```

Run end-to-end tests:
```bash
npm run test:e2e
```

Open Cypress test runner:
```bash
npm run test:e2e:open
```

## 🔍 Code Quality

Run ESLint:
```bash
npm run lint
```

Auto-fix linting issues:
```bash
npm run lint:fix
```

Format code with Prettier:
```bash
npm run format
```

Type checking:
```bash
npm run typecheck
```

## 📁 Project Structure

```
Mystira.Publisher/
├── src/
│   ├── api/              # API clients and gRPC integrations
│   ├── components/       # Reusable UI components
│   ├── features/         # Feature-specific modules
│   ├── hooks/            # Custom React hooks
│   ├── pages/            # Page components and routing
│   ├── state/            # State management (Zustand stores)
│   ├── styles/           # Global styles and themes
│   ├── tests/            # Test utilities and setup
│   ├── utils/            # Utility functions and helpers
│   ├── App.tsx           # Root application component
│   ├── Layout.tsx        # Layout wrapper component
│   ├── ProtectedRoute.tsx # Authentication route guard
│   └── main.tsx          # Application entry point
├── cypress/              # E2E test suites
├── docs/                 # Project documentation
│   ├── prd.md           # Product Requirements Document
│   ├── design-doc.md    # Technical Design Document
│   ├── personas.md      # User personas
│   └── customer-journey.md # Customer journey maps
├── public/              # Static assets
├── .env.example         # Environment variables template
├── index.html           # HTML entry point
├── package.json         # Project dependencies and scripts
├── tsconfig.json        # TypeScript configuration
└── vite.config.ts       # Vite configuration
```

## 📚 Architecture

Mystira.Publisher is a frontend-only React SPA that acts as a UI adapter for backend services:

- **No Business Logic Duplication**: All orchestration, workflow management, and validation are handled by backend APIs
- **API-Driven**: Consumes REST and gRPC APIs for all data operations
- **State Reflection**: UI reflects current state exactly as returned by backend services
- **Modern SPA**: Fast, responsive navigation with real-time updates

### Integration Points

- **Admin API**: Project, attribution, user, and audit management
- **Public API**: Story browsing and public attribution lookups
- **Mystira.Chain (gRPC)**: Blockchain registration and immutable audit records
- **StoryGenerator**: Story and project data source

## 👥 User Personas

The platform serves multiple user roles:

- **Primary Author**: Initiates registration and coordinates the team
- **Illustrator**: Contributes visual assets and claims attribution
- **Co-Author**: Participates in content creation
- **Moderator/Editor**: Facilitates consensus and verifies roles
- **Publisher Representative**: Validates institutional participation
- **IP Rights Manager/Legal Counsel**: Ensures compliance and manages disputes
- **Platform Administrator**: Oversees system governance and user management

## 📖 Documentation

Detailed documentation is available in the `docs/` directory:

- **[Product Requirements Document (PRD)](docs/prd.md)**: Comprehensive product specification with user stories, success metrics, and business requirements
- **[Technical Design Document](docs/design-doc.md)**: Architecture, data structures, API interfaces, and testing strategy
- **[User Personas](docs/personas.md)**: Detailed user role descriptions
- **[Customer Journey](docs/customer-journey.md)**: User flow and interaction patterns

## 🔒 Security & Compliance

- No private keys or sensitive PII stored beyond notification requirements
- Role-based access control for publisher, legal, and admin users
- GDPR and data policy compliance
- Immutable audit logs for all actions and transactions

## 🤝 Contributing

1. Create a feature branch from `main`
2. Make your changes following the existing code style
3. Run tests and linting before committing
4. Submit a pull request with a clear description

## 📄 License

[License information to be added]

## 🆘 Support

For questions or issues:
- Open a GitHub issue
- Refer to the documentation in the `docs/` directory
- Contact the development team

## 🎯 Roadmap

### Current: Phase 1 - Core MVP
- Multi-role registration with consensus/override
- Email/handle invitations
- Basic audit and log flows

### Upcoming: Phase 2 - Expanded Persona Support
- Enhanced publisher, legal, and platform admin workflows
- Advanced audit and cross-role tracking
- User feedback integration

### Future: Phase 3 - Scaling & Compliance
- Localization and multilingual support
- Institutional compliance workflows
- Advanced analytics and reporting

---

Built with ❤️ for creators, publishers, and the future of collaborative IP registration.
