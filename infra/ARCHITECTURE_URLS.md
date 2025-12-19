# Environment URL Architecture

## Visual Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        mystira.app DNS Zone                      │
│                     (Azure DNS - Shared)                         │
└──────────────┬──────────────────┬──────────────────┬────────────┘
               │                  │                  │
               │                  │                  │
    ┌──────────▼──────────┐  ┌───▼──────────┐  ┌───▼──────────┐
    │  Dev Environment    │  │   Staging     │  │ Production   │
    │                     │  │  Environment  │  │ Environment  │
    └─────────────────────┘  └───────────────┘  └──────────────┘
    
    dev.publisher           staging.publisher    publisher
    dev.chain              staging.chain         chain
         ↓                       ↓                    ↓
    ┌────────────┐         ┌────────────┐       ┌────────────┐
    │ Dev Load   │         │ Staging LB │       │  Prod LB   │
    │ Balancer   │         │            │       │            │
    └──────┬─────┘         └──────┬─────┘       └──────┬─────┘
           │                      │                     │
    ┌──────▼───────────┐   ┌─────▼──────────┐   ┌─────▼──────────┐
    │  AKS Dev Cluster │   │ AKS Stg Cluster│   │ AKS Prod Cluster│
    │  Namespace:      │   │ Namespace:     │   │ Namespace:      │
    │  mys-dev         │   │ mys-staging    │   │ mys-prod        │
    └──────────────────┘   └────────────────┘   └─────────────────┘
```

## Request Flow

### Development Environment

```
User Browser
    ↓
https://dev.publisher.mystira.app
    ↓
Azure DNS → Resolves to Dev Load Balancer IP
    ↓
Dev Load Balancer (NGINX Ingress Controller)
    ↓
NGINX checks ingress rules for "dev.publisher.mystira.app"
    ↓
Routes to Service: mystira-publisher (namespace: mys-dev)
    ↓
Service routes to Pods with label: app=mystira-publisher
    ↓
Publisher Pod responds
    ↓
Response encrypted with Let's Encrypt Staging Certificate ⚠️
    ↓
User sees Publisher UI (with browser security warning)
```

### Production Environment

```
User Browser
    ↓
https://publisher.mystira.app
    ↓
Azure DNS → Resolves to Prod Load Balancer IP
    ↓
Prod Load Balancer (NGINX Ingress Controller)
    ↓
NGINX checks ingress rules for "publisher.mystira.app"
    ↓
Routes to Service: mystira-publisher (namespace: mys-prod)
    ↓
Service routes to Pods with label: app=mystira-publisher
    ↓
Publisher Pod responds
    ↓
Response encrypted with Let's Encrypt Production Certificate ✅
    ↓
User sees Publisher UI (no warnings, fully trusted)
```

## DNS Record Structure

```
mystira.app (Zone)
├── @ (apex)
│   └── TXT: "mystira-domain-verification=..."
│
├── dev.publisher
│   └── A: <Dev Load Balancer IP>
│
├── dev.chain
│   └── A: <Dev Load Balancer IP>
│
├── staging.publisher
│   └── A: <Staging Load Balancer IP>
│
├── staging.chain
│   └── A: <Staging Load Balancer IP>
│
├── publisher
│   └── A: <Prod Load Balancer IP>
│
└── chain
    └── A: <Prod Load Balancer IP>
```

## Kubernetes Ingress Configuration

### Dev Environment (mys-dev namespace)

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: mystira-publisher-ingress
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-staging"  # ← Staging cert
spec:
  ingressClassName: nginx
  tls:
    - hosts:
        - dev.publisher.mystira.app  # ← Dev hostname
      secretName: mystira-publisher-tls
  rules:
    - host: dev.publisher.mystira.app  # ← Dev hostname
      http:
        paths:
          - path: /
            backend:
              service:
                name: mystira-publisher
                port:
                  number: 3000
```

### Production Environment (mys-prod namespace)

```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: mystira-publisher-ingress
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"  # ← Production cert
spec:
  ingressClassName: nginx
  tls:
    - hosts:
        - publisher.mystira.app  # ← Production hostname (no prefix)
      secretName: mystira-publisher-tls
  rules:
    - host: publisher.mystira.app  # ← Production hostname
      http:
        paths:
          - path: /
            backend:
              service:
                name: mystira-publisher
                port:
                  number: 3000
```

## Certificate Issuers

### Development & Staging

```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-staging
spec:
  acme:
    server: https://acme-staging-v02.api.letsencrypt.org/directory  # ← Staging server
    email: admin@mystira.app
    privateKeySecretRef:
      name: letsencrypt-staging
    solvers:
      - http01:
          ingress:
            class: nginx
```

**Characteristics:**
- ⚠️ Not trusted by browsers (shows security warning)
- 🔄 No rate limits (can request unlimited certificates)
- 🧪 Perfect for testing

### Production

```yaml
apiVersion: cert-manager.io/v1
kind: ClusterIssuer
metadata:
  name: letsencrypt-prod
spec:
  acme:
    server: https://acme-v02.api.letsencrypt.org/directory  # ← Production server
    email: admin@mystira.app
    privateKeySecretRef:
      name: letsencrypt-prod
    solvers:
      - http01:
          ingress:
            class: nginx
```

**Characteristics:**
- ✅ Fully trusted by all browsers
- ⏱️ Rate limited (50 certificates per week per domain)
- 🔒 Production-ready security

## Deployment Flow

```
1. Developer pushes code
        ↓
2. CI/CD builds Docker image
        ↓
3. Image pushed to ACR with tag (dev/staging/prod)
        ↓
4. Kubernetes deployment updated
        ↓
5. Kustomize applies environment overlay
        ↓
6. Ingress configured with correct hostname
        ↓
7. cert-manager requests SSL certificate
        ↓
8. Let's Encrypt validates domain ownership
        ↓
9. Certificate issued and installed
        ↓
10. Service accessible at environment URL
```

## Access Patterns

### Local Development → Dev Environment

```
Developer's Machine
    ↓
https://dev.publisher.mystira.app
    ↓ (Accept security warning)
Test new features
```

### QA Team → Staging Environment

```
QA Team
    ↓
https://staging.publisher.mystira.app
    ↓ (Accept security warning)
Test release candidates
```

### End Users → Production

```
Public Users
    ↓
https://publisher.mystira.app
    ↓ (No warnings, fully trusted)
Production application
```

## Security Boundaries

```
┌─────────────────────────────────────────────────────────────┐
│                        Internet                              │
└────────────────────┬────────────────────────────────────────┘
                     │
          ┌──────────┼──────────┐
          │          │          │
    ┌─────▼────┐ ┌──▼─────┐ ┌──▼─────┐
    │   Dev    │ │ Staging│ │  Prod  │
    │ Ingress  │ │ Ingress│ │ Ingress│
    │ (Public) │ │(Public)│ │(Public)│
    └─────┬────┘ └──┬─────┘ └──┬─────┘
          │         │          │
    ┌─────▼────┐ ┌──▼─────┐ ┌──▼─────┐
    │   Dev    │ │ Staging│ │  Prod  │
    │   VNet   │ │  VNet  │ │  VNet  │
    │(Private) │ │(Private)│ │(Private)│
    └──────────┘ └────────┘ └────────┘
```

**Network Isolation:**
- Each environment has its own VNet
- No cross-environment traffic
- Separate load balancers
- Separate DNS records
- Separate SSL certificates

## Monitoring Points

```
DNS Resolution
    ↓ (Monitor: DNS query success rate)
Load Balancer
    ↓ (Monitor: Connection count, latency)
Ingress Controller
    ↓ (Monitor: Request rate, 4xx/5xx errors)
Service
    ↓ (Monitor: Endpoint availability)
Pod
    ↓ (Monitor: CPU, memory, restarts)
Application
    ↓ (Monitor: Business metrics, errors)
```

## Quick Reference Matrix

| Aspect         | Dev                       | Staging                       | Production            |
| -------------- | ------------------------- | ----------------------------- | --------------------- |
| **URL**        | dev.publisher.mystira.app | staging.publisher.mystira.app | publisher.mystira.app |
| **SSL**        | Staging (⚠️)               | Staging (⚠️)                   | Production (✅)        |
| **Replicas**   | 1                         | 2                             | 3+ (with HPA)         |
| **Resources**  | Low                       | Medium                        | High                  |
| **Auto-scale** | No                        | No                            | Yes                   |
| **DNS TTL**    | 300s                      | 300s                          | 300s                  |
| **Namespace**  | mys-dev                   | mys-staging                   | mys-prod              |
| **Purpose**    | Development               | QA/Testing                    | Production            |
| **Audience**   | Developers                | QA Team                       | End Users             |

## Related Documentation

- [ENVIRONMENT_URLS_SETUP.md](./ENVIRONMENT_URLS_SETUP.md) - Complete setup guide
- [QUICK_ACCESS.md](./QUICK_ACCESS.md) - Quick access commands
- [ENVIRONMENT_URL_CHANGES_SUMMARY.md](./ENVIRONMENT_URL_CHANGES_SUMMARY.md) - What changed
- [DNS_INGRESS_SETUP.md](./DNS_INGRESS_SETUP.md) - DNS setup details
- [README.md](./README.md) - Infrastructure overview
