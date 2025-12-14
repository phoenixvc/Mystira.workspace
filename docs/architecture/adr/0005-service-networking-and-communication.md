# ADR-0005: Service Networking and Communication

## Status

**Accepted** - 2025-12-14

## Context

The Mystira platform consists of multiple services with different technology stacks and deployment models:

1. **Mystira.App Services** (.NET):
   - Public API (`Mystira.App.Api`)
   - Admin API (`Mystira.App.Admin.Api`) - _to be extracted to separate repository_
   - PWA (Blazor WebAssembly)

2. **Mystira.Chain** (Python gRPC service):
   - Blockchain/Web3 operations
   - Deployed to Kubernetes

3. **Mystira.Publisher** (TypeScript/React):
   - Frontend web application
   - Communicates with Chain service via gRPC
   - Deployed to Kubernetes

4. **Mystira.StoryGenerator** (.NET):
   - AI story generation service
   - Deployed to Kubernetes

5. **Infrastructure Services**:
   - Azure Cosmos DB (shared database)
   - Azure Blob Storage
   - Azure Key Vault
   - Redis (caching)
   - PostgreSQL (StoryGenerator)

### Communication Patterns

Different services communicate using different protocols:

- **HTTP/REST**: Public API, Admin API, StoryGenerator
- **gRPC**: Chain service (Python), Publisher frontend → Chain backend
- **Database**: Direct connections to Cosmos DB, PostgreSQL, Redis
- **Message Queues**: Azure Service Bus (potentially)

### Problem Statement

We need to define:

1. **Service boundaries**: How services communicate and what protocols they use
2. **Network topology**: Public vs. private endpoints, service discovery
3. **API design**: REST vs. gRPC, versioning, contracts
4. **Security boundaries**: Authentication, authorization, network isolation
5. **Service discovery**: How services find each other
6. **Deployment model impact**: How different deployment models (K8s vs. Azure PaaS) affect networking

## Decision

We adopt a **hybrid networking strategy** with clear service boundaries:

### 1. Service-to-Service Communication

#### Public-Facing Services (HTTP/REST)

**Services**: Public API, Admin API, StoryGenerator

**Protocol**: HTTP/REST with JSON

**Characteristics**:

- Standard REST APIs with OpenAPI/Swagger documentation
- JSON request/response payloads
- HTTPS in production
- Standard HTTP status codes

**Examples**:

- `GET /api/v1/scenarios`
- `POST /api/v1/gamesessions`
- `GET /admin/api/v1/users`

#### Internal Services (gRPC)

**Services**: Chain service (Python gRPC server)

**Protocol**: gRPC over HTTP/2

**Characteristics**:

- Type-safe contracts via Protocol Buffers (`.proto` files)
- Binary serialization (efficient)
- Streaming support
- Strong typing across language boundaries

**Examples**:

- `CreateCollection(CreateCollectionRequest) -> CollectionResponse`
- `RegisterAsset(RegisterAssetRequest) -> AssetResponse`

**When to Use gRPC**:

- Internal service-to-service communication
- Cross-language service boundaries (Python ↔ TypeScript/.NET)
- Performance-critical operations
- When strong typing is important

**When to Use REST**:

- Public-facing APIs
- Web browser clients
- Simple request/response patterns
- When HTTP caching is beneficial

#### Frontend-to-Backend Communication

**Publisher Frontend → Chain Service**:

- Protocol: gRPC-Web (for browser compatibility)
- Client: `grpc-web` library in TypeScript
- Server: Python gRPC service with gRPC-Web support

**PWA → Public API**:

- Protocol: HTTP/REST
- Client: HttpClient, Fetch API
- Server: ASP.NET Core REST API

**Admin UI → Admin API**:

- Current: Razor Pages (server-rendered, no API boundary)
- Future: REST API with modern frontend (React/Blazor) - _planned extraction_

### 2. Network Topology

#### Public Endpoints

**Public-Facing Services**:

- Public API: `api.mystira.app` (future) / `prod-wus-app-mystira-api.azurewebsites.net` (current)
- Publisher: `publisher.mystira.app` / Static hosting
- PWA: `mystira.app`

**Access**: Public internet, HTTPS only

#### Internal Endpoints

**Kubernetes Services**:

- Chain service: `mystira-chain:8545` (ClusterIP, internal only)
- Publisher: `mystira-publisher:3000` (ClusterIP, ingress for public access)
- StoryGenerator: `mystira-story-generator:8080` (ClusterIP, ingress for public access)

**Azure PaaS Services**:

- Admin API: `prod-wus-app-mystira-api-admin.azurewebsites.net` (VPN/Private Endpoint for internal access recommended)
- Public API: `prod-wus-app-mystira-api.azurewebsites.net` (public)

#### Service Discovery

**Kubernetes Services**:

- Use Kubernetes DNS: `{service-name}.{namespace}.svc.cluster.local`
- Services discover each other via DNS names
- Example: `http://mystira-chain.mystira-staging.svc.cluster.local:8545`

**Azure PaaS Services**:

- Use Azure App Service hostnames
- Configure via environment variables or configuration
- Use Azure Private Endpoints for internal communication

**Environment Variables**:

```bash
# Publisher → Chain
CHAIN_RPC_URL=http://mystira-chain:8545  # Kubernetes DNS
CHAIN_WS_URL=ws://mystira-chain:8546

# App Services → Cosmos DB
COSMOS_DB_CONNECTION_STRING=AccountEndpoint=...
```

### 3. API Design Principles

#### REST API Design

**Standards**:

- RESTful resource-based URLs
- HTTP methods (GET, POST, PUT, DELETE, PATCH)
- JSON request/response bodies
- Standard HTTP status codes
- OpenAPI/Swagger documentation

**Versioning**:

- URL path versioning: `/api/v1/`, `/api/v2/`
- Header versioning (alternative): `Accept: application/vnd.mystira.v1+json`

**Examples**:

```
GET    /api/v1/scenarios
GET    /api/v1/scenarios/{id}
POST   /api/v1/scenarios
PUT    /api/v1/scenarios/{id}
DELETE /api/v1/scenarios/{id}
```

#### gRPC API Design

**Standards**:

- Protocol Buffers for contract definition
- Service-oriented method names
- Strong typing enforced at compile time
- Streaming support for long-running operations

**Versioning**:

- Package versioning in `.proto` files: `package mystira.chain.v1;`
- Service versioning via package names

**Examples**:

```protobuf
service StoryService {
  rpc CreateCollection(CreateCollectionRequest) returns (CollectionResponse);
  rpc RegisterAsset(RegisterAssetRequest) returns (AssetResponse);
}
```

### 4. Security Boundaries

#### Network-Level Security

**Public Services**:

- HTTPS/TLS everywhere
- WAF (Web Application Firewall) for public endpoints
- DDoS protection
- Rate limiting

**Internal Services**:

- Private networks (Kubernetes cluster, Azure VNet)
- Private endpoints for Azure PaaS
- VPN/Azure Private Link for cross-platform communication
- Service mesh (future consideration)

#### Authentication/Authorization

**Public API**:

- JWT tokens for user authentication
- OAuth 2.0 / OpenID Connect
- Role-based access control (RBAC)

**Admin API**:

- Separate authentication/authorization
- Admin-specific roles and permissions
- Stronger security requirements
- VPN/Private Endpoint recommended

**Service-to-Service**:

- Service-to-service authentication (future: mTLS)
- API keys or service principals
- Azure Managed Identities for Azure services

### 5. Data Access Patterns

#### Shared Databases

**Azure Cosmos DB**:

- Shared by Public API and Admin API
- Different containers/partitions per concern
- Access via connection strings or managed identities

**PostgreSQL**:

- StoryGenerator-specific
- Connection pooling
- Read replicas for scalability

**Redis**:

- Shared caching layer
- Service-specific key prefixes
- TTL-based expiration

#### Direct vs. API Access

**Current Pattern**: Direct database access

- Services connect directly to databases
- No API gateway between services and databases
- Simpler architecture, tighter coupling

**Future Consideration**: API-only access

- Services only communicate via APIs
- No direct database access between services
- Better isolation, more complex

## Rationale

### 1. Protocol Choice: REST vs. gRPC

**REST for Public APIs**:

- ✅ Standard web protocol
- ✅ Browser-friendly
- ✅ Wide tooling support
- ✅ Human-readable
- ✅ Caching-friendly (HTTP semantics)

**gRPC for Internal Services**:

- ✅ Type-safe contracts
- ✅ Efficient binary serialization
- ✅ Cross-language support (Python ↔ TypeScript)
- ✅ Streaming support
- ✅ Better performance for internal communication

### 2. Hybrid Network Topology

**Kubernetes DNS for K8s Services**:

- ✅ Native Kubernetes service discovery
- ✅ No external dependencies
- ✅ Automatic load balancing
- ✅ Works within cluster network

**Environment Variables for Cross-Platform**:

- ✅ Simple and flexible
- ✅ Works across deployment models
- ✅ Environment-specific configuration
- ⚠️ Manual configuration required

### 3. Security Boundaries

**Separate Public and Internal**:

- ✅ Admin API isolated from public API
- ✅ Internal services not exposed publicly
- ✅ Defense in depth
- ✅ Different security postures per service

## Consequences

### Positive

1. **Clear Boundaries**: Each service has defined communication protocols
2. **Flexibility**: Can use best protocol for each use case
3. **Security**: Network isolation improves security posture
4. **Scalability**: Services can scale independently
5. **Technology Diversity**: Supports different tech stacks (Python, TypeScript, .NET)

### Negative

1. **Complexity**: Multiple protocols to maintain (REST, gRPC)
2. **Learning Curve**: Team must understand multiple communication patterns
3. **Tooling**: Different tools for REST vs. gRPC debugging
4. **Cross-Platform Challenges**: Kubernetes ↔ Azure PaaS communication requires careful configuration

### Mitigations

1. **Documentation**: Comprehensive API documentation for all services
2. **Standards**: Clear guidelines on when to use REST vs. gRPC
3. **Tooling**: Unified observability (logging, tracing, monitoring)
4. **Service Mesh** (Future): Consider Istio/Linkerd for unified service-to-service communication

## Service Communication Matrix

| From                 | To         | Protocol                | Authentication                 | Network                      |
| -------------------- | ---------- | ----------------------- | ------------------------------ | ---------------------------- |
| Publisher (Frontend) | Chain      | gRPC-Web                | Metadata headers (private key) | Public → Kubernetes ingress  |
| Publisher (Frontend) | Public API | HTTP/REST               | JWT                            | Public → Azure App Service   |
| Admin UI             | Admin API  | HTTP/REST (Razor Pages) | JWT                            | Internal → Azure App Service |
| Public API           | Cosmos DB  | Direct                  | Managed Identity               | Azure PaaS → Azure PaaS      |
| Admin API            | Cosmos DB  | Direct                  | Managed Identity               | Azure PaaS → Azure PaaS      |
| StoryGenerator       | PostgreSQL | Direct                  | Connection String              | Kubernetes → Database        |
| All Services         | Redis      | Direct                  | Connection String              | Service → Cache              |
| All Services         | Key Vault  | Azure SDK               | Managed Identity               | Service → Key Vault          |

## Future Considerations

### Service Mesh

**Consideration**: Implement service mesh (Istio/Linkerd) for:

- Unified service-to-service communication
- Automatic mTLS
- Traffic management
- Observability

**Timeline**: Evaluate when service count grows or cross-platform communication becomes complex

### API Gateway

**Consideration**: Implement API Gateway for:

- Unified entry point
- Rate limiting
- Authentication/authorization
- Request routing

**Timeline**: Evaluate when service count grows or need unified API management

### Event-Driven Communication

**Consideration**: Add message queues (Azure Service Bus, Kafka) for:

- Asynchronous communication
- Event sourcing
- Decoupled services

**Timeline**: When services need async communication patterns

## Implementation Notes

### Current State

- ✅ Chain service: gRPC (Python)
- ✅ Publisher: gRPC-Web client
- ✅ Public API: REST (ASP.NET Core)
- ✅ Admin API: REST + Razor Pages (ASP.NET Core)
- ✅ StoryGenerator: REST (ASP.NET Core)

### Upcoming Changes

1. **Admin API Extraction**: Will move to separate repository, maintain REST API
2. **Admin UI Modernization**: May separate UI from API, communicate via REST
3. **Service Mesh Evaluation**: Monitor for future implementation

## Related ADRs

- [ADR-0001: Infrastructure Organization](./0001-infrastructure-organization-hybrid-approach.md) - Deployment models affect networking
- [ADR-0003: Release Pipeline Strategy](./0003-release-pipeline-strategy.md) - Deployment processes
- [ADR-0004: Branching Strategy and CI/CD Process](./0004-branching-strategy-and-cicd.md) - CI/CD workflows
- [ADR-0006: Admin API Repository Extraction](./0006-admin-api-repository-extraction.md) - Repository structure changes
- [ADR-0007: NuGet Feed Strategy for Shared Libraries](./0007-nuget-feed-strategy-for-shared-libraries.md) - Package management
- [Admin API Extraction Plan](../migration/ADMIN_API_EXTRACTION_PLAN.md) - Implementation details

## References

- [gRPC Documentation](https://grpc.io/docs/)
- [Protocol Buffers](https://developers.google.com/protocol-buffers)
- [REST API Design Best Practices](https://restfulapi.net/)
- [Kubernetes Service Discovery](https://kubernetes.io/docs/concepts/services-networking/service/)
- [Azure Private Endpoints](https://docs.microsoft.com/en-us/azure/private-link/private-endpoint-overview)
