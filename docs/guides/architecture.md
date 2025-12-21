# Architecture Overview

This document provides a high-level overview of the Mystira platform architecture.

> **Infrastructure Details**: For detailed infrastructure organization, deployment models, and coordination, see [Infrastructure Guide](./infrastructure/infrastructure.md)

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Mystira Platform                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────────────────┐ │
│  │   Web App   │  │ Mobile App  │  │      Admin Dashboard     │ │
│  │  (Next.js)  │  │ (React Nav) │  │      (Next.js)           │ │
│  └──────┬──────┘  └──────┬──────┘  └────────────┬─────────────┘ │
│         │                │                      │               │
│         └────────────────┼──────────────────────┘               │
│                          │                                      │
│                   ┌──────▼──────┐                               │
│                   │   API Layer  │                               │
│                   │  (GraphQL/   │                               │
│                   │   REST API)  │                               │
│                   └──────┬──────┘                               │
│         ┌────────────────┼────────────────┐                     │
│         │                │                │                     │
│  ┌──────▼──────┐  ┌──────▼──────┐  ┌──────▼──────┐             │
│  │Story Engine │  │  Chain/Web3 │  │   Services  │             │
│  │   (AI)      │  │   Layer     │  │   (Auth,etc)│             │
│  │             │  │             │  │             │             │
│  │ - Claude    │  │ - Contracts │  │ - Auth      │             │
│  │ - GPT-4     │  │ - Wallets   │  │ - Storage   │             │
│  │ - Local     │  │ - NFTs      │  │ - Analytics  │             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │                    Infrastructure (Infra)                    ││
│  │   Cloud Services │ Databases │ CDN │ Monitoring │ CI/CD     ││
│  └─────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────┘
```

## Component Details

### Frontend Applications

#### Web Application (`packages/app/web`)

- **Framework**: Next.js 14 with App Router
- **State Management**: Zustand
- **Styling**: Tailwind CSS
- **Data Fetching**: React Query
- **Web3**: Ethers.js / Viem

#### Mobile Application (`packages/app/mobile`)

- **Framework**: React Native with Expo
- **Navigation**: React Navigation
- **State Management**: Zustand
- **Styling**: NativeWind (Tailwind for React Native)

#### Admin Dashboard (`packages/admin-ui`)

- **Framework**: Modern SPA (Single Page Application)
- **Purpose**: Content moderation, administrative workflows, platform management
- **Backend**: Connects to `Mystira.Admin.Api` (REST/gRPC)

### Backend Services

#### Story Generator (`packages/story-generator`)

- **AI Models**: Claude (Anthropic), GPT-4 (OpenAI), Local models
- **Context Management**: Long-term memory system
- **API**: RESTful API with GraphQL support
- **Database**: PostgreSQL for story state, Redis for caching

#### Blockchain Layer (`packages/chain`)

- **Smart Contracts**: Solidity (Ethereum) / Move (Aptos)
- **Development**: Hardhat / Foundry
- **Testing**: Comprehensive test coverage
- **Deployment**: Automated via CI/CD

#### Admin API (`packages/admin-api`)

- **Type**: Pure REST/gRPC API (no Razor Pages UI)
- **Purpose**: Internal-facing API for moderation, content workflows, and administrative tooling
- **Database**: Shared with main application
- **Authentication**: Admin-level access control

### Infrastructure (`infra`)

#### Cloud Infrastructure

- **IaC**: Terraform
- **Orchestration**: Kubernetes
- **Containers**: Docker
- **CI/CD**: GitHub Actions

#### Services

- **Database**: PostgreSQL (managed)
- **Cache**: Redis (managed)
- **CDN**: CloudFlare / AWS CloudFront
- **Monitoring**: Prometheus + Grafana
- **Logging**: Loki / ELK Stack

## Data Flow

### Story Generation Flow

```
User Input → API → Story Generator → AI Models → Context Manager → Response
```

### Blockchain Interaction Flow

```
User Action → Web3 Wallet → Smart Contract → Blockchain → Event → Backend
```

### Authentication Flow

The platform uses a tiered authentication strategy based on user type:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    Authentication Architecture                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                          │
│  ┌─────────────────┐   ┌─────────────────┐   ┌─────────────────────────┐│
│  │   Admin Users   │   │ Consumer Users  │   │   Service-to-Service   ││
│  ├─────────────────┤   ├─────────────────┤   ├─────────────────────────┤│
│  │ Microsoft Entra │   │   Azure AD B2C  │   │    Managed Identity    ││
│  │   ID (OIDC)     │   │   (OAuth 2.0)   │   │    (Azure RBAC)        ││
│  ├─────────────────┤   ├─────────────────┤   ├─────────────────────────┤│
│  │ • Admin UI      │   │ • PWA           │   │ • Cosmos DB access     ││
│  │ • Admin API     │   │ • Public API    │   │ • Key Vault access     ││
│  │ • MFA required  │   │ • Social login: │   │ • Inter-service calls  ││
│  │ • App Roles     │   │   - Google      │   │                         ││
│  │                 │   │   - Discord     │   │                         ││
│  └─────────────────┘   └─────────────────┘   └─────────────────────────┘│
│                                                                          │
└─────────────────────────────────────────────────────────────────────────┘
```

**Authentication Flows by Component**:

| Component | Auth Method | Provider | Token Type |
|-----------|-------------|----------|------------|
| Admin UI | Cookie + OIDC | Microsoft Entra ID | Session cookie |
| Admin API | JWT Bearer | Microsoft Entra ID | Access token |
| Public API | JWT Bearer | Azure AD B2C | Access token |
| PWA | MSAL + B2C | Azure AD B2C | Access + Refresh |
| Services | Managed Identity | Azure | AAD token |

**Social Login** (via Azure AD B2C):
- Google OAuth 2.0 for Google accounts
- Discord OpenID Connect for gaming community
- Email/password for local accounts

For detailed implementation, see:
- [ADR-0010: Authentication Strategy](./architecture/adr/0010-authentication-and-authorization-strategy.md)
- [ADR-0011: Entra ID Integration](./architecture/adr/0011-entra-id-authentication-integration.md)

## Technology Stack

### Frontend

- React 18+
- Next.js 14
- TypeScript
- Tailwind CSS
- React Query
- Zustand

### Backend

- Node.js 18+
- TypeScript
- PostgreSQL
- Redis
- GraphQL (optional)

### Blockchain

- Solidity / Move
- Hardhat / Foundry
- Ethers.js / Viem

### AI/ML

- Anthropic Claude API
- OpenAI GPT-4 API
- Local model support (Ollama, etc.)

### Infrastructure

- Docker
- Kubernetes
- Terraform
- GitHub Actions

## Security Considerations

- All API endpoints are authenticated
- Smart contracts undergo security audits
- Secrets managed via environment variables / Vault
- HTTPS/TLS everywhere
- Regular dependency updates
- Security scanning in CI/CD

## Scalability

- Horizontal scaling via Kubernetes
- Database read replicas
- CDN for static assets
- Redis caching layer
- Load balancing
- Auto-scaling based on metrics

## Monitoring & Observability

- **Metrics**: Prometheus
- **Visualization**: Grafana
- **Logging**: Centralized logging system
- **Tracing**: Distributed tracing (OpenTelemetry)
- **Alerting**: PagerDuty / Slack integration
